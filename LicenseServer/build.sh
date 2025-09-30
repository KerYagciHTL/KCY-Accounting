#!/usr/bin/env bash
# Cross-platform build script for LicenseServer
# Works on Linux, macOS, and Windows (Git Bash / MSYS2). For native Windows PowerShell use build.ps1.

set -euo pipefail
IFS=$'\n\t'

# Argument parsing (flags before any build logic)
RUN_AFTER_BUILD=false
BUILD_ALL_RIDS=${BUILD_ALL_RIDS:-false}
for arg in "$@"; do
  case "$arg" in
    --run|run) RUN_AFTER_BUILD=true ;;
    --all|all) BUILD_ALL_RIDS=true ;;
    -h|--help|help)
      cat <<EOF
Usage: ./build.sh [--run] [--all]
  --run    Run the produced binary for the current platform after publish
  --all    Build all default RIDs (win-x64 linux-x64 osx-arm64 osx-x64)
Environment overrides:
  CONFIGURATION=Release|Debug
  SELF_CONTAINED=true|false
  SINGLE_FILE=true|false
  PUBLISH_DIR=publish
  RIDS="custom list" (overrides automatic RID selection)
  BUILD_ALL_RIDS=true (same as --all)
EOF
      exit 0
      ;;
  esac
done

# Configurable variables (override by exporting before running)
CONFIGURATION="${CONFIGURATION:-Release}"
SELF_CONTAINED="${SELF_CONTAINED:-true}"
SINGLE_FILE="${SINGLE_FILE:-true}"
PUBLISH_DIR="${PUBLISH_DIR:-publish}" # root publish directory
RETRY_ATTEMPTS=${RETRY_ATTEMPTS:-8}
RETRY_DELAY=${RETRY_DELAY:-2}

purple="\033[35m"; green="\033[32m"; red="\033[31m"; yellow="\033[33m"; reset="\033[0m"
log_section(){ echo -e "${purple}\n================================\n$1\n================================${reset}"; }
log_info(){ echo -e "${green}[INFO]${reset} $*"; }
log_warn(){ echo -e "${yellow}[WARN]${reset} $*"; }
log_err(){ echo -e "${red}[ERR ]${reset} $*" >&2; }

retry(){
  local attempt=1
  local cmd=("$@")
  while true; do
    if "${cmd[@]}"; then return 0; fi
    if (( attempt >= RETRY_ATTEMPTS )); then
      log_err "Command failed after ${RETRY_ATTEMPTS} attempts: ${cmd[*]}"
      return 1
    fi
    log_warn "Attempt ${attempt}/${RETRY_ATTEMPTS} failed. Retrying in ${RETRY_DELAY}s: ${cmd[*]}"
    sleep "${RETRY_DELAY}"
    ((attempt++))
  done
}

require_cmd(){ if ! command -v "$1" >/dev/null 2>&1; then log_err "Required command '$1' not found in PATH"; exit 1; fi }

detect_local_rid(){
  local os arch
  os="$(uname -s | tr '[:upper:]' '[:lower:]')"
  arch="$(uname -m)"
  case "$os" in
    linux)
      case "$arch" in
        x86_64|amd64) echo "linux-x64" ;;
        aarch64|arm64) echo "linux-arm64" ;; # may not be supported by native raylib libs
        *) echo "linux-x64" ;;
      esac
      ;;
    darwin)
      case "$arch" in
        arm64) echo "osx-arm64" ;;
        x86_64) echo "osx-x64" ;;
        *) echo "osx-x64" ;;
      esac
      ;;
    msys*|mingw*|cygwin*) echo "win-x64" ;;
    *) echo "linux-x64" ;;
  esac
}

LOCAL_RID="$(detect_local_rid)"

# Determine RIDS array
if [[ -n "${RIDS:-}" ]]; then
  # User provided explicit list
  # shellcheck disable=SC2206
  RIDS=( ${RIDS} )
else
  if [[ "$BUILD_ALL_RIDS" == "true" ]]; then
    RIDS=( win-x64 linux-x64 osx-arm64 osx-x64 )
  else
    RIDS=( "$LOCAL_RID" )
  fi
fi

log_section "License Server Build"
log_info "Local RID detected: ${LOCAL_RID}"
if [[ "$BUILD_ALL_RIDS" == "true" && ${#RIDS[@]} -eq 1 ]]; then
  log_warn "BUILD_ALL_RIDS requested but only one RID resolved."
fi
log_info "RIDs to build: ${RIDS[*]}"
require_cmd dotnet
log_info ".NET SDK: $(dotnet --version)"

# Discover / create project
csproj=$(find . -maxdepth 1 -name '*.csproj' | head -n1 || true)
if [[ -z "${csproj}" ]]; then
  log_warn "No .csproj found. Creating new console project (net9.0)."
  retry dotnet new console -f net9.0 --force
  csproj=$(find . -maxdepth 1 -name '*.csproj' | head -n1)
fi
log_info "Using project: ${csproj}"

# Ensure required packages (idempotent - ignore already added warnings)
add_pkg(){ local name="$1" version="$2"; if ! grep -q "<PackageReference Include=\"${name}\"" "$csproj"; then retry dotnet add "$csproj" package "$name" --version "$version"; else log_info "Package ${name} already referenced"; fi }
add_pkg Raylib-cs 5.0.0 || true
add_pkg System.Text.Json 8.0.0 || true
add_pkg DotNetEnv 2.4.0 || true

retry dotnet restore "$csproj"
retry dotnet build "$csproj" --configuration "$CONFIGURATION" --no-restore

# Publish per RID
for rid in "${RIDS[@]}"; do
  outDir="${PUBLISH_DIR}/${rid}"
  log_section "Publishing ${rid} -> ${outDir}"
  mkdir -p "${outDir}"
  publish_args=(publish "$csproj" --configuration "$CONFIGURATION" --runtime "$rid" --self-contained "$SELF_CONTAINED" -p:IncludeNativeLibrariesForSelfExtract=true --output "$outDir")
  if [[ "$SINGLE_FILE" == "true" ]]; then
    publish_args+=(-p:PublishSingleFile=true)
  fi
  if ! retry dotnet "${publish_args[@]}"; then
     log_warn "Publish failed for RID ${rid}. Continuing."; continue
  fi
  for f in config.json licenses.json .env; do
    [[ -f "$f" ]] && cp "$f" "${outDir}/" || true
  done
  log_info "Published ${rid} size: $(du -sh "${outDir}" | cut -f1)"
  find "${outDir}" -maxdepth 1 -type f -printf '%f\n' | sort | sed 's/^/  - /'
 done

log_section "Summary"
if [[ -d "${PUBLISH_DIR}" ]]; then
  find "${PUBLISH_DIR}" -type f -printf '%p (%k KB)\n' | sed 's/^/  /'
fi

if [[ "$RUN_AFTER_BUILD" == "true" ]]; then
  exePath="${PUBLISH_DIR}/${LOCAL_RID}/LicenseServer"
  if [[ "$LOCAL_RID" == win-* ]]; then exePath+=".exe"; fi
  if [[ -f "$exePath" ]]; then
    log_section "Running (${exePath})"
    "$exePath"
  else
    log_warn "Executable not found for run: $exePath"
  fi
fi

echo -e "${green}Done.${reset}"

using System.Net.Sockets;
using System.Text;
using dotenv.net;
using System.Buffers;

namespace KCY_Accounting.Core
{
    public static class Client
    {
        private static string _serverIp = "127.0.0.1"; // fallback
        private const int PORT = 4053;
        private const int CONNECTION_TIMEOUT_MS = 2500; 
        private const int READ_WRITE_TIMEOUT_MS = 2500;
        private const int MAX_RETRIES = 2;
        private const int RETRY_DELAY_MS = 300;
        private const int BUFFER_SIZE = 512;
        private const int MAX_RESPONSE_BYTES = 4096;

        private static readonly NetworkHelper NetworkHelper = new();
        private static long _lastRequestTicks = 0;
        private static readonly TimeSpan MinDelay = TimeSpan.FromMilliseconds(1000);
        private static volatile bool _initialized;

        static Client()
        {
            try
            {
                if (!_initialized)
                {
                    DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: true));
                    _initialized = true;
                }
                var envIp = Environment.GetEnvironmentVariable("LICENSE_SERVER_IP");
                if (!string.IsNullOrWhiteSpace(envIp))
                {
                    _serverIp = envIp.Trim();
                    Logger.Log($"Client: Using server ip from environment: {_serverIp}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Client: Failed loading environment overrides ({ex.Message}). Using default server ip {_serverIp}.");
            }
        }

        public static async Task<string> GetVersion(CancellationToken cancellationToken = default)
        {
            using var cts = CreateLinkedCts(cancellationToken);
            return await SendMessageAsync("getversion", cts.Token);
        }
        
        public static async Task<string> GetUserName(CancellationToken cancellationToken = default)
        {
            using var cts = CreateLinkedCts(cancellationToken);
            var response = await SendMessageAsync($"getusername-{Config.LicenseKey}-{Config.McAddress}", cts.Token);
            return response;
        }
        
        public static async Task<bool> IsValidLicenseAsync(string licenseKey, string mcAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                throw new ClientException("Lizenzschlüssel darf nicht leer sein.");

            await RateLimitAsync();

            return await ExecuteWithRetriesAsync(async attempt =>
            {
                using var cts = CreateLinkedCts(cancellationToken);
                var combined = licenseKey + "-" + mcAddress;
                var ok = await ValidateLicenseInternalAsync(combined, cts.Token);
                return ok;
            });
        }

        public static async Task<bool> ClearMcAddress(CancellationToken cancellationToken = default)
        {
            using var cts = CreateLinkedCts(cancellationToken);
            var response = await SendMessageAsync($"logout-{Config.LicenseKey}-{Config.McAddress}", cts.Token);
            return response.Equals(string.Empty);
        }

        private static async Task<bool> ValidateLicenseInternalAsync(string licenseKey, CancellationToken cancellationToken)
        {
            var response = await SendMessageAsync(licenseKey, cancellationToken);
            return response.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        
        private static async Task<bool> ValidateLicenseInternalAsync(string licenseKey)
        {
            using var cts = new CancellationTokenSource(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);
            return await ValidateLicenseInternalAsync(licenseKey, cts.Token);
        }

        private static async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ClientException("Nachricht darf nicht leer sein.");

            using var client = new TcpClient();
            using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            connectCts.CancelAfter(CONNECTION_TIMEOUT_MS);
            try
            {
                await client.ConnectAsync(_serverIp, PORT, connectCts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Verbindung zum Server hat zu lange gedauert.");
            }
            catch (SocketException ex)
            {
                throw new ClientException($"Fehler bei der Verbindung zum Server: {ex.SocketErrorCode}");
            }

            await using var stream = client.GetStream();
            stream.ReadTimeout = READ_WRITE_TIMEOUT_MS;
            stream.WriteTimeout = READ_WRITE_TIMEOUT_MS;

            byte[]? rentBuffer = null;
            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                using var rwTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                rwTimeoutCts.CancelAfter(READ_WRITE_TIMEOUT_MS);
                await stream.WriteAsync(data, rwTimeoutCts.Token);

                rentBuffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
                var totalBytes = 0;
                var sb = new StringBuilder();
                using var readTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                readTimeoutCts.CancelAfter(READ_WRITE_TIMEOUT_MS);

                while (totalBytes < MAX_RESPONSE_BYTES)
                {
                    var bytesRead = await stream.ReadAsync(rentBuffer.AsMemory(0, BUFFER_SIZE), readTimeoutCts.Token);
                    if (bytesRead == 0)
                    {
                        break; // server closed connection
                    }
                    totalBytes += bytesRead;
                    sb.Append(Encoding.UTF8.GetString(rentBuffer, 0, bytesRead));
                    if (sb.ToString().Contains('\n') || !stream.DataAvailable) break;
                }

                if (totalBytes == 0)
                    throw new SocketException(10054); // connection closed unexpectedly

                var result = sb.ToString().Trim();
                return result;
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Zeitüberschreitung beim Lesen oder Schreiben.");
            }
            catch (IOException ex) when (ex.InnerException is SocketException se)
            {
                throw new ClientException($"Socket Fehler: {se.SocketErrorCode}");
            }
            finally
            {
                if (rentBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(rentBuffer);
                }
            }
        }

        private static async Task<T> ExecuteWithRetriesAsync<T>(Func<int, Task<T>> action)
        {
            Exception? last = null;
            for (var attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    return await action(attempt);
                }
                catch (TimeoutException ex)
                {
                    last = ex;
                    if (attempt < MAX_RETRIES)
                        await Task.Delay(ComputeDelay(attempt));
                }
                catch (SocketException ex)
                {
                    last = ex;
                    if (attempt < MAX_RETRIES)
                        await Task.Delay(ComputeDelay(attempt));
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (!NetworkHelper.HasInternetConnection())
                        throw new ClientException("Keine Internetverbindung. Bitte überprüfen Sie Ihre Verbindung.");
                    throw new ClientException("Unbekannter Fehler. Bitte kontaktieren Sie den Support.");
                }
            }
            throw new ClientException("Verbindung oder Antwort fehlgeschlagen nach mehreren Versuchen." + (last != null ? $" (Letzter Fehler: {last.Message})" : string.Empty));
        }

        private static int ComputeDelay(int attempt)
        {
            var baseDelay = RETRY_DELAY_MS * attempt;
            var exp = RETRY_DELAY_MS * (int)Math.Pow(2, attempt - 1);
            var delay = Math.Min(Math.Max(baseDelay, exp), 2000);
            var jitter = Random.Shared.Next(0, 100);
            return delay + jitter;
        }

        private static CancellationTokenSource CreateLinkedCts(CancellationToken external)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(external);
            cts.CancelAfter(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);
            return cts;
        }

        private static async Task RateLimitAsync()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            while (true)
            {
                var last = Interlocked.Read(ref _lastRequestTicks);
                var elapsed = new TimeSpan(nowTicks - last);
                if (elapsed >= MinDelay)
                {
                    if (Interlocked.CompareExchange(ref _lastRequestTicks, nowTicks, last) == last)
                        return;
                }
                else
                {
                    var remaining = MinDelay - elapsed;
                    if (remaining > TimeSpan.Zero)
                        await Task.Delay(remaining);
                    nowTicks = DateTime.UtcNow.Ticks;
                }
            }
        }
    }
}

using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace KCY_Accounting.Core
{
    public static class Client
    {
        private const string SERVER_IP = "192.168.178.161";
        private const int PORT = 4053;
        private const int CONNECTION_TIMEOUT_MS = 2500; 
        private const int READ_WRITE_TIMEOUT_MS = 2500;
        private const int MAX_RETRIES = 2;
        private const int RETRY_DELAY_MS = 300;

        private static readonly NetworkHelper NetworkHelper = new();
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan MinDelay = TimeSpan.FromMilliseconds(1000);

        public static async Task<string> GetVersion()
        {
            using var cts = new CancellationTokenSource(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);
            var response = await SendMessageAsync("getversion", cts.Token);
            return response;
        }
        
        public static async Task<string> GetUserName()
        {
            using var cts = new CancellationTokenSource(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);
            var response = await SendMessageAsync($"getusername-{Config.LicenseKey}-{Config.McAddress}", cts.Token);
            return response;
        }
        public static async Task<bool> IsValidLicenseAsync(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                throw new ClientException("Lizenzschlüssel darf nicht leer sein.");

            await RateLimitAsync();

            for (var attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    return await ValidateLicenseInternalAsync(licenseKey + "-" + Config.McAddress);
                }
                catch (TimeoutException) when (attempt < MAX_RETRIES)
                {
                    await Task.Delay(RETRY_DELAY_MS * attempt);
                }
                catch (SocketException) when (attempt < MAX_RETRIES)
                {
                    await Task.Delay(RETRY_DELAY_MS * attempt);
                }
                catch (Exception)
                {
                    if (!NetworkHelper.HasInternetConnection())
                        throw new ClientException("Keine Internetverbindung. Bitte überprüfen Sie Ihre Verbindung.");

                    throw new ClientException("Unbekannter Fehler. Bitte kontaktieren Sie den Support.");
                }
            }

            throw new ClientException("Verbindung oder Antwort fehlgeschlagen nach mehreren Versuchen.");
        }

        private static async Task<bool> ValidateLicenseInternalAsync(string licenseKey)
        {
            using var cts = new CancellationTokenSource(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);
            var response = await SendMessageAsync(licenseKey, cts.Token);
            return response.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<string> SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ClientException("Nachricht darf nicht leer sein.");

            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(CONNECTION_TIMEOUT_MS + READ_WRITE_TIMEOUT_MS);

            try
            {
                await client.ConnectAsync(SERVER_IP, PORT, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Verbindung zum Server hat zu lange gedauert.");
            }
            catch (SocketException)
            {
                throw new ClientException("Fehler bei der Verbindung zum Server.");
            }

            await using var stream = client.GetStream();
            stream.ReadTimeout = READ_WRITE_TIMEOUT_MS;
            stream.WriteTimeout = READ_WRITE_TIMEOUT_MS;

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, cts.Token);

                var buffer = new byte[256];
                var bytesRead = await stream.ReadAsync(buffer, cts.Token);

                if (bytesRead == 0)
                    throw new SocketException(10054); // Connection to server was closed

                return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Zeitüberschreitung beim Lesen oder Schreiben.");
            }
            catch (IOException ex) when (ex.InnerException is SocketException se)
            {
                throw se;
            }
        }

        private static async Task RateLimitAsync()
        {
            var elapsed = DateTime.Now - _lastRequestTime;
            if (elapsed < MinDelay)
            {
                await Task.Delay(MinDelay - elapsed);
            }
            _lastRequestTime = DateTime.Now;
        }
    }
}

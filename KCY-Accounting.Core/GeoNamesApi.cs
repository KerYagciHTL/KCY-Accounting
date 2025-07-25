using System.Text.Json;

namespace KCY_Accounting.Core
{
    public static class GeoNamesApi
    {
        private const string Username = "kerimcan";

        private class GeoName
        {
            public string CountryCode { get; set; }
            public string Name { get; set; }
        }

        private class GeoNamesResponse
        {
            public GeoName[] Geonames { get; set; }
        }

        public static async Task<string?> GetCountryCodeAsync(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return null;

            string url = $"http://api.geonames.org/searchJSON?q={cityName}&maxRows=1&username={Username}";

            using HttpClient client = new();
            try
            {
                var response = await client.GetStringAsync(url);

                var result = JsonSerializer.Deserialize<GeoNamesResponse>(response);

                return result?.Geonames?.Length > 0
                    ? result.Geonames[0].CountryCode
                    : null;
            }
            catch
            {
                // Fehlerbehandlung kann hier erweitert werden (Loggen, Retry, etc.)
                return null;
            }
        }
    }
}
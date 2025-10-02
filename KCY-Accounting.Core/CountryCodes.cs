namespace KCY_Accounting.Core;

public static class CountryCodes
{
    private static readonly Dictionary<Country, CountryCode> Map = new()
    {
        { Country.Germany, CountryCode.DE },
        { Country.Austria, CountryCode.AT },
        { Country.Switzerland, CountryCode.CH },
        { Country.France, CountryCode.FR },
        { Country.Italy, CountryCode.IT },
        { Country.Spain, CountryCode.ES },
        { Country.Netherlands, CountryCode.NL },
        { Country.Belgium, CountryCode.BE },
        { Country.Poland, CountryCode.PL },
        { Country.Portugal, CountryCode.PT },
        { Country.Sweden, CountryCode.SE },
        { Country.Norway, CountryCode.NO },
        { Country.Denmark, CountryCode.DK },
        { Country.Finland, CountryCode.FI },
        { Country.Ireland, CountryCode.IE },
        { Country.Greece, CountryCode.GR },
        { Country.CzechRepublic, CountryCode.CZ },
        { Country.Slovakia, CountryCode.SK },
        { Country.Hungary, CountryCode.HU },
        { Country.Romania, CountryCode.RO },
        { Country.Bulgaria, CountryCode.BG },
        { Country.Croatia, CountryCode.HR },
        { Country.Slovenia, CountryCode.SI },
        { Country.Estonia, CountryCode.EE },
        { Country.Latvia, CountryCode.LV },
        { Country.Lithuania, CountryCode.LT },
        { Country.Cyprus, CountryCode.CY },
        { Country.Malta, CountryCode.MT },
        { Country.Luxembourg, CountryCode.LU },
        { Country.UnitedKingdom, CountryCode.GB },
        { Country.UnitedStates, CountryCode.US },
        { Country.Canada, CountryCode.CA },
        { Country.Australia, CountryCode.AU },
        { Country.NewZealand, CountryCode.NZ },
        { Country.China, CountryCode.CN },
        { Country.Japan, CountryCode.JP },
        { Country.SouthKorea, CountryCode.KR },
        { Country.Brazil, CountryCode.BR },
        { Country.Mexico, CountryCode.MX },
        { Country.Russia, CountryCode.RU },
        { Country.Turkey, CountryCode.TR },
        { Country.India, CountryCode.IN },
        { Country.SouthAfrica, CountryCode.ZA },
        { Country.Ukraine, CountryCode.UA },
        { Country.Serbia, CountryCode.RS },
        { Country.BosniaAndHerzegovina, CountryCode.BA },
        { Country.NorthMacedonia, CountryCode.MK },
        { Country.Albania, CountryCode.AL },
        { Country.Kosovo, CountryCode.XK }
    };

    // Reverse lookup dictionary to make GetCountry O(1) instead of O(n)
    private static readonly Dictionary<CountryCode, Country> ReverseMap = Map.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    
    public static CountryCode GetCountryCode(Country country)
    {
        return Map.TryGetValue(country, out var code) ? code : throw new ArgumentException($"Country {country} does not have a defined country code.");
    }
    
    public static Country GetCountry(CountryCode code)
    {
        return ReverseMap.TryGetValue(code, out var country) ? country : throw new ArgumentException($"Country code {code} does not correspond to any defined country.");
    }
}
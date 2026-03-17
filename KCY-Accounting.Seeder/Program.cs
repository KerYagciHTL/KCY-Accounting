using System.Text;
using KCY_Accounting.Core.Models;
using KCY_Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

// Force UTF-8 for console output so umlauts display correctly
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding  = Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║  KCY-Accounting – Datenbank Seeder       ║");
Console.WriteLine("╚══════════════════════════════════════════╝");

// Same DB path as the UI application uses
var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "KCY-Accounting",
    "kcy_accounting.db");

Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
Console.WriteLine($"  DB-Pfad: {dbPath}");

// Use explicit UTF-8 encoding in the connection string
var connectionString = $"Data Source={dbPath};";

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .Options;

await using var db = new AppDbContext(options);
await db.Database.EnsureCreatedAsync();

// ── Guard ────────────────────────────────────────────────────────────────────
if (await db.Customers.AnyAsync())
{
    Console.WriteLine("  ⚠ Datenbank enthält bereits Daten – Seed übersprungen.");
    Console.WriteLine("    Lösche zuerst die .db-Datei für einen Neustart.");
    return;
}

Console.WriteLine("  Seeding läuft...\n");

// ─────────────────────────────────────────────────────────────────────────────
// KUNDEN
// ─────────────────────────────────────────────────────────────────────────────
var customers = new List<Customer>
{
    new() {
        CustomerNumber        = "KD-00001",
        CompanyName           = "Mayer Logistik GmbH",
        ContactPerson         = "Thomas Mayer",
        Phone                 = "+43 1 234 5670",
        Email                 = "t.mayer@mayer-logistik.at",
        Street                = "Lagerstraße 12",
        ZipCode               = "1030",
        City                  = "Wien",
        Country               = "Österreich",
        VatNumber             = "ATU12345678",
        PaymentTermDays       = 30,
        Currency              = "EUR",
        FreightPayerName      = "Mayer Logistik GmbH",
        FreightPayerVatNumber = "ATU12345678",
        Notes                 = "Stammkunde seit 2018. Bevorzugt Freitag-Lieferungen.",
        IsActive              = true
    },
    new() {
        CustomerNumber  = "KD-00002",
        CompanyName     = "Schneider Transport AG",
        ContactPerson   = "Anna Schneider",
        Phone           = "+49 89 99887766",
        Email           = "a.schneider@schneider-transport.de",
        Street          = "Industriepark 5",
        ZipCode         = "80331",
        City            = "München",
        Country         = "Deutschland",
        VatNumber       = "DE987654321",
        PaymentTermDays = 14,
        Currency        = "EUR",
        Notes           = "Expressaufträge regelmäßig.",
        IsActive        = true
    },
    new() {
        CustomerNumber  = "KD-00003",
        CompanyName     = "Bauer & Söhne KG",
        ContactPerson   = "Markus Bauer",
        Phone           = "+43 732 445566",
        Email           = "m.bauer@bauer-soehne.at",
        Street          = "Frachtgasse 3",
        ZipCode         = "4020",
        City            = "Linz",
        Country         = "Österreich",
        VatNumber       = "ATU55667788",
        PaymentTermDays = 30,
        Currency        = "EUR",
        Notes           = "Palettentausch immer vereinbaren!",
        IsActive        = true
    },
    new() {
        CustomerNumber        = "KD-00004",
        CompanyName           = "Kovács Kereskedelmi Kft.",
        ContactPerson         = "Péter Kovács",
        Phone                 = "+36 1 300 4400",
        Email                 = "p.kovacs@kovacs-kft.hu",
        Street                = "Váci út 88",
        ZipCode               = "1133",
        City                  = "Budapest",
        Country               = "Ungarn",
        VatNumber             = "HU12345678",
        PaymentTermDays       = 45,
        Currency              = "EUR",
        FreightPayerName      = "Kovács Holding Zrt.",
        FreightPayerVatNumber = "HU87654321",
        Notes                 = "Rechnungen auf Englisch schicken.",
        IsActive              = true
    },
    new() {
        CustomerNumber  = "KD-00005",
        CompanyName     = "Hofer Spedition e.U.",
        ContactPerson   = "Sabine Hofer",
        Phone           = "+43 316 778899",
        Email           = "s.hofer@hofer-spedition.at",
        Street          = "Güterbahnhofstraße 21",
        ZipCode         = "8020",
        City            = "Graz",
        Country         = "Österreich",
        VatNumber       = "ATU33445566",
        PaymentTermDays = 30,
        Currency        = "EUR",
        Notes           = "Kühltransporte bevorzugt.",
        IsActive        = true
    },
    new() {
        CustomerNumber  = "KD-00006",
        CompanyName     = "Müller Handels GmbH",
        ContactPerson   = "Josef Müller",
        Phone           = "+43 662 112233",
        Email           = "j.mueller@mueller-handel.at",
        Street          = "Salzachufer 7",
        ZipCode         = "5020",
        City            = "Salzburg",
        Country         = "Österreich",
        VatNumber       = "ATU77889900",
        PaymentTermDays = 60,
        Currency        = "EUR",
        Notes           = "Inaktiv – kein Neugeschäft.",
        IsActive        = false
    },
    new() {
        CustomerNumber  = "KD-00007",
        CompanyName     = "Gruber & Partner OG",
        ContactPerson   = "Herbert Gruber",
        Phone           = "+43 5574 22334",
        Email           = "h.gruber@gruber-partner.at",
        Street          = "Rheinstraße 44",
        ZipCode         = "6900",
        City            = "Bregenz",
        Country         = "Österreich",
        VatNumber       = "ATU44556677",
        PaymentTermDays = 30,
        Currency        = "EUR",
        Notes           = "Lieferungen nach CH und DE wöchentlich.",
        IsActive        = true
    },
    new() {
        CustomerNumber  = "KD-00008",
        CompanyName     = "Züricher Handelshaus AG",
        ContactPerson   = "Ursula Zürcher",
        Phone           = "+41 44 999 8877",
        Email           = "u.zuercher@zh-ag.ch",
        Street          = "Hardturmstraße 101",
        ZipCode         = "8005",
        City            = "Zürich",
        Country         = "Schweiz",
        VatNumber       = "CHE-123.456.789",
        PaymentTermDays = 30,
        Currency        = "CHF",
        Notes           = "Immer Vorauszahlung bei Erstauftrag.",
        IsActive        = true
    },
    new() {
        CustomerNumber  = "KD-00009",
        CompanyName     = "Österreichische Bauholding AG",
        ContactPerson   = "Dieter Öhlinger",
        Phone           = "+43 1 880 44 0",
        Email           = "d.oehlinger@bau-holding.at",
        Street          = "Wienerbergstraße 11",
        ZipCode         = "1100",
        City            = "Wien",
        Country         = "Österreich",
        VatNumber       = "ATU99887766",
        PaymentTermDays = 45,
        Currency        = "EUR",
        FreightPayerName = "ÖBH Logistik GmbH",
        FreightPayerVatNumber = "ATU11223344",
        Notes           = "Schwertransporte und Überlängen möglich.",
        IsActive        = true
    },
    new() {
        CustomerNumber  = "KD-00010",
        CompanyName     = "Nürnberger Maschinenbau GmbH",
        ContactPerson   = "Klaus Nürnberger",
        Phone           = "+49 911 44556677",
        Email           = "k.nuernberger@nmb-gmbh.de",
        Street          = "Sigmundstraße 100",
        ZipCode         = "90431",
        City            = "Nürnberg",
        Country         = "Deutschland",
        VatNumber       = "DE123456789",
        PaymentTermDays = 30,
        Currency        = "EUR",
        Notes           = "Maschinenteile – immer ADR prüfen.",
        IsActive        = true
    },
};

db.Customers.AddRange(customers);
await db.SaveChangesAsync();
Console.WriteLine($"  ✓ {customers.Count} Kunden angelegt");

// ─────────────────────────────────────────────────────────────────────────────
// FRÄCHTER
// ─────────────────────────────────────────────────────────────────────────────
var carriers = new List<Carrier>
{
    new() {
        CarrierNumber = "FR-00001",
        CompanyName   = "Nowak Transport s.r.o.",
        ContactPerson = "Jakub Nowak",
        Phone         = "+420 602 111222",
        Email         = "j.nowak@nowak-transport.cz",
        Street        = "Průmyslová 14",
        ZipCode       = "30100",
        City          = "Plzeň",
        Country       = "Tschechien",
        BankName      = "Česká spořitelna",
        Iban          = "CZ6508000000192000145399",
        Bic           = "GIBACZPX",
        Notes         = "Zuverlässig, 2 eigene Fahrzeuge."
    },
    new() {
        CarrierNumber = "FR-00002",
        CompanyName   = "Balkan Trans d.o.o.",
        ContactPerson = "Mirko Petrović",
        Phone         = "+381 11 3344556",
        Email         = "m.petrovic@balkantrans.rs",
        Street        = "Bulevar Oslobođenja 120",
        ZipCode       = "11000",
        City          = "Beograd",
        Country       = "Serbien",
        BankName      = "Raiffeisen Bank Serbia",
        Iban          = "RS35265100000012345678",
        Bic           = "RZBSRSBG",
        Notes         = "Nur Westeuropa-Touren. Günstige Konditionen."
    },
    new() {
        CarrierNumber = "FR-00003",
        CompanyName   = "Huber Transporte GmbH",
        ContactPerson = "Wolfgang Huber",
        Phone         = "+49 851 556677",
        Email         = "w.huber@huber-transporte.de",
        Street        = "Gewerbepark Nord 8",
        ZipCode       = "94032",
        City          = "Passau",
        Country       = "Deutschland",
        BankName      = "Sparkasse Passau",
        Iban          = "DE89370400440532013000",
        Bic           = "COBADEFFXXX",
        Notes         = "Schwertransporte möglich. 3 LKW verfügbar."
    },
    new() {
        CarrierNumber = "FR-00004",
        CompanyName   = "Adriatic Cargo d.o.o.",
        ContactPerson = "Ivan Horvat",
        Phone         = "+385 1 5544332",
        Email         = "i.horvat@adriaticcargo.hr",
        Street        = "Slavonska avenija 6",
        ZipCode       = "10000",
        City          = "Zagreb",
        Country       = "Kroatien",
        BankName      = "Erste Bank Hrvatska",
        Iban          = "HR1210010051863000160",
        Bic           = "ESBCHR22",
        Notes         = "Günstig für Balkan-Touren."
    },
    new() {
        CarrierNumber = "FR-00005",
        CompanyName   = "Vogel & Partner Spedition",
        ContactPerson = "Christine Vogel",
        Phone         = "+43 2236 889900",
        Email         = "c.vogel@vogel-spedition.at",
        Street        = "Hafenstraße 33",
        ZipCode       = "2344",
        City          = "Maria Enzersdorf",
        Country       = "Österreich",
        BankName      = "Raiffeisenbank NÖ",
        Iban          = "AT483200000012345864",
        Bic           = "RLNWATWWGD",
        Notes         = "Langjähriger Partner, Sonderkonditionen."
    },
    new() {
        CarrierNumber = "FR-00006",
        CompanyName   = "Pöschl Transporte KG",
        ContactPerson = "Franz Pöschl",
        Phone         = "+43 7229 78899",
        Email         = "f.poeschl@poeschl-trans.at",
        Street        = "Welser Straße 88",
        ZipCode       = "4600",
        City          = "Wels",
        Country       = "Österreich",
        BankName      = "Volksbank OÖ",
        Iban          = "AT611904300234573201",
        Bic           = "VBOEATWWSIM",
        Notes         = "Kühlfahrzeuge vorhanden."
    },
    new() {
        CarrierNumber = "FR-00007",
        CompanyName   = "Rychlý Kurýr s.r.o.",
        ContactPerson = "Tomáš Blažek",
        Phone         = "+420 731 555666",
        Email         = "t.blazek@rychly-kuryr.cz",
        Street        = "Korunní 47",
        ZipCode       = "12000",
        City          = "Praha",
        Country       = "Tschechien",
        BankName      = "Komerční banka",
        Iban          = "CZ0100000000123456789012",
        Bic           = "KOMBCZPPXXX",
        Notes         = "Expressdienst für Stückgut."
    },
    new() {
        CarrierNumber = "FR-00008",
        CompanyName   = "Alpen Trans GmbH",
        ContactPerson = "Günther Alpenreiter",
        Phone         = "+43 5522 77665",
        Email         = "g.alpenreiter@alpentrans.at",
        Street        = "Dornbirner Straße 15",
        ZipCode       = "6800",
        City          = "Feldkirch",
        Country       = "Österreich",
        BankName      = "Hypo Vorarlberg",
        Iban          = "AT565400003100000120",
        Bic           = "HYPVAT2B",
        Notes         = "Spezialist für Alpenüberquerungen, CH und IT."
    },
};

db.Carriers.AddRange(carriers);
await db.SaveChangesAsync();
Console.WriteLine($"  ✓ {carriers.Count} Frächter angelegt");

// ─────────────────────────────────────────────────────────────────────────────
// TRANSPORTAUFTRÄGE
// ─────────────────────────────────────────────────────────────────────────────
// Helper so code stays readable
static TransportStop Stop(string name, string street, string zip, string city,
    string country, DateTime? from = null, DateTime? to = null,
    string contact = "", string phone = "", string? reference = null) => new()
{
    CompanyOrPersonName = name,
    Street              = street,
    ZipCode             = zip,
    City                = city,
    Country             = country,
    DateFrom            = from,
    DateTo              = to,
    ContactPerson       = contact,
    Phone               = phone,
    Reference           = reference
};

var d = new DateTime(2026, 1, 10); // base date

var orders = new List<TransportOrder>
{
    // ── TA-20260001  Zugestellt ───────────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260001",
        CustomerId        = customers[0].Id,
        CarrierId         = carriers[0].Id,
        OrderDate         = d,
        CustomerReference = "PO-MAYER-001",
        LoadingPoint = Stop("Mayer Logistik Lager Wien", "Lagerstraße 12", "1030", "Wien", "Österreich",
            d.AddDays(1), d.AddDays(1).AddHours(4), "Karl Huber", "+43 664 1234567", "LADE-001"),
        UnloadingPoint = Stop("Schneider Transport München", "Industriepark 5", "80331", "München", "Deutschland",
            d.AddDays(2), d.AddDays(2).AddHours(6), "Anna Schneider", "+49 89 99887766", "ENT-001"),
        GoodsDescription = "Elektronikbauteile auf Europaletten",
        WeightKg = 8500m, PalletCount = 18, LoadingMeters = 7.5m,
        FreightType = FreightType.EuroPalletWithExchange, IsHazardousGoods = false,
        LicensePlate = "W-NX-4521", DriverName = "Pavel Dvořák",
        SalePrice = 1850m, PurchasePrice = 1200m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.Delivered
    },

    // ── TA-20260002  Abgerechnet ──────────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260002",
        CustomerId        = customers[1].Id,
        CarrierId         = carriers[2].Id,
        OrderDate         = d.AddDays(2),
        CustomerReference = "SCH-2026-089",
        LoadingPoint = Stop("BMW Werk München", "Petuelring 130", "80809", "München", "Deutschland",
            d.AddDays(3), d.AddDays(3).AddHours(3), "Stefan Maier", "+49 89 38220", "BMW-LADE-08"),
        UnloadingPoint = Stop("Bauer & Söhne Lager Linz", "Frachtgasse 3", "4020", "Linz", "Österreich",
            d.AddDays(4), d.AddDays(4).AddHours(5), "Markus Bauer", "+43 732 445566"),
        GoodsDescription = "Fahrzeugteile FTL",
        WeightKg = 22000m, PalletCount = 33, LoadingMeters = 13.6m,
        FreightType = FreightType.FtlStandard, IsHazardousGoods = false,
        LicensePlate = "PA-HU-872", DriverName = "Franz Gruber",
        SalePrice = 3400m, PurchasePrice = 2100m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.Invoiced
    },

    // ── TA-20260003  Unterwegs + Gefahrgut ───────────────────────────────────
    new() {
        OrderNumber       = "TA-20260003",
        CustomerId        = customers[2].Id,
        CarrierId         = carriers[3].Id,
        OrderDate         = d.AddDays(5),
        CustomerReference = null,
        LoadingPoint = Stop("Bauer & Söhne Linz", "Frachtgasse 3", "4020", "Linz", "Österreich",
            d.AddDays(6), d.AddDays(6).AddHours(2), "Hans Gruber", "+43 732 445566", "LIN-003"),
        UnloadingPoint = Stop("Adriatic Cargo Zagreb", "Slavonska avenija 6", "10000", "Zagreb", "Kroatien",
            d.AddDays(7), d.AddDays(7).AddHours(4), "Ivan Horvat", "+385 1 5544332", "ZAG-22"),
        GoodsDescription = "Chemikalien ADR Klasse 3 – Flüssigkeiten",
        WeightKg = 14000m, PalletCount = 20, LoadingMeters = 9.0m,
        FreightType = FreightType.EuroPalletWithoutExchange, IsHazardousGoods = true,
        LicensePlate = "ZG-123-AB", DriverName = "Tomislav Barić",
        SalePrice = 2750m, PurchasePrice = 1800m, VatRate = 0m, Currency = "EUR",
        Status = OrderStatus.InTransit
    },

    // ── TA-20260004  Beauftragt ───────────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260004",
        CustomerId        = customers[3].Id,
        CarrierId         = carriers[1].Id,
        OrderDate         = d.AddDays(7),
        CustomerReference = "KOV-2026-44",
        LoadingPoint = Stop("Kovács Lager Budapest", "Váci út 88", "1133", "Budapest", "Ungarn",
            d.AddDays(9), d.AddDays(9).AddHours(3), "Péter Kovács", "+36 1 300 4400", "BUD-44"),
        UnloadingPoint = Stop("Hofer Spedition Graz", "Güterbahnhofstraße 21", "8020", "Graz", "Österreich",
            d.AddDays(10), d.AddDays(10).AddHours(4), "Sabine Hofer", "+43 316 778899"),
        GoodsDescription = "Textilwaren auf EUR-Paletten",
        WeightKg = 6200m, PalletCount = 14, LoadingMeters = 5.6m,
        FreightType = FreightType.EuroPalletWithoutExchange, IsHazardousGoods = false,
        LicensePlate = "BG-MK-1122", DriverName = "Dragan Milić",
        SalePrice = 1400m, PurchasePrice = 920m, VatRate = 0m, Currency = "EUR",
        Status = OrderStatus.Assigned
    },

    // ── TA-20260005  Neu – kein Frächter ─────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260005",
        CustomerId        = customers[4].Id,
        CarrierId         = null,
        OrderDate         = d.AddDays(10),
        CustomerReference = "HOF-ORD-07",
        LoadingPoint = Stop("Hofer Spedition Graz", "Güterbahnhofstraße 21", "8020", "Graz", "Österreich",
            d.AddDays(12), null, "Sabine Hofer", "+43 316 778899"),
        UnloadingPoint = Stop("Endkunde Salzburg", "Europark Allee 1", "5020", "Salzburg", "Österreich",
            d.AddDays(13), null, "Petra Lang", "+43 662 555888", "SAL-7788"),
        GoodsDescription = "Lebensmittel (gekühlt) EUR-Pal mit Tausch",
        WeightKg = 5000m, PalletCount = 10, LoadingMeters = 4.0m,
        FreightType = FreightType.EuroPalletWithExchange, IsHazardousGoods = false,
        LicensePlate = null, DriverName = null,
        SalePrice = 980m, PurchasePrice = 0m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.New
    },

    // ── TA-20260006  Zugestellt FTL ──────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260006",
        CustomerId        = customers[0].Id,
        CarrierId         = carriers[4].Id,
        OrderDate         = d.AddDays(12),
        CustomerReference = "PO-MAYER-002",
        LoadingPoint = Stop("Bosch Werk Wien", "Quellenstraße 1", "1100", "Wien", "Österreich",
            d.AddDays(13), d.AddDays(13).AddHours(3), "Rudi Brandner", "+43 1 60110", "BOSCH-6"),
        UnloadingPoint = Stop("Vogel & Partner Lager", "Hafenstraße 33", "2344", "Maria Enzersdorf", "Österreich",
            d.AddDays(13).AddHours(8), d.AddDays(13).AddHours(12), "Christine Vogel", "+43 2236 889900"),
        GoodsDescription = "Elektromotoren verpalettiert",
        WeightKg = 18500m, PalletCount = 28, LoadingMeters = 13.6m,
        FreightType = FreightType.FtlStandard, IsHazardousGoods = false,
        LicensePlate = "ME-VP-3300", DriverName = "Gerhard Wieser",
        SalePrice = 2200m, PurchasePrice = 1450m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.Delivered
    },

    // ── TA-20260007  Abgerechnet ──────────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260007",
        CustomerId        = customers[1].Id,
        CarrierId         = carriers[0].Id,
        OrderDate         = d.AddDays(15),
        CustomerReference = "SCH-2026-101",
        LoadingPoint = Stop("Siemens AG Erlangen", "Werner-von-Siemens-Str. 50", "91052", "Erlangen", "Deutschland",
            d.AddDays(16), d.AddDays(16).AddHours(4), "Michael Braun", "+49 9131 70", "SIE-101"),
        UnloadingPoint = Stop("Schneider Wien Depot", "Erdberger Lände 30", "1030", "Wien", "Österreich",
            d.AddDays(17), d.AddDays(17).AddHours(3), "Lisa Klein", "+43 1 796060", "WIE-101"),
        GoodsDescription = "Industriesteuerungen",
        WeightKg = 9200m, PalletCount = 16, LoadingMeters = 6.4m,
        FreightType = FreightType.EuroPalletWithExchange, IsHazardousGoods = false,
        LicensePlate = "W-NX-4521", DriverName = "Pavel Dvořák",
        SalePrice = 1950m, PurchasePrice = 1280m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.Invoiced
    },

    // ── TA-20260008  Neu – Stahlcoils ─────────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260008",
        CustomerId        = customers[2].Id,
        CarrierId         = null,
        OrderDate         = d.AddDays(18),
        CustomerReference = "BAU-888",
        LoadingPoint = Stop("voestalpine Linz", "voestalpine-Straße 1", "4020", "Linz", "Österreich",
            d.AddDays(20), null, "Werner Steiner", "+43 50304-0", "VA-888"),
        UnloadingPoint = Stop("Bauer & Söhne Lager", "Frachtgasse 3", "4020", "Linz", "Österreich",
            d.AddDays(20).AddHours(4), null, "Markus Bauer", "+43 732 445566"),
        GoodsDescription = "Stahlcoils – Überlänge 14m",
        WeightKg = 24000m, PalletCount = null, LoadingMeters = 14.0m,
        FreightType = FreightType.FtlMegatrailer, IsHazardousGoods = false,
        LicensePlate = null, DriverName = null,
        SalePrice = 1600m, PurchasePrice = 0m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.New
    },

    // ── TA-20260009  Zugestellt CH-Tour ──────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260009",
        CustomerId        = customers[7].Id,
        CarrierId         = carriers[7].Id,
        OrderDate         = d.AddDays(3),
        CustomerReference = "ZH-2026-019",
        LoadingPoint = Stop("Gruber & Partner Bregenz", "Rheinstraße 44", "6900", "Bregenz", "Österreich",
            d.AddDays(5), d.AddDays(5).AddHours(3), "Herbert Gruber", "+43 5574 22334", "BRG-019"),
        UnloadingPoint = Stop("Züricher Handelshaus", "Hardturmstraße 101", "8005", "Zürich", "Schweiz",
            d.AddDays(6), d.AddDays(6).AddHours(5), "Ursula Zürcher", "+41 44 999 8877", "ZH-IN-19"),
        GoodsDescription = "Möbel und Einrichtungsgegenstände",
        WeightKg = 7800m, PalletCount = 22, LoadingMeters = 10.0m,
        FreightType = FreightType.EuroPalletWithoutExchange, IsHazardousGoods = false,
        LicensePlate = "FK-AT-9900", DriverName = "Günther Alpenreiter",
        SalePrice = 2400m, PurchasePrice = 1550m, VatRate = 0m, Currency = "CHF",
        Status = OrderStatus.Delivered
    },

    // ── TA-20260010  Unterwegs – Maschinenteile DE ───────────────────────────
    new() {
        OrderNumber       = "TA-20260010",
        CustomerId        = customers[9].Id,
        CarrierId         = carriers[2].Id,
        OrderDate         = d.AddDays(20),
        CustomerReference = "NMB-2026-55",
        LoadingPoint = Stop("Nürnberger Maschinenbau", "Sigmundstraße 100", "90431", "Nürnberg", "Deutschland",
            d.AddDays(22), d.AddDays(22).AddHours(4), "Klaus Nürnberger", "+49 911 44556677", "NMB-OUT-55"),
        UnloadingPoint = Stop("Österreichische Bauholding", "Wienerbergstraße 11", "1100", "Wien", "Österreich",
            d.AddDays(23), d.AddDays(23).AddHours(6), "Dieter Öhlinger", "+43 1 880440", "ÖBH-IN-55"),
        GoodsDescription = "Maschinenbauteile, schwer – ADR geprüft",
        WeightKg = 19500m, PalletCount = null, LoadingMeters = 13.6m,
        FreightType = FreightType.FtlStandard, IsHazardousGoods = false,
        LicensePlate = "PA-HU-872", DriverName = "Franz Gruber",
        SalePrice = 2950m, PurchasePrice = 1900m, VatRate = 0m, Currency = "EUR",
        Status = OrderStatus.InTransit
    },

    // ── TA-20260011  Beauftragt – Kühlware ───────────────────────────────────
    new() {
        OrderNumber       = "TA-20260011",
        CustomerId        = customers[4].Id,
        CarrierId         = carriers[5].Id,
        OrderDate         = d.AddDays(22),
        CustomerReference = "HOF-ORD-12",
        LoadingPoint = Stop("Großmarkt Graz", "Ostbahnstraße 10", "8041", "Graz", "Österreich",
            d.AddDays(24), d.AddDays(24).AddHours(2), "Helmut Groß", "+43 316 891100", "GRZ-K-12"),
        UnloadingPoint = Stop("Rewe Lager Wels", "Schwechater Straße 16", "4600", "Wels", "Österreich",
            d.AddDays(24).AddHours(6), d.AddDays(24).AddHours(10), "Sandra Reithofer", "+43 7242 55000"),
        GoodsDescription = "Frischware Obst & Gemüse (Kühlung +4°C)",
        WeightKg = 11000m, PalletCount = 24, LoadingMeters = 9.6m,
        FreightType = FreightType.EuroPalletWithExchange, IsHazardousGoods = false,
        LicensePlate = "WE-PÖ-3311", DriverName = "Franz Pöschl",
        SalePrice = 1750m, PurchasePrice = 1100m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.Assigned
    },

    // ── TA-20260012  Heute angelegt – Neu ────────────────────────────────────
    new() {
        OrderNumber       = "TA-20260012",
        CustomerId        = customers[6].Id,
        CarrierId         = null,
        OrderDate         = DateTime.Today,
        CustomerReference = "GRP-2026-77",
        LoadingPoint = Stop("Gruber & Partner Bregenz", "Rheinstraße 44", "6900", "Bregenz", "Österreich",
            DateTime.Today.AddDays(3), null, "Herbert Gruber", "+43 5574 22334"),
        UnloadingPoint = Stop("Müller Handels Salzburg", "Salzachufer 7", "5020", "Salzburg", "Österreich",
            DateTime.Today.AddDays(4), null, "Josef Müller", "+43 662 112233"),
        GoodsDescription = "Büromöbel auf EUR-Paletten",
        WeightKg = 3200m, PalletCount = 8, LoadingMeters = 3.2m,
        FreightType = FreightType.EuroPalletWithoutExchange, IsHazardousGoods = false,
        LicensePlate = null, DriverName = null,
        SalePrice = 750m, PurchasePrice = 0m, VatRate = 20m, Currency = "EUR",
        Status = OrderStatus.New
    },

    // ── TA-20260013  LTL – Teilladung mit Abmessungen ────────────────────────
    new() {
        OrderNumber       = "TA-20260013",
        CustomerId        = customers[3].Id,
        CarrierId         = carriers[6].Id,
        OrderDate         = d.AddDays(25),
        CustomerReference = "KOV-2026-LTL-01",
        LoadingPoint = Stop("Kovács Lager Budapest", "Váci út 88", "1133", "Budapest", "Ungarn",
            d.AddDays(27), null, "Péter Kovács", "+36 1 300 4400", "BUD-LTL-01"),
        UnloadingPoint = Stop("Rychlý Kurýr Lager Praha", "Korunní 47", "12000", "Praha", "Tschechien",
            d.AddDays(28), null, "Tomáš Blažek", "+420 731 555666", "PRG-REC-88"),
        GoodsDescription = "Maschinenteil auf Sonderpalette – LTL Teilladung",
        WeightKg = 1800m, PalletCount = 2, LoadingMeters = 2.4m,
        FreightType = FreightType.Ltl, IsHazardousGoods = false,
        // LTL cargo dimensions: 2.40 m × 1.20 m × 1.80 m
        LengthM = 2.40m, WidthM = 1.20m, HeightM = 1.80m,
        LicensePlate = "B-RK-55AA", DriverName = "Tomáš Blažek",
        SalePrice = 620m, PurchasePrice = 390m, VatRate = 0m, Currency = "EUR",
        Status = OrderStatus.Assigned
    },
};

db.TransportOrders.AddRange(orders);
await db.SaveChangesAsync();
Console.WriteLine($"  ✓ {orders.Count} Transportaufträge angelegt");

// ─────────────────────────────────────────────────────────────────────────────
// RECHNUNGEN
// ─────────────────────────────────────────────────────────────────────────────
var invoices = new List<Invoice>
{
    // TA-20260001 – Kunden- & Frächterrechnung
    new() { InvoiceNumber = "RE-20260001", TransportOrderId = orders[0].Id,
        Type = InvoiceType.CustomerInvoice, Amount = 1850m, VatRate = 20m, Currency = "EUR",
        IssuedAt = d.AddDays(3),  DueDate = d.AddDays(3 + 30), IsPaid = true },
    new() { InvoiceNumber = "FK-20260001", TransportOrderId = orders[0].Id,
        Type = InvoiceType.CarrierCost, Amount = 1200m, VatRate = 0m, Currency = "EUR",
        IssuedAt = d.AddDays(4),  DueDate = d.AddDays(4 + 30),
        CarrierInvoiceNumber = "NOW-2026-0055", IsPaid = true },

    // TA-20260002 – Kunden- & Frächterrechnung
    new() { InvoiceNumber = "RE-20260002", TransportOrderId = orders[1].Id,
        Type = InvoiceType.CustomerInvoice, Amount = 3400m, VatRate = 20m, Currency = "EUR",
        IssuedAt = d.AddDays(5),  DueDate = d.AddDays(5 + 14), IsPaid = true },
    new() { InvoiceNumber = "FK-20260002", TransportOrderId = orders[1].Id,
        Type = InvoiceType.CarrierCost, Amount = 2100m, VatRate = 0m, Currency = "EUR",
        IssuedAt = d.AddDays(6),  DueDate = d.AddDays(6 + 30),
        CarrierInvoiceNumber = "HUB-INV-2026-012", IsPaid = false },

    // TA-20260006 – Kunden- & Frächterrechnung
    new() { InvoiceNumber = "RE-20260003", TransportOrderId = orders[5].Id,
        Type = InvoiceType.CustomerInvoice, Amount = 2200m, VatRate = 20m, Currency = "EUR",
        IssuedAt = d.AddDays(14), DueDate = d.AddDays(14 + 30), IsPaid = false },
    new() { InvoiceNumber = "FK-20260003", TransportOrderId = orders[5].Id,
        Type = InvoiceType.CarrierCost, Amount = 1450m, VatRate = 0m, Currency = "EUR",
        IssuedAt = d.AddDays(15), DueDate = d.AddDays(15 + 30),
        CarrierInvoiceNumber = "VOG-2026-009", IsPaid = true },

    // TA-20260007 – Kundenrechnung
    new() { InvoiceNumber = "RE-20260004", TransportOrderId = orders[6].Id,
        Type = InvoiceType.CustomerInvoice, Amount = 1950m, VatRate = 20m, Currency = "EUR",
        IssuedAt = d.AddDays(18), DueDate = d.AddDays(18 + 14), IsPaid = true },
    new() { InvoiceNumber = "FK-20260004", TransportOrderId = orders[6].Id,
        Type = InvoiceType.CarrierCost, Amount = 1280m, VatRate = 0m, Currency = "EUR",
        IssuedAt = d.AddDays(19), DueDate = d.AddDays(19 + 30),
        CarrierInvoiceNumber = "NOW-2026-0071", IsPaid = true },

    // TA-20260009 – CH-Tour
    new() { InvoiceNumber = "RE-20260005", TransportOrderId = orders[8].Id,
        Type = InvoiceType.CustomerInvoice, Amount = 2400m, VatRate = 0m, Currency = "CHF",
        IssuedAt = d.AddDays(7),  DueDate = d.AddDays(7 + 30), IsPaid = true },
    new() { InvoiceNumber = "FK-20260005", TransportOrderId = orders[8].Id,
        Type = InvoiceType.CarrierCost, Amount = 1550m, VatRate = 0m, Currency = "CHF",
        IssuedAt = d.AddDays(8),  DueDate = d.AddDays(8 + 30),
        CarrierInvoiceNumber = "APT-2026-033", IsPaid = true },
};

db.Invoices.AddRange(invoices);
await db.SaveChangesAsync();
Console.WriteLine($"  ✓ {invoices.Count} Rechnungen angelegt");

// ─────────────────────────────────────────────────────────────────────────────
// FRAECHTERAUFTRAEGE (CarrierOrders)
// ─────────────────────────────────────────────────────────────────────────────
static TransportStop CoStop(string name, string street, string zip, string city,
    string country, DateTime? date = null, string? reference = null) => new()
{
    CompanyOrPersonName = name,
    Street  = street,
    ZipCode = zip,
    City    = city,
    Country = country,
    DateFrom  = date,
    Reference = reference
};

var carrierOrders = new List<CarrierOrder>
{
    // FA-20260001 – Nowak Transport, TA-20260001
    new() {
        CarrierOrderNumber = "FA-20260001",
        CarrierId          = carriers[0].Id,
        TransportOrderId   = orders[0].Id,
        IssuedAt           = d.AddDays(1),
        DueDate            = d.AddDays(1 + 30),
        GoodsDescription   = "Elektronikbauteile auf Europaletten",
        NetAmount          = 1200m, VatRate = 0m, Currency = "EUR",
        IsPaid             = true,
        LoadingPoint   = CoStop("Mayer Logistik Lager Wien",    "Lagerstraße 12", "1030", "Wien",    "Österreich", d.AddDays(1)),
        UnloadingPoint = CoStop("Schneider Transport München",  "Industriepark 5","80331","München", "Deutschland",d.AddDays(2)),
        FreightItems = new List<FreightItem>
        {
            new() { Quantity=18, LengthM=1.20m, WidthM=0.80m, HeightM=1.20m, WeightKgPerUnit=472m, Description="Europalette Elektronikbauteile" }
        }
    },
    // FA-20260002 – Huber Transporte, TA-20260002
    new() {
        CarrierOrderNumber = "FA-20260002",
        CarrierId          = carriers[2].Id,
        TransportOrderId   = orders[1].Id,
        IssuedAt           = d.AddDays(4),
        DueDate            = d.AddDays(4 + 30),
        GoodsDescription   = "Fahrzeugteile FTL",
        NetAmount          = 2100m, VatRate = 0m, Currency = "EUR",
        IsPaid             = false,
        LoadingPoint   = CoStop("BMW Werk München",         "Petuelring 130", "80809","München","Deutschland",d.AddDays(3), "BMW-LADE-08"),
        UnloadingPoint = CoStop("Bauer & Söhne Lager Linz", "Frachtgasse 3",  "4020", "Linz",  "Österreich", d.AddDays(4)),
        FreightItems = new List<FreightItem>
        {
            new() { Quantity=10, LengthM=2.00m, WidthM=1.20m, HeightM=0.80m, WeightKgPerUnit=800m,  Description="Fahrzeugrahmen" },
            new() { Quantity=23, LengthM=1.20m, WidthM=0.80m, HeightM=1.00m, WeightKgPerUnit=452m,  Description="Kleinteile EUR-Pal." }
        }
    },
    // FA-20260003 – Adriatic Cargo, TA-20260003 (Gefahrgut)
    new() {
        CarrierOrderNumber = "FA-20260003",
        CarrierId          = carriers[3].Id,
        TransportOrderId   = orders[2].Id,
        IssuedAt           = d.AddDays(6),
        DueDate            = d.AddDays(6 + 30),
        GoodsDescription   = "Chemikalien ADR Klasse 3",
        NetAmount          = 1800m, VatRate = 0m, Currency = "EUR",
        IsPaid             = false,
        LoadingPoint   = CoStop("Bauer & Söhne Linz",   "Frachtgasse 3",       "4020", "Linz",   "Österreich",d.AddDays(6),  "LIN-003"),
        UnloadingPoint = CoStop("Adriatic Cargo Zagreb", "Slavonska avenija 6", "10000","Zagreb", "Kroatien",  d.AddDays(7),  "ZAG-22"),
        FreightItems = new List<FreightItem>
        {
            new() { Quantity=20, LengthM=1.20m, WidthM=0.80m, HeightM=0.90m, WeightKgPerUnit=700m, Description="ADR Klasse 3 Fässer" }
        }
    },
    // FA-20260004 – Vogel & Partner, TA-20260006
    new() {
        CarrierOrderNumber = "FA-20260004",
        CarrierId          = carriers[4].Id,
        TransportOrderId   = orders[5].Id,
        IssuedAt           = d.AddDays(13),
        DueDate            = d.AddDays(13 + 30),
        GoodsDescription   = "Elektromotoren verpalettiert FTL",
        NetAmount          = 1450m, VatRate = 20m, Currency = "EUR",
        IsPaid             = true,
        LoadingPoint   = CoStop("Bosch Werk Wien",         "Quellenstraße 1",  "1100","Wien",           "Österreich",d.AddDays(13),           "BOSCH-6"),
        UnloadingPoint = CoStop("Vogel & Partner Lager",   "Hafenstraße 33",   "2344","Maria Enzersdorf","Österreich",d.AddDays(13).AddHours(8)),
        FreightItems = new List<FreightItem>
        {
            new() { Quantity=28, LengthM=1.20m, WidthM=0.80m, HeightM=1.10m, WeightKgPerUnit=661m, Description="Elektromotor EUR-Pal." }
        }
    },
    // FA-20260005 – Alpen Trans, TA-20260009 (CH-Tour)
    new() {
        CarrierOrderNumber = "FA-20260005",
        CarrierId          = carriers[7].Id,
        TransportOrderId   = orders[8].Id,
        IssuedAt           = d.AddDays(5),
        DueDate            = d.AddDays(5 + 30),
        GoodsDescription   = "Möbel und Einrichtungsgegenstände",
        NetAmount          = 1550m, VatRate = 0m, Currency = "CHF",
        IsPaid             = true,
        LoadingPoint   = CoStop("Gruber & Partner Bregenz", "Rheinstraße 44",      "6900","Bregenz","Österreich",d.AddDays(5), "BRG-019"),
        UnloadingPoint = CoStop("Züricher Handelshaus",     "Hardturmstraße 101",  "8005","Zürich", "Schweiz",   d.AddDays(6), "ZH-IN-19"),
        FreightItems = new List<FreightItem>
        {
            new() { Quantity=8,  LengthM=2.00m, WidthM=0.90m, HeightM=1.40m, WeightKgPerUnit=450m, Description="Schrankwand (Paket)" },
            new() { Quantity=14, LengthM=1.20m, WidthM=0.80m, HeightM=1.00m, WeightKgPerUnit=257m, Description="Kleinmöbel EUR-Pal." }
        }
    },
    // FA-20260006 – Rychlý Kurýr, TA-20260013 (LTL)
    new() {
        CarrierOrderNumber = "FA-20260006",
        CarrierId          = carriers[6].Id,
        TransportOrderId   = orders[12].Id,
        IssuedAt           = d.AddDays(26),
        DueDate            = d.AddDays(26 + 30),
        GoodsDescription   = "Maschinenteil auf Sonderpalette – LTL",
        NetAmount          = 390m, VatRate = 0m, Currency = "EUR",
        IsPaid             = false,
        LoadingPoint   = CoStop("Kovács Lager Budapest", "Váci út 88",   "1133","Budapest","Ungarn",     d.AddDays(27), "BUD-LTL-01"),
        UnloadingPoint = CoStop("Rychlý Kurýr Lager",   "Korunní 47",   "12000","Praha",  "Tschechien", d.AddDays(28), "PRG-REC-88"),
        FreightItems = new List<FreightItem>
        {
            // LTL single piece: L × W × H in metres
            new() { Quantity=2, LengthM=2.40m, WidthM=1.20m, HeightM=1.80m, WeightKgPerUnit=900m, Description="Maschinenteil Sondermaß" }
        }
    },
};

db.CarrierOrders.AddRange(carrierOrders);
await db.SaveChangesAsync();
Console.WriteLine($"  ✓ {carrierOrders.Count} Fraechterauftraege angelegt");

// ─────────────────────────────────────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║  Seed erfolgreich abgeschlossen!         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine($"  Kunden             : {customers.Count}");
Console.WriteLine($"  Fraechter          : {carriers.Count}");
Console.WriteLine($"  Transportauftraege : {orders.Count}");
Console.WriteLine($"  Rechnungen         : {invoices.Count}");
Console.WriteLine($"  Fraechterauftraege : {carrierOrders.Count}");


namespace KCY_Accounting.Core;

public class Order
{
    public readonly string InvoiceNumber;
    public readonly DateTime OrderDate;
    public readonly string CustomerNumber;
    public readonly Customer Customer;
    public readonly int InvoiceReference;
    public readonly Route Route;
    public readonly DateTime DateOfService;
    public readonly Driver Driver;
    public readonly FreightType FreightType;
    public readonly bool Pods;
    public readonly float NetAmount;
    public readonly NetCalculationType TaxStatus;
    public readonly float TaxAmount;
    public readonly float GrossAmount;
    public readonly string Description;
    
    public Order(string invoiceNumber, DateTime orderDate, string customerNumber, Customer customer, int invoiceReference, Route route, DateTime dateOfService, Driver driver, FreightType freightType, bool pods, float netAmount, NetCalculationType taxStatus, string description)
    {
        InvoiceNumber = invoiceNumber;
        OrderDate = orderDate;
        CustomerNumber = customerNumber;
        Customer = customer;
        InvoiceReference = invoiceReference;
        Route = route;
        DateOfService = dateOfService;
        Driver = driver;
        FreightType = freightType;
        Pods = pods;
        NetAmount = netAmount;
        TaxStatus = taxStatus;
        
        if (taxStatus == NetCalculationType.Yes)
        {
            TaxAmount = NetAmount * 0.2f;
            GrossAmount = NetAmount + TaxAmount;
        }
        else
        {
            TaxAmount = 0;
            GrossAmount = NetAmount;
        }
        
        Description = description;
    }

    public static Order? ReadCsvLine(string line, Customer[] customers, bool skip = true)
    {
        //[AUFTRAG]Rechnungsnummer;Auftragsdatum;Kundennummer;Kunden(Name);Rechnungsnummer;[Route]Von Bis;Leistungsdatum;[FAHRER]Fahrername Fahrernachname Kennzeichen Geburtstag Tel;Frachttyp;PODS;NettoBetrag;Steuerstatus;MwStBetrag;BruttoBetrag;Notiz
        var span = line.AsSpan();
        Span<Range> fields = stackalloc Range[15];
        int field = 0, start = 0;
        for (var i = 0; i < span.Length && field < fields.Length; i++)
        {
            if (span[i] != ';') continue;
            fields[field++] = new Range(start, i);
            start = i + 1;
        }
        if (field < fields.Length - 1)
        {
            if (skip) return null;
            throw new ArgumentException("Zu wenige Felder in der CSV-Zeile.");
        }
        fields[field] = new Range(start, span.Length);

        try
        {
            var invoiceNumber = span[fields[0]].ToString();
            var orderDate = DateTime.Parse(span[fields[1]]);
            var customerNumber = span[fields[2]].ToString();
            var customer = customers.FirstOrDefault(c => c.CustomerNumber == customerNumber)
                           ?? throw new ArgumentException("Kunde nicht gefunden");
            var invoiceReference = int.Parse(span[fields[4]]);
            var route = Route.ReadCsvLine(span[fields[5]].ToString(), skip) ?? throw new ArgumentException("Route ungültig");
            var dateOfService = DateTime.Parse(span[fields[6]]);
            var driver = Driver.ReadCsvLine(span[fields[7]].ToString(), skip) ?? throw new ArgumentException("Fahrer ungültig");
            var freightType = Enum.Parse<FreightType>(span[fields[8]]);
            var pods = span[fields[9]].Equals("Ja", StringComparison.OrdinalIgnoreCase);
            var netAmount = float.Parse(span[fields[10]]);
            var taxStatus = Enum.Parse<NetCalculationType>(span[fields[11]]);
            // Felder 12 und 13 (MwStBetrag, BruttoBetrag) werden ignoriert
            var description = span[fields[14]].ToString();

            return new Order(
                invoiceNumber, orderDate, customerNumber, customer, invoiceReference,
                route, dateOfService, driver, freightType, pods, netAmount, taxStatus, description
            );
        }
        catch when (skip)
        {
            return null;
        }
    }
}
namespace KCY_Accounting.Core;

public class Customer
{
    public string CustomerNumber { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public Country Country { get; set; }
    public string Uid { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public string Email { get; set; }
    public NetCalculationType NetCalculationType { get; set; }

    public Customer(string customerNumber, string name, string address, string postalCode, string city, Country country,
        string uid, int paymentDue, string email, NetCalculationType netCalculationType)
    {
        CustomerNumber = customerNumber;
        Name = name;
        Address = address;
        PostalCode = postalCode;
        City = city;
        Country = country;
        Uid = uid;
        PaymentDueDate = DateTime.Today.AddDays(paymentDue);
        Email = email;
        NetCalculationType = netCalculationType;
    }


    public static Customer? ReadCsvLine(string csvLine, bool skip = true)
    {
        var span = csvLine.AsSpan();

        Span<int> indices = stackalloc int[9];
        var found = 0;
        for (var i = 0; i < span.Length && found < 9; i++)
        {
            if (span[i] == ';')
                indices[found++] = i;
        }
        try
        {
            if (found != 9) 
                throw new ArgumentException("Invalid CSV Line format. Expected 10 values.", nameof(csvLine));
            
            var customerNumber = Slice(span, 0, indices[0]).ToString();
            var name = Slice(span, indices[0] + 1, indices[1]).ToString();
            var address = Slice(span, indices[1] + 1, indices[2]).ToString();
            var postalCode = Slice(span, indices[2] + 1, indices[3]).ToString();

            var city = Slice(span, indices[3] + 1, indices[4]).ToString();
            if (!Enum.TryParse<Country>(Slice(span, indices[4] + 1, indices[5]), out var country))
                throw new ArgumentException("Invalid country format.");

            var uid = Slice(span, indices[5] + 1, indices[6]).ToString();
            if (!int.TryParse(Slice(span, indices[6] + 1, indices[7]), out var paymentDue))
                throw new ArgumentException("Invalid payment due date format.");

            var email = Slice(span, indices[7] + 1, indices[8]).ToString();
            if (!Enum.TryParse<NetCalculationType>(SliceAfter(span, indices[8]), out var netCalculationType))
                throw new ArgumentException("Invalid net calculation type format.");

            return new Customer(customerNumber, name, address, postalCode, city, country, uid, paymentDue, email,
                netCalculationType);
        }
        catch when (skip)
        {
            return null;
        }

        static ReadOnlySpan<char> Slice(ReadOnlySpan<char> span, int start, int end) => span.Slice(start, end - start);
        static ReadOnlySpan<char> SliceAfter(ReadOnlySpan<char> span, int i) => span[(i + 1)..];
    }

    public string ToCsvLine()
    {
        return $"{CustomerNumber};{Name};{Address};{PostalCode};{City};{Country};{Uid};{(PaymentDueDate - DateTime.Today).Days};{Email};{NetCalculationType}";
    }
}
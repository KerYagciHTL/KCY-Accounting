namespace KCY_Accounting.Core;
    public class Driver
    {
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string LicenseNumber;
        public readonly DateTime DateOfBirth;
        public readonly string PhoneNumber;
    
        public Driver(string firstName, string lastName, string licenseNumber, DateTime dateOfBirth, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            LicenseNumber = licenseNumber;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
        }
    
        public static Driver? ReadCsvLine(string csvLine, bool skip = true)
        {
            var span = csvLine.AsSpan();
    
            Span<int> indices = stackalloc int[4];
            var found = 0;
            for (var i = 0; i < span.Length && found < 4; i++)
            {
                if (span[i] == ' ')
                    indices[found++] = i;
            }
            try
            {
                if (found != 4)
                    throw new ArgumentException("Invalid CSV Line format. Expected 5 values.", nameof(csvLine));
    
                var firstName = Slice(span, 0, indices[0]).ToString();
                var lastName = Slice(span, indices[0] + 1, indices[1]).ToString();
                var licenseNumber = Slice(span, indices[1] + 1, indices[2]).ToString();
                var dateOfBirth = DateTime.Parse(Slice(span, indices[2] + 1, indices[3]));
                var phoneNumber = SliceAfter(span, indices[3]).ToString();
    
                return new Driver(firstName, lastName, licenseNumber, dateOfBirth, phoneNumber);
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
            return $"{FirstName} {LastName} {LicenseNumber} {DateOfBirth:dd/MM/yyyy} {PhoneNumber}";
        }
    }
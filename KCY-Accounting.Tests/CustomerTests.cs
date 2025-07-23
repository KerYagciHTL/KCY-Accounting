using KCY_Accounting.Core;
using Xunit;

namespace KCY_Accounting.Tests;

public class CustomerTests
{
    [Fact]
    public void TestCustomerCreation()
    {
        var customer = new Customer("K24", "John Doe", "123 Main St", "555-1234", "London", Country.UnitedKingdom,
            "UK12304124", 14, "john-doe@gmail.com", NetCalculationType.NonEu);
        
        Assert.NotNull(customer);
        Assert.Equal("K24", customer.CustomerNumber);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 Main St", customer.Address);
        Assert.Equal("555-1234", customer.PostalCode);
        Assert.Equal("London", customer.City);
        Assert.Equal(Country.UnitedKingdom, customer.Country);
        Assert.Equal("UK12304124", customer.Uid);
        Assert.Equal(DateTime.Today.AddDays(14), customer.PaymentDueDate);
        Assert.Equal("john-doe@gmail.com", customer.Email);
        Assert.Equal(NetCalculationType.NonEu, customer.NetCalculationType);
    }
    
    [Theory]
    [InlineData("K24;John Doe;123 Main St;555-1234;London;UnitedKingdom;UK12304124;14;john-doe@gmail.com;No", true)]
    [InlineData("K25;Jane Smith;456 Elm St;555-5678;New York;UnitedStates;US12304124;30;jane-smith@gmail.com;Yes", true)]
    [InlineData("K26;Max Mustermann;Berlinerneuer Straße 10;555-9012;Berlin;Germany;DE12304124;7;max-musterman@gmail.com;NonEu", true)]
    [InlineData("K27;Anna Müller;Musterweg 5;555-3456;Munich;Deutschland;DE12304124;14;anna-müller@gmail.com;Eu", false)]
    [InlineData("K28;Carlos García;Calle Falsa 123;555-7890;Madrid;Spain;ES12304124;21;Carlos-Garcia@gmail.com;x", false)]
    [InlineData("K29;Luca Rossi;Via Roma 456;555-2345;Rome;Italy;IT12304124;30;EU", false)]
    [InlineData("K30;Sven Svensson;Storgatan 12;555-6789;Stockholm;Sweden;SE12304124;KA;sven-svensson@gmail.com;EU", false)]
    public void TestCustomerReadCsvLine_ShouldThrowException_OnInvalidOnes(string csvLine, bool success)
    {
        if (success)
        {
            var customer = Customer.ReadCsvLine(csvLine, false);
            Assert.NotNull(customer);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => Customer.ReadCsvLine(csvLine, false));
        }
    }

    [Theory]
    [InlineData("K24;John Doe;123 Main St;555-1234;London;UnitedKingdom;UK12304124;14;john-doe@gmail.com;No", true)]
    [InlineData("K25;Jane Smith;456 Elm St;555-5678;New York;UnitedStates;US12304124;30;jane-smith@gmail.com;Yes", true)]
    [InlineData("K26;Max Mustermann;Berlinerneuer Straße 10;555-9012;Berlin;Germany;DE12304124;7;max-musterman@gmail.com;NonEu", true)]
    [InlineData("K27;Anna Müller;Musterweg 5;555-3456;Munich;Deutschland;DE12304124;14;anna-müller@gmail.com;Eu", false)]
    [InlineData("K28;Carlos García;Calle Falsa 123;555-7890;Madrid;Spain;ES12304124;21;Carlos-Garcia@gmail.com;x", false)]
    [InlineData("K29;Luca Rossi;Via Roma 456;555-2345;Rome;Italy;IT12304124;30;EU", false)]
    [InlineData("K30;Sven Svensson;Storgatan 12;555-6789;Stockholm;Sweden;SE12304124;KA;sven-svensson@gmail.com;EU", false)]
    public void TestCustomerReadCsvLine_ShouldGiveNull_OnInvalidOnes(string csvLine, bool success)
    {
        var customer = Customer.ReadCsvLine(csvLine);
        if (success)
        {
            Assert.NotNull(customer);
        }
        else
        {
            Assert.Null(customer);
        }
    }
}
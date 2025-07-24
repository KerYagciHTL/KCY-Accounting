using KCY_Accounting.Core;
using Xunit;

namespace KCY_Accounting.Tests;

public class OrderTests
{
    private readonly Customer[] _customers =
    [
        new("K24", "John Doe", "123 Main St", "555-1234", "London", Country.UnitedKingdom, "UK12304124", 14, "john-doe@gmail.com", NetCalculationType.NonEu),
        new("K25", "Jane Smith", "456 Elm St", "555-5678", "New York", Country.UnitedStates, "US12304124", 30, "jane-smith@gmail.com", NetCalculationType.No)
    ];

    [Fact]
    public void TestOrderCreation()
    {
        var route = new Route("Berlin", "Hamburg");
        var driver = new Driver("Max", "Mustermann", "B-AB1234", new DateTime(1980, 1, 1), "0171-1234567");
        var order = new Order("R123", DateTime.Today, "K24", _customers[0], 123, route, DateTime.Today, driver, FreightType.Palette, true, 100.0f, NetCalculationType.NonEu, "Test");

        Assert.NotNull(order);
        Assert.Equal("R123", order.InvoiceNumber);
        Assert.Equal(_customers[0], order.Customer);
        Assert.Equal(route, order.Route);
        Assert.Equal(driver, order.Driver);
        Assert.Equal(FreightType.Palette, order.FreightType);
        Assert.True(order.Pods);
        Assert.Equal(100.0f, order.NetAmount);
        Assert.Equal(NetCalculationType.NonEu, order.TaxStatus);
        Assert.Equal("Test", order.Description);
    }

    [Theory]
    [InlineData("R123;2024-06-01;K24;John Doe;123;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", true)]
    [InlineData("R124;2024-06-01;K25;Jane Smith;124;New York Boston;2024-06-02;Jane Smith XY-1234 1990-05-05 0172-9876543;Kiste;Nein;200;No;0;200;Expressauftrag", true)]
    [InlineData("R125;2024-06-01;K99;Unbekannt;125;Berlin Paris;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", false)] // Kunde fehlt
    [InlineData("R126;2024-06-01;K24;John Doe;126;Berlin;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", false)] // Route ungültig
    [InlineData("R127;2024-06-01;K24;John Doe;127;Berlin Hamburg;2024-06-02;Max Mustermann;Palette;Ja;100;NonEu;19;119;Test", false)] // Fahrer ungültig
    [InlineData("R128;2024-06-01;K24;John Doe;128;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Unbekannt;Ja;100;NonEu;19;119;Test", false)] // FreightType ungültig
    [InlineData("R129;2024-06-01;K24;John Doe;129;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;abc;NonEu;19;119;Test", false)] // Betrag ungültig
    [InlineData("R130;2024-06-01;K24;John Doe;130;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119", false)] // Zu wenig Felder
    public void TestOrderReadCsvLine_ShouldThrowException_OnInvalidOnes(string csvLine, bool success)
    {
        if (success)
        {
            var order = Order.ReadCsvLine(csvLine, _customers, false);
            Assert.NotNull(order);
        }
        else
        {
            try
            {
                var order = Order.ReadCsvLine(csvLine, _customers, false);
                Assert.True(false);
            }
            catch (Exception e)
            {
                Assert.True(true);
            }
        }
    }

    [Theory]
    [InlineData("R123;2024-06-01;K24;John Doe;123;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", true)]
    [InlineData("R124;2024-06-01;K25;Jane Smith;124;New York Boston;2024-06-02;Jane Smith XY-1234 1990-05-05 0172-9876543;Kiste;Nein;200;No;0;200;Expressauftrag", true)]
    [InlineData("R125;2024-06-01;K99;Unbekannt;125;Berlin Paris;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", false)]
    [InlineData("R126;2024-06-01;K24;John Doe;126;Berlin;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119;Test", false)]
    [InlineData("R127;2024-06-01;K24;John Doe;127;Berlin Hamburg;2024-06-02;Max Mustermann;Palette;Ja;100;NonEu;19;119;Test", false)]
    [InlineData("R128;2024-06-01;K24;John Doe;128;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Unbekannt;Ja;100;NonEu;19;119;Test", false)]
    [InlineData("R129;2024-06-01;K24;John Doe;129;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;abc;NonEu;19;119;Test", false)]
    [InlineData("R130;2024-06-01;K24;John Doe;130;Berlin Hamburg;2024-06-02;Max Mustermann B-AB1234 1980-01-01 0171-1234567;Palette;Ja;100;NonEu;19;119", false)]
    public void TestOrderReadCsvLine_ShouldGiveNull_OnInvalidOnes(string csvLine, bool success)
    {
        var order = Order.ReadCsvLine(csvLine, _customers);
        if (success)
        {
            Assert.NotNull(order);
        }
        else
        {
            Assert.Null(order);
        }
    }
}
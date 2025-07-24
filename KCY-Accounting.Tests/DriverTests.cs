using KCY_Accounting.Core;
using Xunit;

namespace KCY_Accounting.Tests;

public class DriverTests
{
    [Fact]
    public void TestDriverCreation()
    {
        var driver = new Driver("Max", "Mustermann", "AB-1234", new DateTime(1980, 1, 1), "0123456789");

        Assert.NotNull(driver);
        Assert.Equal("Max", driver.FirstName);
        Assert.Equal("Mustermann", driver.LastName);
        Assert.Equal("AB-1234", driver.LicenseNumber);
        Assert.Equal(new DateTime(1980, 1, 1), driver.DateOfBirth);
        Assert.Equal("0123456789", driver.PhoneNumber);
    }

    [Theory]
    [InlineData("Anna Schmidt XY-9876 1990-12-24 0987654321", true)]
    [InlineData("Max Mustermann AB-1234 1980-01-01 0123456789", true)]
    [InlineData("Peter Kurz", false)]
    [InlineData("Lisa Müller XX-1234 1995-05-10", false)]
    public void TestDriverReadCsvLine_ShouldThrowException_OnInvalidOnes(string line, bool success)
    {
        if (success)
        {
            var driver = Driver.ReadCsvLine(line, false);
            Assert.NotNull(driver);
        }
        else
        {
            Assert.Throws<ArgumentException>(() => Driver.ReadCsvLine(line, false));
        }
    }

    [Theory]
    [InlineData("Anna Schmidt XY-9876 1990-12-24 0987654321", true)]
    [InlineData("Max Mustermann AB-1234 1980-01-01 0123456789", true)]
    [InlineData("Peter Kurz", false)]
    [InlineData("Lisa Müller XX-1234 1995-05-10", false)]
    public void TestDriverReadCsvLine_ShouldGiveNull_OnInvalidOnes(string line, bool success)
    {
        var driver = Driver.ReadCsvLine(line);
        if (success)
        {
            Assert.NotNull(driver);
        }
        else
        {
            Assert.Null(driver);
        }
    }
}
using MIS.Application.Common;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class EgyptianHrDataValidatorTests
{
    [Fact]
    public void National_id_is_normalized_and_checked_against_birth_date_and_gender()
    {
        var result = EgyptianHrDataValidator.NormalizeNationalId(
            "٢٩٨٠١٠١٠١٢٣٤٥٦",
            new DateOnly(1998, 1, 1),
            "Male");

        Assert.Equal("29801010123456", result);
        Assert.Throws<HrValidationException>(() => EgyptianHrDataValidator.NormalizeNationalId(
            "29801010123456",
            new DateOnly(1998, 1, 2),
            "Male"));
        Assert.Throws<HrValidationException>(() => EgyptianHrDataValidator.NormalizeNationalId(
            "29801010123456",
            new DateOnly(1998, 1, 1),
            "Female"));
    }

    [Theory]
    [InlineData("010 1234 5678", "01012345678")]
    [InlineData("+20 10 1234 5678", "01012345678")]
    [InlineData("0020-11-1234-5678", "01112345678")]
    public void Egyptian_mobile_numbers_are_stored_in_one_consistent_format(string input, string expected)
    {
        Assert.Equal(expected, EgyptianHrDataValidator.NormalizePhone(input, "Mobile number"));
    }

    [Fact]
    public void Invalid_egyptian_mobile_prefix_is_rejected()
    {
        Assert.Throws<HrValidationException>(() =>
            EgyptianHrDataValidator.NormalizePhone("01312345678", "Mobile number"));
    }

    [Fact]
    public void Egyptian_iban_length_and_checksum_are_validated()
    {
        Assert.Equal(
            "EG170001000000000012345678901",
            EgyptianHrDataValidator.NormalizeIban("EG17 0001 0000 0000 0012 3456 7890 1"));
        Assert.Throws<HrValidationException>(() =>
            EgyptianHrDataValidator.NormalizeIban("EG180001000000000012345678901"));
    }
}

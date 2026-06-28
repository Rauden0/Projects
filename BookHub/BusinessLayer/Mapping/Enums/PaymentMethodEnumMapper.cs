using DataAccessLayer.Enums;

namespace BusinessLayer.Mapping.Enums;

public class PaymentMethodEnumMapper
{
    public static string ToCode(PaymentMethodEnum e)
        => e switch
        {
            PaymentMethodEnum.Cash => "CASH",
            PaymentMethodEnum.Card => "CARD",
            PaymentMethodEnum.Bank => "BANK",
            _ => throw new ArgumentOutOfRangeException()
        };

    public static PaymentMethodEnum FromCode(string code)
        => code switch
        {
            "CASH" => PaymentMethodEnum.Cash,
            "CARD" => PaymentMethodEnum.Card,
            "BANK" => PaymentMethodEnum.Bank,
            _ => throw new ArgumentException("Invalid code")
        };
}
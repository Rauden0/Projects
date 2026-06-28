using DataAccessLayer.Enums;

namespace BusinessLayer.Mapping.Enums;

public static class OrderStateEnumMapper
{
    public static string ToCode(OrderStateEnum e)
        => e switch
        {
            OrderStateEnum.Preparing => "PREPARING",
            OrderStateEnum.Sending => "SENDING",
            OrderStateEnum.Completed => "COMPLETED",
            _ => throw new ArgumentOutOfRangeException()
        };

    public static OrderStateEnum FromCode(string code)
        => code switch
        {
            "PREPARING" => OrderStateEnum.Preparing,
            "SENDING" => OrderStateEnum.Sending,
            "COMPLETED" => OrderStateEnum.Completed,
            _ => throw new ArgumentException("Invalid code")
        };
}
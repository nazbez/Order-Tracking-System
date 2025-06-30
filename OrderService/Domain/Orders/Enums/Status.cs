namespace Domain.Orders.Enums;

public enum Status : byte
{
    Placed,
    Assigned,
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}

namespace WebBanHang.Model.Enums
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Packed,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Failed,
        Refunded
    }

    public enum InventoryMovementType
    {
        IN,
        OUT
    }
}

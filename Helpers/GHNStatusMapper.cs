using PeShop.Models.Enums;

namespace PeShop.Helpers;

public static class GHNStatusMapper
{
    private static readonly HashSet<string> SpecialStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "cancel",
        "delivery_fail",
        "waiting_to_return",
        "return",
        "return_transporting",
        "return_sorting",
        "returning",
        "return_fail",
        "returned"
    };

    public static DeliveryStatus? MapToDeliveryStatus(string status)
    {
        return status.ToLower() switch
        {
            "ready_to_pick" => DeliveryStatus.Ready_To_Pick,
            "picking" => DeliveryStatus.Picking,
            "money_collect_picking" => DeliveryStatus.Money_Collect_Picking,
            "picked" => DeliveryStatus.Picked,
            "storing" => DeliveryStatus.Storing,
            "transporting" => DeliveryStatus.Transporting,
            "sorting" => DeliveryStatus.Sorting,
            "delivering" => DeliveryStatus.Delivering,
            "money_collect_delivering" => DeliveryStatus.Money_Collect_Delivering,
            "delivered" => DeliveryStatus.Delivered,
            "delivery_fail" => DeliveryStatus.Delivery_Fail,
            "waiting_to_return" => DeliveryStatus.Waiting_To_Return,
            "return" => DeliveryStatus.Return,
            "return_transporting" => DeliveryStatus.Return_Transporting,
            "return_sorting" => DeliveryStatus.Return_Sorting,
            "returning" => DeliveryStatus.Returning,
            "return_fail" => DeliveryStatus.Return_Fail,
            "returned" => DeliveryStatus.Returned,
            "cancel" => DeliveryStatus.Cancel,
            _ => null
        };
    }

    public static OrderStatus? MapToOrderStatus(string status)
    {
        return status.ToLower() switch
        {
            "picked" => OrderStatus.PickedUp,
            "delivering" => OrderStatus.Shipping,
            "money_collect_delivering" => OrderStatus.Shipping,
            "delivered" => OrderStatus.Delivered,
            "cancel" => OrderStatus.Cancelled,
            _ => null
        };
    }

    public static bool IsSpecialStatus(string status)
    {
        return SpecialStatuses.Contains(status);
    }
}

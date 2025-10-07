namespace PeShop.Dtos.Shared;

public class ShipmentDto
{
    public AddressDto address_from { get; set; }
    public AddressDto address_to { get; set; }
    public ParcelDto parcel { get; set; }
}

public class AddressDto
{
    public string district { get; set; }
    public string city { get; set; }
}

public class ParcelDto
{
    public decimal cod { get; set; }
    public decimal amount { get; set; }
    public uint width { get; set; }
    public uint height { get; set; }
    public uint length { get; set; }
    public uint weight { get; set; }
}

public class ShipmentRequestDto
{
    public ShipmentDto shipment { get; set; }
}

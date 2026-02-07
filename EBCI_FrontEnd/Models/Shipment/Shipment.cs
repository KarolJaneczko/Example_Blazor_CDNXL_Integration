namespace EBCI_FrontEnd.Models.Shipment {
    public class Shipment {
        public string SupplierCode { get; set; }
        public DateTime ShipmentDate { get; set; }
        public string WarehouseCode { get; set; }
        public IEnumerable<ShipmentPosition> Positions { get; set; }

    }
}

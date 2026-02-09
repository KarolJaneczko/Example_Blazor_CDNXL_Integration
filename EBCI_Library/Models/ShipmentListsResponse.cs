using System.Collections.Generic;

namespace EBCI_Library.Models {
    public class ShipmentListsResponse {
        public IEnumerable<string> Suppliers { get; set; }
        public IEnumerable<string> Warehouses { get; set; }
        public IEnumerable<string> Products { get; set; }
    }
}

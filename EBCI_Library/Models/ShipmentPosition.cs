using System;

namespace EBCI_Library.Models {
    public class ShipmentPosition {
        public string ProductCode { get; set; }
        // Corresponds to [TrE_Ilosc] column [CDN].[TraElem] of type (decimal(11,4), null)
        public decimal Quantity { get; set; }
        public string Batch { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
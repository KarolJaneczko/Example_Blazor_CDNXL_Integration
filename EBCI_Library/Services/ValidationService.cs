using EBCI_Library.Models;
using System;
using System.Linq;

namespace EBCI_Library.Services {
    public static class ValidationService {
        public static bool Validate(this Shipment shipment, out string message) {
            message = null;

            if (string.IsNullOrEmpty(shipment.SupplierCode)) {
                message = "Supplier code cannot be empty!";
                return false;
            }

            if (string.IsNullOrEmpty(shipment.WarehouseCode)) {
                message = "Warehouse code cannot be empty!";
                return false;
            }

            var positions = shipment.Positions;

            if (positions?.Any() != true) {
                message = "Positions cannot be empty!";
                return false;
            }

            foreach (var position in positions) {
                if (string.IsNullOrEmpty(position.ProductCode)) {
                    message = $"Position (LP: {position.Lp}) has an empty product code!";
                    return false;
                }

                if (string.IsNullOrEmpty(position.Batch)) {
                    message = $"Position (LP: {position.Lp}) has an empty batch!";
                    return false;
                }

                if (position.ExpirationDate.Date > DateTime.Now.Date) {
                    message = $"Position (LP: {position.Lp}) has an expiration date set in the future!";
                    return false;
                }
            }

            return true;
        }
    }
}

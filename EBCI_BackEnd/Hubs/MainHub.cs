using EBCI_BackEnd.Services;
using EBCI_Library.Models;
using EBCI_Library.Services;
using Microsoft.AspNet.SignalR;

namespace EBCI_BackEnd.Hubs {
    public class MainHub : Hub {
        private readonly XLService XLService = new XLService();
        private string XLUser = Properties.Settings.Default.XLUser;
        private string XLPassword = Properties.Settings.Default.XLPassword;
        private string XLDatabaseName = Properties.Settings.Default.XLDatabaseName;
        private string XLLicenseServer = Properties.Settings.Default.XLLicenseServer;

        public NewShipmentResponse ExchangeData(NewShipmentRequest request) {            
            if (!XLService.Login(XLUser, XLPassword, XLDatabaseName, XLLicenseServer, out var loginMessage)) {
                return new NewShipmentResponse(false, loginMessage);
            }

            var shipment = request.Shipment;
            LogService.Info($"New shipment request: {shipment.SupplierCode} (supplier code), {shipment.WarehouseCode} (warehouse code)");

            //if (!XLService.Login)

            return new NewShipmentResponse(true, "Success");
        }
    }

}
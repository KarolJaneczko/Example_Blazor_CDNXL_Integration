using cdn_api;
using EBCI_BackEnd.Classes.Dictionaries;
using EBCI_BackEnd.Services;
using EBCI_Library.Models;
using EBCI_Library.Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EBCI_BackEnd.Hubs {
    public class MainHub : Hub {
        private static readonly DatabaseService DatabaseService = new DatabaseService(Properties.Settings.Default.DBServer, Properties.Settings.Default.DBName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);
        private static readonly XLService XLService = new XLService(Properties.Settings.Default.XLUser, Properties.Settings.Default.XLPassword, Properties.Settings.Default.XLDatabaseName, Properties.Settings.Default.XLLicenseServer);
        private HashSet<string> Suppliers, Warehouses, Products;
        private bool IsSynced;

        private void SyncLists() {
            var suppliers = DatabaseService.Query<string>(QueryDictionary.GetQuerySuppliers, null);
            Suppliers = new HashSet<string>(suppliers);

            var warehouses = DatabaseService.Query<string>(QueryDictionary.GetQueryWarehouses, null);
            Warehouses = new HashSet<string>(warehouses);

            var products = DatabaseService.Query<string>(QueryDictionary.GetQueryProducts, null);
            Products = new HashSet<string>(products);
        }

        public ShipmentListsResponse GetLists() {
            if (!IsSynced) {
                SyncLists();
                IsSynced = true;
            }

            return new ShipmentListsResponse {
                Suppliers = Suppliers,
                Warehouses = Warehouses,
                Products = Products
            };
        }

        public NewShipmentResponse ExchangeData(NewShipmentRequest request) {
            if (!IsSynced) {
                SyncLists();
                IsSynced = true;
            }

            var shipment = request?.Shipment;

            if (shipment == null) {
                return new NewShipmentResponse(false, "Request cannot be null");
            }

            if (!request.Shipment.Validate(out var validationMessage)) {
                return new NewShipmentResponse(false, validationMessage);
            }

            if (!Suppliers.Any(x => x.ToLower().Trim() == shipment.SupplierCode.ToLower().Trim())) {
                return new NewShipmentResponse(false, $"There is no '{shipment.SupplierCode}' supplier registered in Comarch ERP XL configuration!");
            }

            if (!Warehouses.Any(x => x.ToLower().Trim() == shipment.WarehouseCode.ToLower().Trim())) {
                return new NewShipmentResponse(false, $"There is no '{shipment.WarehouseCode}' warehouse registered in Comarch ERP XL configuration!");
            }

            foreach (var position in shipment.Positions) {
                var productCode = position.ProductCode;

                if (!Products.Any(x => x.ToLower().Trim() == productCode.ToLower().Trim())) {
                    return new NewShipmentResponse(false, $"There is no '{productCode}' (LP: {position.Lp}) product registered in Comarch ERP XL configuration!");
                }
            }

            if (!XLService.Login(out var loginMessage)) {
                return new NewShipmentResponse(false, loginMessage);
            }
            //todo implement creating and commiting/rollbacking transaction

            // Trying to add new document through Comarch ERP XL API.
            var documentId = -1;
            var now = DateTime.Now;
            var nagInfo = new XLDokumentNagInfo_20251 {
                Typ = 2,
                Rok = now.Year,
                Miesiac = now.Month,
                Tryb = 2,
                Data = ConvertDateTimeToClarionDate(now),
                DataSpr = ConvertDateTimeToClarionDate(now),
                DataMag = ConvertDateTimeToClarionDate(shipment.ShipmentDate),
                MagazynD = shipment.WarehouseCode,
                Akronim = shipment.SupplierCode,
            };
            if (!XLService.CreateDocument(ref documentId, nagInfo, out var nagInfoResult, out var createDocumentMessage)) {
                createDocumentMessage = $"Error ocurred at opening the document, response: {createDocumentMessage}";
                return new NewShipmentResponse(false, createDocumentMessage);
            }

            LogService.Info("\t1 - Created the document successfully");
            foreach (var position in shipment.Positions) {
                var elemInfo = new XLDokumentElemInfo_20251 {
                    Ilosc = position.Quantity.ToString(),
                    TowarKod = position.ProductCode,
                    JmZ = "szt.",
                    Magazyn = nagInfoResult.MagazynD,
                    GIDLp = position.Lp,
                    DataWaznosci = ConvertDateTimeToClarionDate(position.ExpirationDate),
                    CChNumer = Properties.Settings.Default.XLBatchId,
                    CChLp = 1,
                    CChFirma = 3,
                    CChTyp = 192,
                    Cecha = position.Batch
                };

                if (!XLService.NewPosition(documentId, elemInfo, out var elemInfoResult, out var newPositionMessage)) {
                    newPositionMessage = $"Error ocurred at add position to the document (article {position.ProductCode}, LP: {elemInfo.GIDLp}), response: {newPositionMessage}";
                    return new NewShipmentResponse(false, newPositionMessage);
                }

                LogService.Info($"\t2 - Added position successfully, LP: {elemInfoResult.GIDLp}");
            }

            var zamkniecieInfo = new XLZamkniecieDokumentuInfo_20251 {
                Tryb = 1
            };
            if (!XLService.CloseDocument(documentId, ref zamkniecieInfo, out var closeDocumentMessage)) {
                closeDocumentMessage = $"Error ocurred at closing the document, response: {closeDocumentMessage}";
                return new NewShipmentResponse(false, closeDocumentMessage);
            }

            LogService.Info("\t3 - Closed the document successfully");

            var numerInfo = new XLNumerDokumentuInfo_20251 {
                GIDLp = nagInfoResult.GIDLp,
                GIDFirma = nagInfoResult.GIDFirma,
                GIDNumer = nagInfoResult.GIDNumer,
                GIDTyp = nagInfoResult.GIDTyp
            };
            if (!XLService.GetDocumentNumber(numerInfo, out var documentNumberMessage)) {
                documentNumberMessage = $"Error ocurred at getting the document number, response: {documentNumberMessage}";
                return new NewShipmentResponse(false, documentNumberMessage);
            }


            if (!XLService.Logout(out var logoutMessage)) {
                logoutMessage = $"Error ocurred at logout, response: {logoutMessage}";
                return new NewShipmentResponse(false, logoutMessage);
            } else {
                return new NewShipmentResponse(true, $"Success, document was created with number: {numerInfo.NumerDokumentu}");
            }
        }

        private static int ConvertDateTimeToClarionDate(DateTime dateTime) {
            CultureInfo provider = new CultureInfo("pl-PL");
            DateTime dateTime2 = DateTime.Parse("01.01.1801", provider);
            return (dateTime - dateTime2).Days + 4;
        }
    }

}
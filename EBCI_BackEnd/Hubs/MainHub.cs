using cdn_api;
using EBCI_BackEnd.Services;
using EBCI_Library.Models;
using EBCI_Library.Services;
using Microsoft.AspNet.SignalR;
using System;
using System.Globalization;

namespace EBCI_BackEnd.Hubs {
    public class MainHub : Hub {
        private readonly XLService XLService = new XLService();
        private readonly string XLUser = Properties.Settings.Default.XLUser;
        private readonly string XLPassword = Properties.Settings.Default.XLPassword;
        private readonly string XLDatabaseName = Properties.Settings.Default.XLDatabaseName;
        private readonly string XLLicenseServer = Properties.Settings.Default.XLLicenseServer;
        private readonly int XLBatchId = Properties.Settings.Default.XLBatchId;

        public NewShipmentResponse ExchangeData(NewShipmentRequest request) {
            // todo validating request, add validation service to the library - object validation

            // todo data validation - checking if everything exist in the database/xl

            if (!XLService.Login(XLUser, XLPassword, XLDatabaseName, XLLicenseServer, out var loginMessage)) {
                return new NewShipmentResponse(false, loginMessage);
            }

            var shipment = request.Shipment;
            LogService.Info($"New shipment request: {shipment.SupplierCode} (supplier code), {shipment.WarehouseCode} (warehouse code)");

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
                    CChNumer = XLBatchId,
                    CChLp = 1,
                    CChFirma = 3,
                    CChTyp = 192,
                    Cecha = position.Batch
                };

                // TODO add getting CCH information from the database + database service
                if (!XLService.NewPosition(documentId, elemInfo, out var elemInfoResult, out var newPositionMessage)) {
                    newPositionMessage = $"Error ocurred at add position to the document (article {position.ProductCode}, LP: {elemInfo.GIDLp}), response: {newPositionMessage}";
                    return new NewShipmentResponse(false, newPositionMessage);
                }

                LogService.Info($"\t2 - Added position successfully, LP: {elemInfoResult.GIDLp}");
            }

            var zamkniecieInfo = new XLZamkniecieDokumentuInfo_20251 {
                Tryb = 1
            };
            if (XLService.CloseDocument(documentId, ref zamkniecieInfo, out var closeDocumentMessage)) {
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
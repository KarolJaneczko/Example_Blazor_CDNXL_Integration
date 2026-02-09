using EBCI_Library.Models;
using EBCI_Library.Services;
using Microsoft.AspNet.SignalR.Client;

namespace EBCI_FrontEnd.Services {
    public interface ISignalService {
        Task<ShipmentListsResponse> TryToSyncLists();
        Task<NewShipmentResponse> TryToAddShipment(NewShipmentRequest request);
    }

    public class SignalService(IConfigurationService configurationService) : ISignalService {
        private readonly IConfigurationService _configurationService = configurationService;

        public async Task<ShipmentListsResponse> TryToSyncLists() {
            ShipmentListsResponse response = null;

            try {
                var hubConnection = new HubConnection(_configurationService.GetHubConnectionUrl());
                var hubProxy = hubConnection.CreateHubProxy("MainHub");

                await hubConnection.Start();
                response = await hubProxy.Invoke<ShipmentListsResponse>("GetLists");

                hubConnection.Stop();
            } catch (Exception ex) {
                LogService.Error($"Unexpected error occured at {nameof(TryToSyncLists)} method of {nameof(SignalService)} service: {ex.Message}", ex);
                response = new ShipmentListsResponse {
                    Suppliers = null,
                    Warehouses = null,
                    Products = null,
                };
            }

            return response;
        }

        public async Task<NewShipmentResponse> TryToAddShipment(NewShipmentRequest request) {
            NewShipmentResponse response = null;

            try {
                var hubConnection = new HubConnection(_configurationService.GetHubConnectionUrl());
                var hubProxy = hubConnection.CreateHubProxy("MainHub");

                await hubConnection.Start();
                response = await hubProxy.Invoke<NewShipmentResponse>("ExchangeData", request);

                hubConnection.Stop();
            } catch (Exception ex) {
                LogService.Error($"Unexpected error occured at {nameof(TryToAddShipment)} method of {nameof(SignalService)} service: {ex.Message}", ex);
                response = new NewShipmentResponse {
                    IsSuccess = false,
                    ResponseMessage = $"Unexpected error occured at communication with back-end application, check logs at {LogService.LogsPath}"
                };
            }

            return response;
        }
    }
}
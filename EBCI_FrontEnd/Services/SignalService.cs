using EBCI_Library.Models;
using EBCI_Library.Services;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace EBCI_FrontEnd.Services {
    public interface ISignalService {
        Task<NewShipmentResponse> TryToAddShipment(NewShipmentRequest request);
    }

    public class SignalService : ISignalService {
        public async Task<NewShipmentResponse> TryToAddShipment(NewShipmentRequest request) {
            NewShipmentResponse response = null;

            try {
                // todo parametrize this in config
                var hubConnection = new HubConnection("https://localhost:44313");
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

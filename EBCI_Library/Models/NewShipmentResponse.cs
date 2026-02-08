namespace EBCI_Library.Models {
    public class NewShipmentResponse {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; }

        public NewShipmentResponse() {
            IsSuccess = false;
            ResponseMessage = null;
        }

        public NewShipmentResponse(bool isSuccess, string responseMessage) {
            IsSuccess = isSuccess;
            ResponseMessage = responseMessage;
        }
    }
}

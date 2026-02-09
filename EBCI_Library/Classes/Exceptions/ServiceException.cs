using System;

namespace EBCI_Library.Classes.Exceptions {
    public class ServiceException : Exception {
        public ServiceException() : base("Unexpected error occured through custom service") {
        }

        public ServiceException(string message) : base(message) {
        }

        public ServiceException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}

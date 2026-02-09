using System.IO;

namespace EBCI_Library.Utils {
    public static class FileHelper {
        public static void CreateDirectoryIfDoesntExist(string directoryPath) {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
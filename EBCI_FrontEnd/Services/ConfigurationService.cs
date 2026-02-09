using EBCI_Library.Classes.Exceptions;

namespace EBCI_FrontEnd.Services {
    public interface IConfigurationService {
        public string GetLogsPath();
        public string GetHubConnectionUrl();
    }

    public class ConfigurationService(IConfiguration configuration) : IConfigurationService {
        private IConfiguration Configuration { get; } = configuration;

        public string GetLogsPath() {
            const string appSettingPath = "Configuration:LogsPath";
            return Configuration.GetSection(appSettingPath)?.Value ?? throw new ServiceException($"Configuration error, no value for the logs path, path: {appSettingPath}");
        }

        public string GetHubConnectionUrl() {
            const string appSettingPath = "Configuration:HubConnectionUrl";
            return Configuration.GetSection(appSettingPath)?.Value ?? throw new ServiceException($"Configuration error, no value for the hub connection url, path: {appSettingPath}");
        }
    }
}

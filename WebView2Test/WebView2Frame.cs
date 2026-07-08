using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;

namespace WebView2Test
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class WebView2Frame
    {
        public Window? Window { get; set; }

        private const string _serverFolderName = "Servers";
        private readonly ILogger<WebView2Frame> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly HubConfigs _hubConfigs;
        private readonly HubService _hubService;

        public WebView2Frame(
            ILogger<WebView2Frame> logger,
            IHostEnvironment hostEnvironment,
            IConfiguration configuration,
            JsonSerializerOptions jsonSerializerOptions,
            HubConfigs hubConfigs,
            HubService hubService)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _jsonSerializerOptions = jsonSerializerOptions;
            _hubConfigs = hubConfigs;
            _hubService = hubService;
        }

        public void Start(string hubConfigsJson)
        {
            if (string.IsNullOrEmpty(hubConfigsJson))
            {
                _hubService.Start(_hubConfigs);
            }
            else
            {
                var hubConfigs = JsonSerializer.Deserialize<HubConfigs>(hubConfigsJson, _jsonSerializerOptions) ?? throw new InvalidOperationException();
                _hubService.Start(hubConfigs);
            }
        }
        public void Stop()
        {
            _hubService.Stop();
        }
        public string[] GetServerNames()
        {
            var serverFolderPath = Path.Combine(_hostEnvironment.ContentRootPath, _serverFolderName);
            return Directory.EnumerateFiles(serverFolderPath).Select(a => Path.GetFileName(a)).ToArray();
        }
        public string GetHubConfigs()
        {
            var json = JsonSerializer.Serialize(_hubConfigs, _jsonSerializerOptions);
            return json;
        }
    }
}

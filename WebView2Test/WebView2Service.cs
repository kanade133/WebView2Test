using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace WebView2Test
{
    public sealed class WebView2Service
    {
        private readonly ILogger<WebView2Service> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly WebView2MessageService _webView2MessageService;
        private readonly WebView2Frame _webView2Frame;
        private readonly string _distFolderPath;
        private Microsoft.Web.WebView2.Wpf.WebView2? _webView2;

        public WebView2Service(
            ILogger<WebView2Service> logger,
            IHostEnvironment hostEnvironment,
            IConfiguration configuration,
            WebView2MessageService webView2MessageService,
            WebView2Frame webView2Frame)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _configuration = configuration;
            _webView2MessageService = webView2MessageService;
            _webView2Frame = webView2Frame;
            _distFolderPath = Path.Combine(hostEnvironment.ContentRootPath, "dist");
        }

        public void SetWindow(Window window)
        {
            _webView2Frame.Window = window;
        }
        public async Task SetWebView2(Microsoft.Web.WebView2.Wpf.WebView2 webView2, CancellationToken cancellationToken)
        {
            await webView2.EnsureCoreWebView2Async().ConfigureAwait(true);
            _webView2 = webView2;
            webView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView2.CoreWebView2.Settings.IsZoomControlEnabled = false;
            webView2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = _hostEnvironment.IsDevelopment();
            webView2.CoreWebView2.Settings.AreDevToolsEnabled = _hostEnvironment.IsDevelopment();
            webView2.CoreWebView2.AddHostObjectToScript("frame", _webView2Frame);
            if (string.Equals(_configuration.GetValue<string>("Localhost", "false"), "true", StringComparison.OrdinalIgnoreCase))
            {
                webView2.CoreWebView2.Navigate("http://localhost:5578");
            }
            else
            {
                if (!Directory.Exists(_distFolderPath)) throw new DirectoryNotFoundException("图表资源目录缺失");
                webView2.CoreWebView2.SetVirtualHostNameToFolderMapping("tradeDispatchHub.example", _distFolderPath, CoreWebView2HostResourceAccessKind.Allow);
                webView2.CoreWebView2.Navigate($"https://tradeDispatchHub.example/index.html");
            }
            _ = RunPostMessage(webView2, cancellationToken);
        }

        private async Task RunPostMessage(Microsoft.Web.WebView2.Wpf.WebView2 webView2, CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var message in _webView2MessageService.GetMessageAsync(cancellationToken))
                {
                    webView2.CoreWebView2.PostWebMessageAsJson(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log");
            }
        }
    }
}

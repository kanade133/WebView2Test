using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Windows;
using WebView2Test.Extensions;

namespace WebView2Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .Filter.ByExcluding(logEvent =>
                {
                    if (logEvent.Exception is OperationCanceledException)
                        return true;
                    return false;
                })
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "log", "log-.txt"),
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Sink(new WebView2Sink())
                .CreateLogger();
            try
            {
                var builder = Host.CreateApplicationBuilder();
                builder.Logging.ClearProviders();
                builder.Logging.AddSerilog();
                using (var hubConfigsStream = File.OpenRead(Path.Combine(builder.Environment.ContentRootPath, "hubConfigs.json")))
                {
                    var options = new JsonSerializerOptions();
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.ReadCommentHandling = JsonCommentHandling.Skip;
                    builder.Services.AddSingleton(JsonSerializer.Deserialize<HubConfigs>(hubConfigsStream, options) ?? throw new InvalidOperationException("解析配置文件失败"));
                }
                builder.Services.AddJsonSerializerOptions();
                builder.Services.AddSingleton<MainWindow>();
                builder.Services.AddSingleton<WebView2Frame>();
                builder.Services.AddSingleton<WebView2Service>();
                builder.Services.AddSingleton<WebView2MessageService>();
                _host = builder.Build();
                RegisterUnhandledException();
                base.OnStartup(e);
                _host.Start();
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var webView2Service = _host.Services.GetRequiredService<WebView2MessageService>();
                WebView2Sink.SetOutput(webView2Service.OnLog);
                RunCore(_host.Services);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Shutdown();
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_host != null)
                {
                    _host.StopAsync().GetAwaiter().GetResult();
                    _host.Dispose();
                }
            }
            catch (Exception)
            {

            }
            Log.Information("Application terminated");
            Log.CloseAndFlush();
            base.OnExit(e);
            Environment.Exit(e.ApplicationExitCode); // 账号连接无法断得很干净，会卡死导致无法完美退出，手动强制退出
        }

        private void RunCore(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var hostApplicationLifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            var cancellationToken = hostApplicationLifetime.ApplicationStopping;
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            this.MainWindow = mainWindow;
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            cancellationToken.Register(mainWindow.Close, useSynchronizationContext: true);
            mainWindow.ShowDialog();
        }
        private void RegisterUnhandledException()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled Dispatcher Exception");
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Error(ex, "Unhandled Domain Exception");
            }
        }
        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled Task Exception");
        }
    }
}

using Serilog.Core;
using Serilog.Events;

namespace WebView2Test
{
    public class WebView2Sink : ILogEventSink
    {
        // 静态委托，外部可设置
        private static Action<string>? _outputAction;

        public static void SetOutput(Action<string> outputAction)
        {
            _outputAction = outputAction;
        }

        public void Emit(LogEvent logEvent)
        {
            if (_outputAction == null) return;

             var message = $"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss} {logEvent.Level}] {logEvent.RenderMessage()}";
            _outputAction(message);
        }
    }
}

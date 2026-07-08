using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;

namespace WebView2Test
{
    public class WebView2MessageService
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly Channel<string> _messageChannel = Channel.CreateUnbounded<string>();

        public WebView2MessageService(
            JsonSerializerOptions jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public void OnLog(string message)
        {
            _messageChannel.Writer.TryWrite(JsonSerializer.Serialize(new WebMessage<string>
            {
                Type = MessageTypes.OnLog,
                Data = message,
            }, _jsonSerializerOptions));
        }
        public void OnOrder(OrderInfo order)
        {
            _messageChannel.Writer.TryWrite(JsonSerializer.Serialize(new WebMessage<OrderInfo>
            {
                Type = MessageTypes.OnOrder,
                Data = order,
            }, _jsonSerializerOptions));
        }
        public void OnTick(TickData tickData)
        {
            _messageChannel.Writer.TryWrite(JsonSerializer.Serialize(new WebMessage<TickData>
            {
                Type = MessageTypes.OnTick,
                Data = tickData,
            }, _jsonSerializerOptions));
        }
        public async IAsyncEnumerable<string> GetMessageAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in _messageChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item;
            }
        }

        private class MessageTypes
        {
            public const string OnLog = "OnLog";
            public const string OnOrder = "OnOrder";
            public const string OnTick = "OnTick";
        }
        private class WebMessage<T>
        {
            public string Type { get; set; } = "";
            public T? Data { get; set; }
        }
    }
}

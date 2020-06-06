using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Automation;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkUniversal1
{
    public class VKLongPoll
    {
        private LongPollServerResponse _longPollParams;
        public Action<Message> NewMessageReceived { get; set; }

        public VKLongPoll()
        {
            UpdateLongPollParams();
        }

        private void UpdateLongPollParams()
        {
            _longPollParams = VKObjects.Api.Messages.GetLongPollServer(true);
        }

        private void RunEventHandlers(LongPollHistoryResponse historyResponse)
        {
            foreach (var update in historyResponse.Messages)
            {
                if (update == null) continue;
                NewMessageReceived?.Invoke(update);
            }
        }

        public async Task FetchHistory(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var ts = _longPollParams.Ts;
            var pts = _longPollParams.Pts;
            while (!cancellationToken.IsCancellationRequested)
            {
                LongPollHistoryResponse longPollHistory;
                try
                {
                    longPollHistory = await VKObjects.Api.Messages.GetLongPollHistoryAsync(
                        new MessagesGetLongPollHistoryParams
                        {
                            Ts = ulong.Parse(ts),
                            Pts = pts
                        });
                }
                catch (VkApiMethodInvokeException)
                {
                    UpdateLongPollParams();
                    continue;
                }

                if (!longPollHistory.Messages.Any())
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                pts = longPollHistory.NewPts;
                RunEventHandlers(longPollHistory);
            }
        }
    }
}
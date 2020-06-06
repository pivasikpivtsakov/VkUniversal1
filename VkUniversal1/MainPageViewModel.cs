using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VkUniversal1
{
    public class MainPageViewModel
    {
        public long SelectedPeerId { get; set; }
        public ObservableCollection<ChatsListItemData> InboxesList { get; set; }
        public ObservableCollection<ChatsListItemData> ChatsList { get; set; }
        public Dictionary<long, ObservableCollection<ChatsListItemData>> MessageList { get; set; }
    }
}
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace VkUniversal1
{
    public class ChatsListItemData
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public long PeerId { get; set; }
        public bool IsThisUser { get; set; }

        public ImageSource Avatar { get; set; }

        //bound in xaml, do not remove
        public HorizontalAlignment IsThisUserAsAlignment =>
            IsThisUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }
}
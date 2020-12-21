using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkUniversal1.DbContext;
using Application = Windows.UI.Xaml.Application;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VkUniversal1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly MainPageViewModel _viewModel = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();
            CustomizeTitleBar();

            // Task.Run(async () => await VkDataToClassesUtils.GetChatsListAsItemData())
            // .ContinueWith(antecedent => ChatsList.ItemsSource = antecedent.Result);

            SystemNavigationManager.GetForCurrentView().BackRequested +=
                (sender, e) =>
                {
                    BackAction();
                    e.Handled = true;
                };
        }

        private void CustomizeTitleBar()
        {
            Window.Current.SetTitleBar(TrickyTitleBar);
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }

        private void TwoPaneView_ModeChanged(TwoPaneView sender, object args)
        {
            if (ThisChatPanel.Visibility == Visibility.Visible)
            {
                TwoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
            }

            if (TwoPaneView.Mode == TwoPaneViewMode.SinglePane)
            {
                if (TwoPaneView.PanePriority == TwoPaneViewPriority.Pane2)
                    TitleTextBlock.Margin = new Thickness(12, 0, 0, 0);
            }
            else
            {
                TitleTextBlock.Margin = new Thickness(76, 0, 0, 0);
            }
        }

        private void ChatsListListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedListViewItem = e.ClickedItem as ChatsListItemData;
            if (clickedListViewItem?.PeerId != null)
                _viewModel.SelectedPeerId = clickedListViewItem.PeerId;
            if (!_viewModel.MessageDict.ContainsKey(_viewModel.SelectedPeerId))
                _viewModel.MessageDict.Add(_viewModel.SelectedPeerId, new ObservableCollection<ChatsListItemData>());
            MessagesList.ItemsSource = _viewModel.MessageDict[_viewModel.SelectedPeerId];
            if (clickedListViewItem != null)
            {
                NameThisChatTextBlock.Text = clickedListViewItem.Name;
                ThisChatPersonPicture.ProfilePicture = clickedListViewItem.Avatar;
                LastSeenThisChatTextBlock.Text = "Online";
            }

            DialogChoice();
        }

        private void BackToChatListButton_Click(object sender, RoutedEventArgs e)
        {
            BackAction();
        }

        private void BackAction()
        {
            if (TwoPaneView.Mode == TwoPaneViewMode.SinglePane)
            {
                TwoPaneView.PanePriority = TwoPaneViewPriority.Pane1;
                TitleTextBlock.Margin = new Thickness(76, 0, 0, 0);
            }

            ThisChatPanel.Visibility = Visibility.Collapsed;
            ChatsListView.SelectedIndex = -1;
        }

        private void DialogChoice()
        {
            if (TwoPaneView.Mode == TwoPaneViewMode.SinglePane)
            {
                TwoPaneView.PanePriority = TwoPaneViewPriority.Pane2;
                TitleTextBlock.Margin = new Thickness(12, 0, 0, 0);
            }

            ThisChatPanel.Visibility = Visibility.Visible;
        }

        private void GlobalNavigationButton_OnClick(object sender, RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private void SplitView_PaneOpening(SplitView sender, object args)
        {
            AccountViewGrid.Background =
                Application.Current.Resources["AccountViewExpandedPaneBackground"] as AcrylicBrush;
            TitleTextBlock.Margin = new Thickness(12, 0, 0, 0);
        }

        private void SplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            AccountViewGrid.Background =
                Application.Current.Resources["AccountViewCollapsedPaneBackground"] as AcrylicBrush;
            TitleTextBlock.Margin = new Thickness(76, 0, 0, 0);
        }

        private void SplitView_Loaded(object sender, RoutedEventArgs e)
        {
            if (SplitView.IsPaneOpen)
            {
                AccountViewGrid.Background =
                    Application.Current.Resources["AccountViewExpandedPaneBackground"] as AcrylicBrush;
                TitleTextBlock.Margin = new Thickness(12, 0, 0, 0);
            }
            else
            {
                AccountViewGrid.Background =
                    Application.Current.Resources["AccountViewCollapsedPaneBackground"] as AcrylicBrush;
                TitleTextBlock.Margin = new Thickness(76, 0, 0, 0);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            VkUtils.SendMessage(_viewModel.SelectedPeerId, MessageTextBox.Text);
        }

        private void UserGroupsListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
        }

        private static async Task ViewAttachmentInWindow( /*ReadOnlyCollection<Attachment>*/ Attachment attachments)
        {
            var newView = CoreApplication.CreateNewView();
            var newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                frame.Navigate(typeof(PhotoViewerPage),
                    ((Photo) attachments.Instance).Sizes.Last().Url
                    .ToString()); // todo pages dependent on attachment type
                Window.Current.Content = frame;
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

        private async void OnNewMessageReceived(Message message)
        {
            // var forwardedMessages = message.ForwardedMessages?.ToArray() ?? new Message[] { };
            var attachments = message.Attachments?.ToArray() ?? new Attachment[] { };
            // var replyMessage = message.ReplyMessage;
            // var actionObject = message.Action;

            using (var db = new CacheDbContext())
            {
                if (message.FromId == null || message.PeerId == null) return;
                string readableName;
                var foundPeerByFromId = await db.VkPeers.FindAsync(message.FromId);
                if (foundPeerByFromId != null)
                    readableName = foundPeerByFromId.ReadableName;
                else
                {
                    var peer = (await VkUtils.GetAndCacheNewPeers(new[] {(long) message.FromId})).First();
                    readableName = message.FromId > 0
                        ? ((User) peer).FirstName + " " + ((User) peer).LastName
                        : ((Group) peer).Name;
                    foundPeerByFromId = await db.VkPeers.FindAsync(message.FromId);
                }
                var foundPeerByPeerId = await db.VkPeers.FindAsync(message.PeerId);
                
                var indexToUpdate = 0;
                ChatsListItemData itemToUpdate = null;
                for (var i = 0; i < _viewModel.ChatsList.Count; i++)
                {
                    var item = _viewModel.ChatsList[i];
                    if (item.PeerId != message.PeerId) continue;
                    indexToUpdate = i;
                    itemToUpdate = item;
                    (await db.VkPeers.FindAsync(item.PeerId)).LastMessage = message.Text;
                    break;
                }

                _viewModel.ChatsList.RemoveAt(indexToUpdate);
                _viewModel.ChatsList.Insert(0, new ChatsListItemData
                {
                    Avatar = itemToUpdate?.Avatar,
                    Message = message.Text,
                    Name = itemToUpdate?.Name,
                    PeerId = message.PeerId ?? 0,
                    IsThisUser = itemToUpdate?.IsThisUser ?? false
                });

                await db.VkMessages.AddAsync(new VkMessage
                {
                    ConversationMessageId = message.ConversationMessageId ?? 0,
                    Text = message.Text,
                    FromId = message.FromId ?? 0,
                    PeerId = message.PeerId ?? 0,
                    VkPeer = foundPeerByPeerId,
                    ReceiveTime = DateTime.Now
                });
                await db.SaveChangesAsync();

                _viewModel.MessageDict[(long) message.PeerId].Add(new ChatsListItemData
                {
                    Name = readableName,
                    Message = message.Text,
                    IsThisUser = message.FromId == db.SessionInfo.First().UserId
                });
                foreach (var attachment in attachments)
                {
                    if (attachment.Type == typeof(Photo))
                    {
                        await ViewAttachmentInWindow(attachment);
                    }
                }
            }
        }

        private void OnNetworkStatusChanged(object sender)
        {
            var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetConnectionProfile == null)
            {
                //has internet
            }
            else
            {
                //no connection
            }
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var isInternetConnected = NetworkInformation.GetInternetConnectionProfile() != null;
            var networkStatusCallback = new NetworkStatusChangedEventHandler(OnNetworkStatusChanged);
            _viewModel.ChatsList = new ObservableCollection<ChatsListItemData>();
            ChatsListView.ItemsSource = _viewModel.ChatsList;
            _viewModel.InboxesList = new ObservableCollection<ChatsListItemData>();
            InboxesListView.ItemsSource = _viewModel.InboxesList;
            using (var db = new CacheDbContext())
            {
                foreach (var vkPeer in db.VkPeers)
                {
                    // last is unsupported in ef
                    //var lastMessageText = db.VkMessages.Where(x => x.PeerId == vkPeer.PeerId)
                    //    .OrderByDescending(x => x.ConversationMessageId).FirstOrDefault()?.Text;
                    //if (string.IsNullOrEmpty(vkPeer.LastMessage)) continue;
                    _viewModel.ChatsList.Add(new ChatsListItemData
                    {
                        Avatar = await ImageUtils.ConvertToImageSource(vkPeer.Avatar),
                        Message = vkPeer.LastMessage,
                        Name = vkPeer.ReadableName,
                        PeerId = vkPeer.PeerId,
                        IsThisUser = false
                    });
                }

                foreach (var inbox in db.Inboxes)
                    _viewModel.InboxesList.Add(new ChatsListItemData
                    {
                        Name = inbox.ReadableName,
                        Avatar = await ImageUtils.ConvertToImageSource(inbox.Avatar)
                    });
            }

            _viewModel.MessageDict = new Dictionary<long, ObservableCollection<ChatsListItemData>>();
            foreach (var item in _viewModel.ChatsList)
                _viewModel.MessageDict.Add(item.PeerId, new ObservableCollection<ChatsListItemData>());

            using (var db = new CacheDbContext())
            {
                foreach (var vkMessage in db.VkMessages)
                {
                    if (!_viewModel.MessageDict.ContainsKey(vkMessage.PeerId))
                        _viewModel.MessageDict.Add(vkMessage.PeerId, new ObservableCollection<ChatsListItemData>());
                    //FromId - because one peer can have several senders
                    var vkPeer = await db.VkPeers.FindAsync(vkMessage.FromId);
                    _viewModel.MessageDict[vkMessage.PeerId].Add(new ChatsListItemData
                    {
                        Avatar = await ImageUtils.ConvertToImageSource(vkPeer.Avatar),
                        Message = vkMessage.Text,
                        Name = vkPeer.ReadableName,
                        PeerId = vkMessage.PeerId,
                        IsThisUser = vkMessage.FromId == db.SessionInfo.First().UserId
                    });
                }
            }

            if (isInternetConnected)
            {
                _viewModel.InboxesList = await VkDataToClassesUtils.GetInboxesAsItemData();
                _viewModel.ChatsList = await VkDataToClassesUtils.GetChatsListAsItemData();
                ChatsListView.ItemsSource = _viewModel.ChatsList;
                InboxesListView.ItemsSource = _viewModel.InboxesList;
                InboxesProgressRing.IsActive = false;
                InboxesProgressRing.Visibility = Visibility.Collapsed;
                ChatsListProgressBar.IsEnabled = false;
                ChatsListProgressBar.Visibility = Visibility.Collapsed;
            }


            if (!isInternetConnected) return;
            var vkLongPoll = new VKLongPoll
            {
                NewMessageReceived = OnNewMessageReceived
            };
            await vkLongPoll.FetchHistory();
        }
    }
}
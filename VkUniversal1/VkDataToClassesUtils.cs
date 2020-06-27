using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace VkUniversal1
{
    public static class VkDataToClassesUtils
    {
        public static async Task<ObservableCollection<ChatsListItemData>> GetInboxesAsItemData()
        {
            var thisUser = await VKObjects.Api.Users.GetAsync(new long[] { },
                ProfileFields.Photo50 | ProfileFields.FirstName | ProfileFields.LastName);
            //this user is first, groups will go after
            //extract to another method
            using (var db = new CacheDbContext())
            {
                db.SessionInfo.First().UserId = thisUser.First().Id;
                db.SessionInfo.First().ReadableName = thisUser.First().FirstName + " " + thisUser.First().LastName;
                await db.SaveChangesAsync();
            }
            var groupsNavigationViewData = new List<ChatsListItemData>
            {
                await thisUser.Select(async sthisUser =>
                {
                    var avatar = await ImageUtils.ConvertToImageSource(await ImageUtils.UriToByte(sthisUser.Photo50));
                    return new ChatsListItemData
                    {
                        Name = sthisUser.FirstName + " " + sthisUser.LastName,
                        Avatar = avatar
                    };
                }).First(),
            };
            var groupsVkCollection = await VKObjects.Api.Groups.GetAsync(new GroupsGetParams
            {
                Filter = GroupsFilters.Moderator,
                Extended = true,
                Count = 1000
            });
            foreach (var vkGroup in groupsVkCollection)
            {
                var avatar = await ImageUtils.ConvertToImageSource(await ImageUtils.UriToByte(vkGroup.Photo50));
                var groupItem = new ChatsListItemData
                {
                    Name = vkGroup.Name,
                    Avatar = avatar
                };
                groupsNavigationViewData.Add(groupItem);
            }

            using (var db = new CacheDbContext())
            {
                foreach (var vkGroup in groupsVkCollection)
                    //todo update, not add
                    if (db.Inboxes.Find(vkGroup.Id) == null)
                        await db.Inboxes.AddAsync(new VkInbox
                        {
                            //todo double download to single
                            Avatar = await ImageUtils.UriToByte(vkGroup.Photo50),
                            Id = vkGroup.Id,
                            Type = InboxType.Group,
                            ReadableName = vkGroup.Name
                        });
                if (db.Inboxes.Find(thisUser.First().Id) == null)
                    await db.Inboxes.AddAsync(new VkInbox
                    {
                        Avatar = await ImageUtils.UriToByte(thisUser.First().Photo50),
                        Id = thisUser.First().Id,
                        Type = InboxType.User,
                        ReadableName = thisUser.First().FirstName + " " + thisUser.First().LastName
                    });
                await db.SaveChangesAsync();
            }

            return new ObservableCollection<ChatsListItemData>(groupsNavigationViewData);
        }

        public static async Task<ObservableCollection<ChatsListItemData>> GetChatsListAsItemData(ulong? offset = null)
        {
            var chats = await VKObjects.Api.Messages.GetConversationsAsync(new GetConversationsParams
            {
                Count = 20,
                Offset = offset
            }); 
            var chatsListDatas = new ObservableCollection<ChatsListItemData>();
            foreach (var chat in chats.Items)
            {
                var peerName = "";
                Uri peerImageUri = null;
                if (chat.Conversation.Peer.Type == ConversationPeerType.User)
                {
                    var user = (await VKObjects.Api.Users.GetAsync(new[] {chat.Conversation.Peer.Id},
                        ProfileFields.Photo50 | ProfileFields.FirstName | ProfileFields.LastName))[0];
                    peerName = user.FirstName + ' ' + user.LastName;
                    peerImageUri = user.Photo50;
                }
                else if (chat.Conversation.Peer.Type == ConversationPeerType.Chat)
                {
                    var chatSettings = chat.Conversation.ChatSettings;
                    peerName = chatSettings.Title;
                    peerImageUri = chatSettings.Photo?.Photo50;
                }
                else if (chat.Conversation.Peer.Type == ConversationPeerType.Group)
                {
                    //method requires all params bug in library
                    var group = (await VKObjects.Api.Groups.GetByIdAsync(
                        new[] {(-chat.Conversation.Peer.Id).ToString()},
                        "", GroupsFields.IsVerified))[0];
                    peerName = group.Name;
                    peerImageUri = group.Photo50;
                }

                var peerImage = await ImageUtils.UriToByte(peerImageUri);
                using (var db = new CacheDbContext())
                {
                    if (await db.VkPeers.FindAsync(chat.Conversation.Peer.Id) == null)
                    {
                        await db.VkPeers.AddAsync(new VkPeer
                        {
                            PeerId = chat.Conversation.Peer.Id,
                            ReadableName = peerName,
                            Avatar = peerImage,
                            VkMessages = new List<VkMessage>()
                        });
                        await db.SaveChangesAsync();
                    }
                }

                var chatsListItemData = new ChatsListItemData
                {
                    PeerId = chat.Conversation.Peer.Id,
                    IsThisUser = false,
                    Message = chat.LastMessage.Text,
                    Name = peerName,
                    Avatar = await ImageUtils.ConvertToImageSource(peerImage)
                };
                chatsListDatas.Add(chatsListItemData);
            }

            return chatsListDatas;
        }
    }
}
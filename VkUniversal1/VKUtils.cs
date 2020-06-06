using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VkUniversal1
{
    public static class VkUtils
    {
        public static async void SendMessage(long peerId, string message,
            IEnumerable<MediaAttachment> attachments = null)
        {
            var random = new Random();
            await VKObjects.Api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId,
                RandomId = random.Next(),
                Attachments = attachments,
                Message = message
            });
        }

        private static async Task AddUserToDb(User user)
        {
            using (var db = new CacheDbContext())
            {
                await db.VkPeers.AddAsync(new VkPeer
                {
                    Avatar = user.Photo50 == null ? null : await ImageUtils.UriToByte(user.Photo50),
                    PeerId = user.Id,
                    ReadableName = user.FirstName + " " + user.LastName,
                    VkMessages = new List<VkMessage>()
                });
                await db.SaveChangesAsync();
            }
        }

        private static async Task AddGroupToDb(Group group)
        {
            using (var db = new CacheDbContext())
            {
                await db.VkPeers.AddAsync(new VkPeer
                {
                    Avatar = group.Photo50 == null ? null : await ImageUtils.UriToByte(group.Photo50),
                    PeerId = -group.Id,
                    ReadableName = group.Name,
                    VkMessages = new List<VkMessage>()
                });
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// gets peers and stores them in db
        /// you are responsible for determining whether result is a group or user
        /// </summary>
        /// <param name="peerIds"></param>
        /// <param name="lookupInDb"></param>
        /// <returns></returns>
        public static async Task<List<object>> GetAndCacheNewPeers(IEnumerable<long> peerIds,
            bool lookupInDb = false)
        {
            var ids = peerIds.ToList();
            var userIds = ids.Where(x => x > 0).ToList();
            var users = new ReadOnlyCollection<User>(new List<User>());
            if (userIds.Any())
                users = await VKObjects.Api.Users.GetAsync(userIds, ProfileFields.Photo50);
            var groupIds = ids.Where(x => x < 0).Select(x => (-x).ToString()).ToList();
            var groups = new ReadOnlyCollection<Group>(new List<Group>());
            if (groupIds.Any())
            //groups.get requires all params
                groups = await VKObjects.Api.Groups.GetByIdAsync(
                groupIds, "",
                GroupsFields.IsVerified);
            using (var db = new CacheDbContext())
            {
                foreach (var user in users)
                    if (lookupInDb)
                    {
                        if (await db.VkPeers.FindAsync(user.Id) == null)
                            await AddUserToDb(user);
                    }
                    else
                        await AddUserToDb(user);

                foreach (var group in groups)
                    if (lookupInDb)
                    {
                        if (await db.VkPeers.FindAsync(group.Id) == null)
                            await AddGroupToDb(group);
                    }
                    else
                        await AddGroupToDb(group);
            }

            var collection = new List<object>(users);
            collection.AddRange(groups);
            return collection;
        }
    }
}
using System.Linq;
using VkNet;
using VkNet.Model;

namespace VkUniversal1
{
    public static class VKObjects
    {
        public static VkApi Api { get; private set; }

        public static void InitializeApi()
        {
            Api = new VkApi();
            using (var db = new CacheDbContext())
            {
                Api.Authorize(new ApiAuthParams
                {
                    AccessToken = db.SessionInfo.First().AccessToken
                });
            }
        }
    }
}
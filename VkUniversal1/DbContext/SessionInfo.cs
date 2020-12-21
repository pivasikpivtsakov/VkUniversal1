using System.ComponentModel.DataAnnotations;

namespace VkUniversal1.DbContext
{
    public class SessionInfo
    {
        [Key] public int DummyKey { get; set; }
        public string AccessToken { get; set; }
        public long UserId { get; set; }
        public string ReadableName { get; set; }
    }
}
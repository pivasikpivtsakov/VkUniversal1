using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VkUniversal1.DbContext
{
    public class VkPeer
    {
        [Key] public long PeerId { get; set; }

        public string ReadableName { get; set; }
        public byte[] Avatar { get; set; }
        public string LastMessage { get; set; }
        public List<VkMessage> VkMessages { get; set; }
    }
}
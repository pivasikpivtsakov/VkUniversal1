using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VkUniversal1.DbContext
{
    public class VkInbox
    {
        [Key] public long Id { get; set; }
        public InboxType Type { get; set; }
        public byte[] Avatar { get; set; }
        public string ReadableName { get; set; }
        public List<VkMessage> VkMessages { get; set; }
    }
}
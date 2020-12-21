using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VkUniversal1.DbContext
{
    public class VkMessage
    {
        [Key] public int LocalId { get; set; }
        public long PeerId { get; set; }
        public long FromId { get; set; }
        public long ConversationMessageId { get; set; }
        public long InboxId { get; set; }
        public string Text { get; set; }
        //todo attachments, fwd_messages

        public DateTime ReceiveTime { get; set; }
        
        public VkPeer VkPeer { get; set; }
        [ForeignKey("PeerId")]
        public long VkPeerId { get; set; }
    }
}
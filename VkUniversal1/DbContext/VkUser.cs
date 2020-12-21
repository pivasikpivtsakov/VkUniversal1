using System.ComponentModel.DataAnnotations;

namespace VkUniversal1.DbContext
{
    public class VkUser
    {
        [Key] public long UserId { get; set; }
        
    }
}
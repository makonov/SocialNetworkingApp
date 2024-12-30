using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.Models
{
    public class CommunityType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}

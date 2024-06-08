using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class FindFriendViewModel
    {
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [StringLength(50)]
        public string? City { get; set; }
        public string? Gender { get; set; }
        [Range(13, 100, ErrorMessage = "Поле 'Возраст от' должно принимать значение от 13 до 100")]
        public int? FromAge { get; set; }
        [Range(13, 100, ErrorMessage = "Поле 'Возраст до' должно принимать значение от 13 до 100")]
        public int? ToAge { get; set; }
        public IEnumerable<User>? Users { get; set; }
        public string? CurrentUserId { get; set; }
    }
}

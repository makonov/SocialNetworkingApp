using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CreateCommunityViewModel
    {
        [Required(ErrorMessage = "Введите название сообщества")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Введите описание сообщества")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Выберите тип сообщества")]
        public int TypeId { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace SocialNetworkingApp.ViewModels
{
    public class FindCommunityViewModel
    {
        public string? Title { get; set; }
        public int? TypeId { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
    }
}

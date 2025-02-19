using Microsoft.AspNetCore.Mvc.Rendering;
using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class CommunityCatalogueViewModel
    {
        [Required]
        public User? User { get; set; }
        public string? CurrentUserId { get; set; }
        public IEnumerable<Community?> Communities { get; set; }
        public IEnumerable<SelectListItem>? Types { get; set; }
    }
}

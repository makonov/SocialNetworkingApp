using SocialNetworkingApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SocialNetworkingApp.ViewModels
{
    public class ProjectCatalogueViewModel
    {
        [Required]
        public User? User { get; set; }
        public string? CurrentUserId { get; set; }
        public IEnumerable<Project?> Projects { get; set; }
    }
}

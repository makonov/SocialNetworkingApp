using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SocialNetworkingApp.ViewModels
{
    public class FilterUsersViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set;}
        public int? GroupId { get; set; }
        public string? UserRole {  get; set; }
        public DateTime? BirthDate { get; set; }
        public List<SelectListItem>? Groups { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public IEnumerable<UserReferenceViewModel> Users { get; set; }

        public FilterUsersViewModel()
        {
            AllRoles = new List<IdentityRole>();
        }
    }
}

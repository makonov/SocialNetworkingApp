using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SocialNetworkingApp.ViewModels
{
    public class EditUserViewModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public string UserRole { get; set; }
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public int? GroupId { get; set; }
        public List<SelectListItem>? Groups { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsMale { get; set; }
        public EditUserViewModel()
        {
            AllRoles = new List<IdentityRole>();
        }
    }
}

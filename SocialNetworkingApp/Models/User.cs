using Microsoft.AspNetCore.Identity;

namespace SocialNetworkingApp.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Education {  get; set; }
        public DateTime BirthDate { get; set; }
        public string? City { get; set; }
        public bool? IsMale { get; set; }
        public string? Status { get; set; }
        public DateTime LastLogin {  get; set; }
        public string? ProfilePicture {  get; set; }
    }
}

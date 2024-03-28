using Microsoft.AspNetCore.Identity;

namespace SocialNetworkingApp.Models
{
    public class User : IdentityUser
    {
        string? FirstName { get; set; }
        string? LastName { get; set; }
        string? Education {  get; set; }
        DateTime BirthDate { get; set; }
        string? City { get; set; }
        string? Gender { get; set; }
        string? Status { get; set; }
        DateTime LastLogin {  get; set; }
        string? ProfilePicture {  get; set; }
    }
}

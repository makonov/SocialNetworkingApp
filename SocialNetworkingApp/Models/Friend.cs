using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Friend
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? FirstUserId { get; set; }
        public User? FirstUser { get; set; }
        [ForeignKey("User")]
        public string? SecondUserId { get; set; }
        public User? SecondUser { get; set; }
    }
}

﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public Post? Post { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; } 
    }
}

﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
        [ForeignKey("Gif")]
        public int? GifId { get; set; }
        public Gif? Gif {  get; set; }
        public string? Text { get; set; }
        public int Likes { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

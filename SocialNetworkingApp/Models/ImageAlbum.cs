﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetworkingApp.Models
{
    public class ImageAlbum
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }
        public string? Name { get; set; }
        public string? CoverPath { get; set; }
        public bool IsRequired { get; set; }
        public string? Description { get; set; }
    }
}

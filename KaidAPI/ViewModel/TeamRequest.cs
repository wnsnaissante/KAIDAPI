using System;
using System.ComponentModel.DataAnnotations;

namespace KaidAPI.ViewModel
{
    public class TeamRequest
    {
        [Required]
        public string TeamName { get; set; }
        
        public string? Description { get; set; }
        
        public Guid? LeaderId { get; set; }
        
        [Required]
        public Guid ProjectId { get; set; }
    }
} 
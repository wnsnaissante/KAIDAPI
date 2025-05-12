using System;
using System.ComponentModel.DataAnnotations;

namespace KaidAPI.ViewModel
{
    public class TeamRequest
    {
        [Required]
        public string TeamName { get; set; }
        
        public string? Description { get; set; }
        
        public Guid? LeaderID { get; set; }
        
        [Required]
        public Guid ProjectID { get; set; }
    }
} 
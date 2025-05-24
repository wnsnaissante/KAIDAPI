using System;
using System.ComponentModel.DataAnnotations;

namespace KaidAPI.ViewModel
{
    public class ProjectTaskRequest
    {
        [Required]
        public string TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public Guid? Assignee { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid TeamId { get; set; }
    }
}
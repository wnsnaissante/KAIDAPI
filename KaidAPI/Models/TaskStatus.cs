namespace KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public enum TaskStatusEnum
{
    Todo = 1,
    WorkInProgress = 2,
    Delayed = 3,
    Finished = 4
}

public class TaskStatus
{
    public int StatusId { get; set; }
    public string StatusName { get; set; }
    public string StatusDescription { get; set; }
}

public class TaskStatusConfiguration : IEntityTypeConfiguration<TaskStatus>
{
    public void Configure(EntityTypeBuilder<TaskStatus> builder)
    {
        builder.HasData(
            new TaskStatus 
            { 
                StatusId = (int)TaskStatusEnum.Todo, 
                StatusName = "Todo", 
                StatusDescription = "To Do" 
            },
            new TaskStatus 
            { 
                StatusId = (int)TaskStatusEnum.WorkInProgress, 
                StatusName = "Work in Progress", 
                StatusDescription = "Work in Progress" 
            },
            new TaskStatus 
            { 
                StatusId = (int)TaskStatusEnum.Delayed, 
                StatusName = "Delayed", 
                StatusDescription = "Delayed" 
            },
            new TaskStatus 
            { 
                StatusId = (int)TaskStatusEnum.Finished, 
                StatusName = "Finished", 
                StatusDescription = "Finished" 
            }
        );
    }
}
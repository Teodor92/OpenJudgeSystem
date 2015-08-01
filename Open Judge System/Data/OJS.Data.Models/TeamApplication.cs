namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class TeamApplication : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public int TeamId { get; set; }

        public Team Team { get; set; }

        [Required]
        public string RequesterId { get; set; }

        public UserProfile Requester { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}

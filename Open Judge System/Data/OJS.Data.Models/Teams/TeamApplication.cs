namespace OJS.Data.Models.Teams
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
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

        [MinLength(GlobalConstants.TeamApplicationCommentMinLength)]
        [MaxLength(GlobalConstants.TeamApplicationCommentMaxLength)]
        public string Comment { get; set; }
    }
}

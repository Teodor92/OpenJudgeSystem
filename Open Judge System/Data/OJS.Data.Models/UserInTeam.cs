namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Models.Teams;

    [Table("UsersInTeams")]
    public class UserInTeam
    {
        [Key]
        [Column(Order = 1)]
        public string UserId { get; set; }

        public UserProfile User { get; set; }

        [Key]
        [Column(Order = 2)]
        public int TeamId { get; set; }

        public Team Team { get; set; }

        public TeamRole Role { get; set; }
    }
}

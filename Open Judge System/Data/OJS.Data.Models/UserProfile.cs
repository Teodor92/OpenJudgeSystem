namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Contracts.DataAnnotations;

    public class UserProfile : IdentityUser, IDeletableEntity, IAuditInfo
    {
        private ICollection<Team> ledTeams;
        private ICollection<Team> teams;

        private ICollection<TeamApplication> teamApplications; 

        public UserProfile()
            : this(string.Empty, string.Empty)
        {
        }

        public UserProfile(string userName, string email)
            : base(userName)
        {
            this.Email = email;
            this.UserSettings = new UserSettings();
            this.CreatedOn = DateTime.Now;
            this.ledTeams = new HashSet<Team>();
            this.teams = new HashSet<Team>();
            this.teamApplications = new HashSet<TeamApplication>();
        }

        [Required]
        [MaxLength(80)]
        [IsUnicode(false)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// This property is true when the user comes from the old system and is not preregistered in the new system.
        /// </summary>
        [DefaultValue(false)]
        public bool IsGhostUser { get; set; }

        public int? OldId { get; set; }

        public UserSettings UserSettings { get; set; }

        public Guid? ForgottenPasswordToken { get; set; }

        public virtual ICollection<Team> LedTeams
        {
            get { return this.ledTeams; }
            set { this.ledTeams = value; }
        }

        public virtual ICollection<TeamApplication> TeamApplications
        {
            get { return this.teamApplications; }
            set { this.teamApplications = value; }
        }
        
        [InverseProperty("Members")]
        public virtual ICollection<Team> Teams
        {
            get { return this.teams; }
            set { this.teams = value; }
        }

        #region IDeletableEntity
        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
        #endregion

        #region IAuditInfo
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Specifies whether or not the CreatedOn property should be automatically set.
        /// </summary>
        [NotMapped]
        public bool PreserveCreatedOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }
        #endregion
    }
}

﻿namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    public class Team : DeletableEntity
    {
        private ICollection<UserInTeam> members;
        private ICollection<Participant> participants;
        private ICollection<TeamApplication> teamApplications; 

        public Team()
        {
            this.participants = new HashSet<Participant>();
            this.teamApplications = new HashSet<TeamApplication>();
            this.members = new HashSet<UserInTeam>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<TeamApplication> TeamApplications
        {
            get { return this.teamApplications; }
            set { this.teamApplications = value; }
        }

        public virtual ICollection<Participant> Participants
        {
            get { return this.participants; }
            set { this.participants = value; }
        }

        public virtual ICollection<UserInTeam> Members
        {
            get { return this.members; }
            set { this.members = value; }
        }
    }
}

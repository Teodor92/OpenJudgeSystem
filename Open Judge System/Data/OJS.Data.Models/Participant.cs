﻿namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Participant : AuditInfo
    {
        private ICollection<Submission> submissions;
        private ICollection<ParticipantAnswer> answers;
        private ICollection<ParticipantScore> scores;

        public Participant()
        {
            this.submissions = new HashSet<Submission>();
            this.answers = new HashSet<ParticipantAnswer>();
            this.scores = new HashSet<ParticipantScore>();
        }

        public Participant(int contestId, string userId, bool isOfficial)
            : this()
        {
            this.ContestInstanceId = contestId;
            this.UserId = userId;
            this.IsOfficial = isOfficial;
        }

        [Key]
        public int Id { get; set; }

        public int OldId { get; set; }

        public int ContestInstanceId { get; set; }

        public string UserId { get; set; }

        [Index]
        public bool IsOfficial { get; set; }

        [Required]
        public virtual ContestInstance Contest { get; set; }

        public virtual UserProfile User { get; set; }

        public virtual ICollection<Submission> Submissions
        {
            get { return this.submissions; }
            set { this.submissions = value; }
        }

        public virtual ICollection<ParticipantAnswer> Answers
        {
            get { return this.answers; }
            set { this.answers = value; }
        }

        public virtual ICollection<ParticipantScore> Scores
        {
            get { return this.scores; }
            set { this.scores = value; }
        }
    }
}

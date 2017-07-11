﻿namespace OJS.Web.Areas.Contests.ViewModels.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ParticipantViewModel
    {
        public ParticipantViewModel(Participant participant, bool official)
        {
            this.ContestInstance = ContestInstanceViewModel.FromContest.Compile()(participant.Contest);
            this.LastSubmissionTime = participant.Submissions.Any() ? (DateTime?)participant.Submissions.Max(x => x.CreatedOn) : null;
            this.ContestIsCompete = official;
        }

        public ContestInstanceViewModel ContestInstance { get; set; }

        public DateTime? LastSubmissionTime { get; set; }

        public bool ContestIsCompete { get; set; }
    }
}
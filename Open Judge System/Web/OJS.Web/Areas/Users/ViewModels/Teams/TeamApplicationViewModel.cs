namespace OJS.Web.Areas.Users.ViewModels.Teams
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class TeamApplicationViewModel
    {
        public static Expression<Func<TeamApplication, TeamApplicationViewModel>> ViewModel
        {
            get
            {
                return a => new TeamApplicationViewModel
                {
                    Id = a.Id,
                    TeamId = a.TeamId,
                    TeamName = a.Team.Name,
                    Status = a.Status
                };
            }
        }

        public int Id { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}
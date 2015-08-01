namespace OJS.Web.Areas.Users.ViewModels.Teams
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class TeamViewModel
    {
        public static Expression<Func<Team, TeamViewModel>> ViewModel
        {
            get
            {
                return team => new TeamViewModel
                {
                    Id = team.Id,
                    Name = team.Name,
                    MembersCount = team.Members.Count
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsTeamLeader { get; set; }

        public int MembersCount { get; set; }
    }
}
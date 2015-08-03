namespace OJS.Web.Areas.Users.ViewModels.Teams
{
    using System;
    using System.Linq;
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

        public int Role { get; set; }

        public int MembersCount { get; set; }

        public DateTime CreatedOn { get; set; }

        public static Expression<Func<Team, TeamViewModel>> GetViewModelExpression(string userId)
        {
            return team => new TeamViewModel
            {
                Id = team.Id,
                Name = team.Name,
                Role = (int)team.Members.FirstOrDefault(x => x.UserId == userId).Role,
                MembersCount = team.Members.Count,
                CreatedOn = team.CreatedOn
            };
        }
    }
}
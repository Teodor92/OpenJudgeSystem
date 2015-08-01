namespace OJS.Web.Areas.Users.ViewModels.Teams
{
    using System.Collections.Generic;

    public class UserTeamsViewModel
    {
        public UserTeamsViewModel()
        {
            this.Teams = new List<TeamViewModel>();
            this.Applications = new List<TeamApplicationViewModel>();
        }

        public IEnumerable<TeamViewModel> Teams { get; set; }

        public IEnumerable<TeamApplicationViewModel> Applications { get; set; }
    }
}
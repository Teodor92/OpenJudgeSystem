namespace OJS.Web.Areas.Users.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Users.ViewModels.Teams;
    using OJS.Web.Controllers;

    [Authorize]
    public class TeamsController : BaseController
    {
        public TeamsController(IOjsData data)
            : base(data)
        {
        }

        public TeamsController(IOjsData data, UserProfile profile) 
            : base(data, profile)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            var userId = this.UserProfile.Id;
            var userTeams = this.Data.Teams
                .ForUser(userId)
                .Select(TeamViewModel.ViewModel)
                .ToList();

            var userApplication =
                this.Data.TeamApplications.All()
                    .Where(x => x.RequesterId == userId)
                    .Select(TeamApplicationViewModel.ViewModel)
                    .ToList();

            var viewModel = new UserTeamsViewModel
            {
                Teams = userTeams,
                Applications = userApplication
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult CreateApplication()
        {
            return this.View();
        }
    }
}
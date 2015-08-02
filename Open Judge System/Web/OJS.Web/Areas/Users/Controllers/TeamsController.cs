namespace OJS.Web.Areas.Users.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Users.InputModels.Teams;
    using OJS.Web.Areas.Users.ViewModels.Teams;
    using OJS.Web.Controllers;
    using OJS.Web.ViewModels.Shared;

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
                .WithUser(userId)
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
            var inputModel = new TeamApplicationInputModel
            {
                TeamsDropdDownData = this.GetActiveTeamsDropDownData()
            };

            return this.View(inputModel);
        }

        [HttpPost]
        public ActionResult CreateApplication(TeamApplicationInputModel inputModel)
        {
            if (this.ModelState.IsValid)
            {
                var team = this.Data.Teams.GetById(inputModel.TeamId);

                if (team == null)
                {
                    this.ModelState.AddModelError("TeamId", "Няма такъв отбор");

                    inputModel.TeamsDropdDownData = this.GetActiveTeamsDropDownData();
                    return this.View(inputModel);
                }

                var userId = this.UserProfile.Id;

                var userAlreadyInTeam = team.Members.Any(x => x.UserId == userId);
                if (userAlreadyInTeam)
                {
                    this.ModelState.AddModelError("TeamId", "Вече сте в този отбор");

                    inputModel.TeamsDropdDownData = this.GetActiveTeamsDropDownData();
                    return this.View(inputModel);
                }

                var existingApplication = this.Data.TeamApplications.GetByTeamAndUserId(inputModel.TeamId, userId);
                if (existingApplication.Any())
                {
                    this.ModelState.AddModelError("TeamId", "Вече сте кандидаствали за този отбор");

                    inputModel.TeamsDropdDownData = this.GetActiveTeamsDropDownData();
                    return this.View(inputModel);
                }

                var newApplication = new TeamApplication
                {
                    TeamId = inputModel.TeamId,
                    RequesterId = userId,
                    Status = ApplicationStatus.Pending
                };

                this.Data.TeamApplications.Add(newApplication);
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = string.Format("Успешна заявление за включване в отбора {0}!", team.Name);
                return this.RedirectToAction("Index", "Teams", new { area = "Users" });
            }

            inputModel.TeamsDropdDownData = this.GetActiveTeamsDropDownData();
            return this.View(inputModel);
        }

        private IEnumerable<DropdownViewModel> GetActiveTeamsDropDownData()
        {
            var userId = this.UserProfile.Id;
            return this.Data.Teams
                .ExcludingUser(userId)
                .Select(DropdownViewModel.FromTeam)
                .ToList();
        }
    }
}
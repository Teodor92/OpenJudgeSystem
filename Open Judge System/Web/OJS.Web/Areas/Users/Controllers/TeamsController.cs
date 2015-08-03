namespace OJS.Web.Areas.Users.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

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
            var viewModel = new UserTeamsViewModel
            {
                UserHasTeams = this.Data.Teams.Any(userId),
                UserHasTeamApplications = this.Data.TeamApplications.Any(userId)
            };

            return this.View(viewModel);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var inputModel = new TeamInputModel
            {
                ActiveUsers = this.GetAllActiveUsersForMultiselect()
            };

            return this.View(inputModel);
        }

        [HttpPost]
        public ActionResult Create(TeamInputModel inputModel)
        {
            if (this.ModelState.IsValid)
            {
                var team = new Team
                {
                    Name = inputModel.Name
                };

                // Add creator as teamLeader
                team.Members.Add(new UserInTeam
                {
                    UserId = this.UserProfile.Id,
                    Role = TeamRole.Leader
                });

                this.Data.Teams.Add(team);
                this.Data.SaveChanges();

                this.TempData[GlobalConstants.InfoMessage] = string.Format("Успешна заявка за създаване на отбор!");
                return this.RedirectToAction("Index", "Teams", new { area = "Users" });
            }

            return this.View(inputModel);
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

        [HttpPost]
        public ActionResult ReadTeams([DataSourceRequest]DataSourceRequest request)
        {
            var userId = this.UserProfile.Id;
            var viewModelExpression = TeamViewModel.GetViewModelExpression(userId);
            var teams = this.Data.Teams
                .WithUser(userId)
                .Select(viewModelExpression)
                .ToList();
            return this.Json(teams.ToDataSourceResult(request, this.ModelState));
        }

        [HttpPost]
        public ActionResult ReadTeamApplications([DataSourceRequest]DataSourceRequest request)
        {
            var userId = this.UserProfile.Id;
            var teamApplications =
                this.Data.TeamApplications.All()
                    .Where(x => x.RequesterId == userId)
                    .Select(TeamApplicationViewModel.ViewModel)
                    .ToList();

            return this.Json(teamApplications.ToDataSourceResult(request, this.ModelState));
        }

        private IEnumerable<DropdownViewModel> GetActiveTeamsDropDownData()
        {
            var userId = this.UserProfile.Id;
            return this.Data.Teams
                .ExcludingUser(userId)
                .Select(DropdownViewModel.FromTeam)
                .ToList();
        }

        private IEnumerable<UserViewModel> GetAllActiveUsersForMultiselect()
        {
            return this.Data.Users
                .All()
                .Select(UserViewModel.ViewModel)
                .ToList();
        }
    }
}
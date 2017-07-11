﻿namespace OJS.Web.Areas.Api.Controllers
{
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Api.Models;

    public class ResultsController : ApiController
    {
        private readonly IOjsData data;

        public ResultsController(IOjsData data)
        {
            this.data = data;
        }

        // TODO: Extract method from these two methods since 90% of their code is the same
        public ContentResult GetPointsByAnswer(string apiKey, int? contestId, string answer)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(answer) || !contestId.HasValue)
            {
                return this.Content("ERROR: Invalid arguments");
            }

            var isValidApiKey = this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Content("ERROR: Invalid API key");
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestInstanceId == contestId.Value && x.Answers.Any(a => a.Answer == answer));

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content("ERROR: No participants found");
            }

            if (participants.Count() > 1)
            {
                return this.Content("ERROR: More than one participants found");
            }

            var points =
                participant.Contest.Problems.Select(
                    problem =>
                    problem.Submissions.Where(z => z.ParticipantId == participant.Id)
                        .OrderByDescending(z => z.Points)
                        .Select(z => z.Points)
                        .FirstOrDefault()).Sum();

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public ContentResult GetPointsByEmail(string apiKey, int? contestId, string email)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(email) || !contestId.HasValue)
            {
                return this.Content("ERROR: Invalid arguments");
            }

            var isValidApiKey = this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Content("ERROR: Invalid API key");
            }

            var participants = this.data.Participants.All().Where(
                x => x.IsOfficial && x.ContestInstanceId == contestId.Value && x.User.Email == email);

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content("ERROR: No participants found");
            }

            if (participants.Count() > 1)
            {
                return this.Content("ERROR: More than one participants found");
            }

            var points =
                participant.Contest.Problems.Select(
                    problem =>
                    problem.Submissions.Where(z => z.ParticipantId == participant.Id)
                        .OrderByDescending(z => z.Points)
                        .Select(z => z.Points)
                        .FirstOrDefault()).Sum();

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public JsonResult GetAllResultsForContest(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments"), JsonRequestBehavior.AllowGet);
            }

            var isValidApiKey = this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestInstanceId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.Problems
                        .Where(problem => !problem.IsDeleted)
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    Minutes = participant.Submissions
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.Minutes)
                .ThenBy(x => x.UserName)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllUserResultsPercentageByForContest(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments"), JsonRequestBehavior.AllowGet);
            }

            var isValidApiKey = this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var contestMaxPoints = this.data
                .Problems
                .All()
                .Where(p => !p.IsDeleted && p.ContestId == contestId)
                .Select(p => (double)p.MaximumPoints)
                .DefaultIfEmpty(1)
                .Sum();

            var participants = this.data.Participants
                .All()
                .Where(x => x.ContestInstanceId == contestId.Value)
                .Select(participant => new
                {
                    participant.UserId,
                    ResultsInPercentages = participant
                        .Scores
                        .Where(s => s.Problem.ContestId == contestId.Value)
                        .Select(p => p.Points)
                        .DefaultIfEmpty(0)
                        .Sum() / contestMaxPoints * 100
                })
                .ToList()
                .GroupBy(p => p.UserId)
                .Select(pg => pg.OrderByDescending(p => p.ResultsInPercentages).FirstOrDefault());

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllResultsForContestWithPoints(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments"), JsonRequestBehavior.AllowGet);
            }

            var isValidApiKey = this.data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var participants = this.data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestInstanceId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.Problems
                        .Where(problem => !problem.IsDeleted)
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    ExamTimeInMinutes = participant.Submissions
                        .Where(x => x.Problem.ContestId == contestId.Value && !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.ExamTimeInMinutes)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }
    }
}

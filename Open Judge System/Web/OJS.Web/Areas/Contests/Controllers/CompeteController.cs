﻿namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Helpers;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Participants;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests;

    public class CompeteController : BaseController
    {
        public const string CompeteActionName = "Compete";
        public const string PracticeActionName = "Practice";

        public CompeteController(IOjsData data)
            : base(data)
        {
        }

        public CompeteController(IOjsData data, UserProfile userProfile)
            : base(data, userProfile)
        {
        }

        /// <summary>
        /// Validates if the selected submission type from the participant is allowed in the current problem
        /// </summary>
        /// <param name="submissionTypeId">The id of the submission type selected by the participant</param>
        /// <param name="problem">The problem which the user is attempting to solve</param>
        [NonAction]
        public static void ValidateSubmissionType(int submissionTypeId, Problem problem)
        {
            if (problem.SubmissionTypes.All(submissionType => submissionType.Id != submissionTypeId))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_type_not_found);
            }
        }

        /// <param name="official">A flag checking if the contest will be practiced or competed</param>
        /// Validates if a contest is correctly found. If the user wants to practice or compete in the contest
        /// checks if the contest can be practiced or competed.
        /// </summary>
        /// <param name="contest">Contest to validate.</param>
        /// <param name="official">A flag checking if the contest will be practiced or competed</param>
        [NonAction]
        public void ValidateContest(ContestInstance contest, bool official)
        {
            if (contest == null ||
                contest.IsDeleted ||
                !(contest.IsVisible ||
                    this.User.IsAdmin() ||
                    contest.Lecturers.Any(c => c.LecturerId == this.UserProfile?.Id) ||
                    contest.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile?.Id)))
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Contest_not_found);
            }

            if (official &&
                !contest.CanBeCompeted &&
                !(this.User.IsAdmin() ||
                    contest.Lecturers.Any(c => c.LecturerId == this.UserProfile?.Id) ||
                    contest.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile?.Id)))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Contest_cannot_be_competed);
            }

            if (!official && !contest.CanBePracticed)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Contest_cannot_be_practiced);
            }
        }

        /// <summary>
        /// Displays user compete information: tasks, send source form, ranking, submissions, ranking, etc.
        /// Users only.
        /// </summary>
        [Authorize]
        public ActionResult Index(int id, bool official)
        {
            var contest = this.Data.ContestInstances.GetById(id);
            this.ValidateContest(contest, official);

            if (official && !this.ValidateContestIp(this.Request.UserHostAddress, id))
            {
                return this.RedirectToAction("NewContestIp", new { id });
            }

            var participantFound = this.Data.Participants.Any(id, this.UserProfile.Id, official);

            if (!participantFound)
            {
                if (!contest.ShouldShowRegistrationForm(official))
                {
                    this.Data.Participants.Add(new Participant(id, this.UserProfile.Id, official));
                    this.Data.SaveChanges();
                }
                else
                {
                    // Participant not found, the contest requires password or the contest has questions
                    // to be answered before registration. Redirect to the registration page.
                    // The registration page will take care of all security checks.
                    return this.RedirectToAction("Register", new { id, official });
                }
            }

            var participant = this.Data.Participants.GetWithContest(id, this.UserProfile.Id, official);
            var participantViewModel = new ParticipantViewModel(participant, official);

            this.ViewBag.CompeteType = official ? CompeteActionName : PracticeActionName;

            return this.View(participantViewModel);
        }

        /// <summary>
        /// Displays form for contest registration.
        /// Users only.
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult Register(int id, bool official)
        {
            var participantFound = this.Data.Participants.Any(id, this.UserProfile.Id, official);
            if (participantFound)
            {
                // Participant exists. Redirect to index page.
                return this.RedirectToAction(GlobalConstants.Index, new { id, official });
            }

            var contest = this.Data.ContestInstances.All().Include(x => x.Questions).FirstOrDefault(x => x.Id == id);

            this.ValidateContest(contest, official);

            if (contest.ShouldShowRegistrationForm(official))
            {
                var contestRegistrationModel = new ContestRegistrationViewModel(contest, official);
                return this.View(contestRegistrationModel);
            }

            var participant = new Participant(id, this.UserProfile.Id, official);
            this.Data.Participants.Add(participant);
            this.Data.SaveChanges();

            return this.RedirectToAction(GlobalConstants.Index, new { id, official });
        }

        /// <summary>
        /// Accepts form input for contest registration.
        /// Users only.
        /// </summary>
        //// TODO: Refactor
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Register(bool official, ContestRegistrationModel registrationData)
        {
            // check if the user has already registered for participation and redirect him to the correct action
            var participantFound = this.Data.Participants.Any(registrationData.ContestId, this.UserProfile.Id, official);

            if (participantFound)
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id = registrationData.ContestId, official });
            }

            var contest = this.Data.ContestInstances.GetById(registrationData.ContestId);
            this.ValidateContest(contest, official);

            if (official && contest.HasContestPassword)
            {
                if (string.IsNullOrEmpty(registrationData.Password))
                {
                    this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Empty_Password);
                }
                else if (contest.ContestPassword != registrationData.Password)
                {
                    this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Incorrect_password);
                }
            }

            if (!official && contest.HasPracticePassword)
            {
                if (string.IsNullOrEmpty(registrationData.Password))
                {
                    this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Empty_Password);
                }
                else if (contest.PracticePassword != registrationData.Password)
                {
                    this.ModelState.AddModelError("Password", Resource.Views.CompeteRegister.Incorrect_password);
                }
            }

            var questionsToAnswerCount = official ?
                contest.Questions.Count(x => !x.IsDeleted && x.AskOfficialParticipants) :
                contest.Questions.Count(x => !x.IsDeleted && x.AskPracticeParticipants);

            if (questionsToAnswerCount != registrationData.Questions.Count())
            {
                this.ModelState.AddModelError("Questions", Resource.Views.CompeteRegister.Not_all_questions_answered);
            }

            var contestQuestions = contest.Questions.Where(x => !x.IsDeleted).ToList();

            var participant = new Participant(registrationData.ContestId, this.UserProfile.Id, official);
            this.Data.Participants.Add(participant);
            var counter = 0;
            foreach (var question in registrationData.Questions)
            {
                var contestQuestion = contestQuestions.FirstOrDefault(x => x.Id == question.QuestionId);

                var regularExpression = contestQuestion.RegularExpressionValidation;
                bool correctlyAnswered = false;

                if (!string.IsNullOrEmpty(regularExpression))
                {
                    correctlyAnswered = Regex.IsMatch(question.Answer, regularExpression);
                }

                if (contestQuestion.Type == ContestQuestionType.DropDown)
                {
                    int contestAnswerId;
                    if (int.TryParse(question.Answer, out contestAnswerId) && contestQuestion.Answers.Where(x => !x.IsDeleted).Any(x => x.Id == contestAnswerId))
                    {
                        correctlyAnswered = true;
                    }

                    if (!correctlyAnswered)
                    {
                        this.ModelState.AddModelError(string.Format("Questions[{0}].Answer", counter), Resource.ContestsGeneral.Invalid_selection);
                    }
                }

                participant.Answers.Add(
                    new ParticipantAnswer
                    {
                        ContestQuestionId = question.QuestionId,
                        Answer = question.Answer
                    });

                counter++;
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(new ContestRegistrationViewModel(contest, registrationData, official));
            }

            this.Data.SaveChanges();

            return this.RedirectToAction(GlobalConstants.Index, new { id = registrationData.ContestId, official });
        }

        /// <summary>
        /// Processes a participant's submission for a problem.
        /// </summary>
        /// <param name="participantSubmission">Participant submission.</param>
        /// <param name="official">A check whether the contest is official or practice.</param>
        /// <returns>Returns confirmation if the submission was correctly processed.</returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(SubmissionModel participantSubmission, bool official)
        {
            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == participantSubmission.ProblemId);
            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.Problem_not_found);
            }

            if (official && !this.ValidateContestIp(this.Request.UserHostAddress, problem.ContestId))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ContestId });
            }

            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);
            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            this.ValidateContest(participant.Contest, official);
            ValidateSubmissionType(participantSubmission.SubmissionTypeId, problem);

            if (!this.ModelState.IsValid)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_request);
            }

            if (this.Data.Submissions.HasSubmissionTimeLimitPassedForParticipant(participant.Id, participant.Contest.LimitBetweenSubmissions))
            {
                throw new HttpException((int)HttpStatusCode.ServiceUnavailable, Resource.ContestsGeneral.Submission_was_sent_too_soon);
            }

            if (problem.SourceCodeSizeLimit < participantSubmission.Content.Length)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_too_long);
            }

            if (this.Data.Submissions.HasUserNotProcessedSubmissionForProblem(problem.Id, this.UserProfile.Id))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.User_has_not_processed_submission_for_problem);
            }

            var contest = participant.Contest;

            this.Data.Submissions.Add(new Submission
            {
                ContentAsString = participantSubmission.Content,
                ProblemId = participantSubmission.ProblemId,
                SubmissionTypeId = participantSubmission.SubmissionTypeId,
                ParticipantId = participant.Id,
                IpAddress = this.Request.UserHostAddress,
                IsPublic = ((participant.IsOfficial && contest.ContestPassword == null) ||
                     (!participant.IsOfficial && contest.PracticePassword == null))
                    && contest.IsVisible && !contest.IsDeleted
                    && problem.ShowResults
            });

            this.Data.SaveChanges();

            return this.Json(participantSubmission.ProblemId);
        }

        // TODO: Extract common logic between SubmitBinaryFile and Submit methods
        public ActionResult SubmitBinaryFile(BinarySubmissionModel participantSubmission, bool official, int? returnProblem)
        {
            if (participantSubmission?.File == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Upload_file);
            }

            var problem = this.Data.Problems.All().FirstOrDefault(x => x.Id == participantSubmission.ProblemId);
            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.Problem_not_found);
            }

            if (official && !this.ValidateContestIp(this.Request.UserHostAddress, problem.ContestId))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ContestId });
            }

            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);
            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            this.ValidateContest(participant.Contest, official);
            ValidateSubmissionType(participantSubmission.SubmissionTypeId, problem);

            if (this.Data.Submissions.HasSubmissionTimeLimitPassedForParticipant(participant.Id, participant.Contest.LimitBetweenSubmissions))
            {
                throw new HttpException((int)HttpStatusCode.ServiceUnavailable, Resource.ContestsGeneral.Submission_was_sent_too_soon);
            }

            if (problem.SourceCodeSizeLimit < participantSubmission.File.ContentLength)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Submission_too_long);
            }

            // Validate submission type existence
            var submissionType = this.Data.SubmissionTypes.GetById(participantSubmission.SubmissionTypeId);
            if (submissionType == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_request);
            }

            // Validate if binary files are allowed
            if (!submissionType.AllowBinaryFilesUpload)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Binary_files_not_allowed);
            }

            // Validate file extension
            if (!submissionType.AllowedFileExtensionsList.Contains(
                participantSubmission.File.FileName.GetFileExtension()))
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_extention);
            }

            if (!this.ModelState.IsValid)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.ContestsGeneral.Invalid_request);
            }

            var newSubmission = new Submission
            {
                Content = participantSubmission.File.InputStream.ToByteArray(),
                FileExtension = participantSubmission.File.FileName.GetFileExtension(),
                ProblemId = participantSubmission.ProblemId,
                SubmissionTypeId = participantSubmission.SubmissionTypeId,
                ParticipantId = participant.Id
            };

            this.Data.Submissions.Add(newSubmission);

            this.Data.SaveChanges();

            this.TempData.Add(GlobalConstants.InfoMessage, Resource.ContestsGeneral.Solution_uploaded);
            return this.Redirect(string.Format("/Contests/{2}/Index/{0}#{1}", problem.ContestId, returnProblem ?? 0, official ? CompeteActionName : PracticeActionName));
        }

        /// <summary>
        /// Obtains the partial view for a particular problem.
        /// </summary>
        /// <param name="id">The problem Id</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns a partial view with the problem information.</returns>
        [Authorize]
        public ActionResult Problem(int id, bool official)
        {
            this.ViewBag.IsOfficial = official;
            this.ViewBag.CompeteType = official ? CompeteActionName : PracticeActionName;

            var problem = this.Data.Problems.GetById(id);

            if (problem == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Problem_not_found);
            }

            this.ValidateContest(problem.ContestInstance, official);

            if (!this.Data.Participants.Any(problem.ContestId, this.UserProfile.Id, official))
            {
                return this.RedirectToAction("Register", new { id = problem.ContestId, official });
            }

            if (official && !this.ValidateContestIp(this.Request.UserHostAddress, problem.ContestId))
            {
                return this.RedirectToAction("NewContestIp", new { id = problem.ContestId });
            }

            var problemViewModel = new ContestProblemViewModel(problem)
            {
                UserHasAdminRights = this.CheckIfUserHasProblemPermissions(problem.Id)
            };

            return this.PartialView("_ProblemPartial", problemViewModel);
        }

        /// <summary>
        /// Gets a participant's submission results for a problem.
        /// </summary>
        /// <param name="request">The Kendo data source request.</param>
        /// <param name="id">The problem id.</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns the submissions results for a participant's problem.</returns>
        [Authorize]
        public ActionResult ReadSubmissionResults([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.Data.Problems.GetById(id);
            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            if (!problem.ShowResults)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Problem_results_not_available);
            }

            var userSubmissions = this.Data.Submissions.All()
                                                       .Where(x =>
                                                               x.ProblemId == id &&
                                                               x.ParticipantId == participant.Id)
                                                       .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }

        [Authorize]
        public ActionResult ReadSubmissionResultsAreCompiled([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.Data.Problems.GetById(id);
            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.ContestsGeneral.User_is_not_registered_for_exam);
            }

            var userSubmissions = this.Data.Submissions.All()
                                                       .Where(x =>
                                                               x.ProblemId == id &&
                                                               x.ParticipantId == participant.Id)
                                                       .Select(SubmissionResultIsCompiledViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }

        /// <summary>
        /// Gets the allowed submission types for a problem.
        /// </summary>
        /// <param name="id">The problem id.</param>
        /// <returns>Returns the allowed submission types as JSON.</returns>
        public ActionResult GetAllowedSubmissionTypes(int id)
        {
            var submissionTypesSelectListItems =
                this.Data.Problems.All()
                    .Where(x => x.Id == id)
                    .SelectMany(x => x.SubmissionTypes)
                    .ToList()
                    .Select(x => new
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(CultureInfo.InvariantCulture),
                        Selected = x.IsSelectedByDefault,
                        x.AllowBinaryFilesUpload,
                        x.AllowedFileExtensions
                    });  

            return this.Json(submissionTypesSelectListItems, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets a problem resource and sends it to the user. If the user is not logged in redirects him to the
        /// login page. If the user is not registered for the exam - redirects him to the appropriate page.
        /// </summary>
        /// <param name="id">The resource id.</param>
        /// <param name="official">A check whether the problem is practiced or competed.</param>
        /// <returns>Returns a file with the resource contents or redirects the user to the appropriate
        /// registration page.</returns>
        public ActionResult DownloadResource(int id, bool official)
        {
            var problemWithResource = this.Data.Problems
                                                    .All()
                                                    .FirstOrDefault(problem =>
                                                            problem.Resources.Any(res => res.Id == id && !res.IsDeleted));

            if (problemWithResource == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Problem_not_found);
            }

            var contest = problemWithResource.ContestInstance;
            bool userCanDownloadResource = false;

            if (this.UserProfile == null)
            {
                this.ValidateContest(contest, official);
            }
            else if (this.User != null && this.User.IsAdmin())
            {
                // TODO: add unit tests
                // If the user is an administrator he can download the resource at any time.
                userCanDownloadResource = true;
            }
            else
            {
                this.ValidateContest(contest, official);
                userCanDownloadResource = this.Data.Participants.Any(contest.Id, this.UserProfile.Id, official);
            }

            if (userCanDownloadResource ||
                (contest.CanBeCompeted && !contest.HasContestPassword) ||
                (contest.CanBePracticed && !contest.HasPracticePassword))
            {
                var resource = problemWithResource.Resources.FirstOrDefault(res => res.Id == id);

                if (string.IsNullOrWhiteSpace(resource.FileExtension) || resource.File == null || resource.File.Length == 0)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Resource_cannot_be_downloaded);
                }

                return this.File(resource.File, GlobalConstants.BinaryFileMimeType, string.Format("{0}_{1}.{2}", resource.Problem.Name, resource.Name, resource.FileExtension));
            }

            if ((contest.CanBePracticed && !official) || (contest.CanBeCompeted && official))
            {
                return this.RedirectToAction("Register", new { official, id = contest.Id });
            }

            throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Resource_cannot_be_downloaded);
        }

        /// <summary>
        /// Gets the content of a participant submission for a particular problem.
        /// </summary>
        /// <param name="id">The submission id.</param>
        /// <returns>Returns a JSON with the submission content.</returns>
        //// TODO: Remove if not used
        [Authorize]
        public ActionResult GetSubmissionContent(int id)
        {
            var submission = this.Data.Submissions.All().FirstOrDefault(x => x.Id == id);

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.ContestsGeneral.Submission_not_found);
            }

            if (submission.Participant.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.ContestsGeneral.Submission_not_made_by_user);
            }

            var contentString = submission.ContentAsString;

            return this.Json(contentString, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult NewContestIp(int id)
        {
            if (!this.Data.Participants.Any(id, this.UserProfile.Id, true))
            {
                return this.RedirectToAction("Register", new { id, official = true });
            }

            if (this.ValidateContestIp(this.Request.UserHostAddress, id))
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id, official = true });
            }

            var contest = this.Data.ContestInstances.GetById(id);

            this.ValidateContest(contest, true);

            var model = new NewContestIpViewModel { ContestId = id };
            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult NewContestIp(NewContestIpViewModel model)
        {
            if (!this.Data.Participants.Any(model.ContestId, this.UserProfile.Id, true))
            {
                return this.RedirectToAction("Register", new { id = model.ContestId, official = true });
            }

            if (this.ValidateContestIp(this.Request.UserHostAddress, model.ContestId))
            {
                return this.RedirectToAction(GlobalConstants.Index, new { id = model.ContestId, official = true });
            }

            var contest = this.Data.ContestInstances.All().Include(x => x.AllowedIps).Include("AllowedIps.Ip").FirstOrDefault(x => x.Id == model.ContestId);

            this.ValidateContest(contest, true);

            if (!string.Equals(contest.NewIpPassword, model.NewIpPassword, StringComparison.InvariantCulture))
            {
                this.ModelState.AddModelError("NewIpPassword", "Невалидна парола.");
                return this.View(model);
            }

            var currentUserIpAddress = this.Request.UserHostAddress;
            if (contest.AllowedIps.All(y => y.Ip.Value != currentUserIpAddress))
            {
                var ip = this.Data.Ips.All().FirstOrDefault(x => x.Value == currentUserIpAddress);
                if (ip == null)
                {
                    ip = new Ip { Value = currentUserIpAddress };
                    this.Data.Ips.Add(ip);
                }

                contest.AllowedIps.Add(new ContestIp { Ip = ip, IsOriginallyAllowed = false });

                this.Data.SaveChanges();
            }

            return this.RedirectToAction(GlobalConstants.Index, new { id = model.ContestId, official = true });
        }

        private bool ValidateContestIp(string ip, int contestId)
        {
            var isValidIp = this.Data.ContestInstances
                .All()
                .Any(x => x.Id == contestId && (!x.AllowedIps.Any() || x.AllowedIps.Any(y => y.Ip.Value == ip)));

            return isValidIp;
        }
    }
}
namespace OJS.Web.Areas.Users.InputModels.Teams
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Web.Areas.Users.ViewModels.Teams;

    public class TeamInputModel
    {
        public TeamInputModel()
        {
            this.Members = new HashSet<UserViewModel>();
            this.ActiveUsers = new HashSet<UserViewModel>();
        }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Моля въведете име")]
        [StringLength(
            GlobalConstants.TeamNameMaxLength,
            MinimumLength = GlobalConstants.TeamNameMinLength,
            ErrorMessage = "Името на отбора трябва да е между {2} и {1} символа")]           
        public string Name { get; set; }

        [Display(Name = "Участници")]
        public IEnumerable<UserViewModel> Members { get; set; }

        public IEnumerable<UserViewModel> ActiveUsers { get; set; }
    }
}
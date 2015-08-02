namespace OJS.Web.Areas.Users.InputModels.Teams
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Web.ViewModels.Shared;

    public class TeamApplicationInputModel
    {
        [Display(Name = "Отбор")]
        public int TeamId { get; set; }

        public IEnumerable<DropdownViewModel> TeamsDropdDownData { get; set; }
    }
}
namespace OJS.Web.Areas.Users.InputModels.Teams
{
    using System.Collections.Generic;

    using OJS.Web.ViewModels.Shared;

    public class TeamApplicationInputModel
    {
        public int TeamId { get; set; }

        public IEnumerable<DropdownViewModel> TeamsDropdDownData { get; set; }
    }
}
namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Areas.Administration.Problems.Views.EditorTemplates.ProblemMoveEditorTemplate;

    public class MoveProblemViewModel
    {
        public int Id { get; set; }

        [UIHint("CategoryComboBox")]
        [Display(Name = "Destination", ResourceType = typeof(Resource))]
        public int? DestinationCategoryId { get; set; }

        public string DestinationCategoryName { get; set; }

        [UIHint("ContestsInCategoryDropDown")]
        public int DestinationContestId { get; set; }
    }
}
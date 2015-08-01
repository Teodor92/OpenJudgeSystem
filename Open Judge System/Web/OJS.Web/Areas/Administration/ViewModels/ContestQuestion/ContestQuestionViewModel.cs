﻿namespace OJS.Web.Areas.Administration.ViewModels.ContestQuestion
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class ContestQuestionViewModel : AdministrationViewModel<ContestQuestion>
    {
        [ExcludeFromExcel]
        public static Expression<Func<ContestQuestion, ContestQuestionViewModel>> ViewModel
        {
            get
            {
                return question => new ContestQuestionViewModel
                {
                    QuestionId = question.Id,
                    ContestId = question.ContestId,
                    Text = question.Text,
                    AskOfficialParticipants = question.AskOfficialParticipants,
                    AskPracticeParticipants = question.AskPracticeParticipants,
                    Type = question.Type,
                    RegularExpressionValidation = question.RegularExpressionValidation,
                    CreatedOn = question.CreatedOn,
                    ModifiedOn = question.ModifiedOn,
                };
            }
        }

        [DatabaseProperty(Name = "Id")]
        [DefaultValue(null)]
        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public int? QuestionId { get; set; }

        [DatabaseProperty]
        [HiddenInput(DisplayValue = false)]
        public int? ContestId { get; set; }

        [DatabaseProperty]
        [Display(Name = "Текст")]
        [Required(ErrorMessage = "Текста е задължителен!", AllowEmptyStrings = false)]
        [StringLength(
            GlobalConstants.ContestQuestionMaxLength, 
            MinimumLength = GlobalConstants.ContestQuestionMinLength)]
        [UIHint("SingleLineText")]
        public string Text { get; set; }

        [DatabaseProperty]
        [Display(Name = "Тип въпрос")]
        [UIHint("ContestQuestionType")]
        public ContestQuestionType Type { get; set; }

        [DatabaseProperty]
        [Display(Name = "Reg-Ex валидация")]
        [UIHint("SingleLineText")]
        public string RegularExpressionValidation { get; set; }

        [DatabaseProperty]
        [Display(Name = "Задаване към състезанията")]
        [DefaultValue(true)]
        public bool AskOfficialParticipants { get; set; }

        [DatabaseProperty]
        [Display(Name = "Задаване към упражненията")]
        [DefaultValue(true)]
        public bool AskPracticeParticipants { get; set; }
    }
}
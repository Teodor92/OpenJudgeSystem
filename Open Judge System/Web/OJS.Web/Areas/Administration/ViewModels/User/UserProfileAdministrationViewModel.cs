﻿namespace OJS.Web.Areas.Administration.ViewModels.User
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class UserProfileAdministrationViewModel : AdministrationViewModel<UserProfile>
    {
        [ExcludeFromExcel]
        public static Expression<Func<UserProfile, UserProfileAdministrationViewModel>> ViewModel
        {
            get
            {
                return user => new UserProfileAdministrationViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsGhostUser = user.IsGhostUser,
                    FirstName = user.UserSettings.FirstName,
                    LastName = user.UserSettings.LastName,
                    City = user.UserSettings.City,
                    EducationalInstitution = user.UserSettings.EducationalInstitution,
                    FacultyNumber = user.UserSettings.FacultyNumber,
                    DateOfBirth = user.UserSettings.DateOfBirth,
                    Company = user.UserSettings.Company,
                    JobTitle = user.UserSettings.JobTitle,
                    CreatedOn = user.CreatedOn,
                    ModifiedOn = user.ModifiedOn,
                };
            }
        }

        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public string Id { get; set; }

        [Display(Name = "Потребителско име")]
        [UIHint("NonEditable")]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [RegularExpression(
            GlobalConstants.EmailRegEx,
            ErrorMessage = "Невалиден имейл адрес")]
        [Required(ErrorMessage = "Email-а е задължителен")]
        [StringLength(
            GlobalConstants.EmailMaxLength, 
            ErrorMessage = "Въведеният e-mail е твърде дълъг")]
        [UIHint("SingleLineText")]
        public string Email { get; set; }

        [Display(Name = "От старата система?")]
        [HiddenInput(DisplayValue = false)]
        public bool IsGhostUser { get; set; }

        [Display(Name = "Име")]
        [StringLength(
            GlobalConstants.FirstNameMaxLength, 
            ErrorMessage = "Въведеното име е твърде дълго")]
        [DisplayFormat(
            NullDisplayText = "Няма информация", 
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [StringLength(
            GlobalConstants.LastNameMaxLength, 
            ErrorMessage = "Въведената фамилия е твърде дълга")]
        [DisplayFormat(
            NullDisplayText = "Няма информация", 
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string LastName { get; set; }

        [Display(Name = "Град")]
        [StringLength(
            GlobalConstants.CityNameMaxLength, 
            ErrorMessage = "Въведеният град е твърде дълъг")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string City { get; set; }

        [Display(Name = "Образование")]
        [StringLength(
            GlobalConstants.EducationalInstitutionMaxLength, 
            ErrorMessage = "Въведеното образование е твърде дълго")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string EducationalInstitution { get; set; }

        [Display(Name = "Факултетен номер")]
        [StringLength(
            GlobalConstants.EducationalInstitutionMaxLength, 
            ErrorMessage = "Въведеният факултетен номер е твърде дълъг")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("PositiveInteger")]
        public string FacultyNumber { get; set; }

        [Display(Name = "Дата на раждане")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        [UIHint("Date")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Месторабота")]
        [StringLength(
            GlobalConstants.CompanyNameMaxLength, 
            ErrorMessage = "Въведената месторабота е твърде дълга")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string Company { get; set; }

        [Display(Name = "Позиция")]
        [StringLength(
            GlobalConstants.JobTitleMaxLenth, 
            ErrorMessage = "Въведената позиция е твърде дълга")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string JobTitle { get; set; }

        [Display(Name = "Възраст")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("NonEditable")]
        public byte Age
        {
            get
            {
                return Calculator.Age(this.DateOfBirth) ?? default(byte);
            }
        }

        public override UserProfile GetEntityModel(UserProfile model = null)
        {
            model = model ?? new UserProfile();

            model.Id = this.Id;
            model.UserName = this.UserName;
            model.Email = this.Email;
            model.UserSettings = new UserSettings
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                City = this.City,
                EducationalInstitution = this.EducationalInstitution,
                FacultyNumber = this.FacultyNumber,
                DateOfBirth = this.DateOfBirth,
                Company = this.Company,
                JobTitle = this.JobTitle,
            };
            model.CreatedOn = this.CreatedOn.GetValueOrDefault();
            model.ModifiedOn = this.ModifiedOn;

            return model;
        }
    }
}
namespace OJS.Web.Areas.Users.ViewModels.Teams
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class UserViewModel
    {
        public static Expression<Func<UserProfile, UserViewModel>> ViewModel
        {
            get
            {
                return u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName
                };
            }
        }

        public string Id { get; set; }

        public string UserName { get; set; }
    }
}
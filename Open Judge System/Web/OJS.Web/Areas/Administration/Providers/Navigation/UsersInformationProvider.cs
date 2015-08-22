namespace OJS.Web.Areas.Administration.Providers.Navigation
{
    using System;
    using System.Linq;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Providers.Navigation.Base;
    using OJS.Web.Areas.Administration.Providers.Navigation.Contracts;

    public class UsersInformationProvider : BaseInformationProvider, IInformationProvider
    {
        public UsersInformationProvider(IOjsData data) : base(data)
        {
        }

        public string GetInformation()
        {
            string outputFormat = "Потребители: {0}";
            var userCount = this.Data.Users.All().Count();

            return string.Format(outputFormat, userCount);
        }
    }
}
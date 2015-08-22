namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Areas.Administration.Providers.Navigation;
    using OJS.Web.Areas.Administration.ViewModels.Navigation;
    using OJS.Web.Controllers;

    public class NavigationController : AdministrationController
    {
        public NavigationController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            var model = new NavigationItem(new UsersInformationProvider(this.Data))
            {
                Name = "Потребители",
                GlyphIconName = "some",
            };

            return this.View();
        }
    }
}
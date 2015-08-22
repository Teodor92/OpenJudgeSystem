namespace OJS.Web.Areas.Administration.ViewModels.Navigation
{
    using OJS.Web.Areas.Administration.Providers.Navigation.Contracts;

    public class NavigationItem
    {
        private IInformationProvider provider;

        public NavigationItem(IInformationProvider provider = null)
        {
            this.provider = provider;
        }

        public string Name { get; set; }

        public string GlyphIconName { get; set; }

        public string AdditionalInfo
        {
            get
            {
                return this.provider != null ? this.provider.GetInformation() : string.Empty;
            }
        }
    }
}
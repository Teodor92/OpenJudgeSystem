namespace OJS.Web.Areas.Administration.Providers.Navigation.Base
{
    using OJS.Data;

    public abstract class BaseInformationProvider
    {
        protected BaseInformationProvider(IOjsData data)
        {
            this.Data = data;
        }

        public IOjsData Data { get; set; }
    }
}
namespace OJS.Data.Models
{
    using System.ComponentModel;

    public enum ApplicationStatus
    {
        [Description("Одобрена")]
        Approved = 0,

        [Description("В изчакване")]
        Pending = 1,

        [Description("Отхвърлена")]
        Rejected = 2
    }
}

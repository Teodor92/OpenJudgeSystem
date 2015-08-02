namespace OJS.Web.ViewModels.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class DropdownViewModel
    {
        public static Expression<Func<Team, DropdownViewModel>> FromTeam
        {
            get { return x => new DropdownViewModel { Id = x.Id, Name = x.Name }; }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public static IEnumerable<DropdownViewModel> GetEnumValues<T>() where T : struct, IConvertible
        {
            return Enum
                    .GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new DropdownViewModel
                    {
                        Id = Convert.ToInt32(x),
                        Name = x.GetDescription()
                    });
        }

        public override bool Equals(object obj)
        {
            var other = obj as DropdownViewModel;
            return other != null && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
﻿namespace OJS.Data.Repositories
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ContestsRepository : DeletableEntityRepository<Contest>, IContestsRepository
    {
        public ContestsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public IQueryable<Contest> AllActive() =>
            this.All()
                .Where(c =>
                    !c.IsDeleted &&
                    c.IsVisible &&
                    c.StartTime <= DateTime.Now &&
                    DateTime.Now <= c.EndTime);

        public IQueryable<Contest> AllInactive() =>
            this.All()
                .Where(c =>
                    !c.IsDeleted &&
                    (c.StartTime > DateTime.Now || c.EndTime < DateTime.Now));

        public IQueryable<Contest> AllUpcoming() =>
            this.All().Where(c => c.StartTime > DateTime.Now && c.IsVisible);

        public IQueryable<Contest> AllPast() =>
            this.All().Where(c => !c.IsDeleted && c.EndTime < DateTime.Now && c.IsVisible);

        public IQueryable<Contest> AllVisible() => this.All().Where(c => c.IsVisible);

        public IQueryable<Contest> AllVisibleByCategory(int categoryId) =>
            this.All().Where(c => c.IsVisible && c.CategoryId == categoryId);
    }
}
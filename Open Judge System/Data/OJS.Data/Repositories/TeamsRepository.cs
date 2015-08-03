namespace OJS.Data.Repositories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class TeamsRepository : DeletableEntityRepository<Team>, ITeamsRepository
    {
        public TeamsRepository(IOjsDbContext context) 
            : base(context)
        {
        }

        public IQueryable<Team> WithUser(string userId)
        {
            return this.All()
                .Where(x => x.Members.Any(y => y.UserId == userId));
        }

        public IQueryable<Team> ExcludingUser(string userId)
        {
            return this.All()
                .Where(x => x.Members.All(y => y.UserId != userId));
        }

        public bool Any(string userId)
        {
            return this.All()
                .Any(x => x.Members.Any(y => y.UserId == userId));
        }
    }
}

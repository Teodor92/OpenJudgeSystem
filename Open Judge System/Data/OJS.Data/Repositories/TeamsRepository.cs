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

        public IQueryable<Team> ForUser(string userId)
        {
            return this.All()
                .Where(x => x.LeaderId == userId || x.Members.Any(y => y.Id == userId));
        }
    }
}

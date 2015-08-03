namespace OJS.Data.Repositories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class TeamApplicationsRepository : DeletableEntityRepository<TeamApplication>, ITeamApplicationsRepository
    {
        public TeamApplicationsRepository(IOjsDbContext context) 
            : base(context)
        {
        }

        public IQueryable<TeamApplication> GetByTeamAndUserId(int teamId, string userId)
        {
            return this.All()
                .Where(x => x.TeamId == teamId && x.RequesterId == userId);
        }

        public bool Any(string userId)
        {
            return this.All()
                .Any(x => x.RequesterId == userId);
        }
    }
}

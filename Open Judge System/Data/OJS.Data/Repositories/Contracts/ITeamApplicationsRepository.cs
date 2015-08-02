namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ITeamApplicationsRepository : IDeletableEntityRepository<TeamApplication>
    {
        IQueryable<TeamApplication> GetByTeamAndUserId(int teamId, string userId);
    }
}

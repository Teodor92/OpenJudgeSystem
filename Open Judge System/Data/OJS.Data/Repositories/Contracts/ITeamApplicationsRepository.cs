namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Models.Teams;

    public interface ITeamApplicationsRepository : IDeletableEntityRepository<TeamApplication>
    {
        IQueryable<TeamApplication> GetByTeamAndUserId(int teamId, string userId);

        /// <summary>
        /// Checks if a spesific user has TeamApplications
        /// </summary>
        /// <param name="userId">The id of the user</param>
        bool Any(string userId);
    }
}

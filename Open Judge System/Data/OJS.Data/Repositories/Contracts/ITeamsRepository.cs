namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ITeamsRepository : IDeletableEntityRepository<Team>
    {
        IQueryable<Team> WithUser(string userId);

        IQueryable<Team> ExcludingUser(string userId);

        /// <summary>
        /// Checks if a spesific user is a member of any teams
        /// </summary>
        /// <param name="userId">The id of the user</param>
        bool Any(string userId);
    }
}

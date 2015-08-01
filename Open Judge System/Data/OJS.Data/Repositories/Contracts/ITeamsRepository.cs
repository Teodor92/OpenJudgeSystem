namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ITeamsRepository : IDeletableEntityRepository<Team>
    {
        IQueryable<Team> ForUser(string userId);
    }
}

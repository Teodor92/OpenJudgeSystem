namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Models;

    public interface ITeamApplicationsRepository
    {
        IQueryable<TeamApplication> GetByTeamAndUserId(string teamId, string userId);
    }
}

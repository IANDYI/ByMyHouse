using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Core.Interfaces
{
    public interface IMortgageRepository
    {
        // Write operations
        Task CreateApplicationAsync(MortgageApplication application);
        Task UpdateApplicationStatusAsync(int applicationId, MortgageStatus status);
        
        // Read operations (CQRS)
        Task<MortgageApplication?> GetApplicationByIdAsync(int applicationId);
        Task<IEnumerable<MortgageApplication>> GetPendingApplicationsAsync();
        Task<IEnumerable<MortgageApplication>> GetApprovedApplicationsAsync();
        
        // Income tracking
        Task SaveApplicantIncomeAsync(ApplicantIncome income);
    }
}
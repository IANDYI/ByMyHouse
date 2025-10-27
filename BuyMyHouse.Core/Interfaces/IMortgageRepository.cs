using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Core.Interfaces
{
    public interface IMortgageRepository
    {
        // Command operations for modifying data
        Task SaveNewApplicationAsync(MortgageApplication application);
        Task ChangeApplicationStateAsync(int applicationId, ApplicationState status);
        
        // Query operations implementing Command Query Responsibility Segregation
        Task<MortgageApplication?> FetchApplicationByIdAsync(int applicationId);
        Task<IEnumerable<MortgageApplication>> FetchAwaitingApplicationsAsync();
        Task<IEnumerable<MortgageApplication>> FetchAcceptedApplicationsAsync();
        
        // Persist applicant income information
        Task StoreCandidateIncomeAsync(ApplicantIncome income);
    }
}
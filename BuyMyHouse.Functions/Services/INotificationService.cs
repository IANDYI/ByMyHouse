namespace BuyMyHouse.Functions.Services;

public interface INotificationService
{
    Task SendOfferEmailAsync(string toEmail, string applicantName, string offerUrl);
    Task<string> GenerateOfferDocumentAsync(int applicationId, decimal approvedAmount, decimal interestRate, int termYears, decimal monthlyPayment);
}

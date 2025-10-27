using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Functions.Services;

public interface IMortgageOfferService
{
    Task<MortgageOffer> GenerateOfferAsync(MortgageApplication application);
    bool IsApproved(MortgageApplication application);
}

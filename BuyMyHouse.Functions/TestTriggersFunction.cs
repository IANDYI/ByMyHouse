using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BuyMyHouse.Functions;

/// <summary>
/// HTTP endpoints to manually trigger the timer functions for testing
/// Access at: http://localhost:7071/api/test-process and http://localhost:7071/api/test-send
/// </summary>
public class TestTriggersFunction
{
    private readonly ILogger<TestTriggersFunction> _logger;
    private readonly ProcessMortgageApplicationsFunction _processFunction;
    private readonly SendMortgageOffersFunction _sendFunction;

    public TestTriggersFunction(
        ILogger<TestTriggersFunction> logger,
        ProcessMortgageApplicationsFunction processFunction,
        SendMortgageOffersFunction sendFunction)
    {
        _logger = logger;
        _processFunction = processFunction;
        _sendFunction = sendFunction;
    }

    /// <summary>
    /// HTTP endpoint to manually trigger ProcessMortgageApplications
    /// GET http://localhost:7071/api/test-process
    /// </summary>
    [Function("TestProcessMortgageApplications")]
    public async Task<HttpResponseData> TestProcess(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "test-process")] HttpRequestData req)
    {
        _logger.LogInformation("Manual trigger: ProcessMortgageApplications");

        try
        {
            await _processFunction.Run(new TimerInfo());
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("ProcessMortgageApplications executed successfully. Check the logs above.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ProcessMortgageApplications");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// HTTP endpoint to manually trigger SendMortgageOffers
    /// GET http://localhost:7071/api/test-send
    /// </summary>
    [Function("TestSendMortgageOffers")]
    public async Task<HttpResponseData> TestSend(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "test-send")] HttpRequestData req)
    {
        _logger.LogInformation("Manual trigger: SendMortgageOffers");

        try
        {
            await _sendFunction.Run(new TimerInfo());
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("SendMortgageOffers executed successfully. Check the logs above.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SendMortgageOffers");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }
}

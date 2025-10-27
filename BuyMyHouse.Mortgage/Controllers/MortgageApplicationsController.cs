using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Mortgage.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuyMyHouse.Mortgage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MortgageApplicationsController : ControllerBase
{
    private readonly IMortgageRepository _mortgageRepository;
    private readonly ILogger<MortgageApplicationsController> _logger;

    public MortgageApplicationsController(
        IMortgageRepository mortgageRepository,
        ILogger<MortgageApplicationsController> logger)
    {
        _mortgageRepository = mortgageRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates new mortgage application following CQRS command pattern
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MortgageApplication), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MortgageApplication>> CreateApplication(
        [FromBody] CreateMortgageApplicationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating mortgage application for {ApplicantEmail}", dto.CandidateEmail);

        var application = new MortgageApplication
        {
            CandidateEmail = dto.CandidateEmail,
            CandidateName = dto.CandidateName,
            YearlyIncome = dto.YearlyIncome,
            LoanAmount = dto.LoanAmount,
            PropertyId = dto.PropertyId,
            SubmittedDate = DateTime.UtcNow,
            CurrentStatus = ApplicationState.AwaitingReview
        };

        // Persist application record to Table Storage with auto-generated identifier
        await _mortgageRepository.SaveNewApplicationAsync(application);

        // Archive applicant income data for future analysis
        var income = new ApplicantIncome
        {
            PartitionKey = application.SubmittedDate.ToString("yyyy-MM"),
            RowKey = application.Id.ToString(),
            CandidateEmailAddress = application.CandidateEmail,
            YearlyIncome = application.YearlyIncome,
            CreationTimestamp = application.SubmittedDate,
            ApplicationIdentifier = application.Id.ToString()
        };
        await _mortgageRepository.StoreCandidateIncomeAsync(income);

        _logger.LogInformation("Mortgage application {ApplicationId} created successfully", application.Id);

        return CreatedAtAction(nameof(GetApplicationById), new { id = application.Id }, application);
    }

    /// <summary>
    /// Retrieves specific application using identifier following CQRS query pattern
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MortgageApplication), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MortgageApplication>> GetApplicationById(int id)
    {
        _logger.LogInformation("Fetching mortgage application {ApplicationId}", id);

        var application = await _mortgageRepository.FetchApplicationByIdAsync(id);

        if (application == null)
        {
            _logger.LogWarning("Mortgage application {ApplicationId} not found", id);
            return NotFound(new { message = $"Application with ID {id} not found" });
        }

        return Ok(application);
    }

    /// <summary>
    /// Returns collection of applications awaiting review
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<MortgageApplication>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MortgageApplication>>> GetPendingApplications()
    {
        _logger.LogInformation("Fetching all pending mortgage applications");
        
        var applications = await _mortgageRepository.FetchAwaitingApplicationsAsync();
        
        return Ok(applications);
    }

    /// <summary>
    /// Modifies application state following CQRS command pattern
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ApplicationState status)
    {
        _logger.LogInformation("Updating mortgage application {ApplicationId} status to {Status}", id, status);

        var application = await _mortgageRepository.FetchApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound(new { message = $"Application with ID {id} not found" });
        }

        await _mortgageRepository.ChangeApplicationStateAsync(id, status);

        return NoContent();
    }
}

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
    /// Submit a new mortgage application (CQRS Write)
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

        _logger.LogInformation("Creating mortgage application for {ApplicantEmail}", dto.ApplicantEmail);

        var application = new MortgageApplication
        {
            ApplicantEmail = dto.ApplicantEmail,
            ApplicantName = dto.ApplicantName,
            AnnualIncome = dto.AnnualIncome,
            RequestedAmount = dto.RequestedAmount,
            HouseId = dto.HouseId,
            ApplicationDate = DateTime.UtcNow,
            Status = MortgageStatus.Pending
        };

        // CQRS Write - Save to Table Storage (ID will be auto-generated)
        await _mortgageRepository.CreateApplicationAsync(application);

        // CQRS Write - Save applicant income for future reference
        var income = new ApplicantIncome
        {
            PartitionKey = application.ApplicationDate.ToString("yyyy-MM"),
            RowKey = application.Id.ToString(),
            ApplicantEmail = application.ApplicantEmail,
            AnnualIncome = application.AnnualIncome,
            RecordedDate = application.ApplicationDate,
            ApplicationId = application.Id.ToString()
        };
        await _mortgageRepository.SaveApplicantIncomeAsync(income);

        _logger.LogInformation("Mortgage application {ApplicationId} created successfully", application.Id);

        return CreatedAtAction(nameof(GetApplicationById), new { id = application.Id }, application);
    }

    /// <summary>
    /// Get mortgage application by ID (CQRS Read)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MortgageApplication), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MortgageApplication>> GetApplicationById(int id)
    {
        _logger.LogInformation("Fetching mortgage application {ApplicationId}", id);

        var application = await _mortgageRepository.GetApplicationByIdAsync(id);

        if (application == null)
        {
            _logger.LogWarning("Mortgage application {ApplicationId} not found", id);
            return NotFound(new { message = $"Application with ID {id} not found" });
        }

        return Ok(application);
    }

    /// <summary>
    /// Get all pending applications (CQRS Read)
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IEnumerable<MortgageApplication>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MortgageApplication>>> GetPendingApplications()
    {
        _logger.LogInformation("Fetching all pending mortgage applications");
        
        var applications = await _mortgageRepository.GetPendingApplicationsAsync();
        
        return Ok(applications);
    }

    /// <summary>
    /// Update application status (CQRS Write)
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] MortgageStatus status)
    {
        _logger.LogInformation("Updating mortgage application {ApplicationId} status to {Status}", id, status);

        var application = await _mortgageRepository.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound(new { message = $"Application with ID {id} not found" });
        }

        await _mortgageRepository.UpdateApplicationStatusAsync(id, status);

        return NoContent();
    }
}

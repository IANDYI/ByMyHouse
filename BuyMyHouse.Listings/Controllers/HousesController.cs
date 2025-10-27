using BuyMyHouse.Core.Interfaces;
using BuyMyHouse.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuyMyHouse.Listings.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HousesController : ControllerBase
{
    private readonly IHouseRepository _houseRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<HousesController> _logger;

    public HousesController(
        IHouseRepository houseRepository, 
        IImageStorageService imageStorageService,
        ILogger<HousesController> logger)
    {
        _houseRepository = houseRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available houses
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<House>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<House>>> GetAllHouses()
    {
        _logger.LogInformation("Fetching all houses");
        var houses = await _houseRepository.GetAllAsync();
        return Ok(houses);
    }

    /// <summary>
    /// Get house by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(House), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<House>> GetHouseById(int id)
    {
        _logger.LogInformation("Fetching house with ID: {HouseId}", id);
        var house = await _houseRepository.GetByIdAsync(id);
        
        if (house == null)
        {
            _logger.LogWarning("House with ID {HouseId} not found", id);
            return NotFound(new { message = $"House with ID {id} not found" });
        }

        return Ok(house);
    }

    /// <summary>
    /// Search houses by price range
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<House>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<House>>> SearchByPriceRange(
        [FromQuery] decimal minPrice = 0, 
        [FromQuery] decimal maxPrice = decimal.MaxValue)
    {
        if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
        {
            return BadRequest(new { message = "Invalid price range" });
        }

        _logger.LogInformation("Searching houses in price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);
        var houses = await _houseRepository.GetByPriceRangeAsync(minPrice, maxPrice);
        return Ok(houses);
    }

    /// <summary>
    /// Upload image for a house
    /// </summary>
    [HttpPost("{id}/images")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UploadHouseImage(int id, IFormFile image)
    {
        var house = await _houseRepository.GetByIdAsync(id);
        if (house == null)
        {
            return NotFound(new { message = $"House with ID {id} not found" });
        }

        if (image == null || image.Length == 0)
        {
            return BadRequest(new { message = "No image file provided" });
        }

        // Validate image file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Invalid file type. Only jpg, jpeg, png, and gif are allowed" });
        }

        _logger.LogInformation("Uploading image for house {HouseId}", id);
        
        using var stream = image.OpenReadStream();
        var imageUrl = await _imageStorageService.UploadHouseImageAsync(id, stream, image.FileName);

        // Update house with new image URL
        house.ImageUrls.Add(imageUrl);
        await _houseRepository.UpdateAsync(house);

        _logger.LogInformation("Image uploaded successfully for house {HouseId}: {ImageUrl}", id, imageUrl);

        return Ok(new { imageUrl, message = "Image uploaded successfully" });
    }

    /// <summary>
    /// Get all images for a house
    /// </summary>
    [HttpGet("{id}/images")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetHouseImages(int id)
    {
        var house = await _houseRepository.GetByIdAsync(id);
        if (house == null)
        {
            return NotFound(new { message = $"House with ID {id} not found" });
        }

        return Ok(new { houseId = id, imageUrls = house.ImageUrls });
    }
}


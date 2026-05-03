// 'using' — imports System.Security.Claims so ClaimTypes and related types are in scope
using System.Security.Claims;
// 'using' — imports the Delivery application interfaces namespace (IUnitOfWork)
using Delivery.API.Application.Interfaces;
// 'using' — imports the Delivery domain entities namespace (DeliveryAgent)
using Delivery.API.Domain.Entities;
// 'using' — imports ASP.NET Core Authorization so [Authorize] attribute is available
using Microsoft.AspNetCore.Authorization;
// 'using' — imports ASP.NET Core MVC so ControllerBase and action-result helpers are available
using Microsoft.AspNetCore.Mvc;

// 'namespace' — logical grouping that prevents name collisions; mirrors the folder structure
namespace Delivery.API.Controllers;

/// <summary>
/// API controller for managing delivery agent profiles.
/// Provides endpoints for agent registration, profile management, and admin listing.
/// </summary>
// [ApiController] — attribute that enables automatic model validation, binding source inference, and problem-details responses
[ApiController]
// [Route] — attribute that sets the URL template for all actions in this controller
[Route("api/v1/agents")]
// [Authorize] — attribute that requires the caller to be authenticated (valid JWT) for every action
[Authorize]
// 'public' — access modifier: the class is visible to the ASP.NET Core routing infrastructure
// 'class' — reference type; each HTTP request gets a fresh instance (scoped lifetime by default)
// ControllerBase — base class providing action-result helpers (Ok, NotFound, Conflict, etc.) without view support
public class AgentsController : ControllerBase
{
    // 'private readonly' — field is encapsulated and immutable after construction (set once via DI)
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="AgentsController"/>.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for data access.</param>
    // 'public' — constructor must be public so the DI container can instantiate the controller
    public AgentsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Registers a new delivery agent profile for the authenticated user.
    /// </summary>
    /// <param name="request">The registration request containing vehicle type.</param>
    /// <returns>The created agent profile, or 409 Conflict if a profile already exists.</returns>
    // [HttpPost] — maps HTTP POST requests to this action; "register" is the route suffix
    [HttpPost("register")]
    // [Authorize(Roles = "DeliveryAgent")] — restricts this action to users whose JWT contains the "DeliveryAgent" role claim
    [Authorize(Roles = "DeliveryAgent")]
    // 'public' — action must be public to be discovered by the MVC routing engine
    // 'async' — enables await inside; the compiler generates a state machine
    // 'Task<IActionResult>' — Task wraps the async work; IActionResult is the polymorphic HTTP response type
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
        // 'var' — compiler infers Guid; Guid.Parse converts the string claim to a strongly-typed identifier
        // User.FindFirstValue — reads a claim value from the authenticated user's ClaimsPrincipal
        // '!' — null-forgiving operator: asserts the claim is present (guaranteed by [Authorize])
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'var' — compiler infers string; reads the Name claim from the JWT
        var fullName = User.FindFirstValue(ClaimTypes.Name)!;

        // 'var' — compiler infers DeliveryAgent? (nullable); null means no profile exists yet
        // 'await' — suspends until the database query completes
        var existing = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        // 'if' — null-check: if a profile already exists, return 409 Conflict
        if (existing != null)
            return Conflict("Agent profile already exists."); // Conflict() — produces HTTP 409

        // 'var' — compiler infers DeliveryAgent
        // 'new' — allocates a new DeliveryAgent entity on the managed heap
        var agent = new DeliveryAgent
        {
            UserId = userId,
            FullName = fullName,
            VehicleType = request.VehicleType,
            IsAvailable = true // 'true' — boolean literal; new agents start as available
        };

        // 'await' — asynchronously adds the entity to the EF change tracker
        await _unitOfWork.Agents.AddAsync(agent);
        // 'await' — flushes all pending changes to the database
        await _unitOfWork.SaveChangesAsync();
        // CreatedAtAction — produces HTTP 201 with a Location header pointing to GetMyProfile
        return CreatedAtAction(nameof(GetMyProfile), ToDto(agent));
    }

    // GET /api/v1/agents/me
    /// <summary>
    /// Retrieves the authenticated agent's own profile.
    /// </summary>
    /// <returns>The agent profile DTO, or 404 if not registered.</returns>
    // [HttpGet("me")] — maps HTTP GET /api/v1/agents/me to this action
    [HttpGet("me")]
    [Authorize(Roles = "DeliveryAgent")]
    // 'async Task<IActionResult>' — asynchronous action returning a polymorphic HTTP response
    public async Task<IActionResult> GetMyProfile()
    {
        // 'var' — compiler infers Guid; extracts the user's identity from the JWT claims
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'var' — compiler infers DeliveryAgent?; null means the agent has not registered yet
        // 'await' — suspends until the database lookup completes
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        // 'if' — null-guard: return 404 if no profile exists
        // 'null' — absence of an object reference
        if (agent == null) return NotFound("Agent profile not found. Please register first."); // NotFound() — HTTP 404
        // Ok() — produces HTTP 200 with the DTO as the JSON body
        return Ok(ToDto(agent));
    }

    // PATCH /api/v1/agents/me/availability
    /// <summary>
    /// Toggles the availability status of the authenticated agent.
    /// </summary>
    /// <returns>The updated availability state.</returns>
    // [HttpPatch] — maps HTTP PATCH requests (partial update) to this action
    [HttpPatch("me/availability")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> ToggleAvailability()
    {
        // 'var' — compiler infers Guid from Guid.Parse
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'var' — compiler infers DeliveryAgent?
        // 'await' — suspends until the query completes
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        // 'if' — null-guard: return 404 if the agent profile does not exist
        if (agent == null) return NotFound();

        // '!' — logical NOT operator; flips the boolean value (true → false, false → true)
        agent.IsAvailable = !agent.IsAvailable;
        _unitOfWork.Agents.Update(agent); // marks entity as modified in the EF change tracker
        // 'await' — persists the change to the database
        await _unitOfWork.SaveChangesAsync();
        // 'new' — creates an anonymous object for the JSON response body
        return Ok(new { agent.Id, agent.IsAvailable });
    }

    // PATCH /api/v1/agents/me/vehicle — Update vehicle type
    /// <summary>
    /// Updates the vehicle type for the authenticated agent.
    /// </summary>
    /// <param name="request">The request containing the new vehicle type.</param>
    /// <returns>The updated agent profile, or 400/404 on failure.</returns>
    [HttpPatch("me/vehicle")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleRequest request)
    {
        // 'if' — validates input; string.IsNullOrWhiteSpace checks for null, empty, or whitespace-only strings
        if (string.IsNullOrWhiteSpace(request.VehicleType))
            return BadRequest("Vehicle type is required."); // BadRequest() — HTTP 400

        // 'var' — compiler infers Guid
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'var' — compiler infers DeliveryAgent?
        // 'await' — suspends until the database query completes
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        // 'if' — null-guard
        if (agent == null) return NotFound();

        // Trim() — removes leading/trailing whitespace from the string before persisting
        agent.VehicleType = request.VehicleType.Trim();
        _unitOfWork.Agents.Update(agent);
        // 'await' — persists the updated vehicle type
        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(agent));
    }

    // PATCH /api/v1/agents/me/location — Update agent's own standing location
    /// <summary>
    /// Updates the standing GPS location of the authenticated agent.
    /// </summary>
    /// <param name="request">The request containing the new latitude and longitude.</param>
    /// <returns>The updated agent profile.</returns>
    [HttpPatch("me/location")]
    [Authorize(Roles = "DeliveryAgent")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        // 'var' — compiler infers Guid
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        // 'var' — compiler infers DeliveryAgent?
        // 'await' — suspends until the database query completes
        var agent = await _unitOfWork.Agents.GetByUserIdAsync(userId);
        // 'if' — null-guard
        if (agent == null) return NotFound();

        agent.CurrentLat = request.Lat; // double: latitude coordinate (−90 to +90)
        agent.CurrentLng = request.Lng; // double: longitude coordinate (−180 to +180)
        _unitOfWork.Agents.Update(agent);
        // 'await' — persists the new GPS coordinates
        await _unitOfWork.SaveChangesAsync();
        return Ok(ToDto(agent));
    }

    // GET /api/v1/agents — Admin only
    /// <summary>
    /// Retrieves all registered delivery agents. Accessible by Admin only.
    /// </summary>
    /// <returns>A list of all agent profile DTOs.</returns>
    [HttpGet]
    // [Authorize(Roles = "Admin")] — only users with the Admin role claim can call this endpoint
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        // 'var' — compiler infers IEnumerable<DeliveryAgent> (or similar collection)
        // 'await' — suspends until all agents are fetched from the database
        var agents = await _unitOfWork.Agents.GetAllAsync();
        // Select(ToDto) — LINQ projection: transforms each DeliveryAgent into an anonymous DTO object
        return Ok(agents.Select(ToDto));
    }

    // 'private' — helper method is only used within this controller
    // 'static' — no instance state is needed; the method belongs to the class, not an instance
    // 'object' — return type is the base type; the anonymous object is inferred at compile time
    private static object ToDto(DeliveryAgent a) => new
    {
        a.Id,
        a.UserId,
        a.FullName,
        a.VehicleType,
        a.IsAvailable,
        a.TotalDeliveries,
        a.TotalEarnings,
        a.CurrentLat,
        a.CurrentLng,
        a.CreatedAt
    };
}

/// <summary>Request model for registering a new delivery agent profile.</summary>
// 'public class' — DTO visible to the model binder so it can deserialize the JSON request body
public class RegisterAgentRequest
{
    /// <summary>Gets or sets the type of vehicle used by the agent (e.g., "Bike", "Car").</summary>
    // 'public' — property must be public for JSON deserialization
    // 'string' — built-in reference type representing a sequence of Unicode characters
    public string VehicleType { get; set; } = string.Empty; // string.Empty — initialises to "" instead of null
}

/// <summary>Request model for updating a delivery agent's vehicle type.</summary>
public class UpdateVehicleRequest
{
    /// <summary>Gets or sets the new vehicle type for the agent.</summary>
    // 'string' — reference type; = string.Empty ensures the property is never null by default
    public string VehicleType { get; set; } = string.Empty;
}

/// <summary>Request model for updating a delivery agent's GPS location.</summary>
public class UpdateLocationRequest
{
    /// <summary>Gets or sets the latitude coordinate.</summary>
    // 'double' — 64-bit floating-point value type; suitable for GPS coordinates
    public double Lat { get; set; }

    /// <summary>Gets or sets the longitude coordinate.</summary>
    // 'double' — same 64-bit floating-point type for the longitude value
    public double Lng { get; set; }
}

using FoodFleet.Shared.Messaging.Interfaces;
using Identity.API.Application.Commands;
using Identity.API.Application.Interfaces;
using Identity.API.Application.Services;
using Identity.API.Domain.Entities;
using Identity.API.Domain.Enums;
using Moq;

namespace FoodFleet.Tests.Identity;

/// <summary>
/// NUnit unit tests for <see cref="AuthService"/>.
/// All external dependencies (database, JWT, password hashing, event bus) are mocked
/// with Moq so tests run in-process with no infrastructure required.
/// </summary>
[TestFixture]
public class AuthServiceTests
{
    // ── Mocks ────────────────────────────────────────────────────────────────
    private Mock<IUnitOfWork>      _uow       = null!;
    private Mock<IUserRepository>  _userRepo  = null!;
    private Mock<IJwtTokenService> _jwt       = null!;
    private Mock<IPasswordService> _pwd       = null!;
    private Mock<IEventPublisher>  _publisher = null!;

    // System under test
    private AuthService _sut = null!;

    // ── Setup / Teardown ─────────────────────────────────────────────────────

    [SetUp]
    public void SetUp()
    {
        // Create fresh mocks before every test to avoid state leakage
        _uow       = new Mock<IUnitOfWork>();
        _userRepo  = new Mock<IUserRepository>();
        _jwt       = new Mock<IJwtTokenService>();
        _pwd       = new Mock<IPasswordService>();
        _publisher = new Mock<IEventPublisher>();

        // Wire the unit-of-work mock to return the user-repository mock
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);

        // Build the system under test with all mocked dependencies
        _sut = new AuthService(
            _uow.Object,
            _jwt.Object,
            _pwd.Object,
            _publisher.Object);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // RegisterAsync — happy path
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task RegisterAsync_NewEmail_ReturnsAuthResponseWithToken()
    {
        // Arrange
        var command = new RegisterUserCommand(
            FullName:     "Sathwika Kurma",
            Email:        "sathwika@example.com",
            Password:     "Secret123!",
            MobileNumber: "+91 9876543210",
            Role:         "Customer");

        _userRepo.Setup(r => r.EmailExistsAsync(command.Email))
                 .ReturnsAsync(false);                          // email is free

        _pwd.Setup(p => p.HashPassword(command.Password))
            .Returns("hashed_password");

        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("jwt_access_token");

        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _publisher.Setup(p => p.PublishAsync(
                It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(command);

        // Assert — response carries the correct identity data
        Assert.That(result,                  Is.Not.Null);
        Assert.That(result.Email,            Is.EqualTo(command.Email));
        Assert.That(result.FullName,         Is.EqualTo(command.FullName));
        Assert.That(result.Role,             Is.EqualTo("Customer"));
        Assert.That(result.AccessToken,      Is.EqualTo("jwt_access_token"));

        // Assert — user was persisted and changes were saved
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);

        // Assert — domain event was published
        _publisher.Verify(p => p.PublishAsync(
            It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_UnknownRole_DefaultsToCustomer()
    {
        // Arrange — role string that does not match any UserRole enum member
        var command = new RegisterUserCommand(
            "Test User", "test@example.com", "pass", "0000000000", "UnknownRole");

        _userRepo.Setup(r => r.EmailExistsAsync(command.Email)).ReturnsAsync(false);
        _pwd.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hash");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _publisher.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterAsync(command);

        // Assert — role falls back to "Customer"
        Assert.That(result.Role, Is.EqualTo(UserRole.Customer.ToString()));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // RegisterAsync — failure paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void RegisterAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange — email already exists in the database
        var command = new RegisterUserCommand(
            "Existing User", "existing@example.com", "pass", "0000000000", "Customer");

        _userRepo.Setup(r => r.EmailExistsAsync(command.Email)).ReturnsAsync(true);

        // Act & Assert — service must throw with a descriptive message
        var ex = Assert.ThrowsAsync<Exception>(
            () => _sut.RegisterAsync(command));

        Assert.That(ex!.Message, Does.Contain("Email already registered"));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // LoginAsync — happy path
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithTokens()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "correct_password");

        var existingUser = new User
        {
            Id           = Guid.NewGuid(),
            FullName     = "Test User",
            Email        = command.Email,
            PasswordHash = "stored_hash",
            Role         = UserRole.Customer,
            IsActive     = true
        };

        _userRepo.Setup(r => r.GetByEmailAsync(command.Email))
                 .ReturnsAsync(existingUser);

        _pwd.Setup(p => p.VerifyPassword(command.Password, existingUser.PasswordHash))
            .Returns(true);                                     // password matches

        _jwt.Setup(j => j.GenerateToken(existingUser)).Returns("access_token");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");

        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _publisher.Setup(p => p.PublishAsync(
                It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(command);

        // Assert
        Assert.That(result,             Is.Not.Null);
        Assert.That(result.Email,       Is.EqualTo(command.Email));
        Assert.That(result.AccessToken, Is.EqualTo("access_token"));
        Assert.That(result.RefreshToken, Is.EqualTo("refresh_token"));

        // Refresh-token hash must be persisted
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        _userRepo.Verify(r => r.Update(existingUser), Times.Once);

        // Login event must be published
        _publisher.Verify(p => p.PublishAsync(
            It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // LoginAsync — failure paths
    // ═════════════════════════════════════════════════════════════════════════

    [Test]
    public void LoginAsync_UserNotFound_ThrowsException()
    {
        // Arrange — no user with that email
        var command = new LoginCommand("ghost@example.com", "any_password");

        _userRepo.Setup(r => r.GetByEmailAsync(command.Email))
                 .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(command));
        Assert.That(ex!.Message, Does.Contain("Invalid email or password"));
    }

    [Test]
    public void LoginAsync_WrongPassword_ThrowsException()
    {
        // Arrange — user exists but password is wrong
        var command = new LoginCommand("user@example.com", "wrong_password");

        var user = new User
        {
            Email        = command.Email,
            PasswordHash = "stored_hash",
            IsActive     = true
        };

        _userRepo.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);                                    // password mismatch

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(command));
        Assert.That(ex!.Message, Does.Contain("Invalid email or password"));
    }

    [Test]
    public void LoginAsync_DeactivatedAccount_ThrowsException()
    {
        // Arrange — user exists, password correct, but account is deactivated
        var command = new LoginCommand("inactive@example.com", "correct_password");

        var user = new User
        {
            Email        = command.Email,
            PasswordHash = "stored_hash",
            IsActive     = false                                // deactivated
        };

        _userRepo.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _pwd.Setup(p => p.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(() => _sut.LoginAsync(command));
        Assert.That(ex!.Message, Does.Contain("deactivated"));
    }
}

namespace FoodFleet.Shared.Events.Auth;

public class UserLoggedInEvent
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
}

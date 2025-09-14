namespace AppLogTesting;

public record CreateUserRequest(string Email, string Name, string Password);

public record User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

namespace SportUp.DTO;

public record LoginDto(string Username, string Password);

public record AuthResponseDto(
    string AccessToken,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);

public record UserResponseDto(
    int Id,
    string Username,
    string FullName,
    string Role
);
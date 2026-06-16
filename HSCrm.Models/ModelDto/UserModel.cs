namespace HSCrm.Models.ModelDto
{
    public class UserModel
    {
        public required string UserFirstName { get; set; }
        public required string UserLastName { get; set; }
        public string? UserImageUrl { get; set; }
        public bool UserStatus { get; set; }
        public byte UserType { get; set; }
        public bool UserGender { get; set; }
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public required string PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }

    public class UserLoginDto
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public byte FiscalYearStatus { get; set; }

        public string? UserAvatar { get; set; }

        public string TenantId { get; set; }

        public IEnumerable<string>? Roles { get; set; }

        public IEnumerable<string>? Permissions { get; set; }   // ✅ این را اضافه کن
    }
}

namespace HSCrm.Dashboard.Models.ModelDto
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
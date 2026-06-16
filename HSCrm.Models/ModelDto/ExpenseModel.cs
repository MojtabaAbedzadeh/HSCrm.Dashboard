namespace HSCrm.Models.ModelDto
{
    public class ExpenseModel
    {
        public int Id { get; set; }
        public required string ExpenseTitle { get; set; }
        public int ProjectId { get; set; }
        public ProjectModel Project { get; set; }
        public UserModel CreatedBy { get; set; }
        public int TenantId { get; set; }
        public required string Type { get; set; }
        public DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public required string UserId { get; set; }
    }
}

namespace HSCrm.Models.ModelDto
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public required string ProjectTitle { get; set; }
        public int? CustomerId { get; set; }
        public CustomerModel? Customer { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ProjectLocation { get; set; }
        public required string Status { get; set; }
    }

    public class ProjetcsDropDown
    {
        public string DrId { get; set; }
        public string DrName { get; set; }
    }
}
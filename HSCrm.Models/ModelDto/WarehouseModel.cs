namespace HSCrm.Models.ModelDto
{
    public class WarehousesModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class WarehousesDropDown
    {
        public string DrId { get; set; }
        public string DrName { get; set; }
    }
}

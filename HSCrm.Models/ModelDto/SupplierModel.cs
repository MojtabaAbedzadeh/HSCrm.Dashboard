namespace HSCrm.Models.ModelDto
{
    public class SupplierModel
    {
        public int Id { get; set; }
        public required string SupplierFullName { get; set; }
        public string? SupplierAddress { get; set; }
        public string? SupplierPhone { get; set; }
    }

    public class SuppliersDropDown
    {
        public string DrId { get; set; }
        public string DrName { get; set; }
    }
}

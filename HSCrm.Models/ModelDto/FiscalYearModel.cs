namespace HSCrm.Models.ModelDto
{
    public class FiscalYearDto
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Title { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsClosed { get; set; }
    }

    public class FiscalYearDropDown
    {
        public long DrId { get; set; }
        public string DrName { get; set; }
    }
}

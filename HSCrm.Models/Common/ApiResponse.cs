namespace HSCrm.Models.Common
{    public class ApiResponse<T>
    {
        public required T Data { get; set; }
        public bool Status { get; set; }
        public required string Message { get; set; }
    }
}

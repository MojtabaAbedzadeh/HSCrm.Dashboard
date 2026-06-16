using System.ComponentModel.DataAnnotations;

namespace HSCrm.Models.ModelDto
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage ="نام کاربری وارد نشده است.")]
        public required string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "رمز عبور وارد نشده است.")]
        public required string Password { get; set; }
        public long FiscalYearId { get; set; }
        public IList<string>? Permissions { get; set; }
    }
}

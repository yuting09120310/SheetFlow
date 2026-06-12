using System.ComponentModel.DataAnnotations;

namespace SheetFlow.ViewModels;

public class LoginViewModel
{
    [Display(Name = "帳號")]
    [Required(ErrorMessage = "請輸入帳號")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "密碼")]
    [Required(ErrorMessage = "請輸入密碼")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

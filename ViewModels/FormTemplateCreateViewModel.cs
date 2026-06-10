using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SheetFlow.ViewModels;

public class FormTemplateCreateViewModel
{
    [Required(ErrorMessage = "請輸入表單名稱")]
    [Display(Name = "表單名稱")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "表單說明")]
    public string? Description { get; set; }

    [Display(Name = "Excel 樣板")]
    public IFormFile? ExcelFile { get; set; }

    [Display(Name = "是否啟用")]
    public bool IsActive { get; set; } = true;
}

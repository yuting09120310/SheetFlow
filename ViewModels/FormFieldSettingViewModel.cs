using System.ComponentModel.DataAnnotations;

namespace SheetFlow.ViewModels;

public class FormFieldSettingViewModel
{
    public long Id { get; set; }
    public long FormTemplateId { get; set; }

    [Required(ErrorMessage = "請輸入欄位名稱")]
    [Display(Name = "欄位名稱")]
    public string FieldName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入欄位代碼")]
    [Display(Name = "欄位代碼")]
    public string FieldKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇欄位型態")]
    [Display(Name = "欄位型態")]
    public string FieldType { get; set; } = "Text";

    [Display(Name = "是否必填")]
    public bool IsRequired { get; set; } = true;

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    [Display(Name = "選項內容 (逗號分隔)")]
    public string? OptionsText { get; set; }

    [Display(Name = "預設值")]
    public string? DefaultValue { get; set; }

    [Display(Name = "是否顯示")]
    public bool IsVisible { get; set; } = true;
}

public class FormFieldSettingsViewModel
{
    public long FormTemplateId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public List<FormFieldSettingViewModel> Fields { get; set; } = new();
}

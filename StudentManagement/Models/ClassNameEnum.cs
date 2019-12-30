using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    /// <summary>
    /// 班级的枚举
    /// </summary>
    public enum ClassNameEnum
    {
        [Display(Name = "未分配")]
        None,
        [Display(Name = "一年级")]
        FirstGrade,
        [Display(Name = "二年级")]
        SecondGrade,
        [Display(Name = "三年级")]
        GradeThree
    }
}
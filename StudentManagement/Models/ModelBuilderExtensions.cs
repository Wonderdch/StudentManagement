using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    Name = "ltm",
                    ClassName = ClassNameEnum.FirstGrade,
                    Email = "ltm@ddxc.org"
                },
                new Student
                {
                    Id = 2,
                    Name = "角落的白板报",
                    ClassName = ClassNameEnum.GradeThree,
                    Email = "werltm@qq.com"
                });
        }
    }
}
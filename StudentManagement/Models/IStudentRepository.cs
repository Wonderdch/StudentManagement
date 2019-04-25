namespace StudentManagement.Models
{
    public interface IStudentRepository
    {
        Student GetStudent(int id);
    }
}
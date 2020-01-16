using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace StudentManagement.Models
{
    public class SQLStudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        private readonly ILogger<SQLStudentRepository> _logger;

        public SQLStudentRepository(AppDbContext context,ILogger<SQLStudentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Student Add(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();

            return student;
        }

        public Student Delete(int id)
        {
            var student = _context.Students.Find(id);

            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
            }

            return student;
        }

        public Student Update(Student updateStudent)
        {
            var student = _context.Students.Attach(updateStudent);
            student.State = EntityState.Modified;
            _context.SaveChanges();

            return updateStudent;
        }

        public Student GetStudent(int id)
        {
            return _context.Students.Find(id);
        }

        public IEnumerable<Student> GetAllStudents()
        {
            _logger.LogTrace("学生信息 Trace log");
            _logger.LogDebug("学生信息 Debug log");
            _logger.LogInformation("学生信息 Information log");
            _logger.LogWarning("学生信息 Warning log");
            _logger.LogError("学生信息 Error log");
            _logger.LogCritical("学生信息 Critical log");

            return _context.Students;
        }
    }
}
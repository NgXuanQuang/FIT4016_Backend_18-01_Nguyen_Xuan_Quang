using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;
using System.Text.RegularExpressions;

namespace SchoolManagement.Services
{
    public class StudentService
    {
        private readonly SchoolDbContext _context = new();

        public void GetAll(int page)
        {
            var students = _context.Students
                .Include(s => s.School)
                .Skip((page - 1) * 10)
                .Take(10)
                .ToList();

            foreach (var s in students)
            {
                Console.WriteLine($"{s.FullName} | {s.StudentCode} | {s.Email} | {s.Phone} | {s.School.Name}");
            }
        }
    }
}
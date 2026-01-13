using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;
using System.Text.RegularExpressions;

namespace SchoolManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var context = new SchoolDbContext();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== STUDENT MANAGEMENT SYSTEM =====");
                Console.WriteLine("1. Create student");
                Console.WriteLine("2. View students");
                Console.WriteLine("3. Update student");
                Console.WriteLine("4. Delete student");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");

                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            CreateStudent(context);
                            break;
                        case "2":
                            ViewStudents(context);
                            break;
                        case "3":
                            UpdateStudent(context);
                            break;
                        case "4":
                            DeleteStudent(context);
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Invalid choice!");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        // ================= CREATE =================
        static void CreateStudent(SchoolDbContext context)
        {
            Console.WriteLine("\n--- Create New Student ---");

            Console.Write("Full name: ");
            string fullName = Console.ReadLine();

            Console.Write("Student ID: ");
            string studentCode = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Phone (optional): ");
            string phone = Console.ReadLine();

            Console.Write("School ID: ");
            int.TryParse(Console.ReadLine(), out int schoolId);

            ValidateStudent(context, fullName, studentCode, email, phone, schoolId);

            var student = new Student
            {
                FullName = fullName,
                StudentCode = studentCode,
                Email = email,
                Phone = phone,
                SchoolId = schoolId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            context.Students.Add(student);
            context.SaveChanges();

            Console.WriteLine("Student created successfully!");
        }

        // ================= READ =================
        static void ViewStudents(SchoolDbContext context)
        {
            const int pageSize = 10;
            int page = 1;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- Student List (Page {page}) ---");

                var students = context.Students
                    .Include(s => s.School)
                    .OrderBy(s => s.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (!students.Any())
                {
                    Console.WriteLine("No students found.");
                    return;
                }

                Console.WriteLine("ID | Full Name | Student ID | Email | Phone | School");
                Console.WriteLine("----------------------------------------------------------");

                foreach (var s in students)
                {
                    Console.WriteLine($"{s.Id} | {s.FullName} | {s.StudentCode} | {s.Email} | {s.Phone} | {s.School?.Name}");
                }

                Console.WriteLine("\nN - Next page | P - Previous page | Q - Quit");
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.N) page++;
                else if (key == ConsoleKey.P && page > 1) page--;
                else if (key == ConsoleKey.Q) break;
            }
        }

        // ================= UPDATE =================
        static void UpdateStudent(SchoolDbContext context)
        {
            Console.Write("\nEnter student ID to update: ");
            int.TryParse(Console.ReadLine(), out int id);

            var student = context.Students.Find(id);
            if (student == null)
                throw new Exception("Student not found.");

            Console.Write($"Full name ({student.FullName}): ");
            string fullName = Console.ReadLine();
            if (!string.IsNullOrEmpty(fullName))
                student.FullName = fullName;

            Console.Write($"Email ({student.Email}): ");
            string email = Console.ReadLine();
            if (!string.IsNullOrEmpty(email))
                student.Email = email;

            Console.Write($"Phone ({student.Phone}): ");
            string phone = Console.ReadLine();
            if (!string.IsNullOrEmpty(phone))
                student.Phone = phone;

            Console.Write($"School ID ({student.SchoolId}): ");
            string schoolInput = Console.ReadLine();
            if (int.TryParse(schoolInput, out int schoolId))
                student.SchoolId = schoolId;

            ValidateStudent(context, student.FullName, student.StudentCode, student.Email, student.Phone, student.SchoolId, student.Id);

            student.UpdatedAt = DateTime.Now;
            context.SaveChanges();

            Console.WriteLine("Student updated successfully!");
        }

        // ================= DELETE =================
        static void DeleteStudent(SchoolDbContext context)
        {
            Console.Write("\nEnter student ID to delete: ");
            int.TryParse(Console.ReadLine(), out int id);

            var student = context.Students.Find(id);
            if (student == null)
                throw new Exception("Student not found.");

            Console.Write($"Are you sure you want to delete {student.FullName}? (Y/N): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToUpper() == "Y")
            {
                context.Students.Remove(student);
                context.SaveChanges();
                Console.WriteLine("Student deleted successfully!");
            }
            else
            {
                Console.WriteLine("Delete canceled.");
            }
        }

        // ================= VALIDATION =================
        static void ValidateStudent(
            SchoolDbContext context,
            string fullName,
            string studentCode,
            string email,
            string phone,
            int schoolId,
            int? studentId = null)
        {
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 2 || fullName.Length > 100)
                throw new Exception("Full name must be between 2 and 100 characters.");

            if (string.IsNullOrWhiteSpace(studentCode) || studentCode.Length < 5 || studentCode.Length > 20)
                throw new Exception("Student ID must be between 5 and 20 characters.");

            if (context.Students.Any(s => s.StudentCode == studentCode && s.Id != studentId))
                throw new Exception("Student ID already exists.");

            if (string.IsNullOrWhiteSpace(email) ||
                !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Invalid email format.");

            if (context.Students.Any(s => s.Email == email && s.Id != studentId))
                throw new Exception("Email already exists.");

            if (!string.IsNullOrEmpty(phone) && !Regex.IsMatch(phone, @"^\d{10,11}$"))
                throw new Exception("Phone number must contain 10-11 digits.");

            if (!context.Schools.Any(s => s.Id == schoolId))
                throw new Exception("School does not exist.");
        }
    }
}

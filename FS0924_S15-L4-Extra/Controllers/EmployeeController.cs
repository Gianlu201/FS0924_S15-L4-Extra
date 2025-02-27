using System.Threading.Tasks;
using FS0924_S15_L4_Extra.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FS0924_S15_L4_Extra.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly string _connectionString;

        public EmployeeController()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> Index()
        {
            var employeesList = new EmployeesViewModel()
            {
                Employees = new List<EmployeeViewModel>(),
            };

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query =
                    "SELECT E.IdEmployee, E.EmployeeName, E.EmployeeSurname, E.FiscalCode, E.Age, E.Income, E.TaxDeduction, EE.Title, EE.EmploymentHiring\r\nFROM Employees as E\r\nINNER JOIN\r\nEmployments as EE ON\r\nE.IdEmployment = EE.IdEmployment\r\nORDER BY EE.EmploymentHiring, E.EmployeeSurname;";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employeesList.Employees.Add(
                                new EmployeeViewModel()
                                {
                                    IdEmployee = reader.GetGuid(0),
                                    Name = reader.GetString(1),
                                    Surname = reader.GetString(2),
                                    FiscalCode = reader.GetString(3),
                                    Age = reader.GetInt32(4),
                                    Income = reader.GetDecimal(5),
                                    TaxDeduction = reader.GetBoolean(6),
                                    EmploymentTitle = reader.GetString(7),
                                    HiringDate = DateOnly.FromDateTime(reader.GetDateTime(8)),
                                }
                            );
                        }
                    }
                }
            }

            return View(employeesList);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployee addEmployee)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Try again!";
                return RedirectToAction("Add");
            }

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var newEmploymentId = Guid.NewGuid();

                string query =
                    "INSERT INTO Employments VALUES(@IdEmployment, @Title, @HiringDate);\r\n\r\nINSERT INTO Employees VALUES(NEWID(), @EmployeeName, @EmployeeSurname, @FiscalCode, @Age, @Income, @TaxDeduction, @IdEmploymentFromEmployee);";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdEmployment", newEmploymentId);
                    command.Parameters.AddWithValue("@Title", addEmployee.Employment);
                    command.Parameters.AddWithValue(
                        "@HiringDate",
                        addEmployee.HiringDate != null ? addEmployee.HiringDate : DateTime.Now
                    );
                    command.Parameters.AddWithValue("@EmployeeName", addEmployee.Name);
                    command.Parameters.AddWithValue("@EmployeeSurname", addEmployee.Surname);
                    command.Parameters.AddWithValue("@FiscalCode", addEmployee.FiscalCode);
                    command.Parameters.AddWithValue("@Age", addEmployee.Age);
                    command.Parameters.AddWithValue("@Income", addEmployee.Income);
                    command.Parameters.AddWithValue(
                        "@TaxDeduction",
                        addEmployee.TaxDeduction == "true" ? 1 : 0
                    );
                    command.Parameters.AddWithValue("@IdEmploymentFromEmployee", newEmploymentId);

                    int interestedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet("employee/delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "DELETE FROM Employees WHERE IdEmployee = @EmployeeId;";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", id);

                    int interestedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet("employee/edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var editEmployee = new EditEmployee();

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query =
                    "SELECT E.IdEmployee, E.EmployeeName, E.EmployeeSurname, E.FiscalCode, E.Age, E.Income, E.TaxDeduction, EE.Title, EE.EmploymentHiring, EE.IdEmployment FROM Employees as E\r\nINNER JOIN\r\nEmployments as EE ON\r\nE.IdEmployment = EE.IdEmployment\r\nWHERE E.IdEmployee = @EmployeeId;";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeId", id);

                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            editEmployee.IdEmployee = Guid.Parse(reader["IdEmployee"].ToString());
                            editEmployee.Name = reader.GetString(1);
                            editEmployee.Surname = reader.GetString(2);
                            editEmployee.FiscalCode = reader.GetString(3);
                            editEmployee.Age = reader.GetInt32(4);
                            editEmployee.Income = reader.GetDecimal(5);
                            editEmployee.TaxDeduction = reader.GetBoolean(6) ? "true" : "false";
                            editEmployee.Employment = reader.GetString(7);
                            editEmployee.HiringDate = DateOnly.FromDateTime(reader.GetDateTime(8));
                            editEmployee.IdEmployment = reader.GetGuid(9);
                        }
                    }
                }
            }

            return View(editEmployee);
        }

        [HttpPost("employee/edit/save/{id:guid}")]
        public async Task<IActionResult> EditEmployee(Guid id, EditEmployee editEmployee)
        {
            //if (!ModelState.IsValid)
            //{
            //    return RedirectToAction("Edit");
            //}

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query =
                    "UPDATE Employments SET Title=@EmploymentTitle, EmploymentHiring=@HiringDate WHERE IdEmployment = @EmploymentId;\r\n\r\nUPDATE Employees SET EmployeeName=@EmployeeName, EmployeeSurname=@EmployeeSurname, FiscalCode=@FiscalCode, Age=@Age, Income=@Income, TaxDeduction=@TaxDeduction WHERE IdEmployee = @EmployeeId;";

                await using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EmploymentTitle", editEmployee.Employment);
                    command.Parameters.AddWithValue("@HiringDate", editEmployee.HiringDate);
                    command.Parameters.AddWithValue("@EmploymentId", TempData["IdEmployment"]);
                    command.Parameters.AddWithValue("@EmployeeName", editEmployee.Name);
                    command.Parameters.AddWithValue("@EmployeeSurname", editEmployee.Surname);
                    command.Parameters.AddWithValue("@FiscalCode", editEmployee.FiscalCode);
                    command.Parameters.AddWithValue("@Age", editEmployee.Age);
                    command.Parameters.AddWithValue("@Income", editEmployee.Income);
                    command.Parameters.AddWithValue(
                        "@TaxDeduction",
                        editEmployee.TaxDeduction == "true" ? 1 : 0
                    );
                    command.Parameters.AddWithValue("@EmployeeId", id);

                    int interestedRows = await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ExpenseTracker.Models;
using System.Diagnostics;
using System.Collections.Generic;

namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Expense()
        {
            return View();
        }

        public IActionResult Category()
        {
            List<Category> categories = new List<Category>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT CategoryId, CategoryName, DateCreated FROM Category"; // Ensure you're selecting relevant columns
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                // Execute the query and handle null values in the result
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.IsDBNull(1) ? "No Name" : reader.GetString(1),  // Handle NULL for CategoryName
                            DateCreated = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2)  // Handle NULL for DateCreated
                        });
                    }
                }
            }

            ViewBag.Categories = categories; // Pass categories to the view
            return View();
        }





        // Action for adding a new expense (GET)
        [HttpGet]
        public IActionResult AddExpense()
        {
            // Fetch categories from the database
            List<Category> categories = new List<Category>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT CategoryId, CategoryName FROM Category";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.GetString(1)
                        });
                    }
                }
            }

            // Pass the categories to the view
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public IActionResult AddExpense(string title, decimal amount, DateTime date, int? categoryId, string newCategory, string description)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                int selectedCategoryId;

                // Check if a new category was provided
                if (!string.IsNullOrEmpty(newCategory))
                {
                    // Insert the new category and get its ID
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string insertCategoryQuery = "INSERT INTO Category (CategoryName) OUTPUT INSERTED.CategoryId VALUES (@CategoryName)";
                        SqlCommand command = new SqlCommand(insertCategoryQuery, connection);
                        command.Parameters.AddWithValue("@CategoryName", newCategory);

                        connection.Open();
                        selectedCategoryId = (int)command.ExecuteScalar();
                    }
                }
                else if (categoryId.HasValue)
                {
                    selectedCategoryId = categoryId.Value;
                }
                else
                {
                    // If no category is selected or provided, return an error
                    ViewBag.Error = "Please select or enter a valid category.";
                    return View();
                }

                // Insert the expense into the database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Expenses (Title, Amount, Date, Description, CategoryId) " +
                                   "VALUES (@Title, @Amount, @Date, @Description, @CategoryId)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", selectedCategoryId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                // Redirect to the expense list or home after adding the expense
                return RedirectToAction("Expense");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while adding the expense: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        int count = (int)command.ExecuteScalar();
                        
                        if (count == 1)
                        {
                            return RedirectToAction("Dashboard");
                        }
                        else
                        {
                            ViewBag.Error = "Invalid username or password.";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }

        public IActionResult Dashboard()
        {
            List<Category> categories = new List<Category>();
            List<Expense> expenses = new List<Expense>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Fetch Categories
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT CategoryId, CategoryName, DateCreated FROM Category";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.IsDBNull(1) ? "No Name" : reader.GetString(1),  // Handle NULL for CategoryName
                            DateCreated = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2)  // Handle NULL for DateCreated
                        });
                    }
                }
            }

            // Fetch Expenses
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ExpenseId, Title, Amount, Date, Description, CategoryId FROM Expenses";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(new Expense
                        {
                            ExpenseId = reader.GetInt32(0),
                            Title = reader.IsDBNull(1) ? "No Title" : reader.GetString(1),  // Handle NULL for Title
                            Amount = reader.GetDecimal(2),
                            Date = reader.GetDateTime(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4),  // Handle NULL for Description
                            CategoryId = reader.GetInt32(5)
                        });
                    }
                }
            }

            ViewBag.Categories = categories;
            ViewBag.Expenses = expenses;
            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string email, string username, string password, string confirmPassword)
        {
            // Basic validation: Check if passwords match
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            try
            {
                // Retrieve the connection string from appsettings.json
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to insert user data into the database
                    string query = "INSERT INTO Users (FirstName, LastName, Email, Username, Password) " +
                                   "VALUES (@FirstName, @LastName, @Email, @Username, @Password)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password); // You should hash the password in a real app

                        // Execute the SQL query to insert the data
                        command.ExecuteNonQuery();
                    }
                }

                // Redirect to login after successful registration
                return RedirectToAction("Login");
            }
            catch (SqlException ex)
            {
                // Handle SQL-related exceptions
                ViewBag.Error = "An error occurred while registering: " + ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ViewBag.Error = "An unexpected error occurred: " + ex.Message;
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using itransition4.Models;
using MySql.Data.MySqlClient;
using System.Text;
using System.Security.Cryptography;

namespace itransition4.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Home()
    {
        return View();
    }

    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Registration(string email, string name, string password, string repassword)
    {
        MySqlConnection connection = new MySqlConnection("server=localhost;port=8889;username=root;password=root;database=task4;");
        connection.Open();
        string query = $"SELECT COUNT(*) FROM users WHERE email='{email}';";
        MySqlCommand command = new MySqlCommand(query, connection);
        try
        {
            int kol_email = Convert.ToInt32(command.ExecuteScalar());
            if (kol_email < 0)
            {
                string message = "Пользователь с таким email уже зарегистрирован!";
                ViewBag.Message = message;
            }
            else
            {
                if (password != repassword)
                {
                    string message = "Пароли не совпадают!";
                    ViewBag.Message = message;
                }
                else
                {
                    string hashPassword = HashPassword(password);
                    DateTime now = DateTime.Now;
                    string date = now.ToString("yyyy-MM-dd");
                    string insert_query = $"insert into users VALUES(default,'{name}','{email}','{hashPassword}','{date}','{date}','active');";
                    MySqlCommand insert_command = new MySqlCommand(insert_query, connection);
                    int insert_kol = Convert.ToInt32(insert_command.ExecuteScalar());
                    if (insert_kol < -1)
                    {
                        string message = "Данные не записались!";
                        ViewBag.Message = message;
                    }
                    else
                    {
                        string message = "Пользователь успешно зарегистрирован!";
                        ViewBag.Message = message;
                        return RedirectToAction("Authorization");
                    }
                }
            }
            }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("NullReferenceException occurred: " + ex.Message);
        }
        connection.Close();
        return View();
    }

    public IActionResult Authorization()
    {

        return View();
    }

    public string HashPassword(string pass)
    {
        SHA256 hash = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(pass);
        byte[] password = hash.ComputeHash(bytes);
        string hashPassword = Convert.ToBase64String(password);
        return hashPassword;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


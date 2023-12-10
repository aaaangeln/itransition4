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
    private readonly ApplicationDbContext _context;
    public static string ?userMail;
    public static string? reuserMail;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Home()
    {
        List<Users> usersModel = _context.Users.ToList();
        return View(usersModel);
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

    [HttpPost]
    public IActionResult Authorization(string email, string password)
    {
        MySqlConnection connection = new MySqlConnection("server=localhost;port=8889;username=root;password=root;database=task4;");
        connection.Open();
        string query = $"select count(email) from users where email='{email}';";
        MySqlCommand command = new MySqlCommand(query, connection);
        try
        {
            int kol_email = Convert.ToInt32(command.ExecuteScalar());
            if (kol_email != 1)
            {
                string message = "Пользователя с таким email нет!";
                ViewBag.Message = message;
            }
            else
            {
                string hashPassword = HashPassword(password);
                string query2 = $"select count(*) from users where email='{email}' and password='{hashPassword}'";
                MySqlCommand cmd2 = new MySqlCommand(query2, connection);
                int count = Convert.ToInt32(cmd2.ExecuteScalar());
                if (count == 0)
                {
                    string mess = "Пароль неверный!";
                    ViewBag.Message = mess;
                    return View();
                }
                else
                {
                    DateTime now = DateTime.Now;
                    string date = now.ToString("yyyy-MM-dd HH:mm:ss");

                    string update_query = $"update users set last_data='{date}' where email='{email}';";
                    MySqlCommand update_command = new MySqlCommand(update_query, connection);
                    int update_count = Convert.ToInt32(update_command.ExecuteScalar());
                    if (update_count < -1)
                    {
                    }
                    else
                    {
                        userMail = email;
                        string active_dostup = $"select count(email) from users where dostup='active' and email='{email}';";
                        MySqlCommand cmd = new MySqlCommand(active_dostup, connection);
                        int kol = Convert.ToInt32(cmd.ExecuteScalar());
                        if (kol == 0)
                        {
                            string blocked_dostup = $"select count(email) from users where dostup='blocked' and email='{email}';";
                            MySqlCommand cmd_block = new MySqlCommand(blocked_dostup, connection);
                            int kol_block = Convert.ToInt32(cmd_block.ExecuteScalar());
                            if (kol_block > 0)
                            {
                                string mess = "Вы заблокированы!";
                                ViewBag.Message = mess;
                                return View();
                            }
                            else {
                                string message = "Пользователя с таким email нет!";
                                ViewBag.Message = message;
                                return RedirectToAction("Registration");
                            }
                        }
                        else {
                            return RedirectToAction("Home");
                        }
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


    [HttpPost]
    public IActionResult Block(string selectedBlock)
    {
        var indices = selectedBlock.Split(',');
        string connectionString = "server=localhost;port=8889;username=root;password=root;database=task4;";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            foreach (var index in indices)
            {
                int id = int.Parse(index);
                string query = $"UPDATE users SET dostup='blocked' WHERE id_user={id};";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        return RedirectToAction("Home");
    }

    [HttpPost]
    public IActionResult Unblock(string selectedUnblock)
    {
        var indices = selectedUnblock.Split(',');
        string connectionString = "server=localhost;port=8889;username=root;password=root;database=task4;";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            foreach (var index in indices)
            {
                int id = int.Parse(index);
                string query = $"UPDATE users SET dostup='active' WHERE id_user={id};";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        return RedirectToAction("Home");
    }

    [HttpPost]
    public IActionResult Delete(string selectedDelete)
    {
        var indices = selectedDelete.Split(',');
        string connectionString = "server=localhost;port=8889;username=root;password=root;database=task4;";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
                    foreach (var index in indices)
                    {
                        int id = int.Parse(index);
                        string email = $"select email from users WHERE id_user={id};";
                        using (MySqlCommand cmd = new MySqlCommand(email, connection))
                        {
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                        while (reader.Read())
                        {
                            string value = reader.GetString(0);
                            reuserMail = value;
                        }
                                    if (reuserMail == userMail)
                                    {
                            Delete_User(id);
                                        return RedirectToAction("Registration");
                                    }
                                    else
                                    {
                            Delete_User(id);
                                    }
                                
                            }
                        }
                    }
        }
        return RedirectToAction("Home");
    }


    public void Delete_User(int id)
    {
        string connectionString = "server=localhost;port=8889;username=root;password=root;database=task4;";
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string query = $"delete from users WHERE id_user={id};";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
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


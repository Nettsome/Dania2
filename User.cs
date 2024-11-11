using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General;

/// <summary>
/// Простая модель пользователя
/// </summary>
public class User
{
    public string Name { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";

    // Взять немного кода у Маклецова

    public User() { }
    public User(string name, string login, string password)
    {
        Name = name;
        Login = login;
        Password = password;

    }

    public void UptdateInfoFrom(User user)
    {
        this.Name = user.Name;
        this.Login = user.Login;
        this.Password = user.Password;
    }


    public bool IsValid => !string.IsNullOrEmpty(Login);
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace General;

// Сделать класс RegViewModel
public class LoginViewModel : INotifyPropertyChanged
{
    private RegistrationWindow regWin = new();
    private GameWindow? _gWnd;      // Нужно будет создать это окно
    private bool _isvisible = true;

    public User CurrentUser { get; } = new();

    public bool IsVisible
    {
        get => _isvisible;
        set
        {
            _isvisible = value;
            OnPropertyChanged();
        }
    }


    private Command? _loginCommand;
    // EXPL: Команда для авторизации в форме (переменная => {действие}, переменная => {условие для действия})
    public Command LoginCommand => _loginCommand ??= new Command(
        currWnd =>
        {
            // if (проверка на существование текущего пользователя в бд)
            //          (перейти в окно игры и закрыть это окно)
            if (DbHelper.IsExisting(CurrentUser))
            {
                CurrentUser.UptdateInfoFrom(DbHelper.GetUserByLogin(CurrentUser.Login));        // добавляем имя к текущему пользователю TODO: нужно ли это??? ведь мы можем передавать в таблицу VistiInfo значения только по логину и нам для этого не нужно знать имя пользователя


                StartCommand.Execute(CurrentUser);
                if (currWnd is LoginWindow lWnd) { lWnd.Close(); regWin.Close(); }
            }
        },
        _ => CurrentUser.IsValid                                             // показывает на возможность нажатия кнопки
        );

    private Command? _regCommand;
    public Command RegCommand => _regCommand ??= new Command(
        _ =>
        {
            regWin = new RegistrationWindow();          
            regWin.Show();

        },
        _ => !regWin.IsActive
        );

    private Command? _startcommand;

    public Command StartCommand => _startcommand ??= new Command(
        param =>
        {
            if (param is User player)
            {
                _gWnd = new(player);
                _gWnd.Show();
            }
        }
        );



    public LoginViewModel(string dbname)
    {
        Debug.WriteLine("Открыли конструктор LoginViewModel");

        DbHelper.CreateDataBase(dbname);


        //DbHelper.AddUser(new User("Фанур", "fanur@gmail.com", "12345"));
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

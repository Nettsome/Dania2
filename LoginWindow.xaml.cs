using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace General
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly string dbname = "usersdb";
        private readonly LoginViewModel _lvm;
        public LoginWindow()
        {
            // Инициализация логин вью модель (lvm) 
            _lvm = new(dbname);

            InitializeComponent();

            // Бинд поля логина с логином пользователя
            Binding bLogin = new(nameof(User.Login))
            {
                Source = _lvm.CurrentUser,
                //UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged         // на него пох
            };
            TbLogin.SetBinding(TextBox.TextProperty, bLogin);




            // Бинд кнопок авторизации и регистрации с командами в lvm 
            BtnLogin.Command = _lvm.LoginCommand;
            BtnLogin.CommandParameter = this;
            BtnRegistration.Command = _lvm.RegCommand;  
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Действие при изменении пароля
            _lvm.CurrentUser.Password = PbPassword.Password;
        }
    }
}

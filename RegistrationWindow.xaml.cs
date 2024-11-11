using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace General
{
    public partial class RegistrationWindow : Window
    {
        private readonly RegistrationViewModel _rwm;
        public RegistrationWindow()
        {
            _rwm = new();
            Debug.WriteLine("Открыли окно регистрации");

            InitializeComponent();

            Binding bName = new(nameof(User.Name))
            {
                Source = _rwm.CurrentUser,
                Mode = BindingMode.OneWayToSource
            };
            TbName.SetBinding(TextBox.TextProperty, bName);

            Binding bLogin = new(nameof(User.Login))
            {
                Source = _rwm.CurrentUser,
                Mode = BindingMode.OneWayToSource
            };
            TbLogin.SetBinding(TextBox.TextProperty, bLogin);

            Binding bOutPut = new(nameof(_rwm.OutputInfo))
            {
                Source = _rwm
                //UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged         // без него тоже работает, почему-то
            };
            TbOutput.SetBinding(TextBlock.TextProperty, bOutPut);


            BtnRegistration.Command = _rwm.RegCommand;
        }

        private void PbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _rwm.CurrentUser.Password = PbPassword.Password;
        }
    }



    public class RegistrationViewModel  : INotifyPropertyChanged
    {
        public User CurrentUser { get; init; }

        public RegistrationViewModel()
        {
            Debug.WriteLine("Открыли конструктор RegistrationViewModel");

            CurrentUser = new User();
        }

        private Command? _regCommand;
        public Command? RegCommand => _regCommand ??= new Command(
            _ => 
            {
                if (DbHelper.IsExisting(CurrentUser))
                {
                    OutputInfo = "Такой пользователь уже существует";
                    return;
                }

                if (DbHelper.AddUser(CurrentUser))
                {
                    OutputInfo = "Регистрация прошла успешно";
                }
                else
                {
                    OutputInfo = "Пользователь с таким логином уже существует";
                }
            },
            _ => CurrentUser.IsValid
            );


        private string _outputInfo = "";
        public string OutputInfo
        {
            get => _outputInfo;
            set
            {
                _outputInfo = value;
                OnPropertyChanged("OutPutInfo");
                //OnPropertyChanged(nameof(OutputInfo));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

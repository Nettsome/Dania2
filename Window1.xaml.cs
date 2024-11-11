using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Grafic.View
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string _login;
        DBService db = new DBService();
        public Window1(string login)
        {

            InitializeComponent();
            _login = login;
            
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            db.Logout(_login);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                if (MyRectangle.Width - 10 > 0 && MyRectangle.Height - 10 > 0)
                {
                    MyRectangle.Width -= 10;
                    MyRectangle.Height -= 10;
                }
                
            }
            else
            {
                if(MyRectangle.Width + 10 < 800 && MyRectangle.Height + 10 < 450)
                {
                    MyRectangle.Width += 10;
                    MyRectangle.Height += 10;
                }
                
                
            }
        }
    }
}

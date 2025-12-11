using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShoesShop.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbLogin.Text))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            if (string.IsNullOrEmpty(pbPassword.Password))
            {
                MessageBox.Show("Введите Пароль");
                return;
            }

            List<User> actualUsers = Entities.GetContext().User.ToList();

            User user = actualUsers.Where(u=> u.login.Equals(tbLogin.Text)).FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show("Пользователь с таким именем не найден");
                return;
            }

            if(user.password != pbPassword.Password)
            {
                MessageBox.Show("Введен неверный пароль");
                return;
            }

            if(user.roleId == 1)
            {
                NavigationService.Navigate(new AdminPage());
            }
            else if (user.roleId == 2)
            {
                NavigationService.Navigate(new ManagerPage());
            }
            else if (user.roleId == 3)
            {
                NavigationService.Navigate(new UserPage());
            }
            else
            {
                MessageBox.Show("Ошибка идентификации роли пользователя");
                return;
            }
        }
    }
}

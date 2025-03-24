using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SatelliteSoft
{    
    public partial class LoginPage : Page
    {
        private SatelliteSoftDBEntities context;

        public LoginPage()
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            LoginTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LoginTextBox.Text) && !string.IsNullOrEmpty(PasswordBox.Password))
            {
                string Login = LoginTextBox.Text.ToUpper();
                string Password = PasswordBox.Password.ToUpper();
                var user = context.Users.Include("Roles").FirstOrDefault(u => u.login == Login && u.password == Password);

                if (user != null)
                {
                    MessageBox.Show("Успешная авторизация!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new ProjectsPage(user.login, user.Roles.name));
                }
                else
                {
                    MessageBox.Show("Пользователь не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}

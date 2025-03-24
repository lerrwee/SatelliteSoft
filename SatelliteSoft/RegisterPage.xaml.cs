using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SatelliteSoft
{
    public partial class RegisterPage : Page
    {
        private SatelliteSoftDBEntities context;

        public RegisterPage()
        {
            InitializeComponent();
            context = new SatelliteSoftDBEntities();
            LoginTextBox.Focus();
            LoadRoles();
        }

        private void LoadRoles()
        {
            RoleComboBox.ItemsSource = context.Roles.ToList();
            RoleComboBox.DisplayMemberPath = "name";
            RoleComboBox.SelectedValuePath = "id";
        }

        private void RegisterButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsValidInput())
            {
                string Login = LoginTextBox.Text.ToUpper();
                string Password = PasswordBox.Password.ToUpper();
                int RoleId = (int)RoleComboBox.SelectedValue;

                if (IsLoginUnique(Login))
                {
                    Users user = new Users
                    {
                        login = Login,
                        password = Password,
                        role_id = RoleId
                    };

                    context.Users.Add(user);
                    context.SaveChanges();
                    ClearFields();
                    NavigationService.Navigate(new LoginPage());
                    MessageBox.Show("Регистрация успешна! Войдите в аккаунт.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Логин уже занят! Выберите другой логин.", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Регистрация не прошла! " +
                    "Логин и пароль должны содержать только английские буквы и цифры, а также иметь длину не менее " +
                    "8 и 6 символов соответственно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        private bool IsValidInput()
        {
            return IsValidLogin(LoginTextBox.Text) &&
                   IsValidPassword(PasswordBox.Password) &&
                   PasswordBox.Password == RepeatPasswordBox.Password &&
                   RoleComboBox.SelectedItem != null;
        }

        private bool IsValidLogin(string login)
        {
            // Логин должен содержать только английские буквы, цифры и символы _ и иметь длину не менее 8 символов
            return Regex.IsMatch(login, @"^[a-zA-Z0-9_]{8,}$");
        }

        private bool IsValidPassword(string password)
        {
            // Пароль должен содержать только английские буквы и цифры и иметь длину не менее 6 символов
            return Regex.IsMatch(password, @"^[a-zA-Z0-9]{6,}$");
        }

        private bool IsLoginUnique(string login)
        {
            return !context.Users.Any(u => u.login == login);
        }

        private void ClearFields()
        {
            LoginTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            RepeatPasswordBox.Password = string.Empty;
            RoleComboBox.SelectedItem = null;
        }
    }
}

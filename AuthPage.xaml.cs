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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bashkircev41Size
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        int countError = 0;
        DispatcherTimer timer = new DispatcherTimer();
        private string currentCaptcha;

        public AuthPage()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        int timeLeft = 30;

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;

            TBErrorText.Text = $"Повторите попытку через {timeLeft} секунд";

            if (timeLeft <= 0)
            {
                timer.Stop();
                TBErrorText.Text = "";
                LoginBtn.IsEnabled = true;
                countError = 0;
            }
        }

        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            currentCaptcha = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            captchaOneWord.Text = currentCaptcha[0].ToString();
            captchaTwoWord.Text = currentCaptcha[1].ToString();
            captchaThreeWord.Text = currentCaptcha[2].ToString();
            captchaFourWord.Text = currentCaptcha[3].ToString();

            TBInputCaptcha.Visibility = Visibility.Visible;
        }

        private async void BlockLoginFor10Seconds()
        {
            LoginBtn.IsEnabled = false;
            TBErrorText.Text = "Попробуйте снова через 10 секунд";
            await Task.Delay(10000);
            LoginBtn.IsEnabled = true;
            TBErrorText.Text = "";
            TBInputCaptcha.Text = "";
            GenerateCaptcha();
        }
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text;
            string password = TBPassword.Password;
            if (login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            if (countError >= 1)
            {
                if (TBInputCaptcha.Text != currentCaptcha)
                {
                    MessageBox.Show("Неверная капча!");
                    BlockLoginFor10Seconds(); // блокируем кнопку
                    return;
                }
            }

            User user = Bashkircev41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);
            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                TBLogin.Text = "";
                TBPassword.Password = "";
                TBInputCaptcha.Text = "";
                captchaOneWord.Text = "";
                captchaTwoWord.Text = "";
                captchaThreeWord.Text = "";
                captchaFourWord.Text = "";
                TBInputCaptcha.Visibility = Visibility.Hidden;

                return; // успешный вход, выходим
            }

            countError++;

            TBErrorText.Text = "Неверный логин или пароль";

            if (countError >= 1)
            {
                GenerateCaptcha();
            }

            if (countError >= 3)
            {
                LoginBtn.IsEnabled = false;
                timeLeft = 30;
                timer.Start();
            }

            return;
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserClass.user = null;
            Manager.MainFrame.Navigate(new ProductPage(CurrentUserClass.user));
        }
    }
}

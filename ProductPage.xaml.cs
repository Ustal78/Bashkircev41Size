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

namespace Bashkircev41Size
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        List<OrderProduct> OrderProductList = new List<OrderProduct>();
        int CountRecords;//кол-во записей в таблице
        int CountPage;//Общее кол-во страниц 
        int CurrentPage = 0;//Текущая страница
        List<Product> CurrentPageList = new List<Product>();
        List<Product> TableList;
        public ProductPage(User user)
        {
            InitializeComponent();
            CurrentUser = user;
            if (user != null)
            {
                // Отображаем ФИО
                FIOIB.Text = $"{user.UserSurname} {user.UserName} {user.UserPatronymic}";

                // Отображаем роль
                switch (user.UserRole)
                {
                    case 1:
                        RoleIB.Text = "Менеджер";
                        break;
                    case 2:
                        RoleIB.Text = "Клиент";
                        break;
                    case 3:
                        RoleIB.Text = "Администратор";
                        break;
                    default:
                        RoleIB.Text = "Неизвестно";
                        break;
                }
            }
            else
            {
                FIOIB.Text = "Гость";
                RoleIB.Text = "—";
            }

            var currentServices = Bashkircev41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentServices;

            CBDiscount.SelectedIndex = 0;
            UpdateProducts();
        }


        private void UpdateProducts()
        {
            var currentProducts = Bashkircev41Entities.GetContext().Product.ToList();

            if (CBDiscount.SelectedIndex == 0)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 100).ToList();

            if (CBDiscount.SelectedIndex == 1)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 9.99).ToList();

            if (CBDiscount.SelectedIndex == 2)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount <= 14.99).ToList();

            if (CBDiscount.SelectedIndex == 3)
                currentProducts = currentProducts.Where(p => p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100).ToList();


            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();


            if (RButtonUp.IsChecked == true)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();

            if (RButtonDown.IsChecked == true)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();


            ProductListView.ItemsSource = currentProducts;


            TBCount.Text = currentProducts.Count.ToString();
            TBAllRecords.Text = Bashkircev41Entities.GetContext().Product.Count().ToString();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void CBDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }


        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }



        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Product product = ProductListView.SelectedItem as Product;

            if (product == null)
            {
                return;
            }

            OrderProduct orderProduct = OrderProductList
                .FirstOrDefault(p => p.ProductArticleNumber == product.ProductArticleNumber);

            if (orderProduct != null)
            {
                orderProduct.Count++;
            }
            else
            {
                OrderProduct newOrderProduct = new OrderProduct();
                newOrderProduct.ProductArticleNumber = product.ProductArticleNumber;
                newOrderProduct.Product = product;
                newOrderProduct.Count = 1;

                OrderProductList.Add(newOrderProduct);
            }
            OrderBtn.Visibility = Visibility.Visible;

            MessageBox.Show("Товар добавлен в заказ");

        }

        User CurrentUser;

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow window = new OrderWindow(OrderProductList, CurrentUser);
            window.Show();
        }



    }

}

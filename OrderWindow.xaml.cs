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
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<OrderProduct> OrderProductList;
        User CurrentUser;
        public OrderWindow(List<OrderProduct> orderProductList,User user)
        {
            InitializeComponent();
            OrderProductList = orderProductList;
            OrderListView.ItemsSource = OrderProductList;

            CurrentUser = user;

            CBPickupPoint.ItemsSource = Bashkircev41Entities.GetContext().PickupPoint.ToList();
            CBPickupPoint.SelectedValuePath = "PickupPointID";

            OrderDate.SelectedDate = DateTime.Now;
            DeliveryDate.SelectedDate = GetDeliveryDate();

            if (CurrentUser != null)
                TBFIO.Text = $"{CurrentUser.UserSurname} {CurrentUser.UserName} {CurrentUser.UserPatronymic}";
            else
                TBFIO.Text = "Гость";

            int nextOrderId =
                (Bashkircev41Entities.GetContext().Order.Max(o => (int?)o.OrderID) ?? 0) + 1;

            TBOrderNumber.Text = nextOrderId.ToString();

            CalculateTotal();

        }

        private void CalculateTotal()
        {
            decimal total = 0;

            foreach (var item in OrderProductList)
            {
                if (item.Product != null)
                {
                    total += item.Product.ProductCost * item.Count;
                }
            }

            TBTotal.Text = $"Итого: {total} руб.";
        }

        private DateTime GetDeliveryDate()
        {
            bool isAvailable = true;

            foreach (var item in OrderProductList)
            {
                if (item.Product == null)
                    continue;

                if (item.Count > item.Product.ProductQuantityInStock)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable)
                return DateTime.Now.AddDays(3);
            else
                return DateTime.Now.AddDays(6);
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CBPickupPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи");
                return;
            }

            Order order = new Order();
            order.OrderDate = DateTime.Now;
            order.OrderDeliveryDate = GetDeliveryDate();
            order.OrderStatus = "Новый";
            order.OrderPickupPoint = (int)CBPickupPoint.SelectedValue;
            order.OrderPickupCode = new Random().Next(100, 999).ToString();

            if (CurrentUser != null)
                order.OrderClientID = CurrentUser.UserID;

            Bashkircev41Entities.GetContext().Order.Add(order);
            Bashkircev41Entities.GetContext().SaveChanges();

            foreach (var item in OrderProductList)
            {
                item.OrderID = order.OrderID;
                Bashkircev41Entities.GetContext().OrderProduct.Add(item);

                item.Product.ProductQuantityInStock -= item.Count;
            }

            Bashkircev41Entities.GetContext().SaveChanges();

            MessageBox.Show("Заказ успешно оформлен");
            Close();
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            OrderProduct item = (sender as Button).DataContext as OrderProduct;
            item.Count++;
            OrderListView.Items.Refresh();
            CalculateTotal();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            OrderProduct item = (sender as Button).DataContext as OrderProduct;

            item.Count--;

            if (item.Count <= 0)
                OrderProductList.Remove(item);

            OrderListView.Items.Refresh();
            CalculateTotal();
        }
    }



}

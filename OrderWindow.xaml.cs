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
            CBPickupPoint.ItemsSource = Bashkircev41Entities.GetContext().PickupPoint.ToList();

            CBPickupPoint.DisplayMemberPath = "PickupStreet"; 
            CBPickupPoint.SelectedValuePath = "PickupPointID";
            OrderListView.ItemsSource = orderProductList;
            OrderProductList = orderProductList;
            CurrentUser = user;

            OrderListView.ItemsSource = OrderProductList;

            OrderDate.SelectedDate = DateTime.Now;
            DeliveryDate.SelectedDate = GetDeliveryDate();

            if (CurrentUser != null)
                TBFIO.Text = $"{CurrentUser.UserSurname} {CurrentUser.UserName} {CurrentUser.UserPatronymic}";
            else
                TBFIO.Text = "Гость";

            CalculateTotal();


        }

        private void CalculateTotal()
        {
            decimal total = 0;

            foreach (var item in OrderProductList)
            {
                total += item.Product.ProductCost * item.Count;
            }

            TBTotal.Text = $"Итого: {total} руб.";
        }

        private DateTime GetDeliveryDate()
        {
            bool isAvailable = true;

            foreach (var item in OrderProductList)
            {
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
            DateTime deliveryDate = GetDeliveryDate();

            Order order = new Order();
            order.OrderDate = DateTime.Now;
            order.OrderDeliveryDate = deliveryDate;
            order.OrderStatus = "Новый";

            order.OrderPickupPoint = (int)CBPickupPoint.SelectedValue;

            order.OrderPickupCode = new Random().Next(100, 999).ToString();


            if (CurrentUser != null)
            {
                order.OrderClientID = CurrentUser.UserID;
            }


            Bashkircev41Entities.GetContext().Order.Add(order);
            Bashkircev41Entities.GetContext().SaveChanges();

            foreach (var item in OrderProductList)
            {
                item.OrderID = order.OrderID;
                Bashkircev41Entities.GetContext().OrderProduct.Add(item);

                item.Product.ProductQuantityInStock -= item.Count;
            }


            Bashkircev41Entities.GetContext().SaveChanges();
            TBOrderNumber.Text = order.OrderID.ToString();
            MessageBox.Show("Заказ оформлен");
            this.Close();
        }


    }


}

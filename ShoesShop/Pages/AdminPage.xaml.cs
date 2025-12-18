using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Windows.Media;

namespace ShoesShop.Pages
{
    public partial class AdminPage : Page
    {
        private bool isProductMode = true;

        public AdminPage()
        {
            InitializeComponent();
            LoadData();
            LoadFilters();
        }

        private void LoadData()
        {
            var products = Entities.GetContext().Product
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Include(p => p.Dealer)
                .ToList();
            lvProducts.ItemsSource = products;

            var orders = Entities.GetContext().Order
                .Include(o => o.OrderProduct.Select(op => op.Product))
                .Include(o => o.OrderStatus)
                .Include(o => o.PickUpPoint)
                .Include(o => o.User)
                .ToList();
            lvOrders.ItemsSource = orders;
        }

        private void LoadFilters()
        {
            cmbCategory.ItemsSource = Entities.GetContext().Category.ToList();
            cmbDealer.ItemsSource = Entities.GetContext().Dealer.ToList();
            cmbProducer.ItemsSource = Entities.GetContext().Producer.ToList();

            List<string> sorting = new List<string>
            {
                "Без сортировки",
                "По возрастанию цены",
                "По убыванию цены",
                "По возрастанию количества",
                "По убыванию количества"
            };
            cmbSort.ItemsSource = sorting;
            cmbSort.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            var products = Entities.GetContext().Product
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Include(p => p.Dealer)
                .AsEnumerable();

            if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem is Category selectedCategory)
            {
                products = products.Where(p => p.categoryId == selectedCategory.id);
            }

            if (cmbDealer.SelectedItem != null && cmbDealer.SelectedItem is Dealer selectedDealer)
            {
                products = products.Where(p => p.dealerId == selectedDealer.id);
            }

            if (cmbProducer.SelectedItem != null && cmbProducer.SelectedItem is Producer selectedProducer)
            {
                products = products.Where(p => p.producerId == selectedProducer.id);
            }

            if (!string.IsNullOrEmpty(tbSearch.Text))
            {
                string searchText = tbSearch.Text.ToLower();
                products = products.Where(p =>
                    p.name.ToLower().Contains(searchText) ||
                    p.description.ToLower().Contains(searchText) ||
                    p.articul.ToLower().Contains(searchText));
            }

            if (cmbSort.SelectedItem != null)
            {
                string sortOption = cmbSort.SelectedItem.ToString();

                switch (sortOption)
                {
                    case "По возрастанию цены":
                        products = products.OrderBy(p => p.price);
                        break;
                    case "По убыванию цены":
                        products = products.OrderByDescending(p => p.price);
                        break;
                    case "По возрастанию количества":
                        products = products.OrderBy(p => int.TryParse(p.stockAmount, out int amount) ? amount : 0);
                        break;
                    case "По убыванию количества":
                        products = products.OrderByDescending(p => int.TryParse(p.stockAmount, out int amount) ? amount : 0);
                        break;
                    default:
                        break;
                }
            }

            lvProducts.ItemsSource = products.ToList();
        }

        private void cmbProducer_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void cmbDealer_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbProducer.SelectedItem = null;
            cmbDealer.SelectedItem = null;
            cmbCategory.SelectedItem = null;
            cmbSort.SelectedIndex = 0;
            tbSearch.Text = "";
            LoadData();
        }

        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            isProductMode = true;
            lvOrders.Visibility = Visibility.Collapsed;
            lvProducts.Visibility = Visibility.Visible;
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            isProductMode = false;
            lvOrders.Visibility = Visibility.Visible;
            lvProducts.Visibility = Visibility.Collapsed;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvProducts.Visibility == Visibility.Visible)
            {
                NavigationService.Navigate(new ProductPage());
            }
            else if (lvOrders.Visibility == Visibility.Visible)
            {
                NavigationService.Navigate(new OrderPage());
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvProducts.Visibility == Visibility.Visible && lvProducts.SelectedItem != null)
            {
                NavigationService.Navigate(new ProductPage(lvProducts.SelectedItem as Product));
            }
            else if (lvOrders.Visibility == Visibility.Visible && lvOrders.SelectedItem != null)
            {
                NavigationService.Navigate(new OrderPage(lvOrders.SelectedItem as Order));
            }
            else
            {
                MessageBox.Show("Выберите элемент для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void lvOrders_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var dependencyObject = (DependencyObject)e.OriginalSource;
                var lvItem = FindAncestor<ListViewItem>(dependencyObject);

                if (lvItem != null)
                {
                    var order = lvOrders.ItemContainerGenerator.ItemFromContainer(lvItem) as Order;
                    var result = MessageBox.Show($"Удалить заказ от {order.creationDate:dd.MM.yyyy}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var orderProducts = Entities.GetContext().OrderProduct
                            .Where(op => op.orderId == order.id)
                            .ToList();

                        Entities.GetContext().OrderProduct.RemoveRange(orderProducts);
                        Entities.GetContext().Order.Remove(order);
                        Entities.GetContext().SaveChanges();

                        LoadData();
                        MessageBox.Show("Заказ удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvProducts_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var dependencyObject = (DependencyObject)e.OriginalSource;
                var lvItem = FindAncestor<ListViewItem>(dependencyObject);

                if (lvItem != null)
                {
                    var product = lvProducts.ItemContainerGenerator.ItemFromContainer(lvItem) as Product;
                    var result = MessageBox.Show($"Удалить товар '{product.name}' и все связанные с ним записи из заказов?", "Подтверждение удаления",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var hasOrders = Entities.GetContext().OrderProduct
                            .Where(op=>op.productId == product.id)
                            .Join(Entities.GetContext().Order,
                            op => op.orderId,
                            o => o.id,
                            (op, o)=> o)
                            .Any(o=>o.statusId == 2);

                        if (hasOrders)
                        {
                            MessageBox.Show("Невозможно удалить товар, так как он есть в активных заказах",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var orderProducts = Entities.GetContext().OrderProduct
                            .Where(op => op.productId == product.id)
                            .ToList();

                        if (orderProducts.Any())
                        {
                            Entities.GetContext().OrderProduct.RemoveRange(orderProducts);
                        }

                        Entities.GetContext().Product.Remove(product);
                        Entities.GetContext().SaveChanges();
                        LoadData();
                        MessageBox.Show("Товар удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvProducts.SelectedItem != null)
            {
                NavigationService.Navigate(new ProductPage(lvProducts.SelectedItem as Product));
            }
        }

        private void lvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvOrders.SelectedItem != null)
            {
                NavigationService.Navigate(new OrderPage(lvOrders.SelectedItem as Order));
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T t)
                {
                    return t;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }
    }
}
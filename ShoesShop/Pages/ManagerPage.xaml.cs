using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ShoesShop.Pages
{
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            LoadProducts();
            LoadOrders();
            LoadFilters();
        }

        private void LoadProducts()
        {
            lvProducts.ItemsSource = Entities.GetContext().Product
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Include(p => p.Dealer)
                .ToList();
        }

        private void LoadOrders()
        {
            lvOrders.ItemsSource = Entities.GetContext().Order
                .Include(o => o.OrderProduct.Select(op => op.Product))
                .Include(o => o.OrderStatus)
                .Include(o => o.PickUpPoint)
                .ToList();
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
            LoadProducts();
        }

        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            lvOrders.Visibility = Visibility.Collapsed;
            lvProducts.Visibility = Visibility.Visible;
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            lvOrders.Visibility = Visibility.Visible;
            lvProducts.Visibility = Visibility.Collapsed;
        }

        private void btnCreation_Click(object sender, RoutedEventArgs e)
        {
            if(lvOrders.Visibility != Visibility.Collapsed)
            {
                if(lvOrders.SelectedItem == null)
                {
                    NavigationService.Navigate(new OrderPage());
                }
            }
            else if(lvProducts.Visibility != Visibility.Collapsed) 
            {
                if (lvProducts.SelectedItem == null)
                {
                    NavigationService.Navigate(new ProductPage());
                }
            }
        }

        private void lvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvProducts.SelectedItem != null)
            {
                NavigationService.Navigate(new ProductPage());
            }
        }

        private void lvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvOrders.SelectedItem != null)
            {
                NavigationService.Navigate(new OrderPage());
            }
        }
    }
}
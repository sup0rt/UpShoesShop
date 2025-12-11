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

namespace ShoesShop.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();
            lvProducts.ItemsSource = Entities.GetContext().Product.ToList();
            cmbCategory.ItemsSource = Entities.GetContext().Category.ToList();
            cmbDealer.ItemsSource = Entities.GetContext().Dealer.ToList();
            cmbProducer.ItemsSource = Entities.GetContext().Producer.ToList();
        }

        private void UpdateList()
        {
            List<Product> products = Entities.GetContext().Product.ToList();

            if(cmbCategory.SelectedItem != null)
            {
                products = products.Where(p=>p.categoryId == cmbCategory.SelectedIndex + 1).ToList();
            }

            if(cmbDealer.SelectedItem != null)
            {
                products = products.Where(p=>p.dealerId == cmbDealer.SelectedIndex + 1).ToList();
            }

            if(cmbProducer.SelectedItem != null)
            {
                products = products.Where(p=>p.producerId == cmbProducer.SelectedIndex + 1).ToList();
            }

            if(!string.IsNullOrEmpty(tbSearch.Text))
            {
                products = products.Where(p=>p.name.ToLower().Contains(tbSearch.Text.ToLower())).ToList();
            }

            lvProducts.ItemsSource = products;
        }

        private void cmbProducer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void cmbDealer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateList();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cmbProducer.SelectedItem = null;
            cmbDealer.SelectedItem = null;
            cmbCategory.SelectedItem = null;
            cmbSort.SelectedItem = null;
            tbSearch.Text = "";
            lvProducts.ItemsSource = Entities.GetContext().Product.ToList();
        }
    }
}

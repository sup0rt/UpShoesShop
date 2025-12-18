using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ShoesShop.Pages
{
    public partial class ProductPage : Page
    {
        private Product currentProduct;
        private bool isEditMode = false;

        public ProductPage()
        {
            InitializeComponent();
            currentProduct = new Product();
            isEditMode = false;
            LoadComboBoxes();
        }

        public ProductPage(Product product)
        {
            InitializeComponent();
            currentProduct = product;
            isEditMode = true;
            LoadComboBoxes();
            BindProductData();
        }

        private void LoadComboBoxes()
        {
            var context = Entities.GetContext();

            cmbCategory.ItemsSource = context.Category.ToList();
            cmbDealer.ItemsSource = context.Dealer.ToList();
            cmbProducer.ItemsSource = context.Producer.ToList();

            if (isEditMode)
            {
                cmbCategory.SelectedValue = currentProduct.categoryId;
                cmbDealer.SelectedValue = currentProduct.dealerId;
                cmbProducer.SelectedValue = currentProduct.producerId;
            }
        }

        private void BindProductData()
        {
            tbArticul.Text = currentProduct.articul;
            tbName.Text = currentProduct.name;
            tbMeasureUnit.Text = currentProduct.measureUnit;
            tbPrice.Text = currentProduct.price.ToString(); 

            tbDiscount.Text = currentProduct.activeDiscount?.ToString() ?? "";
            tbStock.Text = currentProduct.stockAmount;
            tbDesc.Text = currentProduct.description;
            tbPhoto.Text = currentProduct.photo;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateProduct())
                return;

            try
            {
                SaveProduct();
                MessageBox.Show("Товар сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateProduct()
        {
            if (string.IsNullOrWhiteSpace(tbArticul.Text))
            {
                MessageBox.Show("Введите артикул", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbMeasureUnit.Text))
            {
                MessageBox.Show("Введите единицу измерения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(tbPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbDealer.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbProducer.SelectedItem == null)
            {
                MessageBox.Show("Выберите производителя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(tbDiscount.Text))
            {
                if (!decimal.TryParse(tbDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Скидка должна быть от 0 до 100%", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(tbStock.Text))
            {
                MessageBox.Show("Введите остаток", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void SaveProduct()
        {
            var context = Entities.GetContext();

            currentProduct.articul = tbArticul.Text.Trim();
            currentProduct.name = tbName.Text.Trim();
            currentProduct.measureUnit = tbMeasureUnit.Text.Trim();

            if (decimal.TryParse(tbPrice.Text, out decimal price))
                currentProduct.price = price;

            if (decimal.TryParse(tbDiscount.Text, out decimal discount))
                currentProduct.activeDiscount = discount;
            else
                currentProduct.activeDiscount = null;

            currentProduct.stockAmount = tbStock.Text.Trim();
            currentProduct.description = tbDesc.Text.Trim();
            currentProduct.photo = tbPhoto.Text.Trim();

            currentProduct.categoryId = (int)cmbCategory.SelectedValue;
            currentProduct.dealerId = (int)cmbDealer.SelectedValue;
            currentProduct.producerId = (int)cmbProducer.SelectedValue;

            if (!isEditMode)
            {
                context.Product.Add(currentProduct);
            }
            else
            {
                context.Entry(currentProduct).State = EntityState.Modified;
            }

            context.SaveChanges();
        }
    }
}
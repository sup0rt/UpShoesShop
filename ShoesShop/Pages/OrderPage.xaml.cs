using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ShoesShop.Pages
{
    public partial class OrderPage : Page
    {
        private Order currentOrder;
        private bool isEditMode = false;
        private List<OrderProductViewModel> orderProducts = new List<OrderProductViewModel>();

        public OrderPage(Order order = null)
        {
            InitializeComponent();

            if (order != null)
            {
                // Режим редактирования
                currentOrder = order;
                isEditMode = true;
                LoadOrderData();
            }
            else
            {
                // Режим создания - НЕ создаем id вручную!
                currentOrder = new Order
                {
                    creationDate = DateTime.Now
                };
            }

            LoadComboBoxes();
            LoadProducts();
            UpdateTotals();
        }

        private void LoadOrderData()
        {
            dpCreationDate.SelectedDate = currentOrder.creationDate;
            dpDeliveryDate.SelectedDate = currentOrder.deliveryDate;
            tbPickUpAdress.Text = currentOrder.PickUpPoint?.adress;
            tbPickUpCode.Text = currentOrder.pickUpCode;

            cmbClient.SelectedValue = currentOrder.clientId; // Изменено с userId на clientId
            cmbStatus.SelectedValue = currentOrder.statusId;
        }

        private void LoadComboBoxes()
        {
            cmbClient.ItemsSource = Entities.GetContext().User.ToList();
            cmbStatus.ItemsSource = Entities.GetContext().OrderStatus.ToList();
        }

        private void LoadProducts()
        {
            var context = Entities.GetContext();

            // Все товары
            var allProducts = context.Product
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .ToList();

            // Существующие товары в заказе
            List<OrderProduct> existingOrderProducts = new List<OrderProduct>();

            if (isEditMode)
            {
                existingOrderProducts = context.OrderProduct
                    .Where(op => op.orderId == currentOrder.id)
                    .Include(op => op.Product)
                    .ToList();
            }

            // Создаем ViewModel для каждого товара
            orderProducts.Clear();

            foreach (var product in allProducts)
            {
                var existingItem = existingOrderProducts.FirstOrDefault(op => op.productId == product.id);
                int quantity = existingItem?.quantity ?? 0;
                int maxQuantity = GetMaxQuantity(product);

                orderProducts.Add(new OrderProductViewModel
                {
                    Product = product,
                    Quantity = quantity,
                    MaxQuantity = maxQuantity,
                    MaxQuantityReached = quantity >= maxQuantity
                });
            }

            dgOrderProducts.ItemsSource = orderProducts;
        }

        private int GetMaxQuantity(Product product)
        {
            if (int.TryParse(product.stockAmount, out int stock))
                return stock;
            return 0;
        }

        private void UpdateTotals()
        {
            int totalItems = orderProducts.Sum(op => op.Quantity);

            // price - decimal (не nullable), поэтому просто умножаем
            decimal totalSum = orderProducts.Sum(op => op.Quantity * op.Product.price);

            tbTotalItems.Text = totalItems.ToString();
            tbTotalSum.Text = totalSum.ToString("N2") + " ₽";
        }

        // ========== ОБРАБОТЧИКИ КНОПОК +/- ==========
        private void btnIncrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is OrderProductViewModel vm)
            {
                if (vm.Quantity < vm.MaxQuantity)
                {
                    vm.Quantity++;
                    vm.MaxQuantityReached = vm.Quantity >= vm.MaxQuantity;
                    dgOrderProducts.Items.Refresh();
                    UpdateTotals();
                }
            }
        }

        private void btnDecrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is OrderProductViewModel vm)
            {
                if (vm.Quantity > 0)
                {
                    vm.Quantity--;
                    vm.MaxQuantityReached = vm.Quantity >= vm.MaxQuantity;
                    dgOrderProducts.Items.Refresh();
                    UpdateTotals();
                }
            }
        }

        // ========== СОХРАНЕНИЕ ==========
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateOrder())
                return;

            if (!orderProducts.Any(op => op.Quantity > 0))
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                SaveOrder();
                MessageBox.Show("Заказ сохранен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateOrder()
        {
            if (dpCreationDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату создания", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbPickUpAdress.Text))
            {
                MessageBox.Show("Введите адрес ПВЗ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbPickUpCode.Text))
            {
                MessageBox.Show("Введите код получения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void SaveOrder()
        {
            var context = Entities.GetContext();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Сохраняем/обновляем PickUpPoint
                    var pickUpPoint = context.PickUpPoint
                        .FirstOrDefault(p => p.adress == tbPickUpAdress.Text);

                    if (pickUpPoint == null)
                    {
                        pickUpPoint = new PickUpPoint
                        {
                            adress = tbPickUpAdress.Text.Trim()
                        };
                        context.PickUpPoint.Add(pickUpPoint);
                        context.SaveChanges();
                    }

                    // 2. Заполняем данные заказа
                    currentOrder.creationDate = dpCreationDate.SelectedDate.Value;
                    currentOrder.deliveryDate = dpDeliveryDate.SelectedDate;
                    currentOrder.pickUpCode = tbPickUpCode.Text.Trim();

                    // ВАЖНО: приводим к int (SelectedValue возвращает object)
                    currentOrder.clientId = (int)cmbClient.SelectedValue;
                    currentOrder.statusId = (int)cmbStatus.SelectedValue;

                    currentOrder.pickUpPointId = pickUpPoint.id;

                    // 3. Сохраняем заказ
                    if (!isEditMode)
                    {
                        context.Order.Add(currentOrder);
                    }
                    else
                    {
                        context.Entry(currentOrder).State = EntityState.Modified;
                    }
                    context.SaveChanges();

                    // 4. Сохраняем состав заказа
                    SaveOrderProducts(context);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void SaveOrderProducts(Entities context)
        {
            // Удаляем старые связи
            if (isEditMode)
            {
                var oldProducts = context.OrderProduct
                    .Where(op => op.orderId == currentOrder.id)
                    .ToList();
                context.OrderProduct.RemoveRange(oldProducts);
            }

            // Добавляем новые связи
            foreach (var vm in orderProducts.Where(vm => vm.Quantity > 0))
            {
                var orderProduct = new OrderProduct
                {
                    orderId = currentOrder.id,
                    productId = vm.Product.id,
                    quantity = vm.Quantity
                };

                context.OrderProduct.Add(orderProduct);
            }

            context.SaveChanges();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС ==========
        public class OrderProductViewModel
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public int MaxQuantity { get; set; }
            public bool MaxQuantityReached { get; set; }
        }
    }
}
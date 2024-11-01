﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Exam.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class MakeOrderPage : Page
    {
        public static int OrderProductsCount;

        public static decimal OrderCost;

        public static decimal OrderDiscount;

        public static List<ExamProduct> examOrderList = new();

        public static List<ExamPickupPoint> examPickupPoints = new();

        public static List<int> existingPickupCodes = new();

        public MakeOrderPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateOrderList();
            DataAccessLayer.GetPickupPoints();

            //вывод информации о пользователе
            if (CurrentUser.IsGuest)
                CurrentUserLabel.Content = "Вы вошли как гость";
            else
                CurrentUserLabel.Content = $"{CurrentUser.UserName.Substring(0, 1)}.{CurrentUser.UserPatronymic.Substring(0, 1)}. {CurrentUser.UserSurname}";

            PickupPointsComboBox.ItemsSource = examPickupPoints;
        }

        private void CreateOrderList() // метод для вывода товаров в корзине на страницу
        {
            productsInOrderStackPanel.Children.Clear();
            int productsCount = examOrderList.Count;
            for (int i = 0; i < productsCount; i++) // создаем отдельные элементы с данными о товарах
            {
                Border productBorder = new Border
                {
                    Width = 600,
                    Margin = new Thickness(80, 5, 0, 5),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC6600")),
                    BorderThickness = new Thickness(3)
                };

                StackPanel productPanel = new StackPanel
                {
                    Tag = i, // Устанавливаем индекс в Tag для последующего удаления
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFCC99"))
                };

                Image productImage = new Image
                {
                    Source = new BitmapImage(new Uri(examOrderList[i].ProductPhoto)),
                    Width = 200,
                    Height = 200
                };
                productPanel.Children.Add(productImage);

                StackPanel articleNumberPanel = new StackPanel { Orientation = Orientation.Horizontal };
                Label articleNumberLabel = new Label
                {
                    Content = "Артикул:",
                    FontFamily = new FontFamily("Comic Sans MS")
                };
                Label articleDataLabel = new Label
                {
                    Content = examOrderList[i].ProductArticleNumber
                };
                articleNumberPanel.Children.Add(articleNumberLabel);
                articleNumberPanel.Children.Add(articleDataLabel);
                productPanel.Children.Add(articleNumberPanel);

                Label nameDataLabel = new Label { Content = examOrderList[i].ProductName };
                productPanel.Children.Add(nameDataLabel);

                Label desciptionDataLabel = new Label { Content = examOrderList[i].ProductDescription };
                productPanel.Children.Add(desciptionDataLabel);

                StackPanel categoryPanel = new StackPanel { Orientation = Orientation.Horizontal };
                Label categoryLabel = new Label
                {
                    Content = "Категория товара:",
                    FontFamily = new FontFamily("Comic Sans MS")
                };
                Label categoryDataLabel = new Label { Content = examOrderList[i].ProductCategory };
                categoryPanel.Children.Add(categoryLabel);
                categoryPanel.Children.Add(categoryDataLabel);
                productPanel.Children.Add(categoryPanel);

                StackPanel manufacturerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                Label manufacturerLabel = new Label
                {
                    Content = "Производитель товара:",
                    FontFamily = new FontFamily("Comic Sans MS")
                };
                Label manufacturerDataLabel = new Label { Content = examOrderList[i].ProductManufacturer };
                manufacturerPanel.Children.Add(manufacturerLabel);
                manufacturerPanel.Children.Add(manufacturerDataLabel);
                productPanel.Children.Add(manufacturerPanel);

                DockPanel costDockPanel = new DockPanel();
                Label costLabel = new Label
                {
                    Content = "Цена товара:",
                    FontFamily = new FontFamily("Comic Sans MS")
                };
                TextBlock costDataTextBlock = new TextBlock
                {
                    Text = examOrderList[i].ProductCost.ToString()
                };
                Label discountLabel = new Label
                {
                    Content = "Скидка:",
                    FontFamily = new FontFamily("Comic Sans MS"),
                    FontSize = 12
                };
                Label discountDataLabel = new Label
                {
                    FontSize = 12,
                    Content = examOrderList[i].ProductDiscountAmount
                };
                costDockPanel.Children.Add(costLabel);
                costDockPanel.Children.Add(discountDataLabel);
                DockPanel.SetDock(discountDataLabel, Dock.Right);
                costDockPanel.Children.Add(discountLabel);
                DockPanel.SetDock(discountLabel, Dock.Right);

                if (examOrderList[i].ProductDiscountAmount > 0)
                {
                    costDataTextBlock.TextDecorations = TextDecorations.Strikethrough;
                    costDataTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    Label costWithDiscountDataLabel = new Label();
                    decimal resultCost = (decimal)Convert.ToDouble(costDataTextBlock.Text) * (100 - Convert.ToInt32(discountDataLabel.Content)) / 100;
                    costWithDiscountDataLabel.Content = resultCost;
                    costDockPanel.Children.Add(costWithDiscountDataLabel);
                }
                costDockPanel.Children.Add(costDataTextBlock);
                productPanel.Children.Add(costDockPanel);

                // Добавляем TextBox для изменения количества товара
                StackPanel quantityPanel = new StackPanel { Orientation = Orientation.Horizontal };
                Label quantityLabel = new Label
                {
                    Content = "Количество:",
                    FontFamily = new FontFamily("Comic Sans MS")
                };
                TextBox quantityTextBox = new TextBox
                {
                    Width = 50,
                    Text = examOrderList[i].ProductCountInOrder.ToString(), // Устанавливаем текущее количество
                    Tag = i // Сохраняем индекс для обработки события
                };
                quantityTextBox.TextChanged += QuantityTextBox_TextChanged;
                quantityPanel.Children.Add(quantityLabel);
                quantityPanel.Children.Add(quantityTextBox);
                productPanel.Children.Add(quantityPanel);

                Button deleteButton = new Button
                {
                    Content = "Удалить",
                    Tag = i // Присваиваем индекс в Tag для использования в DeleteButton_Click
                };
                deleteButton.Click += DeleteButton_Click;
                productPanel.Children.Add(deleteButton);

                productBorder.Child = productPanel;
                productsInOrderStackPanel.Children.Add(productBorder);
            }
        }

        // Обработчик изменения количества товара
        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox quantityTextBox = sender as TextBox;
            int productIndex = (int)quantityTextBox.Tag;

            if (int.TryParse(quantityTextBox.Text, out int newQuantity) && newQuantity >= 0)
            {
                examOrderList[productIndex].ProductCountInOrder = newQuantity;

                UpdateProductsCount();
                UpdateCost();
                UpdateDiscount();

                // Удаляем товар, если количество стало 0
                if (newQuantity == 0)
                {
                    productsInOrderStackPanel.Children.RemoveAt(productIndex);
                    examOrderList.RemoveAt(productIndex);
                    CreateOrderList(); // Перестраиваем список товаров
                }
            }
        }

        private void UpdateProductsCount()//Обновление количества товаров в корзине
        {
            int productsCount = examOrderList.Count;
            OrderProductsCount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                OrderProductsCount += examOrderList[i].ProductCountInOrder;
                CountProductsInOrderLabel.Content = OrderProductsCount.ToString();
            }
            if (examOrderList.Count == 0)
                CountProductsInOrderLabel.Content = 0;
        }

        private void UpdateDiscount()//обновление общей скидки товаров в корзине
        {
            int productsCount = examOrderList.Count;
            OrderDiscount = 0;
            for (int i = 0; i < productsCount; i++)
            {
                OrderDiscount += (examOrderList[i].ProductCost - (examOrderList[i].ProductCost) * (100 - examOrderList[i].ProductDiscountAmount) / 100) * examOrderList[i].ProductCountInOrder;
                OrderDiscountLabel.Content = OrderDiscount.ToString("F2") + " руб.";
            }
            if (examOrderList.Count == 0)
                OrderDiscountLabel.Content = 0;
        }

        private void UpdateCost()//обновление общей стоимости товаров в корзине
        {
            int productsCount = examOrderList.Count;
            OrderCost = 0;
            for (int i = 0; i < productsCount; i++)
            {
                OrderCost += Convert.ToDecimal(Convert.ToDouble(examOrderList[i].ProductCost) * (100 - examOrderList[i].ProductDiscountAmount) / 100) * examOrderList[i].ProductCountInOrder;
                OrderCostLabel.Content = OrderCost.ToString("F2") + " руб.";
            }
            if (examOrderList.Count == 0)
                OrderCostLabel.Content = 0;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Убедимся, что отправитель события — это кнопка и у нее есть Tag
                if (sender is Button deleteButton && deleteButton.Tag is int index && index >= 0 && index < examOrderList.Count)
                {
                    examOrderList.RemoveAt(index);

                    // Обновление списка товаров на странице
                    CreateOrderList();
                }
                else
                {
                    MessageBox.Show("Ошибка: элемент не найден или индекс неверен.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при удалении товара: " + ex.Message);
            }
        }

        private void CountControl_ValueChanged(object sender, RoutedEventArgs e)//изменение количества штук определенного товара в корзине
        {
            CountControl countControl = sender as CountControl;
            examOrderList[Convert.ToInt32(countControl.Tag)].ProductCountInOrder = countControl.Value;
            UpdateProductsCount();
            UpdateCost();
            UpdateDiscount();
            if (countControl.Value == 0)//если ставим количество 0, то товар удаляется
            {
                DockPanel countPanel = countControl.Parent as DockPanel;
                StackPanel productPanel = countPanel.Parent as StackPanel;
                StackPanel productsInOrderStackPanel = productPanel.Parent as StackPanel;
                examOrderList.RemoveAt((int)productPanel.Tag);
                productsInOrderStackPanel?.Children.Remove(productPanel);
                CreateOrderList();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)//возврат на страницу магазинна
        {
            NavigationService.GoBack();
        }

        private void MakeOrderButton_Click(object sender, RoutedEventArgs e)//создание записи о товаре в БД
        {
            if (examOrderList.Count != 0)
            {
                if (PickupPointsComboBox.SelectedItem != null)
                {
                    //получение даты доставки
                    Random rnd = new Random();
                    int travelDays = rnd.Next(3, 8); // Генерирует число от 3 до 7
                    DateTime currentDate = DateTime.Now;
                    DateTime deliveryDate = currentDate.AddDays(travelDays);

                    //получение кода выдачи
                    int pickupCode;
                    do
                    {
                        pickupCode = rnd.Next(1, 10001); // Генерирует число от 1 до 10000
                    }
                    while (existingPickupCodes.Contains(pickupCode));

                    ExamOrder newOrder = new ExamOrder()//создание элемента класса ExamOrder
                    {
                        OrderID = DataAccessLayer.GetLastOrderID(),
                        OrderStatus = "Новый",
                        OrderDate = currentDate,
                        OrderDeliveryDate = deliveryDate,
                        OrderPickupPoint = examPickupPoints[PickupPointsComboBox.SelectedIndex].OrderPickupPoint,
                        OrderPickupCode = pickupCode
                    };

                    DataAccessLayer.UpdateExamOrder(CurrentUser.UserID, newOrder.OrderStatus, newOrder.OrderDate, newOrder.OrderDeliveryDate, newOrder.OrderPickupPoint, newOrder.OrderPickupCode);

                    //добавление записей о товарах в новом заказе в БД
                    for (int i = 0; i < examOrderList.Count; i++)
                    {
                        DataAccessLayer.UpdateExamOrderProduct(DataAccessLayer.GetLastOrderID(), examOrderList[i].ProductArticleNumber, examOrderList[i].ProductCountInOrder);
                    }

                    //если пользователь вошел как гость, то создается отдельная запись о его заказе, которую он может получить пока не вышел из приложения,
                    //тк у него нет никаких данных чтобы этот заказ подтвердить, пусть печатает талончик
                    if (CurrentUser.IsGuest)
                    {
                        OrdersPage.createdByGuestOrdersList.Add(newOrder);
                    }

                    examOrderList.Clear();
                    productsInOrderStackPanel.Children.Clear();
                    NavigationService.Navigate(new OrdersPage());

                }
                else
                    WarnLabel.Content = "*Укажите пункт выдачи";
            }
            else WarnLabel.Content = "*Заказ не может быть пустым";
        }

        private void PickupPointsComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            WarnLabel.Content = string.Empty;
        }

        private void GoToYourOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage());
        }
    }
}

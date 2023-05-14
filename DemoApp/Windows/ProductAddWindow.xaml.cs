using DemoApp.Models;
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
using System.Windows.Shapes;

namespace DemoApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProductAddWindow.xaml
    /// </summary>
    public partial class ProductAddWindow : Window
    {
        public Product Product;
        public ProductAddWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using(var db = new DemoAppDBEntities())
            {
                cbxName.ItemsSource = db.ShopProduct.ToList();
                cbxName.DisplayMemberPath = "ProductName";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (cbxName.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите продукцию");
                return;
            }
            try
            {
                
                this.Product = new Product()
                {
                    ProductCount = Convert.ToInt32(txtCount.Text),
                    ProductCreationDate = DateTime.Parse(txtDate.Text),
                    ProductAgentID = 1,
                    ProductShopID = cbxName.SelectedIndex + 1
                  
                };
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("Неправильный формат значений полей");
            }
        }
    }
}

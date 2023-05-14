using DemoApp.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для MakeEditAgentWindow.xaml
    /// </summary>
    public partial class MakeEditAgentWindow : Window
    {
        Agent Agent;
        List<ProductObject> dataGridProducts;
        private List<Product> addedProductList = new List<Product>();

        public MakeEditAgentWindow(Agent agent)
        {
            InitializeComponent();
            Agent = agent;
            SetWindowData();
            loadData();
        }
        private void SetWindowData()
        {
            using (var db = new DemoAppDBEntities())
            {
                var typeList = db.AgentType.ToArray();
                AgentTypeComboBox.ItemsSource = typeList;
                AgentTypeComboBox.DisplayMemberPath = "AgentTypeName";
            }
        }

        private void loadData()
        {
            dataGridProducts = new List<ProductObject>();
            if (Agent == null)
            {
                btnRemoveProduct.Visibility = Visibility.Collapsed;
                Agent = new Agent();
                DataContext = Agent;
            }
            else
            {
                AgentTypeComboBox.SelectedIndex = Agent.AgentTypeID-1;
                using (var db = new DemoAppDBEntities())
                {
                    DataContext = this.Agent;
                    txtLogoStatus.Text = "Фото загружено";
                    int agentId = Agent.AgentID;
                    var _productList = (from p in db.Product select p).ToList();
                    foreach (var item in _productList)
                        if(item.ProductAgentID == agentId)
                            dataGridProducts.Add(new ProductObject() {Product=item, ShopProduct = db.ShopProduct.First(p=>p.ProductId==item.ProductShopID) });
                }
            }
            SalesHistoryDataGrid.ItemsSource = dataGridProducts;
        }

       
        private void ButtonLoadImg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            string folderpath = System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\agents\\";
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                        "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                        "Portable Network Graphic (*.png)|*.png";

            bool? myResult;
            myResult = op.ShowDialog();
            if (myResult != null && myResult == true)
            {
                string fileName = op.SafeFileName;
                string newFilePath = System.IO.Path.Combine(folderpath, fileName);

                File.Copy(op.FileName, newFilePath);

                this.Agent.AgentLogo = "\\agents\\" + fileName;
                txtLogoStatus.Text = "Фото загружено";
            }
            MessageBox.Show("Изображение загружено!");
        }

        private void btnRemoveAgent_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProducts.Count > 0)
            {
                MessageBox.Show("Агент имеет запись реализации продукции, его нельзя удалить!");
                return;
            }
            using (var db = new DemoAppDBEntities())
            {
                db.Entry(this.Agent).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
            }
            new MainAgentWindow().Show();
            this.Close();
        }

        private void btnAddAgent_Click(object sender, RoutedEventArgs e)
        {

            using (var db = new DemoAppDBEntities())
            {

                if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PriorityTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AddressTextBox.Text) ||
                    string.IsNullOrWhiteSpace(INNTextBox.Text) ||
                    string.IsNullOrWhiteSpace(KPPTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DirectorTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    MessageBox.Show("Не все поля заполнены!");
                    return;
                }
                try
                {
                    Convert.ToInt32(PriorityTextBox.Text);
                }
                catch (FormatException exp)
                {
                    MessageBox.Show("Поля имеют неверный формат!");
                    return;
                }
                if (AgentTypeComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите тип агента");
                    return;
                }
                Agent.AgentTypeID = AgentTypeComboBox.SelectedIndex + 1;
                if (Agent.AgentID == 0)
                {
                    var agent = db.Agent.Add(Agent);
                    foreach(var product in addedProductList)
                    {
                        product.ProductAgentID = agent.AgentID;
                        db.Product.Add(product);
                    }
                    db.SaveChanges();
                    MessageBox.Show("Агент добавлен");
                    new MainAgentWindow().Show();
                    this.Close();
                }
                else
                {
                    db.Entry(Agent).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    MessageBox.Show("Агент обновлен");
                    new MainAgentWindow().Show();
            this.Close();
                }
            }
        }

        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            var product = SalesHistoryDataGrid.SelectedItem as ProductObject;
            dataGridProducts.Remove(product);
            if (Agent.AgentID == 0)
            {
                addedProductList.Remove(product.Product);
            }
            else
            {
                using (var db = new DemoAppDBEntities())
                {
                    db.Entry(product.Product).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            SalesHistoryDataGrid.Items.Refresh();
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            var addProductWindow = new ProductAddWindow();
            using (var db = new DemoAppDBEntities())
            {
                if (addProductWindow.ShowDialog() == true)
                {
                    var product = addProductWindow.Product;
                    product.ProductAgentID = Agent.AgentID;
                    dataGridProducts.Add(new ProductObject() { Product = product, ShopProduct = db.ShopProduct.FirstOrDefault(p=>p.ProductId == product.ProductShopID)});
                    if (Agent.AgentID == 0)
                        addedProductList.Add(product);
                    else
                    {
                        db.Product.Add(product);
                        db.SaveChanges();
                    }
                    SalesHistoryDataGrid.Items.Refresh();
                }
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            new MainAgentWindow().Show();
            this.Close();
        }
        class ProductObject
        {
            public Product Product { get; set; }
            public ShopProduct ShopProduct { get; set; }
        }
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainAgentWindow : Window
    {
        List<DisplayAgent> agentSet = new List<DisplayAgent>();
        List<AgentType> agentsType;
        int currentPage = 1;
        string searchFilter = "";
        int sortMark;
        int maxPriority = 0, previousPriority=0;
        int filterType;
        string[] sorts = new string[] {
            "Наименование (по уб.)",
            "Наименование (по воз.)",
            "Скидка (по уб.)",
            "Скидка (по воз.)",
            "Приоритет (по уб.)",
            "Приоритет (по воз.)",
            "Все"};
        public MainAgentWindow()
        {
            InitializeComponent();
            SetWindowData();
            LoadDataSet();
            LoadData();
        }

        private void SetWindowData()
        {
            using (var db = new DemoAppDBEntities())
            {
                agentsType = db.AgentType.ToList();
                agentsType.Add(new AgentType() { AgentTypeID = 0, AgentTypeName = "Все" });
                comboBoxType.ItemsSource = agentsType;
                comboBoxType.DisplayMemberPath = "AgentTypeName";
                comboBoxType.SelectedIndex = agentsType.Count-1;

                comboBoxSort.ItemsSource = sorts;
                comboBoxSort.SelectedIndex = sorts.Length - 1;
            }
        }

        private void LoadData() 
        {
            agentPanel.Children.Clear();
            using (var db = new DemoAppDBEntities())
            {
                int i = 1;
                foreach (var a in agentSet)
                {
                    if (i >= (currentPage-1)*10 && i < (currentPage-1)*10 + 10) 
                    {
                        var agent = a.Agent;
                        var mainGrid = new Grid();
                        mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                        mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { });
                        mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });

                        var checkBox = new CheckBox() { Margin=new Thickness(5), Tag = agent};
                        checkBox.Checked += CheckBox_Checked;
                        checkBox.Unchecked += CheckBox_Unchecked;

                        var image = new Image() { Margin = new Thickness(10) };
                        BitmapImage bitmap;
                        var imagePath = agent.AgentLogo;
                        if (imagePath != null && !String.IsNullOrEmpty(imagePath))
                            bitmap = new BitmapImage(new Uri(System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\" + imagePath));
                        else
                            bitmap = new BitmapImage(new Uri(System.IO.Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Resources\\" + "picture.png"));
                        image.Source = bitmap;

                        var midPanel = new StackPanel() { Margin = new Thickness(10), Orientation = Orientation.Vertical };
                        var txtTypeName = new TextBlock() { FontSize = 16 };
                        var txtSels = new TextBlock() { FontSize = 14 };
                        var txtPhone = new TextBlock() { FontSize = 14 };
                        var txtPriority = new TextBlock() { FontSize = 14 };

                        var txtDiscount = new TextBlock() { FontSize = 18, FontWeight = FontWeights.Bold };

                        var agentType = db.AgentType.First(at => at.AgentTypeID == agent.AgentTypeID);
                        txtTypeName.Text = agentType.AgentTypeName + " | " + agent.AgentName;
                        int sels = (from p in db.Product where p.ProductAgentID == agent.AgentID select p.ProductCount).ToList().Sum();
                        txtSels.Text = $"{sels} продаж за год.";
                        txtPhone.Text = agent.AgentPhone;
                        txtPriority.Text = "Приоритетность: " + agent.AgentPriority;

                        txtDiscount.Text = a.Discount.ToString();

                        midPanel.Children.Add(txtTypeName);
                        midPanel.Children.Add(txtSels);
                        midPanel.Children.Add(txtPhone);
                        midPanel.Children.Add(txtPriority);

                        Grid.SetColumn(midPanel, 1);
                        Grid.SetColumn(txtDiscount, 2);

                        mainGrid.Children.Add(image);
                        mainGrid.Children.Add(checkBox);
                        mainGrid.Children.Add(midPanel);
                        mainGrid.Children.Add(txtDiscount);

                        ContextMenu contextMenu = new ContextMenu();
                        MenuItem addMenuItem = new MenuItem();
                        addMenuItem.Header = "Изменить";
                        addMenuItem.Tag = agent;
                        contextMenu.Items.Add(addMenuItem);
                        addMenuItem.Click += BtnEdit_Click;
                        mainGrid.ContextMenu = contextMenu;

                        agentPanel.Children.Add(mainGrid);
                    }
                    i++;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            maxPriority = previousPriority;
            txtPriority.Text = maxPriority.ToString();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var agent = (sender as CheckBox).Tag as Agent;
            
                if (agent.AgentPriority > maxPriority)
                {
                    previousPriority = maxPriority;
                    maxPriority = agent.AgentPriority;
                    txtPriority.Text = maxPriority.ToString();
                }
            
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            new MakeEditAgentWindow((sender as MenuItem).Tag as Agent).Show();
            this.Close();            
        }

        private void LoadDataSet() 
        {
            using (var db = new DemoAppDBEntities())
            {
                agentSet.Clear();
                foreach (var a in db.Agent.ToList())
                {
                    agentSet.Add(new DisplayAgent(a, 0));
                }
                if (searchFilter != "")
                    agentSet = agentSet.Where(a => a.Agent.AgentName.Contains(searchFilter) || a.Agent.AgentEmail.Contains(searchFilter) || a.Agent.AgentPhone.Contains(searchFilter)).ToList();
                if(filterType!= agentsType.Count - 1)
                {
                    agentSet = agentSet.Where(a => a.Agent.AgentTypeID == filterType+1).ToList();
                }
                switch (sortMark)
                {
                    case 0:
                        agentSet = agentSet.OrderByDescending(a => a.Agent.AgentName).ToList();
                        break;
                    case 1:
                        agentSet = agentSet.OrderBy(a => a.Agent.AgentName).ToList();
                        break;
                    case 2:
                        agentSet = agentSet.OrderByDescending(a => a.Discount).ToList();
                        break;
                    case 3:
                        agentSet = agentSet.OrderBy(a => a.Discount).ToList();
                        break;
                    case 4:
                        agentSet = agentSet.OrderByDescending(a => a.Agent.AgentPriority).ToList();
                        break;
                    case 5:
                        agentSet = agentSet.OrderBy(a => a.Agent.AgentPriority).ToList();
                        break;
                }

                btnNavigation.Children.Clear(); 
                int buttonCount = (int)Math.Ceiling((double)agentSet.Count / 10);
                if (buttonCount < currentPage)
                    currentPage = 1;
                for (int i = 0; i < buttonCount; i++)
                {
                    var btnPage = new Button() { Content = i + 1}; 
                    if(i==currentPage-1)
                        btnPage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9969E"));
                    btnPage.Click += BtnPage_Click;
                    btnNavigation.Children.Add(btnPage); 
                }
                        
            }
            

        }

        private void BtnPage_Click(object sender, RoutedEventArgs e)
        {
            currentPage = Convert.ToInt32((sender as Button).Content);
            LoadDataSet();
            LoadData();
        }

        private void comboBoxSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sortMark = (sender as ComboBox).SelectedIndex;
            LoadDataSet();
            LoadData();
        }

        private void comboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterType = (sender as ComboBox).SelectedIndex;
            LoadDataSet();
            LoadData();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchFilter = (sender as TextBox).Text;
            LoadDataSet();
            LoadData();
        }
        

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != 1)
            {
                currentPage--;
                LoadDataSet();
                LoadData();
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage != btnNavigation.Children.Count)
            {
                currentPage++;
                LoadDataSet();
                LoadData();
            }

        }

        class DisplayAgent
        {
            public readonly Agent Agent;
            public readonly int Discount;

            public DisplayAgent(Agent agent, int discount)
            {
                this.Agent = agent;
                this.Discount = discount;
            }
        }

        private void btnChangeAgent_Click(object sender, RoutedEventArgs e)
        {
            int priority;
            try
            {
                priority = Convert.ToInt32(txtPriority.Text);
            }
            catch
            {
                MessageBox.Show("Укажите приоритет");
                return;
            }
            foreach (var item in agentPanel.Children)
            {
                foreach (var child in (item as Grid).Children)
                {
                    if (child is CheckBox)
                    {
                        if ((child as CheckBox).IsChecked == true)
                        {
                            var agent = (child as CheckBox).Tag as Agent;
                            using (var db = new DemoAppDBEntities())
                            {
                                agent.AgentPriority = priority;
                                db.Entry(agent).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            LoadDataSet();
            LoadData();
        }

        private void btnAddAgent_Click(object sender, RoutedEventArgs e)
        {
            new MakeEditAgentWindow(null).Show();
            this.Close();
        }
    }
}

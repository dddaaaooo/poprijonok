using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
using WPFPoprijonok.Model;

namespace WPFPoprijonok.Pages
{
    public partial class AgentsGRUD : Page
    {
        private int start = 0;//Номер текущей страницы
        private int fullCount = 0;//Кооличество всех агентов

        private int order = 0;//Номер сортировки
        private int typeID = 0;//Номер выбранного типа агента
        private string fnd = "";//Строка поиска

        List<Model.Agent> agents = new List<Model.Agent>();//Список агентов

        private Frame frame;//Фрагмент для отображения страниц

        public AgentsGRUD(Frame fr)
        {
            InitializeComponent();
            frame = fr;
            //Подготовка списка типов агента для фильтрации
            List<Model.AgentType> types = new List<Model.AgentType> { };
            types = helper.GetContext().AgentType.ToList();
            types.Add(new Model.AgentType { Title = "Все типы" });
            Type.ItemsSource = types.OrderBy(AgentType => AgentType.ID);

            Load();
        }
        public void Load()
        {
            try
            {
                if (!agents.Any())
                {
                    //Получение списка всех агентов , подходящих под условия поиска
                    var ag = helper.GetContext().Agent.Where(Agent => Agent.Title.Contains(fnd) || Agent.Phone.Contains(fnd) || Agent.Email.Contains(fnd));

                    //Изменение количества продаж и процента скидки
                    foreach (Agent agent in ag)
                    {
                        if (agent.Logo == "отсутствует" || agent.Logo == null)
                            agent.Logo = "/agents/picture.png";

                        int sum = 0;
                        double fsum = 0;
                        /*
                        foreach (ProductSale ps in agent.ProductSale)
                        {
                            List<ProductMaterial> mtr = new List<ProductMaterial> { };
                            mtr = helper.GetContext().ProductMaterial.Where(ProductMaterial => ProductMaterial.ProductID == ps.ProductID).ToList();
                            foreach (ProductMaterial mt in mtr)
                            {
                                double f = decimal.ToDouble(mt.Material.Cost);
                                fsum += f * (double)mt.Count;
                            }
                            fsum = fsum * ps.ProductCount;
                            if (ps.SaleDate.AddDays(365).CompareTo(DateTime.Today) > 0)
                                sum += ps.ProductCount;
                        }
                        */
                        foreach (ProductSale ps in agent.ProductSale)
                        {
                            Product product = helper.GetContext().Product.FirstOrDefault(p => p.ID == ps.ProductID);
                            if (product != null)
                            {
                                double price = decimal.ToDouble(product.MinCostForAgent); // цена продукта
                                fsum += price * ps.ProductCount; // на количество продукта
                            }
                            //fsum = fsum * ps.ProductCount;
                            if (ps.SaleDate.AddDays(365).CompareTo(DateTime.Today) > 0)
                                sum += ps.ProductCount;
                        }

                        agent.sale = sum;
                        agent.fsale = fsum;
                        agent.percent = 0;
                        if (fsum >= 10000 && fsum < 50000) agent.percent = 5;
                        if (fsum >= 50000 && fsum < 150000) agent.percent = 10;
                        if (fsum >= 150000 && fsum < 500000) agent.percent = 20;
                        if (fsum >= 500000) agent.percent = 25;
                        agents.Add(agent);

                    }

                }

                //Вывод, если тип не выбран
                if (typeID == 0)
                {
                    fullCount = agents.Count();
                    full.Text = fullCount.ToString();
                    //Сортировка
                    if (order == 0) agentGrid.ItemsSource = agents.OrderBy(Agent => Agent.ID).Skip(start * 10).Take(10).ToList();
                    //По названию
                    if (order == 1) agentGrid.ItemsSource = agents.OrderBy(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                    if (order == 2) agentGrid.ItemsSource = agents.OrderByDescending(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                    //По приоритету
                    if (order == 3) agentGrid.ItemsSource = agents.OrderBy(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                    if (order == 4) agentGrid.ItemsSource = agents.OrderByDescending(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                    //По проценту скидки
                    if (order == 5) agentGrid.ItemsSource = agents.OrderBy(Agent => Agent.percent).Skip(start * 10).Take(10).ToList();
                    if (order == 6) agentGrid.ItemsSource = agents.OrderByDescending(Agent => Agent.percent).Skip(start * 10).Take(10).ToList();
                }

                //Вывод, если тип выбран
                else
                {
                    var agg = agents.Where((Agent => Agent.AgentTypeID == typeID));
                    fullCount = agg.Count();
                    full.Text = fullCount.ToString();
                    //Сортировка
                    if (order == 0) agentGrid.ItemsSource = agg.OrderBy(Agent => Agent.ID).Skip(start * 10).Take(10).ToList();
                    //По названию
                    if (order == 1) agentGrid.ItemsSource = agg.OrderBy(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                    if (order == 2) agentGrid.ItemsSource = agg.OrderByDescending(Agent => Agent.Title).Skip(start * 10).Take(10).ToList();
                    //По приоритету
                    if (order == 3) agentGrid.ItemsSource = agg.OrderBy(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                    if (order == 4) agentGrid.ItemsSource = agg.OrderByDescending(Agent => Agent.Priority).Skip(start * 10).Take(10).ToList();
                    //По проценту скидки
                    if (order == 5 || order == 6)
                    {
                        agents.Sort(Srt);
                        agentGrid.ItemsSource = agents.Skip(start * 10).Take(10).ToList();
                    }
                    //if (order == 5) agentGrid.ItemsSource = agg.OrderBy(Agent => Agent.percent).Skip(start * 10).Take(10).ToList();
                    //if (order == 6) agentGrid.ItemsSource = agg.OrderByDescending(Agent => Agent.percent).Skip(start * 10).Take(10).ToList();
                }
            }
            catch
            {
                return;
            };

            int ost = fullCount % 10;
            int pag = (fullCount - ost) / 10;
            if (ost > 0) pag++;
            pagin.Children.Clear();
            //Создание нужного количества кнопок для листания
            for (int i = 0; i < pag; i++)
            {
                Button myButton = new Button();
                myButton.Height = 30;
                myButton.Content = i + 1;
                myButton.Width = 20;
                myButton.HorizontalAlignment = HorizontalAlignment.Center;
                myButton.Tag = i;
                myButton.Click += new RoutedEventHandler(paginButton_Click); ;
                pagin.Children.Add(myButton);
            }
            turnButton();

            changeButtonColor();
        }

        private int Srt(Agent x, Agent y)
        {
            if (x.percent == y.percent) return 0;
            if (((order == 5) && (x.percent > y.percent)) || ((order == 6)) && (x.percent < y.percent)) return 1;
            return -1;
        }


        //Загрузка строк в таблицу, добавление информации о стоимости продаж и проценте
        private void agentGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            /*
            Model.Agent agent = (Model.Agent)e.Row.DataContext;
            if (agent.percent == 25)
            {
                SolidColorBrush hb = new SolidColorBrush(Colors.LightBlue);
                e.Row.Background = hb;
            }
            else
                e.Row.Background = new SolidColorBrush(Colors.White);
            */
        }

        private void turnButton()
        {
            if (start == 0) { back.IsEnabled = false; }
            else { back.IsEnabled = true; };
            if ((start + 1) * 10 >= fullCount) { forward.IsEnabled = false; }
            else { forward.IsEnabled = true; };
        }

        //Изменение цвета кнопки активной страницы
        private void changeButtonColor()
        {
            foreach (Button but in pagin.Children)
            {
                if (start == Convert.ToInt32(but.Tag.ToString()))
                {
                    but.Background = new SolidColorBrush(Color.FromArgb(200, 67, 220, 254)); ;
                }
            }
        }


        //Нажатие на кнопку переключения страниц
        private void paginButton_Click(object sender, RoutedEventArgs e)
        {
            start = Convert.ToInt32(((Button)sender).Tag.ToString());
            Load();
        }
        //Кнопка назад
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            start--;
            Load();
        }
        //Внопка вперед
        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            start++;
            Load();
        }


        //Выбор сортировки (по наименованию агента и приоритету)
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            order = Convert.ToInt32(selectedItem.Tag.ToString());
            agents.Clear();
            start = 0;
            Load();
        }
        //Выбор фильтрации (по типу агента)
        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            typeID = ((Model.AgentType)Type.SelectedItem).ID;
            agents.Clear();
            start = 0;
            Load();
        }
        //Поиск (по наименованию, телефону, email агента)
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fnd = ((TextBox)sender).Text;
            agents.Clear();
            start = 0;
            Load();
        }


        //Открытие страницы редактирования при нажатии на строку агента
        private void agentGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (agentGrid.SelectedItems.Count > 0)
            {
                Model.Agent agent = agentGrid.SelectedItems[0] as Model.Agent;

                if (agent != null)
                {
                    frame.Content = new addAgent(agent);
                }
            }
            agents.Clear();
        }

        //Добавление агента
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = new addAgent(null);
        }

        //Изменение приоритета агента

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (agentGrid.SelectedItems.Count > 0)
            {
                int prt = 0;
                foreach (Model.Agent agent in agentGrid.SelectedItems)
                {
                    if (agent.Priority > prt) prt = agent.Priority;
                }
                ChangePriory dlg = new ChangePriory(prt);
                helper.priority = prt;
                helper.flag = false;
                dlg.ShowDialog();
                if (helper.flag)
                {
                    foreach (Model.Agent agent in agentGrid.SelectedItems)
                    {
                        agent.Priority = helper.priority;
                        helper.GetContext().Entry(agent).State = EntityState.Modified;
                    }
                    helper.GetContext().SaveChanges();
                    Load();
                }
            }
        }
    }
}
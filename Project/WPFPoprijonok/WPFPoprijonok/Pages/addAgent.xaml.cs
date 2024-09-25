using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// <summary>
    /// Логика взаимодействия для addAgent.xaml
    /// </summary>
    public partial class addAgent : Page
    {
        private Agent agent; //Текущий агент

        private int currentProductSale; //Номер выбранной продажи товара
        private int currentTypeAgent; //Номер текущего агента

        public addAgent(Agent ag)
        {
            InitializeComponent();
            try
            {
                Type.ItemsSource = helper.GetContext().AgentType.ToList();
                product.ItemsSource = helper.GetContext().Product.ToList();
            }
            catch { };
            if (ag != null)
            {
                agent = ag;
                Type.SelectedItem = ag.AgentType;
                this.Title.Text = ag.Title;
                this.Adress.Text = ag.Address;
                this.Inn.Text = ag.INN;
                this.Kpp.Text = ag.KPP;
                this.Director.Text = ag.DirectorName;
                this.Phone.Text = ag.Phone;
                this.Email.Text = ag.Email;
                this.Logo.Text = ag.Logo;
                this.Prioritet.Text = ag.Priority.ToString();
                historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == ag.ID).ToList();

                btnDelAg.IsEnabled = true;
            }
            else
            {
                agent = new Model.Agent();
                btnDelAg.IsEnabled = false;
                btnWritHistory.IsEnabled = false;
                btnDelHistory.IsEnabled = false;
            }
            this.DataContext = agent;
        }
        //Добавление агента
        private void btnWriteAg_Click(object sender, RoutedEventArgs e)
        {
            //Проверка введенных данных
            if (this.Title.Text == "")
            {
                MessageBox.Show("Необходимо ввести наименование");
                return;
            }
            if (currentTypeAgent == 0)
            {
                MessageBox.Show("Необходимо выбрать тип");
                return;
            }
            if (!(new Regex(@"\d{10}|\d{12}")).IsMatch(this.Inn.Text))
            {
                MessageBox.Show("ИНН введен некорректно");
                return;
            }
            if (!(new Regex(@"\d{4}[\dA-Z][\dA-Z]\d{3}")).IsMatch(this.Kpp.Text))
            {
                MessageBox.Show("КПП введен некорректно");
                return;
            }
            if (!(new Regex(@"^\+?\d{0,2}\-?\d{3}\-?\d{3}\-?\d{4}")).IsMatch(this.Phone.Text))
            {
                MessageBox.Show("Телефон введен некорректно");
                return;
            }
            if ((this.Email.Text == "") && (!(new Regex(@"(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)")).IsMatch(this.Email.Text)))
            {
                MessageBox.Show("E-mail введен некорректно");
                return;
            }

            //Добавление или обновление информации об агенте
            agent.Title = this.Title.Text;
            agent.AgentTypeID = currentTypeAgent;
            agent.Address = this.Adress.Text;
            agent.INN = this.Inn.Text;
            agent.KPP = this.Kpp.Text;
            agent.Phone = this.Phone.Text;
            agent.DirectorName = this.Director.Text;
            agent.Phone = this.Phone.Text;
            agent.Email = this.Email.Text;
            if (Int32.TryParse(this.Prioritet.Text, out int prioritet))
            {
                agent.Priority = prioritet;
            }
            try
            {
                if (agent.ID > 0)
                {
                    helper.GetContext().Entry(agent).State = EntityState.Modified;
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Обновление информации об агенте завершено");
                }
                else
                {
                    helper.ent.Agent.Add(agent);
                    helper.ent.SaveChanges();
                    MessageBox.Show("Добавление информации об агенте завершено");
                }
            }
            catch { };
            btnDelAg.IsEnabled = true;
            btnWritHistory.IsEnabled = true;
            btnDelHistory.IsEnabled = true;
        }

        //Удаление агента
        private void btnDelAg_Click(object sender, RoutedEventArgs e)
        {
            if (agent.ProductSale.Count > 0)
            {
                MessageBox.Show("Удаление не возможно!");
                return;
            }
            foreach (Model.Shop shop in agent.Shop)
            {
                helper.GetContext().Shop.Remove(shop);
            }
            foreach (Model.AgentPriorityHistory apr in agent.AgentPriorityHistory)
            {
                helper.GetContext().AgentPriorityHistory.Remove(apr);
            }
            helper.GetContext().Agent.Remove(agent);
            helper.GetContext().SaveChanges();
            MessageBox.Show("Удаление информации об агенте завешено!");
            this.NavigationService.GoBack();
        }

        //Изменение типа агента
        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentTypeAgent = ((Model.AgentType)Type.SelectedItem).ID;
        }



        //Поиск названия продукции для ComboBox
        private void mask_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (product.SelectedIndex == -1)
            {
                string fnd = ((TextBox)sender).Text;

                try
                {
                    product.ItemsSource = helper.GetContext().Product.Where(Product => Product.Title.Contains(fnd)).ToList();
                }
                catch { return; };
            }
        }

        //Изменение выделения в таблице
        private void historyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enablingButtonsProduct();
        }

        //Изменение значения даты продажи
        private void date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            enablingButtonsProduct();
        }

        //Изменение значения количества продажи
        private void count_SelectionChanged(object sender, RoutedEventArgs e)
        {
            enablingButtonsProduct();
        }

        //Изменение значения продукции продажи
        private void product_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentProductSale = ((Model.Product)product.SelectedItem).ID;
            enablingButtonsProduct();
        }

        //Включение или выключение кнопок управления продажами
        private void enablingButtonsProduct()
        {
            //Кнопка добавления продажи
            if (currentProductSale > 0 && date.ToString() != "" && Int32.TryParse(count.Text, out int cnt))
                btnWritHistory.IsEnabled = true;
            else
                btnWritHistory.IsEnabled = false;

            //Кнопка удаления продажи
            if (historyGrid.SelectedIndex >= 0)
                btnDelHistory.IsEnabled = true;
            else
                btnDelHistory.IsEnabled = false;
        }


        //Добавление продажи агента
        private void btnWritHistory_Click(object sender, RoutedEventArgs e)
        {
            int cnt = Convert.ToInt32(count.Text);
            string dt = date.ToString();

            if (currentProductSale > 0 && dt != "" && cnt > 0)
            {
                Model.ProductSale pr = new Model.ProductSale();
                pr.AgentID = agent.ID;
                pr.ProductID = currentProductSale;
                pr.SaleDate = (DateTime)date.SelectedDate;
                pr.ProductCount = cnt;
                try
                {
                    helper.GetContext().ProductSale.Add(pr);
                    helper.GetContext().SaveChanges();
                    historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == agent.ID).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка при добавлении продажи: " + ex.Message);
                }
            }
        }

        //Удаление продажи агента
        private void btnDelHistory_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < historyGrid.SelectedItems.Count; i++)
            {
                Model.ProductSale prs = historyGrid.SelectedItems[i] as Model.ProductSale;
                if (prs != null)
                {
                    helper.GetContext().ProductSale.Remove(prs);
                }
            }
            try
            {
                helper.GetContext().SaveChanges();
                historyGrid.ItemsSource = helper.GetContext().ProductSale.Where(ProductSale => ProductSale.AgentID == agent.ID).ToList();
            }
            catch { return; };
        }
    }
}
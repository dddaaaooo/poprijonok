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

namespace WPFPoprijonok.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePriory.xaml
    /// </summary>
    public partial class ChangePriory : Window
    {
        public ChangePriory(int pr)
        {
            InitializeComponent();
            priority.Text = pr.ToString();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Int32.TryParse(priority.Text, out int pr) && pr > 0)
            {
                helper.flag = true;
                helper.priority = pr;
                this.Close();
            }
            else
                MessageBox.Show("Приоритет агента должен быть числом и больше 0!");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}

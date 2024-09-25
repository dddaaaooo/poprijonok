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
using WPFPoprijonok.Model;
using WPFPoprijonok.Pages;

namespace WPFPoprijonok
{
    public class helper
    {
        public static PoprijonokEntities ent;
        public static PoprijonokEntities GetContext()
        {
            if (ent == null)
            {
                ent = new PoprijonokEntities();
            }
            return ent;
        }
        public static bool flag = false;
        public static int priority =0;

    }


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frame.Content = new AgentsGRUD(frame);
        }

        private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            try
            {
                AgentsGRUD pg = (AgentsGRUD)e.Content;
                pg.Load();
            }
            catch { };

        }
    }
}

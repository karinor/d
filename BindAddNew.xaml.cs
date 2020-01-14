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

namespace MarcoMachine
{
    /// <summary>
    /// Логика взаимодействия для BindAddNew.xaml
    /// </summary>
    public partial class BindAddNew : Window
    {
        private Bind bind = new Bind();
        public BindAddNew(MainWindow window)
        {
            InitializeComponent();
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            bind.Events.Add(new Event(EventType.KeyDown, Key.A));
        }
    }
}

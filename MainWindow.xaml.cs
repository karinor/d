using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MarcoMachine
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        public void UpdateBindList(Bind[] binds)
        {
            foreach(Bind bind in binds)
                BindsListViev.Items.Add(bind);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Bind test_bind = new Bind("test", Key.C);
            //test_bind.Events.Add(new Event(EventType.KeyDown, Key.C));
            //test_bind.Events.Add(new Event(EventType.Delay, 30));
            //test_bind.Events.Add(new Event(EventType.KeyUp, Key.C));

            //Hook.BindExec.UpdateBindList(new Bind[] { test_bind }, BindRuntimeInfo.BindList);
        }

        private void BtnProcListUpdate_Click(object sender, RoutedEventArgs e)
        {
            Process[] procList = Process.GetProcesses();
            foreach (Process proc in procList)
            {
                string pName = proc.ProcessName;
                ComboBoxItem item = new ComboBoxItem();
                item.Content = pName;
                if (pName != "")
                    comboBoxProcSelect.Items.Add(pName);
            }
        }

        private void BtnProcListSelect_Click(object sender, RoutedEventArgs e)
        {
            string pname = comboBoxProcSelect.SelectedValue.ToString();
            Process[] procList = Process.GetProcesses();
            foreach (Process proc in procList)
            {
                if (pname == proc.ProcessName)
                {
                    BindRuntimeInfo test_strage = new BindRuntimeInfo();

                    Hook test = new Hook((uint)proc.Id, BindRuntimeInfo.BindList);
                    test.SetHook();
                }
            }
        }

        private void BtnBindAddNew_Click(object sender, RoutedEventArgs e)
        {
            BindAddNew addNewBind = new BindAddNew(this);
            addNewBind.ShowDialog();
            BindsListViev.Items.Add(new Bind("Jopa", Key.J));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindRuntimeInfo.link = this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RA1_2013.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangeType.xaml
    /// </summary>
    public partial class ChangeType : Window
    {

        Model.LokalType type;
        ObservableCollection<Model.Lokal> lokali_tip = new ObservableCollection<Model.Lokal>();
        Model.LokalType SelectedType;
        ObservableCollection<Model.LokalType> novi_tip = new ObservableCollection<Model.LokalType>();

        List<Model.Lokal> referenca_lokali;

        public ChangeType(Model.LokalType type, List<Model.Lokal> lokali_tip)
        {
            InitializeComponent();
            referenca_lokali = lokali_tip;
            this.type = type;

            Prikaz.ItemsSource = referenca_lokali;
        }
        

        private void ponisti_Click(object sender, RoutedEventArgs e)
        {
            foreach (Model.Lokal lokali in referenca_lokali)
                lokali.type = type;

            Close();
        }

        private void izbor_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TypeTable(type);
            w.Owner = this;
            w.ShowDialog();

            SelectedType = TypeTable.SelectedType;

            if (SelectedType != null)
            {
                foreach (Model.Lokal lokali in Prikaz.SelectedItems)
                {
                    lokali.type = SelectedType;
                }

                for (int i = 0; i < Prikaz.SelectedItems.Count; i++)
                {
                    referenca_lokali.Remove((Model.Lokal)(Prikaz.SelectedItems[i]));
                }
            }
            if (referenca_lokali.Count == 0)
            {
                MainWindow.tip.Remove(type);
                Close();
            }
        }

        private void izborZaSve_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TypeTable(type);
            w.Owner = this;
            w.ShowDialog();

            SelectedType = TypeTable.SelectedType;

            if (SelectedType != null)
            {
                foreach (Model.Lokal lokali in referenca_lokali)
                {
                    lokali.type = SelectedType;
                }

                MainWindow.tip.Remove(type);
                Close();
            }    
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine("!!!!!!!");
            IInputElement focusedControl = null;

            IInputElement focusedControlK = FocusManager.GetFocusedElement(this);

            string str;

            if (focusedControlK is DependencyObject)
            {
                focusedControl = focusedControlK;
                str = HelpProvider.GetHelpKey((DependencyObject)focusedControl);
            }
            else
                str = "changeType";


            HelpProvider.ShowHelp(str, this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("changeType", this);
            }
        }
    }
}

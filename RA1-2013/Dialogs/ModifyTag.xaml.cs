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
    /// Interaction logic for ModifyTag.xaml
    /// </summary>
    /// 
    public partial class ModifyTag : Window
    {
        public Model.Tag SelectedTag { get; set; }

        public ModifyTag()
        {
            InitializeComponent();

            Prikaz.ItemsSource = MainWindow.tags;
            etikete.ItemsSource = MainWindow.tags;
        }

        private void Prikaz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void etikete_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedTag = (Model.Tag)etikete.SelectedItem;
                Prikaz.SelectedItem = SelectedTag;
                Prikaz.ScrollIntoView(Prikaz.SelectedItem);
                oznakaTag.Text = SelectedTag.label;
                opisTag.Text = SelectedTag.description;
                bojaTag.SelectedColor = (Color)ColorConverter.ConvertFromString(SelectedTag.color);
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("EtiketePretraga", this);
            }
        }

        private void brisanje_Click(object sender, RoutedEventArgs e)
        {
            if (oznakaTag == null)
                return;

            for(int i=0; i<MainWindow.tags.Count; i++)
            {
                if (MainWindow.tags.ElementAt(i).label.Equals(oznakaTag.Text))
                {
                    MainWindow.tags.RemoveAt(i);
                    break;
                }
            }
        }

        private void kreiranje_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputTag();
            w.Owner = this;
            w.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                (this as Window).Close();
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("modifyTag", this);
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
                str = "modifyTag";


            HelpProvider.ShowHelp(str, this);
        }
    }
}

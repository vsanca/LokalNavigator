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

namespace RA1_2013.Dialogs
{
    /// <summary>
    /// Interaction logic for TagTable.xaml
    /// </summary>
    public partial class TagTable : Window
    {
        private System.Collections.ObjectModel.ObservableCollection<Model.Tag> _selected;

        public TagTable(System.Collections.ObjectModel.ObservableCollection<Model.Tag> selected)
        {
            InitializeComponent();

            _selected = selected;

            PrikazTag.ItemsSource = MainWindow.tags;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {

            if (PrikazTag.SelectedItems != null)
            {
                foreach (Model.Tag tag in PrikazTag.SelectedItems)
                {
                    if(!_selected.Contains(tag))
                        _selected.Add(tag);
                }
            }

            Close();
        }

        private void addTag_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.InputTag();
            w.Owner = this;
            w.ShowDialog();
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
                str = "tagTable";


            HelpProvider.ShowHelp(str, this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("tagTable", this);
            }
        }
    }
}

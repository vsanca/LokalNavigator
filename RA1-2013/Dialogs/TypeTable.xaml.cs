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
    /// Interaction logic for TypeTable.xaml
    /// </summary>
    public partial class TypeTable : Window
    {
        public static Model.LokalType SelectedType { get; set; }
        public List<Model.LokalType> tipovi { get; set; }

        public TypeTable()
        {
            InitializeComponent();
            PrikazTip.ItemsSource = MainWindow.tip;
        }

        public TypeTable(Model.LokalType type)
        {
            InitializeComponent();

            tipovi = new List<Model.LokalType>();

            foreach(Model.LokalType tip in MainWindow.tip)
            {
                if (!tip.Equals(type))
                    tipovi.Add(tip);
            }

            PrikazTip.ItemsSource = tipovi;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedType = null;

            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedType = (Model.LokalType)PrikazTip.SelectedItem;

            if(SelectedType!=null)
                Close();
        }
        private void AddType_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.InputType();
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
                str = "typeTable";


            HelpProvider.ShowHelp(str, this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("typeTable", this);
            }
        }
    }
}

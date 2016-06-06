using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for InputTag.xaml
    /// </summary>
    public partial class InputTag : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _oznaka;
        private string _opis;

        public string oznaka
        {
            get
            {
                return _oznaka;
            }
            set
            {
                if (!value.Equals(_oznaka))
                {
                    _oznaka = value;
                    OnPropertyChanged("oznaka");
                }
            }
        }
        public string opis
        {
            get
            {
                return _opis;
            }
            set
            {
                if (!value.Equals(_opis))
                {
                    _opis = value;
                    OnPropertyChanged("opis");
                }
            }
        }

        public InputTag()
        {
            InitializeComponent();
            this.DataContext = this;
            //Background = RA1_2013.Resources.Colors.DialogFrameAlt;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //string oznaka = oznakaTag.Text;
            //string opis = opisTag.Text;
            Color boja = (Color)bojaTag.SelectedColor;

            Model.Tag t = new Model.Tag(oznaka);
            t.description = opis;
            t.color = boja.ToString();

            MainWindow.tags.Add(t);
            
            Close();
        }
        private void Unos_Enabled_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_err == 0) && (oznaka != null) && (!oznaka.Equals("")));
            e.Handled = true;
        }

        private void Unos_Enabled_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _err++;
            else
                _err--;
        }

        private int _err = 0;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MainWindow w = this.Owner as MainWindow;
                if (w.cts != null)
                {
                    w.cts.Cancel();
                    w.cts = null;
                    w.WindowBorder.BorderThickness = w.windowThickness;
                    w.WindowBorder.BorderBrush = w.windowBrush;
                }
               (this as Window).Close();

            } else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("newTag", this);
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
                str = "newTag";


            HelpProvider.ShowHelp(str, this);
        }
    }
}

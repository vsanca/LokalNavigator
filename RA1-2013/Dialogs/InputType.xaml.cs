using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for InputType.xaml
    /// </summary>
    public partial class InputType : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _ime;
        private string _oznaka;
        private string _opis;

        public string ime
        {
            get
            {
                return _ime;
            }
            set
            {
                if (!value.Equals(_ime))
                {
                    _ime = value;
                    OnPropertyChanged("ime");
                }
            }
        }
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

        public InputType()
        {
            InitializeComponent();
            this.DataContext = this;
            //Background = RA1_2013.Resources.Colors.DialogFrameAlt;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void unosTipa_Click(object sender, RoutedEventArgs e)
        {
            //string ime = imeTip.Text;
            //string oznaka = oznakaTip.Text;
            //BitmapImage img = ikonicaTip.Source as BitmapImage;
            //string opis = opisTip.Text;

            Model.LokalType l = new Model.LokalType(oznaka);
            l.description = opis;
            l.name = ime;
            /*if (((BitmapImage)ikonicaTip.Source).UriSource != null)
                l.image_uri = ((BitmapImage)ikonicaTip.Source).UriSource;*/
            if(ikonicaTip.Source.GetType() == typeof(BitmapImage))
                l.image_uri = ((BitmapImage)ikonicaTip.Source).UriSource;
            else
                l.image_uri = new Uri(Assembly.GetExecutingAssembly().Location + @"\..\..\..\Resources\lokal.png");

            MainWindow.tip.Add(l);
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
            d.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";
            d.Multiselect = false;

            Nullable<bool> result = d.ShowDialog();

            if (result == true)
            {
                string filename = d.FileName;
                Console.WriteLine(filename);

                Uri uri = new Uri(filename);

                string name = System.IO.Path.GetFileName(uri.LocalPath);
                string destination = Assembly.GetExecutingAssembly().Location+@"\..\..\..\local\tip";

                string destFile = System.IO.Path.Combine(destination,name);

                if (!System.IO.Directory.Exists(destination))
                {
                    System.IO.Directory.CreateDirectory(destination);
                }

                if(!System.IO.File.Exists(destFile))
                    System.IO.File.Copy(filename,destFile);

                Console.WriteLine(destFile);

                uri = new Uri(destFile);

                BitmapSource i = new BitmapImage(uri);

                ikonicaTip.Source = i;
            }
        }

        private void ikonicaTip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Button_Click_1(sender, e);
        }

        private void Unos_Enabled_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_err == 0)&&(ime!=null)&&(oznaka!=null)&&(!ime.Equals(""))&&(!oznaka.Equals("")));
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
                HelpProvider.ShowHelp("newType", this);
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
                str = "newType";


            HelpProvider.ShowHelp(str, this);
        }
    }
}

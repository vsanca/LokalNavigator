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
using System.Windows.Markup;
using RA1_2013.Model;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;

namespace RA1_2013.Dialogs
{
    /// <summary>
    /// Interaction logic for InputLokal.xaml
    /// </summary>
    public partial class InputLokal : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private string _key;
        private string _naziv;
        private string _opis;
        private int _kapacitet;
        private DateTime _datum;

        public string key
        {
            get
            {
                return _key;
            }
            set
            {
                if (!value.Equals(_key))
                {
                    _key = value;
                    OnPropertyChanged("key");
                }
            }
        }

        public string naziv
        {
            get
            {
                return _naziv;
            }
            set
            {
                if (!value.Equals(_naziv))
                {
                    _naziv = value;
                    OnPropertyChanged("name");
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
                    OnPropertyChanged("description");
                }
            }
        }

        public int? kapacitet
        {
            get
            {
                return _kapacitet;
            }
            set
            {
                if (value!=null && value != _kapacitet)
                {
                    _kapacitet = (int)value;
                    OnPropertyChanged("kapacitet");
                }
                else if (value==null)
                {
                    _kapacitet = 0;
                    OnPropertyChanged("kapacitet");
                }
            }
        }

        public DateTime datum
        {
            get
            {
                return _datum;
            }
            set
            {
                if (!value.Equals(_datum))
                {
                    _datum = value;
                    OnPropertyChanged("datum");
                }
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<Tag> tagOptions { get; set; }

        public InputLokal()
        {
            InitializeComponent();
            tagOptions = MainWindow.tags;
            this.DataContext = this;
            //Background = RA1_2013.Resources.Colors.DialogFrame;
            //CenovniRang.ItemsSource = Lokal.prices;
            //Alkohol.ItemsSource = Lokal.alcohol;
            ne_sluzi.Content = Lokal.alcohol[0];
            do23.Content = Lokal.alcohol[1];
            sluzi.Content = Lokal.alcohol[2];
            tipLokala.ItemsSource = MainWindow.tip;
            //tipLokala.ValueMemberPath = "name";
            tipLokala.ItemFilter = TypeFilter;
            etikete_prikaz.ItemsSource = _selected;
            etikete.ItemFilter = TagFilter;
            datum = System.DateTime.Today;
        }

        public AutoCompleteFilterPredicate<object> TypeFilter
        {
            get
            {
                return (searchText, obj) =>
                    (obj as Model.LokalType).key.ToLower().Contains(searchText.ToLower())
                    || (obj as Model.LokalType).name.ToLower().Contains(searchText.ToLower());
            }
        }

        public AutoCompleteFilterPredicate<object> TagFilter
        {
            get
            {
                return (searchText, obj) =>
                    (obj as Model.Tag).label.ToLower().Contains(searchText.ToLower())
                    || (obj as Model.Tag).description.ToLower().Contains(searchText.ToLower());
            }
        }

        private void AddType_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.InputType();
            w.Owner = this;
            w.ShowDialog();
        }

        private Model.AlcoholServing decide_Alcohol()
        {
            if ((bool)ne_sluzi.IsChecked)
            {
                return Lokal.alcohol[0];
            } else if ((bool)do23.IsChecked)
            {
                return Lokal.alcohol[1];
            } else
            {
                return Lokal.alcohol[2];
            }
        }

        private Model.PriceCategory decide_Price()
        {
            switch((int)CenovniRang.Value)
            {
                case 0:
                    return Lokal.prices[0];
                case 25:
                    return Lokal.prices[1];
                case 50:
                    return Lokal.prices[2];
                default:
                    return Lokal.prices[3];   
            }
        }

        private void ConfirmAdd_Click(object sender, RoutedEventArgs e)
        {
            Model.Lokal l = new Model.Lokal();
            //l.key = lokalOznaka.Text;
            l.key = key;
            l.name = naziv;
            l.type = (Model.LokalType)tipLokala.SelectedItem;
            l.serves = decide_Alcohol();
            l.description = lokalOpis.Text;
            l.reservations = (bool)lokalRezervacije.IsChecked;
            l.accessible = (bool)lokalDostupnost.IsChecked;
            l.smoking = (bool)lokalPusenje.IsChecked;
            l.price = decide_Price();
            Console.WriteLine(l.price);
            l.capacity = (int)kapacitet;
            l.date = (System.DateTime)lokalDatum.SelectedDate;
            l.tags = selected;
            if (ikonicaPrikaz.Source.GetType() == typeof(BitmapImage))
                l.image_uri = ((BitmapImage)ikonicaPrikaz.Source).UriSource;
            else
                l.image_uri = l.type.image_uri;
            MainWindow.lokali.Add(l);
            Close();
        }

        private void addTag_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.InputTag();
            w.Owner = this;
            w.ShowDialog();
        }

        private void addIcon_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
            d.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg";
            d.Multiselect = false;

            Nullable<bool> result = d.ShowDialog();

            if(result==true)
            {
                string filename = d.FileName;
                Console.WriteLine(filename);

                Uri uri = new Uri(filename);

                string name = System.IO.Path.GetFileName(uri.LocalPath);
                string destination = Assembly.GetExecutingAssembly().Location + @"\..\..\..\local\lokali";

                string destFile = System.IO.Path.Combine(destination, name);

                if (!System.IO.Directory.Exists(destination))
                {
                    System.IO.Directory.CreateDirectory(destination);
                }

                if (!System.IO.File.Exists(destFile))
                    System.IO.File.Copy(filename, destFile);

                Console.WriteLine(destFile);

                uri = new Uri(destFile);

                BitmapSource i = new BitmapImage(uri);

                ikonicaPrikaz.Source = i;
            }
        }

        private void CancelAdd_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ikonicaPrikaz_MouseDown(object sender, MouseButtonEventArgs e)
        {
            addIcon_Click(sender, e);
        }

        private List<Tag> selected = new List<Tag>();
        private System.Collections.ObjectModel.ObservableCollection<Tag> _selected = new System.Collections.ObjectModel.ObservableCollection<Tag>();

        private void etikete_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*foreach (Tag t in e.AddedItems)
                selected.Add(t);

            foreach (Tag t in e.RemovedItems)
                selected.Remove(t);
            */
            /*if (!selected.Contains(etikete.SelectedItem))
                if (MainWindow.tags.Contains(etikete.SelectedItem))
                {
                    selected.Add((Model.Tag)etikete.SelectedItem);
                    _selected.Add((Model.Tag)etikete.SelectedItem);
                }
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach(var component in ((StackPanel)((System.Windows.Controls.Button)sender).Parent).Children)
            {
                if(component.GetType() == typeof(TextBlock))
                {
                    Console.WriteLine(((TextBlock)component).Text);
                    for (int i=0; i<selected.Count; i++)
                    {
                        if (((TextBlock)component).Text.Equals(selected.ElementAt(i).label))
                        {
                            selected.RemoveAt(i);
                            _selected.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void etiketa_add()
        {
            if (!selected.Contains(etikete.SelectedItem))
                if (MainWindow.tags.Contains(etikete.SelectedItem))
                {
                    selected.Add((Model.Tag)etikete.SelectedItem);
                    _selected.Add((Model.Tag)etikete.SelectedItem);
                }
        }

        private void etikete_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                etiketa_add();
            }
        }

        private void Unos_Enabled_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((_err == 0) && (key != null) && (naziv != null) && (!key.Equals("")) && (!naziv.Equals(""))&&(kapacitet>0) && (datum!=null));
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

        private void SelectTypeTable_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TypeTable();
            w.Owner = this;
            w.ShowDialog();

            if (TypeTable.SelectedType != null)
            {
                tipLokala.SelectedItem = TypeTable.SelectedType;
            }
        }

        private void SelectTagTable_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TagTable(_selected);
            w.Owner = this;
            w.ShowDialog();

            if(_selected.Count != selected.Count)
            {
                foreach(Model.Tag tag in _selected)
                {
                    if (!selected.Contains(tag))
                        selected.Add(tag);
                }
            }
        }

        private void LokalInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                MainWindow w = this.Owner as MainWindow;
                if (w.cts != null)
                {
                    w.cts.Cancel();
                    w.cts = null;
                    w.WindowBorder.BorderThickness = w.windowThickness;
                    w.WindowBorder.BorderBrush = w.windowBrush;
                    w.demopopup.IsOpen = false;
                }
                    (this as Window).Close();
                
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("NoviLokal", this);
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
                str = "NoviLokal";


            HelpProvider.ShowHelp(str, this);
        }
    }
}

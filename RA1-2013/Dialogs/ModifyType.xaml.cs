using System;
using System.Collections.Generic;
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
    /// Interaction logic for ModifyType.xaml
    /// </summary>
    public partial class ModifyType : Window
    {
        public Model.LokalType SelectedType { get; set; }

        public ModifyType()
        {
            InitializeComponent();

            Prikaz.ItemsSource = MainWindow.tip;
            tipoviLokala.ItemsSource = MainWindow.tip;
            tipoviLokala.ItemFilter = TypeFilter;
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

        private void Prikaz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedType = (Model.LokalType) Prikaz.SelectedItem;
            if (SelectedType != null)
            {
                BitmapSource i = new BitmapImage(SelectedType.image_uri);
                ikonicaTip.Source = i;
            }
            else
            {
                BitmapSource i = new BitmapImage(new Uri(Assembly.GetExecutingAssembly().Location + @"\..\..\..\Resources\lokal.png"));
                ikonicaTip.Source = i;
            }
            //Console.WriteLine(SelectedType.name);
            //Console.WriteLine(SelectedType.image_uri);
        }

        private void tipoviLokala_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedType = (Model.LokalType)tipoviLokala.SelectedItem;
                Prikaz.SelectedItem = SelectedType;
                Prikaz.ScrollIntoView(Prikaz.SelectedItem);
                oznakaTip.Text = SelectedType.key;
                imeTip.Text = SelectedType.name;
                opisTip.Text = SelectedType.description;
                BitmapSource i = new BitmapImage(SelectedType.image_uri);
                ikonicaTip.Source = i;
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("TipoviPretraga", this);
            }
        }

        private void dodajIkonicu_Click(object sender, RoutedEventArgs e)
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
                string destination = Assembly.GetExecutingAssembly().Location + @"\..\..\..\local\tip";

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

                ikonicaTip.Source = i;

                SelectedType.image_uri = uri;
            }
        }

        private void ikonicaTip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dodajIkonicu_Click(sender, e);
        }

        public bool deleteTypeDialog = false;

        private Model.LokalType deleteType;
        private List<Model.Lokal> modifyList;

        private void brisanje_Click(object sender, RoutedEventArgs e)
        {
            /*if (oznakaTip == null)
                return;

            for (int i = 0; i < MainWindow.tip.Count; i++)
            {
                if (MainWindow.tip.ElementAt(i).key.Equals(oznakaTip.Text))
                {
                    MainWindow.tip.RemoveAt(i);
                    break;
                }
            }*/

            if (SelectedType == null)
                return;

            string key = SelectedType.key;

            Console.WriteLine(SelectedType);

            for (int i = 0; i < MainWindow.tip.Count; i++)
            {
                if (MainWindow.tip.ElementAt(i).key.Equals(SelectedType.key))
                {
                    deleteType = MainWindow.tip.ElementAt(i);
                    break;
                }
            }

            modifyList = new List<Model.Lokal>();

            if (deleteType != null)
                foreach (Model.Lokal lokal in MainWindow.lokali)
                {
                    if (lokal.type.Equals(deleteType))
                    {
                        deleteTypeDialog = true;
                        modifyList.Add(lokal);
                    }
                }

            if (deleteTypeDialog)
            {
                Window w = new RA1_2013.Dialogs.ChangeType(deleteType, modifyList);
                w.Owner = this;
                w.ShowDialog();
            }
            else
            {
                Window w = new RA1_2013.Dialogs.Obavestenje(key);
                w.Owner = this;
                w.ShowDialog();
                MainWindow.tip.Remove(deleteType);
            }
        }

        private void kreiranje_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputType();
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
                HelpProvider.ShowHelp("modifyType", this);
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
                str = "modifyType";


            HelpProvider.ShowHelp(str, this);
        }
    }
}

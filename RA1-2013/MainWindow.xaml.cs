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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using RA1_2013.Resources;
using System.Windows.Media.Animation;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Controls.Primitives;
using RA1_2013.Dialogs;
using System.Threading;
using System.Runtime.InteropServices;
using WindowsInput;

namespace RA1_2013
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Data d { get; set; }

        public static ObservableCollection<Model.Lokal> lokali { get; set; }
        public static ObservableCollection<Model.LokalType> tip { get; set; }
        public static ObservableCollection<Model.Tag> tags { get; set; }

        public ObservableCollection<Model.Lokal> filtrirani { get; set; }

        public Model.Lokal SelectedLokal { get; set; }

        int minKapacitet;
        int maxKapacitet;
        DateTime minDate;
        DateTime maxDate;

        public AutoCompleteFilterPredicate<object> TypeFilter
        {
            get
            {
                return (searchText, obj) =>
                    (obj as Model.LokalType).key.ToLower().Contains(searchText.ToLower())
                    || (obj as Model.LokalType).name.ToLower().Contains(searchText.ToLower());
            }
        }

        public Brush windowBrush;
        public Thickness windowThickness;

        /*******************************************/
        //COMMAND SECTION

        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            kreiranje_Click(sender, e);
        }

        private void NewEtiketaExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            kreiranjeTag_Click(sender, e);
        }

        private void NewTipExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            kreiranjeTip_Click(sender, e);
        }

        private void ModifyEtiketaExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModifikujEtiketu_Click(sender, e);
        }

        private void ModifyTipExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModifikujTipLokala_Click(sender, e);
        }

        /*******************************************/

        public MainWindow()
        {
            InitializeComponent();
            d = new Data();
            this.Background = RA1_2013.Resources.Colors.MainFrame;
            this.DataContext = this;

            windowBrush = WindowBorder.BorderBrush;
            windowThickness = WindowBorder.BorderThickness;

            lokali = new ObservableCollection<Model.Lokal>();
            tip = new ObservableCollection<Model.LokalType>();
            tags = new ObservableCollection<Model.Tag>();

            d.lokali = new ObservableCollection<Model.Lokal>();
            d.tip = new ObservableCollection<Model.LokalType>();
            d.tags = new ObservableCollection<Model.Tag>();

            Stream s = File.Open("config.cfg", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();

            pretragaLokala.ItemsSource = lokali;
            pretragaLokala.ItemFilter = LokalFilter;

            tipoviLokala.ItemsSource = tip;
            tipoviLokala.ItemFilter = TypeFilter;

            priceCB.ItemsSource = Model.Lokal.prices;
            alcoholCB.ItemsSource = Model.Lokal.alcohol;

            if (s.Length != 0)
            {
                d = (Data)bf.Deserialize(s);
                s.Close();
                initializeStatic();
            }
            else
            {
                placeTypes();
                parseInputFile();

                s.Close();
            }
            //etikete.ItemsSource = tags;
            PrikazTag.ItemsSource = MainWindow.tags;
            etiketeTag.ItemsSource = MainWindow.tags;

            etikete.ItemsSource = MainWindow.tags;

            PrikazTip.ItemsSource = MainWindow.tip;
            tipoviLokalaTip.ItemsSource = MainWindow.tip;
            tipoviLokalaTip.ItemFilter = TypeFilter;

            etiketePrikaz.ItemsSource = _selected;

            minKapacitet = 0;
            maxKapacitet = 0;

            minDate = DateTime.Today;
            maxDate = DateTime.Today;

            foreach (Model.Lokal lokal in lokali)
            {
                if (lokal.capacity > maxKapacitet)
                    maxKapacitet = lokal.capacity;

                if (lokal.date < minDate)
                    minDate = lokal.date;
            }

            foreach (Model.Lokal lokal in lokali)
            {
                placeOnMap(lokal);
            }

            FilterAlkohol.ItemsSource = Model.Lokal.alcohol;
            FilterCena.ItemsSource = Model.Lokal.prices;
            FilterEtikete.ItemsSource = tags;
            FilterTip.ItemsSource = tip;
            FilterOdDatum.SelectedDate = minDate;
            FilterOdDatum.DisplayDateStart = minDate;
            FilterDoDatum.SelectedDate = maxDate;
            FilterDoDatum.DisplayDateEnd = maxDate;
            FilterSlider.LowerValue = minKapacitet;
            FilterSlider.Minimum = minKapacitet;
            FilterSlider.HigherValue = maxKapacitet;
            FilterSlider.Maximum = maxKapacitet;
            FilterSlider.Step = 1;

            filterList = new ObservableCollection<Model.Filter>();

            FilterPrikaz.ItemsSource = filterList;

        }

        public ObservableCollection<Model.Filter> filterList { get; set; }

        private void parseInputFile()
        {
            Random r = new Random();
            using (StreamReader reader = new StreamReader(@"..\..\Resources\diskoteke.csv"))
            {
                string line;
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }
                    string[] a = line.Split('\t');

                    for (int i = 0; i < a.Count(); i++)
                    {
                        a[i] = a[i].Substring(1, a[i].Length - 2);
                    }

                    Model.Lokal l = new Model.Lokal();

                    l.capacity = r.Next(50, 500);

                    if (r.Next(0, 2) == 1)
                        l.accessible = true;
                    else
                        l.accessible = false;

                    if (a[4] == null || a[4].Equals("") || a[4].Equals(" "))
                        l.description = "Trenutno nije unesen opis.";
                    else
                        l.description = a[4];

                    if (a[3] == null || a[3].Equals("Nedostupno"))
                    {
                        DateTime dt = new DateTime();
                        dt = DateTime.Now;
                        l.date = dt;
                    }
                    else
                    {
                        int day = r.Next(1, 28);
                        int month = r.Next(1, 12);
                        int year = r.Next(1950, 2016);

                        l.date = new DateTime(year, month, day);
                    }

                    l.name = a[2];

                    l.key = l.name.Replace(" ", "") + l.date.Year;

                    l.type = getTypeByName(a[0]);

                    l.image_uri = addPicture(a[1], l);

                    l.tags = new List<Model.Tag>();

                    if (r.Next(0, 2) == 1)
                        l.smoking = true;
                    else
                        l.smoking = false;

                    if (r.Next(0, 2) == 1)
                        l.reservations = true;
                    else
                        l.reservations = false;

                    l.serves = Model.Lokal.alcohol[r.Next(0, 2)];

                    l.price = Model.Lokal.prices[r.Next(0, 3)];

                    lokali.Add(l);

                }
            }
        }

        private Uri addPicture(string path, Model.Lokal l)
        {
            Uri uri = new Uri(path);

            string name = System.IO.Path.GetFileName(uri.LocalPath);

            if (!name.Equals("prostor_za_logo_vase_firme.jpg"))
            {
                string destination = Assembly.GetExecutingAssembly().Location + @"\..\..\..\local\lokali";

                string destFile = System.IO.Path.Combine(destination, name);

                if (!System.IO.Directory.Exists(destination))
                {
                    System.IO.Directory.CreateDirectory(destination);
                }


                if (!System.IO.File.Exists(destFile))
                    System.IO.File.Copy(uri.LocalPath, destFile);

                return new Uri(destFile);
            }

            return l.type.image_uri;
        }

        private Uri addPictureType(string path)
        {
            Uri uri = new Uri(path);

            string name = System.IO.Path.GetFileName(uri.LocalPath);
            string destination = Assembly.GetExecutingAssembly().Location + @"\..\..\..\local\tip";

            string destFile = System.IO.Path.Combine(destination, name);

            if (!System.IO.Directory.Exists(destination))
            {
                System.IO.Directory.CreateDirectory(destination);
            }


            if (!System.IO.File.Exists(destFile))
                System.IO.File.Copy(uri.LocalPath, destFile);

            return new Uri(destFile);
        }

        private Model.LokalType getTypeByName(string typeName)
        {
            foreach (Model.LokalType lt in tip)
            {
                if (lt.name.Equals(typeName))
                    return lt;
            }

            return null;
        }

        private void placeTypes()
        {
            Model.LokalType t1 = new Model.LokalType("Disko");
            t1.name = "Diskoteka - klub";
            t1.description = "Mesto za provod i ples";
            t1.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1-2013-Better_GUI_Branch\RA1-2013\local\tip\disco_ball.png");

            Model.LokalType t2 = new Model.LokalType("Brza hrana");
            t2.name = "Restoran brze hrane";
            t2.description = "Mesto za brze zalogaje";
            t2.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1 - 2013 - Better_GUI_Branch\RA1 - 2013\local\tip\fast_food.png");

            Model.LokalType t3 = new Model.LokalType("Kafe");
            t3.name = "Kafe bar";
            t3.description = "Mesto za opušteni razgovor i piće.";
            t3.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1 - 2013 - Better_GUI_Branch\RA1 - 2013\local\tip\coffee.png");

            Model.LokalType t4 = new Model.LokalType("Poslastičarnica");
            t4.name = "Poslastičarnica";
            t4.description = "Mesto gde možete probati razne slatke specijalitete.";
            t4.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1 - 2013 - Better_GUI_Branch\RA1 - 2013\local\tip\cake.png");

            Model.LokalType t5 = new Model.LokalType("Splav");
            t5.name = "Splav";
            t5.description = "Mesto za provod i ples na reci.";
            t5.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1 - 2013 - Better_GUI_Branch\RA1 - 2013\local\tip\splav.png");

            Model.LokalType t6 = new Model.LokalType("Vinoteka");
            t6.name = "Vinoteka";
            t6.description = "Mesto za degustaciju i uživanje u vinu.";
            t6.image_uri = addPictureType(@"C:\Users\Viktor\Source\Workspaces\HCI\RA1 - 2013 - Better_GUI_Branch\RA1 - 2013\local\tip\wine.png");

            tip.Add(t1);
            tip.Add(t2);
            tip.Add(t3);
            tip.Add(t4);
            tip.Add(t5);
            tip.Add(t6);
        }

        private void tipoviLokala_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tipoviLokala.SelectedItem != null)
                    ((Model.Lokal)Prikaz.SelectedItem).type = (Model.LokalType)tipoviLokala.SelectedItem;
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("TipoviPretraga", this);
            }
        }

        private void tipoviLokala_PreviewKeyDownFilter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (FilterTip.SelectedItem != null)
                {
                    Model.Filter f = new Model.Filter(((Model.LokalType)FilterTip.SelectedItem).name, "Tip lokala");

                    bool found = false;
                    
                    foreach (Model.Filter ft in filterList)
                    {
                        if (ft.type.Equals("Tip lokala") && ft.value.Equals(f.value))
                        {
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                        filterList.Add(f);        
                }
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("TipoviPretraga", this);
            }
        }

        public AutoCompleteFilterPredicate<object> LokalFilter
        {
            get
            {
                return (searchText, obj) =>
                    (obj as Model.Lokal).key.ToLower().Contains(searchText.ToLower())
                    || (obj as Model.Lokal).name.ToLower().Contains(searchText.ToLower()) || (obj as Model.Lokal).description.ToLower().Contains(searchText.ToLower());
            }
        }

        private void initializeStatic()
        {
            foreach (Model.Lokal l in d.lokali)
                lokali.Add(l);

            foreach (Model.LokalType lt in d.tip)
                tip.Add(lt);

            foreach (Model.Tag t in d.tags)
                tags.Add(t);
        }

        Window window;
        Window obavestenje;

        private void DodajLokal_Click(object sender, RoutedEventArgs e)
        {
            Window s = new Dialogs.InputLokal();
            window = s;
            s.Owner = this;
            s.ShowDialog();
        }

        /* UPRAVLJANJE MAPOM */

        bool isOpenPrevious = false;
        bool reopenPopup = false;

        private void map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            map.ReleaseMouseCapture();

            if (reopenPopup)
                popup.IsOpen = true;
        }

        private Point start;
        private Point original;

        private Rect rect;
        private Rect bounds;

        private void map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            map.CaptureMouse();
            start = e.GetPosition(clipBorder);
            original = new Point(translateTransform.X, translateTransform.Y);
            rect = new Rect(map.RenderSize);
            bounds = map.TransformToAncestor(clipBorder).TransformBounds(rect);

            isOpenPrevious = popup.IsOpen;
            popup.IsOpen = false;

            if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
            {
                Point p = new Point();
                p.X = e.GetPosition(sender as Image).X;
                p.Y = e.GetPosition(sender as Image).Y;
                moveTo(p.X, p.Y);
            }

            if (source != null)
                if (!source.IsMouseOver)
                    reopenPopup = false;
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            if (!map.IsMouseCaptured)
                return;

            map.ClipToBounds = true;

            rect = new Rect(map.DesiredSize);
            bounds = map.TransformToAncestor(clipBorder).TransformBounds(rect);
            Vector move = start - e.GetPosition(clipBorder);

            translateTransform.X = original.X - move.X;
            translateTransform.Y = original.Y - move.Y;

            tt1.X = original.X - move.X;
            tt1.Y = original.Y - move.Y;

            checkAndChange();

            if (isOpenPrevious)
                reopenPopup = true;
            else
                reopenPopup = false;

            //Console.WriteLine("RL-BL:{0} BR-RL:{1} RT-BT:{2} BB-RB:{3} move X:{4}, move Y:{5}", rect.Left-bounds.Left, bounds.Right-rect.Right, rect.Top-bounds.Top, bounds.Bottom-rect.Bottom, move.X, move.Y);
            //Console.WriteLine("W:{0} H:{1}", map.ActualWidth, map.ActualHeight);
        }

        private void map_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = 1.1;
            Image i = sender as Image;

            popup.IsOpen = false;

            Point p = new Point();
            p.X = e.GetPosition(sender as Image).X / i.ActualWidth;
            p.Y = e.GetPosition(sender as Image).Y / i.ActualHeight;

            (sender as Image).RenderTransformOrigin = p;

            Point p1 = new Point();
            p1.X = e.GetPosition(canvas).X / canvas.ActualWidth;
            p1.Y = e.GetPosition(canvas).Y / canvas.ActualHeight;
            canvas.RenderTransformOrigin = p1;

            if (e.Delta > 0)
            {
                st.ScaleX *= scale;
                st.ScaleY *= scale;

                st1.ScaleX *= scale;
                st1.ScaleY *= scale;
            }
            else
            {
                if (st.ScaleX > 1 && st.ScaleY > 1)
                {
                    st.ScaleX /= scale;
                    st.ScaleY /= scale;

                    st1.ScaleX /= scale;
                    st1.ScaleY /= scale;
                }
            }

            checkAndChange();

            //Console.WriteLine("RL-BL:{0} BR-RL:{1} RT-BT:{2} BB-RB:{3}", rect.Left-bounds.Left, bounds.Right-rect.Right, rect.Top-bounds.Top, bounds.Bottom-rect.Bottom);
        }

        public void moveTo(double X, double Y)
        {
            rect = new Rect(map.DesiredSize);
            bounds = map.TransformToAncestor(clipBorder).TransformBounds(rect);
            Vector move = start - new Point(X, Y);

            translateTransform.X = original.X - move.X;
            translateTransform.Y = original.Y - move.Y;

            tt1.X = original.X - move.X;
            tt1.Y = original.Y - move.Y;

            checkAndChange();
        }

        private void checkAndChange()
        {
            rect = new Rect(map.DesiredSize);
            bounds = map.TransformToAncestor(clipBorder).TransformBounds(rect);
            check();
        }

        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);

                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;

                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }


        private void check()
        {
            if (0 >= rect.Left - bounds.Left)
                translateTransform.X += -bounds.Left + rect.Left;

            if (0 >= bounds.Right - rect.Right)
                translateTransform.X += -bounds.Right + rect.Right;

            if (0 >= rect.Top - bounds.Top)
                translateTransform.Y += -bounds.Top + rect.Top;

            if (0 >= bounds.Bottom - rect.Bottom)
                translateTransform.Y += -bounds.Bottom + rect.Bottom;


            if (0 >= rect.Left - bounds.Left)
                tt1.X += -bounds.Left + rect.Left;

            if (0 >= bounds.Right - rect.Right)
                tt1.X += -bounds.Right + rect.Right;

            if (0 >= rect.Top - bounds.Top)
                tt1.Y += -bounds.Top + rect.Top;

            if (0 >= bounds.Bottom - rect.Bottom)
                tt1.Y += -bounds.Bottom + rect.Bottom;
        }

        private void DodajEtiketu_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputTag();
            w.Owner = this;
            w.ShowDialog();
        }

        private void DodajTip_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputType();
            w.Owner = this;
            w.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                if (mainMenu.Visibility == Visibility.Collapsed)
                {
                    mainMenu.Visibility = Visibility.Visible;
                }
                else
                {
                    mainMenu.Visibility = Visibility.Collapsed;
                }
            }
            
            if (e.Key == Key.Escape)
            {
                if (cts != null)
                {
                    cts.Cancel();
                    cts = null;
                    WindowBorder.BorderThickness = windowThickness;
                    WindowBorder.BorderBrush = windowBrush;
                    demopopup.IsOpen = false;

                    for(int i = 0; i < lokali.Count(); i++)
                    {
                        if(lokali[i].key.Equals("Oznaka lokala"))
                        {
                            lokali.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            /*else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("index", this);
            }*/

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void updateData(Data dat)
        {
            foreach (Model.Lokal l in lokali)
                dat.lokali.Add(l);

            foreach (Model.LokalType lt in tip)
                dat.tip.Add(lt);

            foreach (Model.Tag t in tags)
                dat.tags.Add(t);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stream s = File.Open("config.cfg", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            Data dat = new Data();
            updateData(dat);

            bf.Serialize(s, dat);
            s.Close();
        }

        private void tipoviLokala_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Model.LokalType lt in e.AddedItems)
                Console.WriteLine(lt);
        }

        private void etiketeLokala_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Model.LokalType lt in e.AddedItems)
                Console.WriteLine(lt);
        }

        Border selectedElement = new Border();

        private void Prikaz_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected.Clear();

            etikete.Text = "";

            if (SelectedLokal != null)
            {
                tipoviLokala.SelectedItem = ((Model.Lokal)Prikaz.SelectedItem).type;
                preview_Prikaz.Source = new BitmapImage(((Model.Lokal)Prikaz.SelectedItem).image_uri);

                foreach (Model.Tag t in ((Model.Lokal)Prikaz.SelectedItem).tags)
                {
                    if (!_selected.Contains(t))
                        _selected.Add(t);
                }

                foreach (UIElement element in canvas.Children)
                {
                    if (SelectedLokal.key.Equals(((Border)element).Name))
                    {
                        ((Border)selectedElement).BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Navy);
                        ((Border)selectedElement).BorderThickness = new Thickness(1);

                        selectedElement = (Border)element;

                        ((Border)element).BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Green);
                        ((Border)element).BorderThickness = new Thickness(2);
                        break;
                    }
                }
            }

            popup.IsOpen = false;
        }

        private void etiketa_add()
        {
            List<Model.Tag> selected = ((Model.Lokal)Prikaz.SelectedItem).tags;
            if (!selected.Contains(etikete.SelectedItem))
                if (MainWindow.tags.Contains(etikete.SelectedItem))
                {
                    selected.Add((Model.Tag)etikete.SelectedItem);
                    _selected.Add((Model.Tag)etikete.SelectedItem);
                }
        }

        private void etikete_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                etiketa_add();
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("EtiketePretraga", this);
            }
        }

        private void etikete_PreviewKeyDownFilter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!MainWindow.tags.Contains(FilterEtikete.SelectedItem))
                    return;

                Model.Tag selected = (Model.Tag)FilterEtikete.SelectedItem;

                bool found = false;

                foreach(Model.Filter f in filterList)
                {
                    if (f.type.Equals("Etiketa") && f.value.Equals(selected.label))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    filterList.Add(new Model.Filter(selected.label, "Etiketa"));
                }
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("EtiketePretraga", this);
            }
        }

        private void ModifikujEtiketu_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.ModifyTag();
            w.Owner = this;
            w.ShowDialog();
        }

        private void ModifikujTipLokala_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.ModifyType();
            w.Owner = this;
            w.ShowDialog();
        }
        private void pretragaLokala_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (pretragaLokala.SelectedItem != null)
                {
                    Prikaz.SelectedItem = pretragaLokala.SelectedItem;
                    Prikaz.ScrollIntoView(Prikaz.SelectedItem);
                }
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("LokaliPretraga", this);
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<Model.Tag> _selected = new System.Collections.ObjectModel.ObservableCollection<Model.Tag>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var component in ((StackPanel)((System.Windows.Controls.Button)sender).Parent).Children)
            {
                if (component.GetType() == typeof(TextBlock))
                {
                    Console.WriteLine(((TextBlock)component).Text);
                    if (SelectedLokal != null)
                        for (int i = 0; i < SelectedLokal.tags.Count; i++)
                        {
                            if (((TextBlock)component).Text.Equals(SelectedLokal.tags.ElementAt(i).label))
                            {
                                SelectedLokal.tags.RemoveAt(i);
                                _selected.RemoveAt(i);
                            }
                        }
                }
            }
        }

        private void brisanje_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLokal == null)
                return;

            pretragaLokala.Text = "";
            string key = SelectedLokal.key;

            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (((Border)canvas.Children[i]).Name.Equals(SelectedLokal.key))
                {
                    canvas.Children.RemoveAt(i);
                }
            }

            for (int i = 0; i < lokali.Count; i++)
            {
                if (lokali.ElementAt(i).key.Equals(SelectedLokal.key))
                {
                    lokali.RemoveAt(i);
                    Window w = new RA1_2013.Dialogs.Obavestenje(key);
                    obavestenje = w;
                    w.Owner = Application.Current.MainWindow;
                    w.ShowDialog();
                    break;
                }
            }
        }

        private void kreiranje_Click(object sender, RoutedEventArgs e)
        {
            DodajLokal_Click(sender, e);
        }

        public Model.Tag SelectedTag { get; set; }


        private void PrikazTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void etiketeTag_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedTag = (Model.Tag)etiketeTag.SelectedItem;
                PrikazTag.SelectedItem = SelectedTag;
                PrikazTag.ScrollIntoView(PrikazTag.SelectedItem);
                oznakaTag.Text = SelectedTag.label;
                opisTag.Text = SelectedTag.description;
                bojaTag.SelectedColor = (Color)ColorConverter.ConvertFromString(SelectedTag.color);
            }
            else if (e.Key == Key.F1)
            {
                HelpProvider.ShowHelp("EtiketePretraga", this);
            }
        }

        private void brisanjeTag_Click(object sender, RoutedEventArgs e)
        {
            foreach (Model.Lokal lokal in lokali)
            {
                for (int i = 0; i < lokal.tags.Count; i++)
                    if (lokal.tags.ElementAt(i).label.Equals(oznakaTag.Text))
                    {
                        lokal.tags.RemoveAt(i);
                        break;
                    }
            }

            for (int i = 0; i < _selected.Count; i++)
            {
                if (_selected.ElementAt(i).label.Equals(oznakaTag.Text))
                {
                    _selected.RemoveAt(i);
                    break;
                }
            }

            string key = oznakaTag.Text;

            for (int i = 0; i < MainWindow.tags.Count; i++)
            {
                if (MainWindow.tags.ElementAt(i).label.Equals(oznakaTag.Text))
                {
                    MainWindow.tags.RemoveAt(i);
                    Window w = new RA1_2013.Dialogs.Obavestenje(key);
                    obavestenje = w;
                    w.Owner = Application.Current.MainWindow;
                    w.ShowDialog();
                    break;
                }
            }
        }

        private void kreiranjeTag_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputTag();
            w.Owner = this;
            w.ShowDialog();
        }

        public Model.LokalType SelectedType { get; set; }

        private void PrikazTip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedType = (Model.LokalType)PrikazTip.SelectedValue;
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
        }

        private void tipoviLokalaTip_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedType = (Model.LokalType)tipoviLokalaTip.SelectedItem;
                PrikazTip.SelectedItem = SelectedType;
                PrikazTip.ScrollIntoView(PrikazTip.SelectedItem);
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

        private void brisanjeTip_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedType == null)
                return;

            string key = SelectedType.key;

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
                foreach (Model.Lokal lokal in lokali)
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
                obavestenje = w;
                w.Owner = Application.Current.MainWindow;
                w.ShowDialog();
                MainWindow.tip.Remove(deleteType);
            }
        }

        private void kreiranjeTip_Click(object sender, RoutedEventArgs e)
        {
            Window w = new RA1_2013.Dialogs.InputType();
            w.Owner = this;
            w.ShowDialog();
        }

        private void addIcon_Click(object sender, RoutedEventArgs e)
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

                SelectedLokal.image_uri = uri;

                preview_Prikaz.Source = new BitmapImage(((Model.Lokal)Prikaz.SelectedItem).image_uri);

                foreach (UIElement element in canvas.Children)
                {
                    if (SelectedLokal.key.Equals(((Border)element).Name))
                    {
                        ((Image)((Border)element).Child).Source = preview_Prikaz.Source;
                        break;
                    }
                }
            }
        }

        Point startPoint = new Point();

        private void Prikaz_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void Prikaz_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(null);
            Vector diff = startPoint - position;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DataGrid lokali = sender as DataGrid;

                Model.Lokal selected = (Model.Lokal)lokali.SelectedItem;

                if (selected != null)
                {
                    DataObject dragData = new DataObject("lokal", selected);
                    DragDrop.DoDragDrop(lokali, dragData, DragDropEffects.Move);
                }
            }
        }

        private async void map_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("lokal") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        public CancellationTokenSource cts = new CancellationTokenSource();

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        InputSimulator ins = new InputSimulator();
        public Popup demopopup;

        public async Task DemoMode(CancellationToken ct)
        {
            WindowBorder.BorderThickness = new Thickness(5);
            WindowBorder.BorderBrush = Brushes.Green;

            demopopup = new Popup();
            
            TextBlock tb = new TextBlock();
            tb.Text = "Demo mod - izlaz: ESC";
            tb.Foreground = Brushes.Green;
            tb.Background = Brushes.White;
            tb.FontWeight = FontWeights.Bold;
            
            demopopup.Child = tb;

            demopopup.PlacementTarget = this.ToolbarTray;
            demopopup.Placement = PlacementMode.Center;

            demopopup.IsOpen = true;

            Point relativePoint = Expander1.PointToScreen(new Point(0, 0));

            while (true)
            {

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove(Expander1, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                if(Expander1.IsExpanded)
                    ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                if (!Expander1.IsExpanded)
                    ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove(kreiranje,40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).lokalNaziv, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonDown();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Abcd123456", ct);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Oznaka lokala", ct);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Disko", ct);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.DOWN);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).do23, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonDoubleClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).CenovniRang, 10);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonDoubleClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).etikete, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Vino", ct);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).lokalKapacitet, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("123", ct);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).lokalOpis, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum ullamcorper sed purus eget pellentesque.", ct);
                await Task.Delay(8000);
                ct.ThrowIfCancellationRequested();
                mouseMove((window as InputLokal).lokalRezervacije, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                mouseMove((window as InputLokal).ConfirmAdd, 20);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove(pretragaLokala, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                enterText("Abcd", ct);
                await Task.Delay(4000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.DOWN);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                mouseMove(FiltriranjeLokalaExpander, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(4000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(4000);
                ct.ThrowIfCancellationRequested();
                mouseMove(brisanje, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(4000);
                ct.ThrowIfCancellationRequested();

                mouseMove((obavestenje as Obavestenje).OK, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(4000);
                ct.ThrowIfCancellationRequested();
         
                mouseMove(Expander1, 40);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                if (Expander1.IsExpanded)
                    ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                if (!Expander1.IsExpanded)
                    ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);

                mouseMove(Prikaz, 20, (int)(Prikaz.ActualHeight / 2));
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.LeftButtonDown();
                ins.Mouse.MoveMouseBy(250, 0);
                ins.Mouse.LeftButtonUp();

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.MoveMouseBy(0, -5);

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.LeftButtonDoubleClick();

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.RightButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.MoveMouseBy(10, 10);
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
                ins.Mouse.LeftButtonClick();
                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.VerticalScroll(100);

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.LeftButtonDown();
                ins.Mouse.MoveMouseBy(50, 50);
                ins.Mouse.LeftButtonUp();

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();

                ins.Mouse.VerticalScroll(-100);

                await Task.Delay(2000);
                ct.ThrowIfCancellationRequested();
            }
        }

        private async void enterText(string text, CancellationToken ct)
        {
            foreach(char c in text)
            {
                ins.Keyboard.TextEntry(c);
                await Task.Delay(50);
                ct.ThrowIfCancellationRequested();
            }
        }

        public Point[] getPoints(int number, int startX, int startY, int endX, int endY)
        {
            var points = new Point[number];
            int ydiff = endY - startY, xdiff = endX - startX;
            double slope = (double)(endY - startY) / (endX - startX);
            double x, y;

            --number;

            for (double i = 0; i < number; i++)
            {
                y = slope == 0 ? 0 : ydiff * (i / number);
                x = slope == 0 ? xdiff * (i / number) : y / slope;
                points[(int)i] = new Point((int)Math.Round(x) + startX, (int)Math.Round(y) + startY);
            }

            points[number] = new Point(endX, endY);

            return points;
        }

        private async void mouseMove(Control control, int steps, int yOffset=10)
        {
            Point relativePoint = control.PointToScreen(new Point(0, 0));
            Point[] pt = getPoints(steps, (int)PointToScreen(Mouse.GetPosition(this)).X, (int)PointToScreen(Mouse.GetPosition(this)).Y, (int)(relativePoint.X + control.ActualWidth / 2), (int)(relativePoint.Y + yOffset));

            for (int i=0; i<pt.Count(); i++)
            {
                
                SetCursorPos((int)pt[i].X, (int)pt[i].Y);
                await Task.Delay(20);
            }
        }

        private async void DemoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (cts == null)
                cts = new CancellationTokenSource();

                try
                {
                    await DemoMode(cts.Token);
                }
                catch (OperationCanceledException)
                {
                }
        }



        private void map_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("lokal"))
            {
                Model.Lokal lokal = e.Data.GetData("lokal") as Model.Lokal;

                //((Model.Lokal)Prikaz.SelectedItem).position = new System.Drawing.Point((int)e.GetPosition(map).X, (int)e.GetPosition(map).Y);

                ((Model.Lokal)Prikaz.SelectedItem).posX = e.GetPosition(map).X / map.ActualWidth;
                ((Model.Lokal)Prikaz.SelectedItem).posY = e.GetPosition(map).Y / map.ActualHeight;

                Image i = new Image();
                i.Width = 30;
                i.Stretch = Stretch.UniformToFill;
                BitmapSource s = new BitmapImage(lokal.image_uri);
                i.Source = s;
                i.Name = lokal.key.Replace("'","");

                Border b = new Border();
                b.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Navy);
                b.BorderThickness = new Thickness(1);
                b.Background = new SolidColorBrush(System.Windows.Media.Colors.White);

                i.RenderSize = new Size(30, 40);

                b.Child = i;
                b.Name = lokal.key.Replace("'", "");

                b.RenderSize = new Size(30, 40);

                b.MouseLeftButtonDown += mapElement_MouseLeftButtonDown;
                b.MouseMove += mapElement_MouseMove;
                b.MouseLeftButtonUp += mapElement_MouseLeftButtonUp;

                b.AllowDrop = true;
                b.Drop += mapElement_Drop;
                b.DragEnter += mapElement_DragEnter;

                i.AllowDrop = true;
                i.Drop += mapElement_Drop;
                i.DragEnter += mapElement_DragEnter;

                Canvas.SetLeft(b, (int)e.GetPosition(map).X - 15);
                Canvas.SetTop(b, (int)e.GetPosition(map).Y - 20);

                for(int j=0; j<canvas.Children.Count; j++)
                {
                    if(((Border)canvas.Children[j]).Name.Equals(lokal.key))
                    {
                        canvas.Children.RemoveAt(j);
                        break;
                    }
                }

                ContextMenu cm = new ContextMenu();
                MenuItem delete = new MenuItem();
                cm.Items.Add(delete);
                delete.Header = "Ukloni";
                delete.Click += delegate { removeFromMap(lokal.key); };

                b.ContextMenu = cm;

                b.KeyDown += new KeyEventHandler(mapElement_KeyDown);
                i.KeyDown += new KeyEventHandler(mapElement_KeyDown);

                b.PreviewKeyDown += new KeyEventHandler(mapElement_KeyDown);
                i.PreviewKeyDown += new KeyEventHandler(mapElement_KeyDown);

                b.Resources.Add(HelpProvider.HelpKeyProperty, "MapaLokal");
                i.Resources.Add(HelpProvider.HelpKeyProperty, "MapaLokal");

                i.SetValue(HelpProvider.HelpKeyProperty, "MapaLokal");
                b.SetValue(HelpProvider.HelpKeyProperty, "MapaLokal");

                canvas.Children.Add(b);
            }
        }

        private void mapElement_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("!!!!!");
            if (e.Key == Key.A)
            {
                Console.WriteLine(sender);
                //removeFromMap();
            }
        }

        private void removeFromMap(string key)
        {
            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if(((Border)canvas.Children[i]).Name.Equals(key))
                {
                    canvas.Children.RemoveAt(i);
                    popup.IsOpen = false;
                    break;
                }
            }
        }

        private void mapElement_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("tag"))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void mapElement_Drop(object sender, DragEventArgs e)
        {
            Border b = (Border)((Image)e.Source).Parent;

            if (e.Data.GetDataPresent("tag"))
            {
                for (int i = 0; i < lokali.Count; i++)
                    if (b.Name.Equals(lokali[i].key))
                    {
                        if (!lokali[i].tags.Contains((Model.Tag)PrikazTag.SelectedItem))
                        {
                            lokali[i].tags.Add((Model.Tag)PrikazTag.SelectedItem);
                            Prikaz.SelectedItem = lokali[i];
                            _selected.Add((Model.Tag)PrikazTag.SelectedItem);
                        }
                        break;
                    }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void placeOnMap(Model.Lokal lokal)
        {
            //if (lokal.position.X == -1 || lokal.position.Y == -1)
            if (lokal.posX == -1 || lokal.posY == -1)
                return;

            if (lokal.image_uri == null)
                lokal.image_uri = lokal.type.image_uri;

            Image i = new Image();
            i.Width = 30;
            i.Stretch = Stretch.UniformToFill;
            BitmapSource s = new BitmapImage(lokal.image_uri);
            i.Source = s;

            Border b = new Border();
            b.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Navy);
            b.BorderThickness = new Thickness(1);
            b.Background = new SolidColorBrush(System.Windows.Media.Colors.White);

            i.RenderSize = new Size(30, 40);

            b.Child = i;
            b.Name = lokal.key;

            b.RenderSize = new Size(30, 40);

            b.MouseLeftButtonDown += mapElement_MouseLeftButtonDown;
            b.MouseMove += mapElement_MouseMove;
            b.MouseLeftButtonUp += mapElement_MouseLeftButtonUp;

            b.AllowDrop = true;
            b.Drop += mapElement_Drop;
            b.DragEnter += mapElement_DragEnter;

            i.AllowDrop = true;
            i.Drop += mapElement_Drop;
            i.DragEnter += mapElement_DragEnter;

            //Canvas.SetLeft(b, (int)lokal.position.X);
            //Canvas.SetTop(b, (int)lokal.position.Y);

            Canvas.SetLeft(b, (int)(lokal.posX*map.ActualWidth));
            Canvas.SetTop(b, (int)(lokal.posY*map.ActualHeight));

            for (int j = 0; j < canvas.Children.Count; j++)
            {
                if (((Border)canvas.Children[j]).Name.Equals(lokal.key))
                {
                    canvas.Children.RemoveAt(j);
                    break;
                }
            }

            ContextMenu cm = new ContextMenu();
            MenuItem delete = new MenuItem();
            cm.Items.Add(delete);
            delete.Header = "Ukloni";
            delete.Click += delegate { removeFromMap(lokal.key); };

            b.ContextMenu = cm;

            b.KeyDown += new KeyEventHandler(mapElement_KeyDown);
            i.KeyDown += new KeyEventHandler(mapElement_KeyDown);

            b.PreviewKeyDown += new KeyEventHandler(mapElement_KeyDown);
            i.PreviewKeyDown += new KeyEventHandler(mapElement_KeyDown);

            b.Resources.Add(HelpProvider.HelpKeyProperty, "MapaLokal");
            i.Resources.Add(HelpProvider.HelpKeyProperty, "MapaLokal");

            i.SetValue(HelpProvider.HelpKeyProperty, "MapaLokal");
            b.SetValue(HelpProvider.HelpKeyProperty, "MapaLokal");

            canvas.Children.Add(b);
        }

        double element_x, element_y, mouse_x, mouse_y, x, y;
        bool singleClick = false;
        UIElement source;
        Popup popup = new Popup();

        private void mapElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = sender as UIElement;

            if (e.ClickCount == 1)
            {
                Keyboard.Focus(source);
                source.CaptureMouse();
                singleClick = true;

                element_x = Canvas.GetLeft(source);
                mouse_x = e.GetPosition(canvas).X;

                element_y = Canvas.GetTop(source);
                mouse_y = e.GetPosition(canvas).Y;

                for (int i = 0; i < lokali.Count; i++)
                {
                    if (lokali[i].key.Equals(((Border)source).Name))
                    {
                        if(selectedElement!=null)
                        {
                            ((Border)selectedElement).BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Navy);
                            ((Border)selectedElement).BorderThickness = new Thickness(1);
                        }

                        selectedElement = (Border)source;

                        Prikaz.SelectedItem = lokali[i];

                        ((Border)source).BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                        ((Border)source).BorderThickness = new Thickness(2);
                    }
                }
            }

            if (e.ClickCount == 2)
            {
                popup.IsOpen = false;
                for (int i = 0; i < lokali.Count; i++)
                {
                    if (lokali[i].key.Equals(((Border)source).Name))
                    {
                        Prikaz.SelectedItem = lokali[i];

                        popup = new Popup();

                        Grid g = new Grid();

                        ColumnDefinition c1 = new ColumnDefinition();
                        ColumnDefinition c2 = new ColumnDefinition();

                        RowDefinition r1 = new RowDefinition();                    

                        c1.Width = new GridLength(50);
                        c2.Width = new GridLength(100);

                        r1.Height = new GridLength(30);

                        g.ColumnDefinitions.Add(c1);
                        g.ColumnDefinitions.Add(c2);

                        g.RowDefinitions.Add(r1);

                        g.Background = new SolidColorBrush(System.Windows.Media.Colors.White);

                        TextBox tb1 = new TextBox();
                        tb1.Text = lokali[i].name;
                        tb1.FontWeight = FontWeights.Bold;
                        tb1.FontSize = 15;
                        tb1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                        tb1.TextWrapping = TextWrapping.Wrap;
                        tb1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        tb1.IsReadOnly = true;
                        tb1.Margin = new Thickness(5,5,5,0);
                        tb1.VerticalAlignment = VerticalAlignment.Center;
                        tb1.BorderThickness = new Thickness(0);

                        Grid.SetRow(tb1, 0);
                        Grid.SetColumn(tb1, 0);
                        Grid.SetColumnSpan(tb1, 2);

                        g.Children.Add(tb1);

                        RowDefinition r2 = new RowDefinition();
                        r2.Height = new GridLength(30);
                        g.RowDefinitions.Add(r2);

                        TextBox tb2 = new TextBox();
                        tb2.Text = lokali[i].type.name;
                        tb2.FontWeight = FontWeights.Bold;
                        tb2.FontSize = 10;
                        tb2.Foreground = new SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                        tb2.TextWrapping = TextWrapping.Wrap;
                        tb2.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        tb2.IsReadOnly = true;
                        tb2.Margin = new Thickness(5, 0, 5, 0);
                        tb2.BorderThickness = new Thickness(0);
                        tb2.ToolTip = lokali[i].type.description;
                        tb2.VerticalAlignment = VerticalAlignment.Center;

                        Grid.SetRow(tb2, 1);
                        Grid.SetColumn(tb2, 0);
                        Grid.SetColumnSpan(tb2, 2);
                        g.Children.Add(tb2);

                        if (lokali[i].description != null && !lokali[i].description.Equals(""))
                        {
                            RowDefinition r3 = new RowDefinition();
                            r3.Height = new GridLength(70);
                            g.RowDefinitions.Add(r3);

                            TextBox tb3 = new TextBox();
                            tb3.Text = lokali[i].description;
                            tb3.FontSize = 10;
                            tb3.Foreground = new SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                            tb3.TextWrapping = TextWrapping.Wrap;
                            tb3.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                            tb3.IsReadOnly = true;
                            tb3.Margin = new Thickness(5);
                            tb3.BorderThickness = new Thickness(0);


                            Grid.SetRow(tb3, 2);
                            Grid.SetColumn(tb3, 0);
                            Grid.SetColumnSpan(tb3, 2);
                            g.Children.Add(tb3);
                        }
                        
                        Border b = new Border();
                        b.BorderThickness = new Thickness(2);
                        b.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.DarkGreen);

                        b.Child = g;

                        popup.Child = b;

                        popup.PlacementTarget = source;
                        popup.Placement = PlacementMode.Top;

                        popup.IsOpen = true;
                        break;
                    }
                }
            }
        }

        private void mapElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (source != null)
            {
                if (!source.IsMouseCaptured || !singleClick)
                    return;

                x = e.GetPosition(canvas).X;
                y = e.GetPosition(canvas).Y;

                element_x += x - mouse_x;
                mouse_x = x;
                Canvas.SetLeft(source, mouse_x - source.RenderSize.Width/2);

                element_y += y - mouse_y;
                mouse_y = y;
                Canvas.SetTop(source, mouse_y - source.RenderSize.Height/2);

                popup.IsOpen = false;
            }
        }

        private void mapElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!source.IsMouseCaptured || !singleClick)
                return;

            source.ReleaseMouseCapture();
            singleClick = false;

            for (int i = 0; i < lokali.Count; i++)
            {
                if (lokali[i].key.Equals(((Border)source).Name))
                {
                    double x = element_x / map.ActualWidth;
                    double y = element_y / map.ActualHeight;

                    lokali.ElementAt(i).posX = x;
                    lokali.ElementAt(i).posY = y;

                    break;
                }
            }
        }

        private void SelectTypeTable_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TypeTable();
            w.Owner = this;
            w.ShowDialog();

            if (TypeTable.SelectedType != null)
            {
                tipoviLokala.SelectedItem = TypeTable.SelectedType;

                if(SelectedLokal!=null)
                {
                    SelectedLokal.type = TypeTable.SelectedType;
                }
            }
        }

        private void SelectTypeTable_ClickFilter(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TypeTable();
            w.Owner = this;
            w.ShowDialog();

            if (TypeTable.SelectedType != null)
            {
                FilterTip.SelectedItem = TypeTable.SelectedType;

                if (FilterTip.SelectedItem != null)
                {
                    Model.Filter f = new Model.Filter(((Model.LokalType)FilterTip.SelectedItem).name, "Tip lokala");

                    bool found = false;

                    foreach (Model.Filter ft in filterList)
                    {
                        if (ft.type.Equals("Tip lokala") && ft.value.Equals(f.value))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        filterList.Add(f);
                }
            }
        }

        private void SelectTagTable_Click(object sender, RoutedEventArgs e)
        {
            Window w = new Dialogs.TagTable(_selected);
            w.Owner = this;
            w.ShowDialog();

            if(_selected!=null && SelectedLokal!=null)
                if (_selected.Count != SelectedLokal.tags.Count)
                {
                    foreach (Model.Tag tag in _selected)
                    {
                        if (!SelectedLokal.tags.Contains(tag))
                            SelectedLokal.tags.Add(tag);
                    }
                }
        }

        private void SelectTagTable_ClickFilter(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Model.Tag> filterTag = new ObservableCollection<Model.Tag>();

            Window w = new Dialogs.TagTable(filterTag);
            w.Owner = this;
            w.ShowDialog();

            bool found = false;

            foreach (Model.Tag t in filterTag)
            {
                found = false;
                foreach (Model.Filter f in filterList)
                {
                    if(f.type.Equals("Etiketa") && f.value.Equals(t.label))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    filterList.Add(new Model.Filter(t.label, "Etiketa"));
                }
            }

            
        }

        private void FilterPrimeni_Click(object sender, RoutedEventArgs e)
        {
            filtrirani = new ObservableCollection<Model.Lokal>();
            List<Model.Lokal> svi = new List<Model.Lokal>();

            foreach (Model.Lokal l in lokali)
            {
                svi.Add(l);
            }

            bool Bpusenje = false;
            bool Brezervacije = false;
            bool Brampa = false;
            bool Bcenovni_rang = false;
            bool Balkohol = false;
            bool BdatumOd = false;
            bool BdatumDo = false;
            bool Betiketa = false;
            bool BtipLokala = false;

            List<Model.Lokal> pusenje = new List<Model.Lokal>();
            List<Model.Lokal> rezervacije = new List<Model.Lokal>();
            List<Model.Lokal> rampa = new List<Model.Lokal>();
            List<Model.Lokal> cenovni_rang = new List<Model.Lokal>();
            List<Model.Lokal> alkohol = new List<Model.Lokal>();
            List<Model.Lokal> datumOd = new List<Model.Lokal>();
            List<Model.Lokal> datumDo = new List<Model.Lokal>();
            List<Model.Lokal> etiketa = new List<Model.Lokal>();
            List<Model.Lokal> tipLokala = new List<Model.Lokal>();

            foreach (Model.Lokal l in lokali)
                foreach(Model.Filter f in filterList)
                {
                    if(f.type.Equals("Pušenje"))
                    {
                        Bpusenje = true;

                        if (l.smoking && f.value.Equals("Dozvoljeno"))
                            pusenje.Add(l);

                        if (!l.smoking && f.value.Equals("Zabranjeno"))
                            pusenje.Add(l);
                    }

                    if(f.type.Equals("Rezervacije"))
                    {
                        Brezervacije = true;

                        if (l.reservations && f.value.Equals("Da"))
                            rezervacije.Add(l);

                        if (!l.reservations && f.value.Equals("Ne"))
                            rezervacije.Add(l);
                    }

                    if(f.type.Equals("Pristupna rampa"))
                    {
                        Brampa = true;

                        if(l.accessible && f.value.Equals("Da"))
                            rampa.Add(l);

                        if (!l.accessible && f.value.Equals("Ne"))
                            rampa.Add(l);
                    }

                    if(f.type.Equals("Cenovni rang"))
                    {
                        Bcenovni_rang = true;

                        if (l.price.name.Equals(f.value))
                            cenovni_rang.Add(l);
                    }

                    if (f.type.Equals("Alkohol"))
                    {
                        Balkohol = true;

                        if (l.serves.name.Equals(f.value))
                            alkohol.Add(l);
                    }

                    if(f.type.Equals("Datum od"))
                    {
                        BdatumOd = true;

                        if (l.date >= f.date)
                            datumOd.Add(l);
                    }

                    if (f.type.Equals("Datum do"))
                    {
                        BdatumDo = true;

                        if (l.date <= f.date)
                            datumDo.Add(l);
                    }

                    if (f.type.Equals("Etiketa"))
                    {
                        Betiketa = true;

                        foreach(Model.Tag t in l.tags)
                        {
                            if(t.label.Equals(f.value))
                            {
                                etiketa.Add(l);
                                break;
                            }
                        }
                    }

                    if (f.type.Equals("Tip lokala"))
                    {
                        BtipLokala = true;

                        if (l.type.name.Equals(f.value))
                            tipLokala.Add(l);
                    }
                }
            if (!Bpusenje)
                pusenje = svi;

            if (!Brezervacije)
                rezervacije = svi;

            if (!Brampa)
                rampa = svi;

            if (!Bcenovni_rang)
                cenovni_rang = svi;

            if (!Balkohol)
                alkohol = svi;

            if (!BdatumOd)
                datumOd = svi;

            if (!BdatumDo)
                datumDo = svi;

            if (!Betiketa)
                etiketa = svi;

            if (!BtipLokala)
                tipLokala = svi;

            svi = presek(svi, pusenje);
            svi = presek(svi, rezervacije);
            svi = presek(svi, rampa);
            svi = presek(svi, cenovni_rang);
            svi = presek(svi, alkohol);
            svi = presek(svi, datumOd);
            svi = presek(svi, datumDo);
            svi = presek(svi, etiketa);
            svi = presek(svi, tipLokala);

            int lower = (int)FilterSlider.LowerValue;
            int higher = (int)FilterSlider.HigherValue;

            List<Model.Lokal> count = new List<Model.Lokal>();

            foreach (Model.Lokal l in lokali)
            {
                if (l.capacity >= lower && l.capacity <= higher)
                    count.Add(l);
            }

            svi = presek(svi, count);

            foreach (Model.Lokal l in svi)
                filtrirani.Add(l);

            Prikaz.ItemsSource = filtrirani;
            pretragaLokala.ItemsSource = filtrirani;

            canvas.Children.Clear();

            foreach (Model.Lokal l in filtrirani)
                placeOnMap(l);
        }

        private List<Model.Lokal> presek(List<Model.Lokal> a, List<Model.Lokal> b)
        {
            List<Model.Lokal> res = new List<Model.Lokal>();

            for(int i=0; i<a.Count(); i++)
            {
                if (b.Contains(a[i]))
                    res.Add(a[i]);
            }

            return res;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var component in ((StackPanel)((System.Windows.Controls.Button)sender).Parent).Children)
            {
                if (component.GetType() == typeof(TextBlock))
                {
                    Console.WriteLine(((TextBlock)component).Text);
                    for (int i = 0; i < filterList.Count; i++)
                        {
                            if (((TextBlock)component).Text.Equals(filterList.ElementAt(i).label))
                            {
                                filterList.RemoveAt(i);
                            }
                        }
                }
            }
        }

        private void FilterPusenje_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = FilterPusenje.SelectedValue.ToString();
            value = value.Split(' ')[1];
            bool found = false;

            Model.Filter f = new Model.Filter(value, "Pušenje");

            if (value.Equals("Dozvoljeno") || value.Equals("Zabranjeno"))
            {
                foreach (Model.Filter ft in filterList)
                {
                    if (ft.type.Equals("Pušenje") && ft.value.Equals(value))
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                    filterList.Add(f);
            }
        }

        private void FilterRezervacije_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = FilterRezervacije.SelectedValue.ToString();
            value = value.Split(' ')[1];
            bool found = false;

            Model.Filter f = new Model.Filter(value, "Rezervacije");

            Console.WriteLine(value);

            if (value.Equals("Da") || value.Equals("Ne"))
            {
                foreach (Model.Filter ft in filterList)
                {
                    if (ft.type.Equals("Rezervacije") && ft.value.Equals(value))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    filterList.Add(f);
            }
        }

        private void FilterPristupnost_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = FilterPristupnost.SelectedValue.ToString();
            value = value.Split(' ')[1];
            bool found = false;

            Model.Filter f = new Model.Filter(value, "Pristupna rampa");

            if (value.Equals("Da") || value.Equals("Ne"))
            {
                foreach (Model.Filter ft in filterList)
                {
                    if (ft.type.Equals("Pristupna rampa") && ft.value.Equals(value))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    filterList.Add(f);
            }
        }

        private void FilterCena_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = FilterCena.SelectedValue.ToString();
            Console.WriteLine(value);
            bool found = false;

            Model.Filter f = new Model.Filter(value, "Cenovni rang");

            if (value.Equals("Nizak") || value.Equals("Srednji") || value.Equals("Visok") || value.Equals("Vrlo visok"))
            {
                foreach (Model.Filter ft in filterList)
                {
                    if (ft.type.Equals("Cenovni rang") && ft.value.Equals(value))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    filterList.Add(f);
            }
        }

        private void FilterAlkohol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = FilterAlkohol.SelectedValue.ToString();
            bool found = false;

            Model.Filter f = new Model.Filter(value, "Alkohol");

            if (value.Equals("Ne služi") || value.Equals("Služi do 23:00") || value.Equals("Uvek služi"))
            {
                foreach (Model.Filter ft in filterList)
                {
                    if (ft.type.Equals("Alkohol") && ft.value.Equals(value))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    filterList.Add(f);
            }
        }

        private void FilterOdDatum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterOdDatum.SelectedDate == null || filterList==null) 
                return;

            bool found = false;

            Model.Filter f = new Model.Filter(FilterOdDatum.SelectedDate.ToString(), "Datum od");
            f.date = (DateTime)FilterOdDatum.SelectedDate;

            foreach (Model.Filter ft in filterList)
            {
                if (ft.type.Equals("Datum od") && ft.date.Equals(f.date))
                {
                    found = true;
                    break;
                }
            }

            if(!found)
                filterList.Add(f);
        }

        private void FilterDoDatum_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterDoDatum.SelectedDate == null || filterList == null)
                return;

            bool found = false;

            Model.Filter f = new Model.Filter(FilterDoDatum.SelectedDate.ToString(), "Datum do");
            f.date = (DateTime)FilterDoDatum.SelectedDate;

            foreach (Model.Filter ft in filterList)
            {
                if (ft.type.Equals("Datum do") && ft.date.Equals(f.date))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                filterList.Add(f);
        }

        private void FilterPonisti_Click(object sender, RoutedEventArgs e)
        {
            filterList.Clear();
            Prikaz.ItemsSource = lokali;
            pretragaLokala.ItemsSource = lokali;

            canvas.Children.Clear();

            foreach(Model.Lokal l in lokali)
            {
                placeOnMap(l);
            }
        }

        private void PrikazTag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void map_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (cts == null)
                cts = new CancellationTokenSource();

            try
            {
                await DemoMode(cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear();

            foreach (Model.Lokal lokal in lokali)
            {
                placeOnMap(lokal);
            }
        }

        private void PrikazTag_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(null);
            Vector diff = startPoint - position;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                DataGrid etikete = sender as DataGrid;

                Model.Tag selected = (Model.Tag)etikete.SelectedItem;

                DataObject dragData = new DataObject("tag", selected);
                DragDrop.DoDragDrop(etikete, dragData, DragDropEffects.Move);
            }
        }

        private void etiketePrikaz_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("tag") || sender == e.Source || Prikaz.SelectedItem==null)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void etiketePrikaz_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("tag"))
            {
                if (Prikaz.SelectedItem != null)
                {
                    List<Model.Tag> selected = ((Model.Lokal)Prikaz.SelectedItem).tags;
                    Model.Tag tag = (Model.Tag)PrikazTag.SelectedItem;

                    if (!selected.Contains(tag))
                        if (MainWindow.tags.Contains(tag))
                        {
                            selected.Add(tag);
                            _selected.Add(tag);
                        }
                }
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IInputElement focusedControl = null;
            IInputElement focusedControlM = Mouse.DirectlyOver;

            if (focusedControlM is DependencyObject)
                Console.WriteLine(HelpProvider.GetHelpKey((DependencyObject)focusedControlM));

            IInputElement focusedControlK = FocusManager.GetFocusedElement(Application.Current.Windows[0]);

            if (focusedControlK is DependencyObject)
                Console.WriteLine(HelpProvider.GetHelpKey((DependencyObject)focusedControlK));

            if (focusedControlM is DependencyObject && focusedControlK is DependencyObject)
            {
                if (HelpProvider.GetHelpKey((DependencyObject)focusedControlK).Equals("index"))
                    focusedControl = focusedControlM;

                if (HelpProvider.GetHelpKey((DependencyObject)focusedControlM).Equals("index"))
                    focusedControl = focusedControlK;
            }
            else if (focusedControlM is DependencyObject)
            {
                focusedControl = focusedControlM;
            }
            else if (focusedControlK is DependencyObject)
            {
                focusedControl = focusedControlK;
            }

            Console.WriteLine("finally: "+focusedControl);

            string str = HelpProvider.GetHelpKey((DependencyObject)focusedControl);
            HelpProvider.ShowHelp(str, this);
        }
    }

    [Serializable]
    public class Data
    {
        public ObservableCollection<Model.Lokal> lokali { get; set; }
        public ObservableCollection<Model.LokalType> tip { get; set; }
        public ObservableCollection<Model.Tag> tags { get; set; }

        public Data()
        {
            lokali = new ObservableCollection<Model.Lokal>();
            tip = new ObservableCollection<Model.LokalType>();
            tags = new ObservableCollection<Model.Tag>();
        }
    }
}

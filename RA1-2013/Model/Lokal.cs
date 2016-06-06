using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace RA1_2013.Model
{
    [Serializable]
    public class PriceCategory
    {
        public string name { get; set; }

        public PriceCategory(string s)
        {
            name = s;
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PriceCategory))
                return false;

            return ((PriceCategory)obj).name.Equals(this.name);
        }
    }
    [Serializable]
    public class AlcoholServing
    {
        public string name { get; set; }

        public AlcoholServing(string s)
        {
            name = s;
        }

        public override string ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is AlcoholServing))
                return false;

            return ((AlcoholServing)obj).name.Equals(this.name);
        }
    }
    [Serializable]
    public class Lokal
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public static readonly PriceCategory[] prices = new PriceCategory[] { new PriceCategory("Nizak"), new PriceCategory("Srednji"), new PriceCategory("Visok"), new PriceCategory("Vrlo visok") };
        public static readonly AlcoholServing[] alcohol = new AlcoholServing[] { new AlcoholServing("Ne služi"), new AlcoholServing("Služi do 23:00"), new AlcoholServing("Uvek služi") };

        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public AlcoholServing serves { get; set; }
        public bool accessible { get; set; }
        public bool smoking { get; set; }
        public bool reservations { get; set; }
        public PriceCategory price { get; set; }
        public List<Tag> tags { get; set; }
        public int capacity { get; set; }
        public DateTime date { get; set; }
        private Uri _image_uri;
        public Uri image_uri {
            get { return _image_uri; }
            set { if (!value.Equals(_image_uri))
                {
                    _image_uri = value;
                    Console.WriteLine("PROMENA!");
                    OnPropertyChanged("image_uri");
                }
            }
        }
        public LokalType type { get; set; }

        //public Point position { get; set; }

        public Lokal()
        {
            posX = -1;
            posY = -1;
        }

        public override string ToString()
        {
            return key;
        }

        public double posX { get; set; }
        public double posY { get; set; }
    }
}

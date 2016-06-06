using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace RA1_2013.Model
{
    [Serializable]
    public class LokalType
    {
        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Uri image_uri { get; set; }

        public LokalType(string key)
        {
            this.key = key;
        }

        public override string ToString()
        {
            return key;
        }
    }
}

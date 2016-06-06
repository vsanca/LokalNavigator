using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RA1_2013.Model
{
    [Serializable]
    public class Tag
    {
        public string label { get; set; }
        private String _color;
        public String color { get { return _color; } set { _color = value; } }

        public string description { get; set; }

        public Tag(string l, string d, string c)
        {
            label = l;
            description = d;
            color = c;
        }

        public Tag(string l)
        {
            label = l;
        }

        public override string ToString()
        {
            return label;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Tag))
            {
                Tag t = (Tag)obj;

                if (label.Equals(t.label) && color.Equals(t.color))
                    return true;
            }

            return false;
        }
    }
}

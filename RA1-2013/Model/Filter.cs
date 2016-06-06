using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA1_2013.Model
{
    public class Filter
    {
        public string value { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public DateTime date { get; set; }

        public Filter(string value, string type)
        {
            this.value = value;
            this.type = type;
            label = type + ": " + value;
            date = DateTime.Today;
        }
    }
}

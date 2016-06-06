using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RA1_2013.Dialogs
{
    public class NumberValidationRule : ValidationRule
    {
        public int Min
        {
            get;
            set;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is int)
            {
                int d = (int)value;
                if (d < Min) return new ValidationResult(false, "Vrednost mora biti veća od 0.");
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "Unesite ceo broj.");
            }
        }
    }

    public class StringValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string s = value as string;

            if(s==null || s.Trim().Equals(""))
            {
                return new ValidationResult(false, "Ovo polje je obavezno za unos.");
            }
            return new ValidationResult(true, null);
        }
    }

    public class UniqueTypeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string s = value as string;

            if(s!=null)
            foreach(Model.LokalType t in MainWindow.tip)
                if (s.Equals(t.key))
                {
                    return new ValidationResult(false, "Tip sa istim ključem već postoji.");
                }
            return new ValidationResult(true, null);
        }
    }

    public class UniqueTagValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string s = value as string;

            if (s != null)
                foreach (Model.Tag t in MainWindow.tags)
                    if (s.Equals(t.label))
                    {
                        return new ValidationResult(false, "Etiketa sa istim ključem već postoji.");
                    }
            return new ValidationResult(true, null);
        }
    }

    public class UniqueLokalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string s = value as string;

            if (s != null)
                foreach (Model.Lokal t in MainWindow.lokali)
                    if (s.Equals(t.key))
                    {
                        return new ValidationResult(false, "Lokal sa istim ključem već postoji.");
                    }
            return new ValidationResult(true, null);
        }
    }
}

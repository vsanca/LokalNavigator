using System;
using System.Web;
using System.Windows.Controls;

namespace RA1_2013
{
    public class AutoCompleteFocusableBox : System.Windows.Controls.AutoCompleteBox
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textbox = Template.FindName("TextBoxView", this) as TextBox;
            if (textbox != null) textbox.Focus();
        }
        
        public void setHelp(string help)
        {
            var textbox = Template.FindName("TextBoxView", this) as TextBox;
            if (textbox != null)
            {
                textbox.SetValue(HelpProvider.HelpKeyProperty, help);
            }
        }
    }
}

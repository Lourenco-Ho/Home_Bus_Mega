using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace MyLibrary
{
    public class StringValue: Control
    {
        private string _value;

        //proprety
        [Category("Data")]
        [Description("String Value.")]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnValueChange();
            }
        }

        //event
        public event EventHandler ValueChanged;

        protected virtual void OnValueChange()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        /*
         * private void String_ValueChanged(object sender, EventArgs e)
        {
            StringValue Value = sender as StringValue;
            Debug.WriteLine(Value.Value);
        }
        */
    }
}

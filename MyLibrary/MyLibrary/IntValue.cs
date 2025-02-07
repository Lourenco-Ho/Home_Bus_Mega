using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace MyLibrary
{
    public class IntValue:Control
    {
        private int _value;

        //proprety
        [Category("Data")]
        [Description("Int Value.")]
        public int Value
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
    }
}

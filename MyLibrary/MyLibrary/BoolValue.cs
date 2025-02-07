using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace MyLibrary
{
    class BoolValue : Control
    {
        private bool _value;

        //proprety
        [Category("Data")]
        [Description("Bool Value.")]
        public bool Value
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

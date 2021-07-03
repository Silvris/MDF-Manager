using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MDF_Manager
{
    public class TabHeaderViewModel
    {
        private string _header;
        private bool _isSelected = true;

        public string Header
        {
            get { return _header; }
            set
            {
                if (value == _header) return;
                _header = value;
#if NET50
                OnPropertyChanged("Header");
#else
                OnPropertyChanged();
#endif                
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
#if NET50
                OnPropertyChanged("IsSelected");
#else
                OnPropertyChanged();
#endif                
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

#if NET50
        protected virtual void OnPropertyChanged(string propertyName)
#else
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
#endif
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

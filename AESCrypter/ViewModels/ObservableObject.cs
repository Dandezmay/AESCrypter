using System;
using System.ComponentModel;

namespace AESCrypter.ViewModels
{
    class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property_name)
        {
            var handler = PropertyChanged;
            if (TypeDescriptor.GetProperties(this)[property_name] == null) throw new Exception("不存在的属性……");
            handler?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

    }
}

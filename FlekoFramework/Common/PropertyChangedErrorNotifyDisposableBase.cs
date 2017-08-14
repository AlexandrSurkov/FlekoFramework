using System.ComponentModel;

namespace Flekosoft.Common
{
    public class PropertyChangedErrorNotifyDisposableBase : ErrorNotifyDisposableBase, INotifyPropertyChanged
    {
        protected PropertyChangedErrorNotifyDisposableBase()
        {
        }

        protected PropertyChangedErrorNotifyDisposableBase(string name):base(name)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PropertyChanged = null;
            }
            base.Dispose(disposing);
        }
    }
}

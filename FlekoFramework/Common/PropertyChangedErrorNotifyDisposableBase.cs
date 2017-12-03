using System.ComponentModel;

namespace Flekosoft.Common
{
    public class PropertyChangedErrorNotifyDisposableBase : ErrorNotifyDisposableBase, INotifyPropertyChanged
    {
        protected PropertyChangedErrorNotifyDisposableBase()
        {
        }

        protected PropertyChangedErrorNotifyDisposableBase(string name) : base(name)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SendPropertyChangedEvent { get; set; } = true;
        protected void OnPropertyChanged(string propertyName)
        {
            if (SendPropertyChangedEvent)
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

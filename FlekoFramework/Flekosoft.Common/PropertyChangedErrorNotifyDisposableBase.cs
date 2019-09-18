using System.ComponentModel;

namespace Flekosoft.Common
{
    public class PropertyChangedErrorNotifyDisposableBase : ErrorNotifyDisposableBase, INotifyPropertyChanged
    {
        protected PropertyChangedErrorNotifyDisposableBase()
        {
        }

        protected PropertyChangedErrorNotifyDisposableBase(string instanceName) : base(instanceName)
        {
        }

        public bool IsNotifyPropertyChangedEnabled { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (!IsDisposing)
            {
                if (IsNotifyPropertyChangedEnabled) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
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

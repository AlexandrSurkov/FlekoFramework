using System.ComponentModel;

namespace Flekosoft.Common
{
    public class PropertyChangedErrorNotifyDisposableBase : ErrorNotifyDisposableBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
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

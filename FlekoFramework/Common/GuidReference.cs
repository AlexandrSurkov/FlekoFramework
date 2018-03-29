using System;

namespace Flekosoft.Common
{
    public abstract class GuidReference : PropertyChangedErrorNotifyDisposableBase
    {
        private Guid _guid = Guid.Empty;
        private bool _isValid;

        public Guid Guid
        {
            get => _guid;
            set
            {
                if (_guid != value)
                {
                    _guid = value;
                    IsValid = CheckReference();
                }

            }
        }

        public bool IsValid
        {
            get => _isValid;
            private set
            {
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
                OnInvalidityChanged(_isValid);
            }
        }

        protected abstract bool CheckReference();

        public event EventHandler<ValidityChangedEventArgs> ValidityChanged;
        protected void OnInvalidityChanged(bool isValid)
        {
            ValidityChanged?.Invoke(this, new ValidityChangedEventArgs(isValid));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ValidityChanged = null;
            }
            base.Dispose(disposing);
        }
    }
}

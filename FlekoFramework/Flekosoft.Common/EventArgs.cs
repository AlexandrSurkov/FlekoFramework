using System;

namespace Flekosoft.Common
{
    public class ValidityChangedEventArgs : EventArgs
    {
        public ValidityChangedEventArgs(bool isValid)
        {
            IsValid = isValid;
        }

        public bool IsValid { get; }
    }
}

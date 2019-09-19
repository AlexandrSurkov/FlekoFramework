using System;
using System.ComponentModel;
using Flekosoft.Common.Serialization;

namespace Flekosoft.Common
{
    public interface IUniqueEntity : INotifyPropertyChanged, IDisposable, ISerializable
    {
        Guid Guid { get; }
    }
}

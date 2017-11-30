using Flekosoft.Common.Collection;

namespace Flekosoft.Common.Serialization
{
    class SerializerCollection:ListCollection<Serializer<T>>
    {
        public SerializerCollection(ISerializabe parent) : base($"SerializerCollection of {parent}", true)
        {
            Parent = parent;
        }
        private ISerializabe Parent { get; }

        protected override bool InternalAdd(Serializer<T> item)
        {
            return base.InternalAdd(item);
        }
    }
}

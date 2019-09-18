using System;

namespace Flekosoft.Common.Serialization.Marshaling
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EndianAttribute : Attribute
    {
        public Endianness Endianness { get; }

        public EndianAttribute(Endianness endianness)
        {
            Endianness = endianness;
        }
    }
}

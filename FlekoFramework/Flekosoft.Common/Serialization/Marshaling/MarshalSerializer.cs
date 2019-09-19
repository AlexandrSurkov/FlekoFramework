using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Flekosoft.Common.Serialization.Marshaling
{
    /// <summary>
    /// Serialize class from/to array
    /// </summary>
    public static class MarshalSerializer
    {
        private static void RespectEndianness(Type type, byte[] data)
        {
            var fields = GetFields(type).Where(f => f.IsDefined(typeof(EndianAttribute), false)).Select(f => new
            {
                Field = f,
                Attribute = (EndianAttribute)f.GetCustomAttributes(typeof(EndianAttribute), false)[0],
                // ReSharper disable AssignNullToNotNullAttribute
                Offset = Marshal.OffsetOf(f.DeclaringType, f.Name).ToInt32()
                // ReSharper restore AssignNullToNotNullAttribute
            }).ToList();

            foreach (var field in fields)
            {
                if ((field.Attribute.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian) ||
                    (field.Attribute.Endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian))
                {
                    Array.Reverse(data, field.Offset, Marshal.SizeOf(field.Field.FieldType));
                }
            }


        }

        static IEnumerable<FieldInfo> GetFields(Type type)
        {
            var fields = new List<FieldInfo>();

            if (type.BaseType != null)
            {
                fields.AddRange(GetFields(type.BaseType));
            }
            fields.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));

            return fields;
        }

        public static byte[] RawSerialize(object anything)
        {

            var rawData = new byte[Marshal.SizeOf(anything)];

            var handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                var buffer = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(anything, buffer, false);
            }
            finally
            {
                handle.Free();
            }

            RespectEndianness(anything.GetType(), rawData);

            return rawData;
        }

        public static T RawDeserialize<T>(byte[] rawData)
        {
            if (rawData == null)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().GetParameters()[0].Name);

            int rawSize = Marshal.SizeOf(typeof(T));
            if (rawSize > rawData.Length)
                return default;

            object retObj;

            RespectEndianness(typeof(T), rawData);

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr buffer = handle.AddrOfPinnedObject();
                retObj = Marshal.PtrToStructure(buffer, typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return (T)retObj;
        }
    }
}

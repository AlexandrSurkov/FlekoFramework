using System;
using System.Runtime.InteropServices;
using Flekosoft.Common.Serialization.Marshaling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Serialization.Marshaling
{
    [TestClass]
    public class MarshalSerializerTests
    {
        [TestMethod]
        public void MarshalSerializerTest()
        {
            var testClass = new ClassForTestsOfMarshalSerializer
            {
                B = 1,
                UsBe = 2,
                UsLe = 3,
                SBe = -3,
                SLe = -4,
                UiBe = 5,
                UiLe = 6,
                IBe = -7,
                ILe = 8,
                UlBe = 9,
                UlLe = 10,
                LBe = -11,
                LLe = -12,
                FBe = 1.2345f,
                FLe = -2.3456f,
                DBe = 3.4567,
                DLe = -4.5678f,
                Array = new byte[64]
            };

            for (int i = 0; i < 64; i++)
            {
                testClass.Array[i] = (byte)i;
            }

            var buf = MarshalSerializer.RawSerialize(testClass);

            var deserializedObject = MarshalSerializer.RawDeserialize<ClassForTestsOfMarshalSerializer>(buf);

            Assert.AreEqual(testClass.B, deserializedObject.B);
            Assert.AreEqual(testClass.UsBe, deserializedObject.UsBe);
            Assert.AreEqual(testClass.UsLe, deserializedObject.UsLe);
            Assert.AreEqual(testClass.SBe, deserializedObject.SBe);
            Assert.AreEqual(testClass.SLe, deserializedObject.SLe);
            Assert.AreEqual(testClass.UiBe, deserializedObject.UiBe);
            Assert.AreEqual(testClass.UiLe, deserializedObject.UiLe);
            Assert.AreEqual(testClass.IBe, deserializedObject.IBe);
            Assert.AreEqual(testClass.ILe, deserializedObject.ILe);
            Assert.AreEqual(testClass.UlBe, deserializedObject.UlBe);
            Assert.AreEqual(testClass.UlLe, deserializedObject.UlLe);
            Assert.AreEqual(testClass.LBe, deserializedObject.LBe);
            Assert.AreEqual(testClass.LLe, deserializedObject.LLe);
            Assert.AreEqual(testClass.FBe, deserializedObject.FBe);
            Assert.AreEqual(testClass.FLe, deserializedObject.FLe);
            Assert.AreEqual(testClass.DBe, deserializedObject.DBe);
            Assert.AreEqual(testClass.DLe, deserializedObject.DLe);
            Assert.AreEqual(testClass.Array.Length, deserializedObject.Array.Length);

            for (int i = 0; i < 64; i++)
            {
                Assert.AreEqual(testClass.Array[i], deserializedObject.Array[i]);
            }

        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable]
    class ClassForTestsOfMarshalSerializer
    {
        private byte _b;
        [Endian(Endianness.BigEndian)]
        private ushort _usBe;
        [Endian(Endianness.LittleEndian)]
        private ushort _usLe;
        [Endian(Endianness.BigEndian)]
        private short _sBe;
        [Endian(Endianness.LittleEndian)]
        private short _sLe;

        [Endian(Endianness.BigEndian)]
        private uint _uiBe;
        [Endian(Endianness.LittleEndian)]
        private uint _uiLe;
        [Endian(Endianness.BigEndian)]
        private int _iBe;
        [Endian(Endianness.LittleEndian)]
        private int _iLe;

        [Endian(Endianness.BigEndian)]
        private ulong _ulBe;
        [Endian(Endianness.LittleEndian)]
        private ulong _ulLe;
        [Endian(Endianness.BigEndian)]
        private long _lBe;
        [Endian(Endianness.LittleEndian)]
        private long _lLe;

        [Endian(Endianness.BigEndian)]
        private float _fBe;
        [Endian(Endianness.LittleEndian)]
        private float _fLe;
        [Endian(Endianness.BigEndian)]
        private double _dBe;
        [Endian(Endianness.LittleEndian)]
        private double _dLe;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        private byte[] _array;


        public byte B
        {
            get => _b;
            set => _b = value;
        }

        public ushort UsBe
        {
            get => _usBe;
            set => _usBe = value;
        }

        public ushort UsLe
        {
            get => _usLe;
            set => _usLe = value;
        }

        public short SBe
        {
            get => _sBe;
            set => _sBe = value;
        }

        public short SLe
        {
            get => _sLe;
            set => _sLe = value;
        }

        public uint UiBe
        {
            get => _uiBe;
            set => _uiBe = value;
        }

        public uint UiLe
        {
            get => _uiLe;
            set => _uiLe = value;
        }

        // ReSharper disable once InconsistentNaming
        public int IBe
        {
            get => _iBe;
            set => _iBe = value;
        }

        // ReSharper disable once InconsistentNaming
        public int ILe
        {
            get => _iLe;
            set => _iLe = value;
        }

        public ulong UlBe
        {
            get => _ulBe;
            set => _ulBe = value;
        }

        public ulong UlLe
        {
            get => _ulLe;
            set => _ulLe = value;
        }

        public long LBe
        {
            get => _lBe;
            set => _lBe = value;
        }

        public long LLe
        {
            get => _lLe;
            set => _lLe = value;
        }

        public float FBe
        {
            get => _fBe;
            set => _fBe = value;
        }

        public float FLe
        {
            get => _fLe;
            set => _fLe = value;
        }

        public double DBe
        {
            get => _dBe;
            set => _dBe = value;
        }

        public double DLe
        {
            get => _dLe;
            set => _dLe = value;
        }

        public byte[] Array
        {
            get => _array;
            set => _array = value;
        }
    }
}

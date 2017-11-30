using System;

namespace Flekosoft.Common.Math
{
    public static class Crc8
    {
        public enum Crc8Type
        {
            Crc8 = 0xd5,
            Crc8Ccitt = 0x07,
            Crc8DallasMaxim = 0x31,
            Crc8SaeJ1850 = 0x1D,
            Crc8Wcdma = 0x9b,
        };


        private static byte[] GenerateTable(Crc8Type type)
        {
            byte[] csTable = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)type;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                csTable[i] = (byte)curr;
            }

            return csTable;
        }

        /// <summary>
        /// Calculate crc
        /// </summary>
        /// <param name="type">Polinom type</param>
        /// <param name="source">source for calculation</param>
        /// <param name="length">num of elements</param>
        /// <param name="offset">array offset from begining</param>
        /// <returns></returns>
        public static byte Calculate(Crc8Type type, byte[] source, int length, int offset)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.Length < offset + length)
                throw new ArgumentException($"{nameof(offset)} and {nameof(length)}");

            byte[] table = GenerateTable(type);

            byte c = 0;

            for (int i = 0; i < offset + length; i++)
            {
                c = table[c ^ source[i + offset]];
            }
            return c;
        }
    }
}

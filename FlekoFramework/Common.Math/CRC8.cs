using System;

namespace Flekosoft.Common.Math
{
    // ReSharper disable once InconsistentNaming
    public static class CRC8
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
        /// Рассчитать контрольную сумму
        /// </summary>
        /// <param name="type">Тип полинома</param>
        /// <param name="source">исходная последовательность для рассчета</param>
        /// <param name="length">количество элементов для рассчета</param>
        /// <param name="offset">смещение относительно начала для начала рассчета</param>
        /// <returns></returns>
        public static byte Calculate(Crc8Type type , byte[] source, int length, int offset)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (source.Length < offset + length)
                throw new ArgumentException("offset and length");

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

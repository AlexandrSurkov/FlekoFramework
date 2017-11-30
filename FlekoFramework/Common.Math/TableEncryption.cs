//using System;
//using System.Text;

//namespace Flekosoft.Common.Math
//{
//    /// <summary>
//    /// Шифровальщик лицензий
//    /// </summary>
//    public static class TableEncryption
//    {
//        #region StringConversion
//        /// <summary>
//        /// Таблица для преобразования в строку и обратно
//        /// </summary>
//        private static readonly char[] Table = { 'Q', 'A', 'Z', 'X', 'S', 'W', '1', '2', 'E', 'D', 'C', '3', 'V', 'F', 'R', '4', 'B', 'G', 'T', '5', 'N', 'H', 'Y', '6', 'U', 'J', 'M', '7', 'I', 'K', '8', 'L' };

//        /// <summary>
//        /// Проверка таблицы на повторяющиеся символы
//        /// </summary>
//        /// <param name="table"></param>
//        /// <returns></returns>
//        private static bool CheckTableArray(char[] table)
//        {
//            // check table  // не должно быть повторений.
//            foreach (var c in table)
//            {
//                //Смотрим чтобы быти только заглавные буквы или цифпы
//                if (!(((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9'))))
//                {
//                    throw new ArgumentOutOfRangeException("CreateStringFromHex,  table error symbol " + c);
//                }

//                //Смотрим чтобы не было повторений
//                int count = 0;
//                // ReSharper disable once LoopCanBeConvertedToQuery
//                foreach (var z in table)
//                {
//                    if (z == c) count++;
//                }
//                //Повторения нашлись?
//                if (count > 1)
//                {
//                    throw new ArgumentOutOfRangeException("CreateStringFromHex, table symbol duplicated  " + c);
//                }
//            }
//            return true;
//        }

//        internal static string HexToString(byte[] src, bool useRandomeXor)
//        {
//            return HexToString(src, useRandomeXor, 4);
//        }

//        /// <summary>
//        /// Формирование строки из массива байт.
//        /// 
//        /// "идея простая, цифры + буквы = 36 символов, значит можно использовать 5 бит = 32 символа.
//        /// таким образом нарезаем из входящего массива по 5 байт и делаем из них 8 индексов по 5 бит."
//        /// 
//        /// </summary>
//        /// <param name="src">length must be >5,  5-10-15-20-25</param>
//        /// <param name="lengthBytesCount">количество байт под длинну данных</param>
//        /// <remarks>
//        /// Формирование строки происзодит следующим образом:
//        /// Длинна входная строки должна быть кратна 5. 
//        /// 
//        /// Из входной строки формируется таблицы по 5 байт:
//        /// 
//        ///            _______биты________
//        ///          0  1  2  3  4  5  6  7  8
//        ///       0|
//        ///       1|
//        /// байты 2|
//        ///       3|
//        ///       4|
//        /// 
//        /// Для преобразованя в строку берется столбец. 5 Бит столбца являются индексом символа в таблице.
//        /// Таким образом, 5 байт превращаются в 8 символов.
//        /// 
//        /// Так преобразуеются все входные байты. Если входная строка не кратна 5 байтам, то она дополняется мусором.
//        /// Длина строки без мусора кодируется 4 первыми байтами
//        /// Последний байт - случайное число, которое ксорится с остальными, чтобы результаты были не похожи друг на друга
//        /// 
//        /// Если useRandomeXor=true, то в результат подмешивается случайное число, чтобы результаты не были похожи друг на друга
//        ///  </remarks>
//        /// <returns></returns>
//        internal static string HexToString(byte[] src, bool useRandomeXor, int lengthBytesCount)
//        {
//            if (src == null) return null;
//            if (Table == null) return null;
//            if (Table.Length < (32)) return null;
//            if (!CheckTableArray(Table)) return null;

//            var rnd = new System.Random((int)DateTime.Now.Ticks);

//            byte xorByte = 0;
//            if (useRandomeXor)
//            {
//                xorByte = (byte)rnd.Next(byte.MaxValue);
//            }


//            //Формируем новый массив с случайным байтом и длиной в начале
//            byte[] appendedSrc = new byte[src.Length + lengthBytesCount + 1];

//            byte[] lenArr;
//            switch (lengthBytesCount)
//            {
//                case 1:
//                    lenArr = BitConverter.GetBytes((byte)src.Length);
//                    break;
//                case 2:
//                    lenArr = BitConverter.GetBytes((short)src.Length);
//                    break;
//                case 4:
//                    lenArr = BitConverter.GetBytes((int)src.Length);
//                    break;
//                default:
//                    throw new ArgumentException(nameof(lengthBytesCount));
//            }

//            //if (src.Length < 5)
//            //{
//            //    //Если сообщение меньше 5 байт длиной - то добавляем все в начало

//            //    appendedSrc[0] = (byte)0;

//            for (int i = 0; i < lengthBytesCount; i++)
//            {
//                appendedSrc[i + 1] = lenArr[i];
//            }

//            for (int i = lengthBytesCount + 1; i < appendedSrc.Length; i++)
//            {
//                appendedSrc[i] = src[i - (lengthBytesCount + 1)];
//            }
//            //}
//            //else
//            //{
//            //    //Иначе равномерно замешиваем
//            //    var delta = src.Length / 4;

//            //    appendedSrc[1] = (byte) delta;

//            //    appendedSrc[2 * delta] = lenArr[0];
//            //    appendedSrc[3 * delta] = lenArr[1];
//            //    appendedSrc[4 * delta] = lenArr[2];
//            //    appendedSrc[5 * delta] = lenArr[3];

//            //    for (int i = 1; i < appendedSrc.Length; i++)
//            //    {
//            //        if (i == 1 * delta) continue;
//            //        if (i == 2 * delta) continue;
//            //        if (i == 3 * delta) continue;
//            //        if (i == 4 * delta) continue;
//            //        appendedSrc[i] = src[i - 5];
//            //    }
//            //}



//            //Делаем xor для того чтобы не повторялось все
//            for (int i = 1; i < appendedSrc.Length; i++)
//            {
//                appendedSrc[i] ^= xorByte;
//            }

//            appendedSrc[0] = xorByte;



//            byte[] src2;

//            // нормируем длинну массива
//            if ((appendedSrc.Length % 5) != 0)
//            {
//                // создаём массив >src и кратный 5
//                src2 = new byte[appendedSrc.Length + (5 - appendedSrc.Length % 5)];
//                int i;
//                for (i = 0; i < appendedSrc.Length; i++)
//                    src2[i] = appendedSrc[i];  // копируем старый массив
//                for (; i < src2.Length; i++)
//                    src2[i] = (byte)((i << 6) + (i << 3) + i); // добиваем массив мусором
//            }
//            else
//            {
//                src2 = appendedSrc;
//            }

//            // формируем массив индексов
//            int len = (src2.Length / 5) * 8;
//            byte[] indexes = new byte[len];
//            int counter = 0;
//            for (int i = 0; i < src2.Length; i += 5)
//                for (int j = 0; j < 8; j++)
//                {
//                    var sum = (byte)(
//                        (((src2[i + 0] >> j) & 0x1) << 0)
//                        | (((src2[i + 1] >> j) & 0x1) << 1)
//                        | (((src2[i + 2] >> j) & 0x1) << 2)
//                        | (((src2[i + 3] >> j) & 0x1) << 3)
//                        | (((src2[i + 4] >> j) & 0x1) << 4));

//                    if (counter >= indexes.Length)
//                    {
//#if DEBUG
//                        Console.WriteLine(String.Format("Error. Lic.CreateStringFromHex counter({0})>indexes.Length({1})", counter, indexes.Length));
//#endif
//                        break;
//                    }
//                    indexes[counter++] = sum;
//                }

//            // формирование символьной строки из индексов
//            StringBuilder rez = new StringBuilder();
//            for (int i = 0; i < len; i++)
//                rez.Append(Table[indexes[i]]);

//            return rez.ToString();
//        }

//        internal static byte[] StringToHex(string src)
//        {
//            return StringToHex(src, 4);
//        }

//        /// <summary>
//        /// Преобразвание последовательности из строки в байты. См. CreateStringFromHex
//        /// </summary>
//        /// <param name="src"></param>
//        /// <param name="lengthBytesCount">количество байт под длинну данных</param>
//        /// <returns></returns>
//        internal static byte[] StringToHex(string src, int lengthBytesCount)
//        {
//            if (src == null) return null;
//            if (Table == null) return null;
//            if (Table.Length < (2 ^ 5)) return null;
//            if (!CheckTableArray(Table)) return null;
//            if ((src.Length % 8) != 0) throw new Exception("StringToHex: src must be divisible by 8 ");

//            // реконструируем массив индексов из строки
//            byte[] indexes = new byte[src.Length];
//            int j;
//            for (int i = 0; i < src.Length; i++)
//            {
//                for (j = 0; j < Table.Length; j++)
//                    if (Table[j] == src[i])
//                    {
//                        indexes[i] = (byte)j;
//                        break;
//                    }
//                if (j >= Table.Length)
//                {
//                    // не нашли ничего, дальше работать не возможно
//#if DEBUG
//                    Console.WriteLine("CreateHexFromString error: not finded: " + src[i]);
//#endif
//                    return null;
//                }
//            }


//            // восстанавливаем массив данных из массив индексов
//            int len = (src.Length / 8) * 5;
//            byte[] dst = new byte[len];
//            int counter = 0;
//            for (int i = 0; i < src.Length; i += 8)
//                for (j = 0; j < 5; j++)
//                {
//                    var sum = (byte)(
//                        (((indexes[i + 0] >> j) & 0x1) << 0)
//                        | (((indexes[i + 1] >> j) & 0x1) << 1)
//                        | (((indexes[i + 2] >> j) & 0x1) << 2)
//                        | (((indexes[i + 3] >> j) & 0x1) << 3)
//                        | (((indexes[i + 4] >> j) & 0x1) << 4)
//                        | (((indexes[i + 5] >> j) & 0x1) << 5)
//                        | (((indexes[i + 6] >> j) & 0x1) << 6)
//                        | (((indexes[i + 7] >> j) & 0x1) << 7));
//                    if (counter >= len)
//                    {
//#if DEBUG
//                        Console.WriteLine(String.Format("Error. Lic.CreateStringFromHex counter({0})>indexes.Length({1})", counter, len));
//#endif
//                        break;
//                    }
//                    dst[counter++] = sum;
//                }


//            //Получаем длину из результата

//            var xorByte = dst[0];

//            for (int i = 1; i < dst.Length; i++)
//            {
//                dst[i] ^= xorByte;
//            }

//            //if (dst.Length < 5 + 4 + 1)
//            //{
//            //Короткие данные - нет замешивания
//            int srcLen = 0;
//            switch (lengthBytesCount)
//            {
//                case 1:
//                    srcLen = dst[1];
//                    break;
//                case 2:
//                    srcLen = BitConverter.ToInt16(dst, 1);
//                    break;
//                case 4:
//                    srcLen = BitConverter.ToInt32(dst, 1);
//                    break;
//                default:
//                    throw new ArgumentException(nameof(lengthBytesCount));
//            }

//            byte[] result = new byte[srcLen];

//            for (int i = 0; i < result.Length; i++)
//            {
//                result[i] = dst[i + lengthBytesCount + 1];
//            }
//            //}
//            //else
//            //{
//            //    //Длина равномерна замешена

//            //}

//            return result;
//        }

//        #endregion

//        #region Encryption

//        /// <summary>
//        /// Шифруем src с ключем key и формируем результат в символьном виде
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="keyLengthBytesCount">количество байт длинны ключа</param>
//        /// <param name="src"></param>
//        /// <param name="resultLengthBytesCount">количсество байт длины реультата</param>
//        /// <returns></returns>
//        public static string Encrypt(string key, int keyLengthBytesCount, byte[] src, int resultLengthBytesCount)
//        {
//            try
//            {
//                var rez = Convert(key, keyLengthBytesCount, src);
//                if (rez == null) return null;
//                return HexToString(rez, true, resultLengthBytesCount);
//            }
//            catch (Exception ex)
//            {
//                //Logger.Instance.AppendLog(ex.Message);
//                throw;
//            }
//        }

//        /// <summary>
//        /// Расшифровываем данные из символьного вида src с помощью ключа key
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="keyLengthBytesCount">количество байт длинны ключа</param>
//        /// <param name="src"></param>
//        /// <param name="resultLengthBytesCount">количсество байт длины реультата</param>
//        /// <returns></returns>
//        public static byte[] Decrypt(string key, int keyLengthBytesCount, string src, int resultLengthBytesCount)
//        {
//            try
//            {
//                var srcString = StringToHex(src, resultLengthBytesCount);
//                if (srcString == null) return null;
//                return Convert(key, keyLengthBytesCount, srcString);
//            }
//            catch (Exception ex)
//            {
//                //Logger.Instance.AppendLog(ex.Message);
//                throw;
//            }
//        }

//        /// <summary>
//        /// Шифрование или расшифровка последовательности Src с помощью ключа Key
//        /// </summary>
//        /// <param name="key"> ключ в символьном виде</param>
//        /// <param name="keyLengthBytesCount">количество байт длинны ключа</param>
//        /// <param name="src"> входная последовательность которую нужно зашивровать или расшифровать</param>
//        /// <remarks>
//        /// Так как шифрование - результат операции XOR то одной и той же оперцией можно как рашифровать так и расшифровать ключ.
//        /// </remarks>
//        /// <returns></returns>
//        private static byte[] Convert(string key, int keyLengthBytesCount, byte[] src)
//        {
//            // Получаем байтовое значение ключа
//            var keyByte = StringToHex(key, keyLengthBytesCount);
//            if (keyByte == null) return null;

//            // Создаем последовательность для XOR из байтовых данных ключа. 
//            //В результате получаем последовательность равную по длинне исходному сообщению

//            var keyShuffled = CreateShuffledKey(keyByte, src.Length);
//            if (keyShuffled == null) return null;

//            // ксорим
//            byte[] rez = new byte[src.Length];
//            for (int i = 0; i < src.Length; i++)
//                rez[i] = (byte)(src[i] ^ keyShuffled[i]);

//            return rez;
//        }

//        /// <summary>
//        /// Формирование последовательности заданной длинны на основе исходного дначения
//        /// </summary>
//        /// <param name="src"></param>
//        /// <param name="lenNeeded"></param>
//        /// <remarks>
//        /// тасуем и формируем последовательность заданной длинны из ключа
//        /// суть задачи, перетасовать ключь и размножить его до длинны кодируемого сообщения, без прямого циклического повторения
//        /// </remarks>
//        /// <returns></returns>
//        private static byte[] CreateShuffledKey(byte[] src, int lenNeeded)
//        {

//            byte[] src1 = new byte[src.Length];

//            // перемешаем биты по карте
//            for (int i = 0; i < src.Length; i++)
//            {
//                src1[i] = (byte)(
//                        (((src[i] >> 4) & 0x1) << 0)
//                        | (((src[i] >> 1) & 0x1) << 1)
//                        | (((src[i] >> 5) & 0x1) << 2)
//                        | (((src[i] >> 3) & 0x1) << 3)
//                        | (((src[i] >> 6) & 0x1) << 4)
//                        | (((src[i] >> 0) & 0x1) << 5)
//                        | (((src[i] >> 7) & 0x1) << 6)
//                        | (((src[i] >> 2) & 0x1) << 7)
//                    );
//            }


//            if (lenNeeded == src1.Length) return src1;

//            byte[] newKey = new byte[lenNeeded];
//            if (lenNeeded < src1.Length)
//            {

//                for (int i = 0; i < lenNeeded; i++)
//                    newKey[i] = src1[i];

//                int offset = 0;
//                // сворачиваем последовательность до нужной длинны
//                for (int i = 0; i < src1.Length - lenNeeded; i++)
//                {
//                    newKey[offset++] ^= src1[src1.Length - 1 - i];
//                    if (offset >= lenNeeded) offset = 0;
//                }
//            }
//            else
//            {
//                //lenNeeded > src.Length
//                // перемешаем байты и добавим до запрошенной длинны
//                int offsetDst = 0;
//                int offsetSrc = 0;
//                while (offsetDst < lenNeeded)
//                {
//                    //прямой порядок
//                    newKey[offsetDst++] = src1[offsetSrc++];
//                    if (offsetSrc >= src1.Length) break;
//                }



//                //xor двух байт
//                int index1, index2;
//                int i, j;
//                //Console.WriteLine("\r\n-------------");
//                for (int offset = 1; (offset < src1.Length) && (offsetDst < lenNeeded); offset++)
//                {
//                    for (i = 0; (i <= offset) && ((i + offset) < src1.Length) && (offsetDst < lenNeeded); i++)
//                    {
//                        for (j = 0; (j <= src1.Length) && (offsetDst < lenNeeded); j += (1 + offset))
//                        {
//                            index1 = j + i;
//                            index2 = j + i + offset;
//                            if (index2 >= src1.Length) break;

//                            //Console.Write((index1).ToString() + "-" + (index2).ToString() + "   ");

//                            newKey[offsetDst++] = (byte)(src1[index1] ^ src1[index2]);
//                        }
//                        //Console.WriteLine("\r\n-------------");
//                    }
//                    //Console.WriteLine("===================");
//                }

//                //xor трёх байт
//                int index3;
//                //Console.WriteLine("\r\n-------------");
//                for (int step = 1; (step * 2 < src1.Length) && (offsetDst < lenNeeded); step++)
//                {
//                    for (i = 0; (i <= step * 2) && ((i + step * 2) < src1.Length) && (offsetDst < lenNeeded); i++)
//                    {
//                        for (j = 0; (j <= src1.Length) && (offsetDst < lenNeeded); j += (1 + step * 2))
//                        {
//                            index1 = j + i;
//                            index2 = j + i + step;
//                            index3 = j + i + step * 2;
//                            if (index3 >= src1.Length) break;

//                            //Console.Write((index1).ToString() + "-" + (index2).ToString() +"-" + (index3).ToString() + "   ");

//                            newKey[offsetDst++] = (byte)(src1[index1] ^ src1[index2] ^ src1[index3]);
//                        }
//                        //Console.WriteLine("\r\n-------------");
//                    }
//                    //Console.WriteLine("===================" + offsetDst.ToString());
//                }


//                // скручиваем первый с последним, пока не заполним полностью.
//                int lastLen, iDelta = 0;
//                while (offsetDst < lenNeeded)
//                {
//                    lastLen = offsetDst - 1;
//                    for (i = iDelta++, j = lastLen; (i < j) && (offsetDst < lenNeeded); i++, j--)
//                    {
//                        newKey[offsetDst++] = (byte)(newKey[i] ^ newKey[j]);
//                        //Console.Write((i).ToString() + "-" + (j).ToString()+ "   ");
//                    }
//                    //Console.WriteLine("\r\n===================" + offsetDst.ToString());
//                }
//            }
//            return newKey;
//        }

//        #endregion
//    }
//}

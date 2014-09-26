using System;
using System.Collections.Generic;
using System.Text;

namespace PTISignal.Helper
{
    class Tool
    {
        static int[] fieldWidth = new int[] { 1 * 7, 1 * 3, 3 * 4, 3 * 4, 2 * 4, 1 * 4, 1 * 1, 1 * 2, 1 * 1, 4 * 4, 2 * 4, 3 * 4, 1 * 2, 1 * 8 };
        static bool[] BCDEnable = new bool[] { false, false, true, true, true, true, false, false, false, true, true, true, false, false };

        public static void BitArray2ByteArray(byte[] bitArray,byte[] byteArray)
        {
            byte data=0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (i > 0 && ((i % 8) == 0))
                {
                    if (i / 8 <= byteArray.Length)
                    {
                        byteArray[i / 8 - 1] = data;
                    }
                }
                data = (byte)((data << 1) + bitArray[i]);
            }
            if (bitArray.Length / 8 < byteArray.Length)
            {
                byteArray[bitArray.Length / 8] = data;
            }

        }
        public static void ByteArray2BitArray(byte[] bitArray,byte[] byteArray,int offset,int count )
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bitArray[i * 8 + j] = (byte)((byteArray[offset + i] >> (7 - j)) & 0x01);
                }
            }
        }

        public static int FieldValue(byte[] bitArray, int index)
        {
            int startIndex = 0;
            for (int i = 0; i < index; i++)
            {
                startIndex += fieldWidth[i];
            }
            int val = 0;
            if (BCDEnable[index])
            {
                int tmp = 0;
                for (int i = 0; i < fieldWidth[index]; i++)
                {
                    if ((i % 4) == 0)
                    {
                        val = val * 10 + tmp;
                        tmp = 0;
                    }
                    tmp = (tmp << 1) + bitArray[startIndex + i];
                }
                val = val * 10 + tmp;
            }
            else
            {

                for (int i = 0; i < fieldWidth[index]; i++)
                {
                    val = (val << 1) + bitArray[startIndex + i];
                }
            }
            return val;
        }

        public static void Field2BitArray(byte[] bitArray, int[] fieldVal)
        {
            int startIndex = 0;
            for (int i = 0; i < fieldVal.Length; i++)
            {
                if (BCDEnable[i])
                {
                    int div = fieldVal[i];
                    int tmp =0;
                    for (int j = 0; j < fieldWidth[i]; j++)
                    {
                        if ((j % 4) == 0)
                        {
                            div = fieldVal[i];
                            for (int k = 0; k < (fieldWidth[i] - j) / 4-1; k++)
                            {
                                div = div / 10;
                            }
                            div = div % 10;
                            tmp=3;
                        }
                        bitArray[startIndex] = (byte)((div >> tmp)&0x01);
                        startIndex++;
                        tmp--;
                    }
                }
                else
                {
                    for (int j = 0; j < fieldWidth[i]; j++)
                    {
                        bitArray[startIndex] = (byte)((fieldVal[i] >> (fieldWidth[i] - j - 1)) & 0x01);
                        startIndex++;
                    }
                }

            }
        }


        

    }
}

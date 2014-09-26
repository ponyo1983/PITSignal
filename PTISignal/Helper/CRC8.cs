using System;
using System.Collections.Generic;
using System.Text;

namespace PTISignal.Helper
{
    class CRC8
    {
        public const byte CRCExpress = 0x85;
        public static byte ComputeCRC8(byte[] data, int len, byte ploy)
        {

            byte i, tmpt8;
            uint crct16;

            crct16 = (uint)(data[0] << 8);

            for (int k = 0; k < len; k++)
            {
                if (k == (len - 1))
                    tmpt8 = 0;
                else
                    tmpt8 = data[k + 1];

                crct16 += tmpt8;
                for (i = 0; i < 8; i++)
                {
                    if ((crct16 & 0x8000) != 0)
                    {
                        crct16 <<= 1;
                        tmpt8 = (byte)(crct16 >> 8);
                        tmpt8 ^= ploy;  //0x1021;
                        crct16 = (uint)((crct16 & 0x00ff) + (tmpt8 << 8));
                    } /* 余式CRC 乘以2 再求CRC */

                    else
                        crct16 <<= 1;

                }
            }
            tmpt8 = (byte)(crct16 >> 8);
            return (tmpt8);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PTISignal.Protocol
{
    class FrameManager
    {


        private static List<FrameUnit> listArray = new List<FrameUnit>();

        static FrameManager()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">业务类型</param>
        /// <param name="match">匹配</param>
        /// <returns></returns>
        public static FrameUnit CreateFrameUnit(byte type)
        {
            FrameUnit frameUnit = new FrameUnit(type);
            listArray.Add(frameUnit);

            return frameUnit;
        }

        public static void DeleteFrameUnit(FrameUnit frameUinit)
        {

            lock (listArray)
            {
                if (listArray.Contains(frameUinit))
                {
                    listArray.Remove(frameUinit);
                }
            }

        }

        public static void CheckFrame(byte[] originFrame, int frameLength)
        {

            lock (listArray)
            {
                for (int i = 0; i < listArray.Count; i++)
                {
                    if (listArray[i].IsMatch(originFrame, frameLength))
                    {
                        listArray[i].WriteData(originFrame, frameLength);
                        listArray[i].SetEvent();
                    }
                }
            }
        }

    }
}

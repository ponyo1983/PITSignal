using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PTISignal.Protocol
{
    class FrameUnit
    {


        const int FrameLength = 2 * 1024;//最大2KB
        private ReaderWriterLock rwl = new ReaderWriterLock();
        private byte[] frameData = new byte[FrameLength];
        private int frameLength = 0;
        private byte matchData = 0;

        private AutoResetEvent frameEvent = new AutoResetEvent(false);

        public FrameUnit(byte type)
        {
            matchData = type;

        }

        public int TotalLength
        {
            get
            {
                return frameLength;
            }
        }

        public int DataLength
        {
            get
            {
                int len = frameLength - 8;
                if (len < 0) return 0;
                return len;
            }
        }

        /// <summary>
        /// 匹配算法 matchData数组的含义{matchIndex,matchValue},索引从真正的数据开始
        /// </summary>
        /// <param name="originFrame"></param>
        /// <param name="frameLength"></param>
        /// <returns></returns>
        public bool IsMatch(byte[] originFrame, int frameLength)
        {
            return (originFrame[5] == matchData ? true : false);

        }

        public void SetEvent()
        {
            frameEvent.Set();
        }


        public bool WaitData(int time)
        {
            //frameEvent.Reset();
            if (time < 0)
            {
                return frameEvent.WaitOne();
            }
            return frameEvent.WaitOne(time, false);

        }

        public void ClearEvent()
        {
            frameEvent.Reset();
        }
        public bool WaitData(int time, bool clear)
        {
            if (clear)
            {
                frameEvent.Reset();
            }

            if (time < 0)
            {
                return frameEvent.WaitOne();
            }
            return frameEvent.WaitOne(time, false);

        }

        public static int WaitAnyFrame(FrameUnit[] frames, int time)
        {
            WaitHandle[] waitHandle = new WaitHandle[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                waitHandle[i] = frames[i].frameEvent;
            }
            if (time < 0)
            {
                return EventWaitHandle.WaitAny(waitHandle);
            }
            return EventWaitHandle.WaitAny(waitHandle, time, false);

        }

        public static bool WaitAllFrame(FrameUnit[] frames, int time)
        {
            WaitHandle[] waitHandle = new WaitHandle[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                waitHandle[i] = frames[i].frameEvent;
            }
            if (time < 0)
            {
                return EventWaitHandle.WaitAll(waitHandle);
            }
            return EventWaitHandle.WaitAll(waitHandle, time, false);
        }

        /// <summary>
        /// 读取整个数据帧(包括帧起始标志，CRC，帧结束标志)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int ReadTotalData(byte[] data, int count)
        {
            int min = 0;
            try
            {
                rwl.AcquireReaderLock(1000);
                min = count > frameLength ? frameLength : count;
                Array.Copy(frameData, data, min);
                rwl.ReleaseReaderLock();
            }
            catch (ApplicationException)
            {

            }
            return min;

        }
        /// <summary>
        /// 读取实际的数据部分
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int ReadRealData(byte[] data, int count)
        {
            int min = 0;
            try
            {
                rwl.AcquireReaderLock(1000);
                min = count > DataLength ? DataLength : count;
                Array.Copy(frameData, 4, data, 0, min);
                rwl.ReleaseReaderLock();
            }
            catch (ApplicationException)
            {

            }
            return min;
        }

        public void WriteData(byte[] frame, int length)
        {
            if (length > FrameLength) return;
            try
            {
                rwl.AcquireWriterLock(1000);
                Array.Copy(frame, frameData, length);
                frameLength = length;
                rwl.ReleaseWriterLock();
            }
            catch (ApplicationException)
            {

            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace PTISignal.Protocol
{
    class SerialManager
    {
        private static SerialPort serialPort = new SerialPort();
        private static AutoResetEvent serialPortEvent = new AutoResetEvent(false);
        private static Thread threadReceive = null;

        static byte[] frameData = new byte[2 * 1024];
        static byte[] originalData = new byte[2 * 1024];
        static byte[] tranmitData = new byte[2 * 1024];


        static SerialManager()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        static void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            serialPortEvent.Set();
        }
        public static bool Open(string port,int baudrate)
        {
            if (serialPort.IsOpen) return true;
            try
            {
                serialPort.PortName=port;
                serialPort.BaudRate=baudrate;
                serialPort.Open();
                if (threadReceive != null && threadReceive.IsAlive)
                {
                    threadReceive.Abort();
                }
                threadReceive = new Thread(new ThreadStart(ProcReceive));
                threadReceive.IsBackground = true;
                threadReceive.Start();

                return true;
            }
            catch (Exception)
            {
 
            }
            return false;
        }

        public static void Close()
        {
            if (serialPort.IsOpen == false) return;

            if (threadReceive != null && threadReceive.IsAlive)
            {
                threadReceive.Abort();
            }
            serialPort.Close();

        }
        public static bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }


        private static bool VerifyFrame(byte[] frame, int count)
        {
            //检验crc

            ushort crcVal = Helper.CRC16.ComputeCRC16(frame, 2, count - 6);
            ushort frameCrc = (ushort)((frame[count - 2 - 1]) | (frame[count - 2 - 2] << 8));
            return (crcVal == frameCrc) ? true : false;
            
        }

        private static void ProcReceive()
        {

            byte[] frameData = new byte[4 * 1024];
            int frameLength = 0;
            byte[] receiveData = new byte[4 * 1024];
            int receiveLength = 0;
            bool metHeader = false;
            bool metFrame = false;
            while (true)
            {
                serialPortEvent.WaitOne();

                receiveLength = serialPort.Read(receiveData, 0, serialPort.BytesToRead); 

                for (int i = 0; i < receiveLength; i++)
                {
                    if (receiveData[i] == 0x10)
                    {
                        if (metHeader == false)
                        {
                            frameData[frameLength] = 0x10;
                            frameLength++;
                            metHeader = true;
                            if (frameLength >= frameData.Length)
                            {
                                frameLength = 0;
                                metHeader = false;
                                metFrame = false;
                            }
                        }
                        else
                        {
                            metHeader = false;
                        }
                    }
                    else if (receiveData[i] == 0x02)
                    {
                        if (metHeader)
                        {
                            frameData[0] = 0x10;
                            frameData[1] = 0x02;
                            frameLength = 2;
                            metHeader = false;
                            metFrame = true;
                        }
                        else
                        {
                            if (!metFrame)
                            {
                                frameLength = 0;
                                metHeader = false;
                                continue;
                            }
                            frameData[frameLength] = 0x02;
                            frameLength++;
                            if (frameLength >= frameData.Length)
                            {
                                frameLength = 0;
                                metHeader = false;
                                metFrame = false;
                            }

                        }
                    }
                    else if (receiveData[i] == 0x03)
                    {
                        if (!metFrame)
                        {
                            frameLength = 0;
                            metHeader = false;
                            continue;
                        }
                        if (metHeader)
                        {
                            frameData[frameLength] = 0x03;
                            frameLength++;

                            if (VerifyFrame(frameData, frameLength)) //收到数据帧
                            {
                                FrameManager.CheckFrame(frameData, frameLength);
                            }
                            frameLength = 0;
                            metHeader = false;
                            metFrame = false;
                        }
                        else
                        {

                            frameData[frameLength] = 0x03;
                            frameLength++;
                            if (frameLength >= frameData.Length)
                            {
                                frameLength = 0;
                                metHeader = false;
                                metFrame = false;
                            }
                        }
                    }
                    else
                    {
                        if (!metFrame)
                        {
                            frameLength = 0;
                            metHeader = false;
                            continue;
                        }

                        frameData[frameLength] = receiveData[i];
                        frameLength++;
                        if (frameLength >= frameData.Length)
                        {
                            frameLength = 0;
                            metHeader = false;
                            metFrame = false;
                        }

                    }
                }

            }
        }


       
        /// <summary>
        /// 发送数据(需考虑到多线程)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index">数据起始索引</param>
        /// <param name="count">数据长度</param>
        /// <returns> </returns>
        public static bool Send(byte[] data, int index, int count)
        {
            bool success = false;
            lock (originalData.SyncRoot)
            {
                originalData[0] = (byte)((count + 2) >> 8); //数据长度包括2个字节的CRC
                originalData[1] = (byte)((count + 2) & 0xff);
                Array.Copy(data, index, originalData, 2, count);

                ushort crcVal = Helper.CRC16.ComputeCRC16(originalData, 0, count + 2);

                originalData[count + 2] = (byte)(crcVal >> 8);
                originalData[count + 3] = (byte)(crcVal & 0xff);

                tranmitData[0] = 0x10;
                tranmitData[1] = 0x02;

                int transmitIndex = 2;

                for (int i = 0; i < count + 4; i++)
                {
                    if (originalData[i] == 0x10)
                    {
                        tranmitData[transmitIndex] = 0x10;
                        tranmitData[transmitIndex + 1] = 0x10;
                        transmitIndex += 2;
                    }
                    else
                    {
                        tranmitData[transmitIndex] = originalData[i];
                        transmitIndex++;
                    }
                }

                tranmitData[transmitIndex] = 0x10;
                tranmitData[transmitIndex + 1] = 0x03;

                if (serialPort.IsOpen)
                {
                    serialPort.Write(tranmitData, 0, transmitIndex + 2);
                    success = true;
                }
                
            }
            return success;
        }
    }
}

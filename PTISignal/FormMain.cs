using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO.Ports;
using PTISignal.Protocol;
using System.Threading;
using PTISignal.Helper;

namespace PTISignal
{
    public partial class FormMain : Form
    {
        DataTable[] tablePSD = new DataTable[4];
        public FormMain()
        {
            InitializeComponent();
            int[,] var = new int[2, 4] { { 988, 980, 980, 980, }, { 990, 984, 982, 986 } };
            for (int i = 0; i < 4; i++)
            {
                tablePSD[i] = new DataTable("PSD");
                tablePSD[i].Columns.Add("TrainState", typeof(string));
                tablePSD[i].Columns.Add("FZK", typeof(string));
                tablePSD[i].Columns.Add("Crew", typeof(int));
                tablePSD[i].Columns.Add("Trip", typeof(int));
                tablePSD[i].Columns.Add("Dst", typeof(int));
                tablePSD[i].Columns.Add("Res", typeof(string));
                tablePSD[i].Columns.Add("Dir", typeof(string));
                tablePSD[i].Columns.Add("Err", typeof(string));
                tablePSD[i].Columns.Add("CarID", typeof(int));
                tablePSD[i].Columns.Add("ErrInf", typeof(int));
                tablePSD[i].Columns.Add("Mil", typeof(int));
                tablePSD[i].Columns.Add("TelID", typeof(string));

                tablePSD[i].Rows.Add(new object[] { "L", "0", var[0, i], 0, 0, "0", "A", "0", 0, 0, 0, "0" });
                tablePSD[i].Rows.Add(new object[] { "L", "0", var[1, i], 0, 0, "0", "A", "0", 0, 0, 0, "0" });
                tablePSD[i].Rows.Add(new object[] { "L", "0", 988, 0, 0, "0", "A", "0", 0, 0, 0, "0" });
            }

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            groupControl2.Enabled = false;
            groupControl3.Enabled = false;

            gridControl1.DataSource = tablePSD[0];
            comboBoxEdit1.Properties.Items.AddRange(SerialPort.GetPortNames());
            if (comboBoxEdit1.Properties.Items.Count > 0)
            {
                comboBoxEdit1.SelectedIndex = 0;
            }
            comboBoxEdit2.SelectedIndex = 3;
            comboBoxEdit3.SelectedIndex = 0;
        }

        private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator)
            {
                e.Info.DisplayText = "F" + e.RowHandle.ToString();
                if (e.Info.IsRowIndicator)
                {
                    e.Info.DisplayText = "F" + e.RowHandle.ToString();
                    e.Info.ImageIndex = -1;
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (SerialManager.IsOpen)
            {
                SerialManager.Close();
                simpleButton1.Text = "打开";
                comboBoxEdit1.Enabled = true;
                comboBoxEdit2.Enabled = true;
                groupControl2.Enabled = false;
                groupControl3.Enabled = false;
            }
            else
            {
                if (SerialManager.Open(comboBoxEdit1.Text, int.Parse(comboBoxEdit2.Text)))
                {
                    simpleButton1.Text = "关闭";
                    comboBoxEdit1.Enabled = false;
                    comboBoxEdit2.Enabled = false;

                    groupControl2.Enabled = true;
                    groupControl3.Enabled = true;
                }
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            simpleButton2.Enabled = false;
            Thread threadVersion = new Thread(new ThreadStart(ProcVersion));
            threadVersion.IsBackground = true;
            threadVersion.Start();
        }

        private delegate void ShowResultCallback(string val);

        private void ShowVersion(string ver)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowResultCallback(ShowVersion), new object[] { ver });
            }
            else
            {
                textEdit1.Text = ver;
                simpleButton2.Enabled = true;

            }
        }
        private void ProcVersion()
        {
            FrameUnit frameUnit = FrameManager.CreateFrameUnit(0x01);
            byte[] frameData = new byte[20];
            int frameLength = 0;
            try
            {
                SerialManager.Send(new byte[] { 0x01, 0x00 }, 0, 2);
                string txt = "";
                if (frameUnit.WaitData(2000))
                {
                    frameLength = frameUnit.ReadTotalData(frameData, frameData.Length);

                    if (frameData[4] == 0x77) //返回成功
                    {
                        txt = "版本:" + frameData[6] + "." + frameData[7] + " 日期:" + frameData[8] + "月" + frameData[9] + "日";
                    }
                }
                ShowVersion(txt);
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                FrameManager.DeleteFrameUnit(frameUnit);
            }
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = tablePSD[radioGroup1.SelectedIndex];
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            simpleButton3.Enabled = false;
            Thread threadSend = new Thread(new ParameterizedThreadStart(ProcStartSend));
            threadSend.IsBackground = true;
            threadSend.Start(comboBoxEdit3.SelectedIndex);
        }

        private void ShowSendEnd(string txt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowResultCallback(ShowSendEnd), new object[] { txt });
            }
            else
            {
                simpleButton3.Enabled = true;
            }
        }
        private void ProcStartSend(object obj)
        {
            int index = (int)obj;
            FrameUnit frameUnit = FrameManager.CreateFrameUnit(0x03);
            byte[] frameData = new byte[20];
            int frameLength = 0;
            try
            {

                SerialManager.Send(new byte[] { 0x18, 0x01 }, 0, 2);
                Thread.Sleep(500);

                SerialManager.Send(new byte[] { 0x03, (byte)(index + 1) }, 0, 2);
                string txt = "";
                if (frameUnit.WaitData(15000))
                {
                    frameLength = frameUnit.ReadTotalData(frameData, frameData.Length);

                    if (frameData[7] == 0x81) //返回成功
                    {

                    }
                }
                ShowSendEnd(txt);
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {
                FrameManager.DeleteFrameUnit(frameUnit);
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            simpleButton4.Enabled = false;
            radioGroup1.Enabled = false;
            Thread threadReadParam = new Thread(new ParameterizedThreadStart(ProcReadParam));
            threadReadParam.IsBackground = true;
            threadReadParam.Start(radioGroup1.SelectedIndex);
        }



        private void ShowReadParamDone(string txt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowResultCallback(ShowReadParamDone), new object[] { txt });
            }
            else
            {
                simpleButton4.Enabled = true;
                radioGroup1.Enabled = true;
            }
        }


        private void ProcReadParam(object obj)
        {
            int index = (int)obj;
            FrameUnit frameUnit = FrameManager.CreateFrameUnit(0x24);
            byte[] frameData = new byte[50];
            int frameLength = 0;
            byte[] bitArray = new byte[12 * 8];


            try
            {

                for (int i = 0; i < 3; i++)
                {
                    SerialManager.Send(new byte[] { 0x24, (byte)(((index + 1) << 4) + (i + 1)) }, 0, 2);

                    if (frameUnit.WaitData(2000))
                    {
                        frameLength = frameUnit.ReadTotalData(frameData, frameData.Length);
                        if (frameData[4] == 0x77) //查询成功
                        {
                            //显示数据
                            if (frameData[7] == 0x60) //12*8Bit
                            {
                                Tool.ByteArray2BitArray(bitArray, frameData, 8, 12);

                                int state = Tool.FieldValue(bitArray, 0);
                                switch (state)
                                {
                                    case 0x1c:
                                        tablePSD[index].Rows[i]["TrainState"] = "L";
                                        break;
                                    case 0x26:
                                        tablePSD[index].Rows[i]["TrainState"] = "G";
                                        break;
                                    case 0x52:
                                        tablePSD[index].Rows[i]["TrainState"] = "R";
                                        break;
                                    default:
                                        tablePSD[index].Rows[i]["TrainState"] = "--";
                                        break;
                                }

                                int fzk = Tool.FieldValue(bitArray, 1);
                                tablePSD[index].Rows[i]["FZK"] = fzk.ToString();

                                int crew = Tool.FieldValue(bitArray, 2);

                                tablePSD[index].Rows[i]["Crew"] = crew;

                                int trip = Tool.FieldValue(bitArray, 3) + Tool.FieldValue(bitArray, 5) * 1000;
                                tablePSD[index].Rows[i]["Trip"] = trip;

                                int dst = Tool.FieldValue(bitArray, 4);
                                tablePSD[index].Rows[i]["Dst"] = dst;

                                int res = Tool.FieldValue(bitArray, 6);
                                tablePSD[index].Rows[i]["Res"] = res.ToString();

                                int dir = Tool.FieldValue(bitArray, 7);
                                switch (dir)
                                {
                                    case 0:
                                        tablePSD[index].Rows[i]["Dir"] = "EAB";
                                        break;
                                    case 1:
                                        tablePSD[index].Rows[i]["Dir"] = "A";
                                        break;
                                    case 2:
                                        tablePSD[index].Rows[i]["Dir"] = "B";
                                        break;
                                    case 3:
                                        tablePSD[index].Rows[i]["Dir"] = "AB";
                                        break;
                                }


                                int err = Tool.FieldValue(bitArray, 8);
                                tablePSD[index].Rows[i]["Err"] = err.ToString();

                                int carId = Tool.FieldValue(bitArray, 9);
                                tablePSD[index].Rows[i]["CarID"] = carId;

                                int inf = Tool.FieldValue(bitArray, 10);
                                tablePSD[index].Rows[i]["ErrInf"] = inf;

                                int mil = Tool.FieldValue(bitArray, 11);
                                tablePSD[index].Rows[i]["Mil"] = mil;

                                int tel = Tool.FieldValue(bitArray, 12);
                                tablePSD[index].Rows[i]["TelID"] = tel.ToString();
                            }
                        }
                    }



                }
                ShowReadParamDone("");

            }
            catch (ThreadAbortException)
            {

            }
            finally
            {

            }
        }

        private void ShowWriteParamDone(string txt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ShowResultCallback(ShowWriteParamDone), new object[] { txt });
            }
            else
            {
                simpleButton5.Enabled = true;
                radioGroup1.Enabled = true;
            }
        }

        private void ProcWriteParam(object obj)
        {
            int index = (int)obj;
            int[] fieldVal = new int[13];
            byte[] bitArray = new byte[12 * 8];

            byte[] byteArray = new byte[12];

            byte[] dataSend = new byte[3 + 12];
            FrameUnit frameUnit = FrameManager.CreateFrameUnit(0x14);
            byte[] frameData = new byte[100];
            try
            {
                for (int i = 0; i < 3; i++)
                {

                    switch (tablePSD[index].Rows[i]["TrainState"].ToString())
                    {
                        case "L":
                            fieldVal[0] = 0x1c;
                            break;
                        case "G":
                            fieldVal[0] = 0x26;
                            break;
                        case "R":
                            fieldVal[0] = 0x52;
                            break;
                        default:
                            fieldVal[0] = 0x1c;
                            break;
                    }
                    fieldVal[1] = int.Parse(tablePSD[index].Rows[i]["FZK"].ToString());
                    fieldVal[2] = (int)tablePSD[index].Rows[i]["Crew"];
                    int trip = (int)tablePSD[index].Rows[i]["Trip"];

                    fieldVal[3] = trip % 1000;
                    fieldVal[4] = (int)tablePSD[index].Rows[i]["Dst"];
                    fieldVal[5] = trip / 1000;
                    fieldVal[6] = int.Parse(tablePSD[index].Rows[i]["Res"].ToString());
                    switch (tablePSD[index].Rows[i]["Dir"].ToString())
                    {
                        case "A":
                            fieldVal[7] = 1;
                            break;
                        case "B":
                            fieldVal[7] = 2;
                            break;
                        case "AB":
                            fieldVal[7] = 3;
                            break;
                        case "EAB":
                            fieldVal[7] = 0;
                            break;
                        default:
                            fieldVal[7] = 2;
                            break;
                    }
                    fieldVal[8] = int.Parse(tablePSD[index].Rows[i]["Err"].ToString());
                    fieldVal[9] = (int)tablePSD[index].Rows[i]["CarID"];
                    fieldVal[10] = (int)tablePSD[index].Rows[i]["ErrInf"];
                    fieldVal[11] = (int)tablePSD[index].Rows[i]["Mil"];
                    fieldVal[12] = int.Parse(tablePSD[index].Rows[i]["TelID"].ToString());

                    Tool.Field2BitArray(bitArray, fieldVal);

                    Tool.BitArray2ByteArray(bitArray, byteArray);

                    byteArray[11] = CRC8.ComputeCRC8(byteArray, 11, CRC8.CRCExpress);


                    dataSend[0] = 0x14;
                    dataSend[1] = (byte)(((index + 1) << 4) + (i + 1));
                    dataSend[2] = 0x60;
                    Array.Copy(byteArray, 0, dataSend, 3, 12);

                    SerialManager.Send(dataSend, 0, dataSend.Length);

                    if (frameUnit.WaitData(1000))
                    {

                    }
                }

                ShowWriteParamDone("");
            }
            catch (ThreadAbortException)
            {

            }
            finally
            {

            }
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            simpleButton5.Enabled = false;
            radioGroup1.Enabled = false;
            Thread threadWriteParam = new Thread(new ParameterizedThreadStart(ProcWriteParam));
            threadWriteParam.IsBackground = true;
            threadWriteParam.Start(radioGroup1.SelectedIndex);

        }
    }
}
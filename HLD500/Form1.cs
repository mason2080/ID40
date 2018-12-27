﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Driver.Port.CSerialPort;
using System.Threading;
using System.Data.SqlClient;
using Driver.File.Ini;
using System.IO;
using System.Security.Cryptography;

namespace HLD500
{
    public partial class Form1 : Form
    {
        enum DeviceType
        {
            HLD5000,
            P3000
        }

        enum DeviceMode
        {
            RUN,
            STANDBY
        }

        DeviceType deviceType = DeviceType.HLD5000;
        DeviceMode deviceMode = DeviceMode.RUN;

        CSerialPort com = new CSerialPort();
        SqlConnection sqlConn = new SqlConnection();
        SqlConnection sqlConn1 = new SqlConnection();

        IniFileClass iniFile = new IniFileClass();
        Thread RecvRs485Thread;
        string fPath = Directory.GetCurrentDirectory() + @"\History.csv";

        double valueBackUp;

        int SQL_ID;
        uint timeCnt = 50;

        Int32 runModePeriod = 250;
        Int32 standbyModePeriod = 500;
        uint portNum=0;
        int baudRate = 9600;

        string tempId = "";

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            textBoxValue.Text = "0";

            textBox5.KeyUp += new KeyEventHandler(textBox5_KeyUp);
            textBox5.Focus();

            string fName=Directory.GetCurrentDirectory().ToString()+"\\Ref.ini";
            FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Close();
             try
             {
                 textBoxIP.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "IP"));
                 textBoxDataBase.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "DataBase"));
                 textBoxUserName.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "User"));
                 textBoxPassword.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "Password"));
                 textBoxTableName.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "Table"));
                 textBoxRef.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "RefValue"));
                 textBoxTestId.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "TestID"));
                 textBoxTestMachine.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "TestMachine"));
                 textBoxSQLPort.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "SQLPort"));
                 textBoxAuxTable.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "AuxTable"));
                 textBoxUploadTime.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "TimeToUpload"));

                 textBoxRunModeTime.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "AUTO"));
                 //textBoxStandbyTime.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "STANDBY"));
                // deviceType =(DeviceType)( uint.Parse(decode(iniFile.IniReadValue(fName, "基准", "DeviceType"))));


                 textBoxIP1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "IP1"));
                 textBoxDataBase1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "DataBase1"));
                 textBoxUserName1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "User1"));
                 textBoxPassword1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "Password1"));
                 textBoxTableName1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "Table1"));
                 textBoxSQLPort1.Text = decode(iniFile.IniReadValue(fName, "CONFIG", "SQLPort1"));

                 tempId = decode(iniFile.IniReadValue(fName, "CONFIG", "MAXID"));

                 standbyModePeriod = Int32.Parse(textBoxStandbyTime.Text);
                 runModePeriod = Int32.Parse(textBoxRunModeTime.Text);

                 label1TestId.Text = textBoxTestId.Text;
                 textBoxMsg.Text = "配置加载成功\r\n";
                 string tempString = @"Server=" + textBoxIP.Text+","+textBoxSQLPort.Text + ";Database=" + textBoxDataBase.Text + ";User Id=" + textBoxUserName.Text + ";Password=" + textBoxPassword.Text + ";";
                 sqlConn = new SqlConnection(tempString);
                 textBoxMsg.Text += tempString + "\r\n";

                 //string tempString1 = @"Server=" + textBoxIP1.Text + "," + textBoxSQLPort1.Text + ";Database=" + textBoxDataBase1.Text + ";User Id=" + textBoxUserName1.Text + ";Password=" + textBoxPassword1.Text + ";";
                 //sqlConn1 = new SqlConnection(tempString1);

                 string tempString1 = @"Server=" + textBoxIP1.Text + ";Database=" + textBoxDataBase1.Text + ";integrated security=SSPI";
                 sqlConn1 = new SqlConnection(tempString1);

                 textBoxMsg.Text += tempString1 + "\r\n" ;

             }
             catch
             { }

             checkBoxAutoUpToServer.Checked = true;

             //if (deviceType == DeviceType.HLD5000)
             //{
             //    baudRate = 9600;
             //    labelLogo.Text = "阿里斯顿HLD5000";
             //}
             //else 
             //{
             //    baudRate = 19200;
             //    labelLogo.Text = "阿里斯顿P3000";
             //}


            //try{
            //    comboBox1.SelectedIndex = int.Parse(decode(iniFile.IniReadValue(fName, "基准", "Rs232Port")));
            //    if (com.Open(("COM" + (comboBox1.SelectedIndex + 1).ToString()),baudRate) == 1)
            //    {
            //        text485Status.Text = "连接成功";
            //        RecvRs485Thread = new Thread(RecvRs485Func);
            //        RecvRs485Thread.Start();

            //        textBoxMsg.Text += "连接成功\r\n";
            //    }
            //   }
            //catch
            //{
            //    text485Status.Text = "连接失败";

            //    textBoxMsg.Text += "连接失败\r\n";
            //}
             //try
             //{
             //    if (text485Status.Text != "连接成功")
             //    {
             //        if (com.Open("COM" + (comboBox1.SelectedIndex + 1).ToString()) == 1)
             //        {
             //            text485Status.Text = "连接成功";
             //            RecvRs485Thread = new Thread(RecvRs485Func);
             //            RecvRs485Thread.Start();

             //            textBoxMsg.Text += "连接成功\r\n";
             //        }
             //    }
             //}
             //catch
             //{
             //    text485Status.Text = "连接失败";

             //    textBoxMsg.Text += "连接失败\r\n";

             //}

        }

        private void textBox5_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Control || e.KeyCode == Keys.Enter)
            {
                if (textBox5.Text != "")
                {
                    textBox2.Text = textBox5.Text;
                    textBox5.Text = "";

                    textBox5.Focus();
                }
            }
        }

        private void btnOpen485_Click(object sender, EventArgs e)
        {
            try
            {
                if (text485Status.Text != "连接成功")
                {
                    if (com.Open(("COM" + (comboBox1.SelectedIndex + 1).ToString()),baudRate) == 1)
                    {
                        text485Status.Text = "连接成功";
                        RecvRs485Thread = new Thread(RecvRs485Func);
                        RecvRs485Thread.Start();

                        textBoxMsg.Text += "连接成功\r\n" ;

                        
                        try
                        {

                            string fName = Directory.GetCurrentDirectory().ToString() + "\\Ref.ini";
                            FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            fs.Close();
                            iniFile.IniWriteValue(fName, "基准", "Rs232Port", encode(comboBox1.SelectedIndex.ToString()));
                        }
                        catch { }
           
                    }
                }
            }
            catch
            {
                text485Status.Text = "连接失败";

                textBoxMsg.Text += "连接失败\r\n";

            }
        }

        private void RecvRs485Func()
        {
            byte[] data = new byte[3];
            byte[] time = new byte[10];
            byte[] tempByte = new byte[1000];
            byte[] crcresult = new byte[2];
            int len = 0;
            string tempValue;
            string read;
            decimal tempValuedouble;

            Int32 sleeptiem = 0;

            while (true)
            {
                if (deviceMode == DeviceMode.RUN)
                {
                    Thread.Sleep(runModePeriod);
                }
                else
                {
                    Thread.Sleep(standbyModePeriod);
                }

                if (deviceType == DeviceType.HLD5000)
                {
                    HdL5000SendCmd();

                    Thread.Sleep(100);

                    len = com.GetBytesToRead();
                    if (len >= 1)
                    {
                        if (textBox31.Text.Length > 500)
                        {
                            textBox31.Text = "";
                        }
                        tempByte = com.ReadHexData(len);
                        if ((tempByte[0] == 0x02) && (tempByte[1] == 0x04) && (tempByte[2] == 0x06))
                        {
                            if (Crc16(tempByte, 2, 4) == tempByte[6])
                            {
                                data[0] = tempByte[3];
                                data[1] = tempByte[4];
                                data[2] = tempByte[5];
                                tempValue = System.Text.Encoding.ASCII.GetString(data);
                                textBoxValue.Text = (double.Parse(tempValue) * 2 / 100).ToString();
                                if (textBoxLed.BackColor == Color.Green)//指示灯变化
                                {
                                    textBoxLed.BackColor = Color.White;
                                }
                                else
                                {
                                    textBoxLed.BackColor = Color.Green;
                                }

                                if (double.Parse(tempValue) == 0)//值==0时，放慢周期
                                {
                                    deviceMode = DeviceMode.STANDBY;
                                }
                                else 
                                {
                                    deviceMode = DeviceMode.RUN;
                                }

                                if ((double.Parse(tempValue) * 2 / 100) <= 2)//结果判断
                                {
                                    textBoxResult.Text = "正常";
                                    textBoxResult.BackColor = Color.Green;
                                }
                                else
                                {
                                    textBoxResult.Text = "超标";
                                    textBoxResult.BackColor = Color.Red;
                                }
                            }
                        }
                    }


                }
                else//P3000
                {
                    com.SendStringData("*read 1?\r\n");
                    Thread.Sleep(100);
                    read = com.ReadStringData();
                    len = read.IndexOf("g/a");
                    if (len > 0)
                    {
                        try
                        {
                            tempValuedouble = ChangeDataToD(read.Substring(0, len - 1));
                            textBoxValue.Text = tempValuedouble.ToString();

                            if (textBoxLed.BackColor == Color.Green)//指示灯变化
                            {
                                textBoxLed.BackColor = Color.White;
                            }
                            else
                            {
                                textBoxLed.BackColor = Color.Green;
                            }


                            if (tempValuedouble == 0)//值==0时，放慢周期
                            {
                                deviceMode = DeviceMode.STANDBY;
                            }
                            else
                            {
                                deviceMode = DeviceMode.RUN;
                            }


                            if (tempValuedouble <= 2)//结果判断
                            {
                                textBoxResult.Text = "正常";
                                textBoxResult.BackColor = Color.Green;
                            }
                            else
                            {
                                textBoxResult.Text = "超标";
                                textBoxResult.BackColor = Color.Red;
                            }
                        }
                        catch
                        {

                        }
                    }
                }



            }
        }

        private Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                dData = Convert.ToDecimal(Decimal.Parse(strData.ToString(), System.Globalization.NumberStyles.Float));
            }
            else
            {
                dData = decimal.Parse(strData);
            }
            return dData;
        }

        private void label4_Click(object sender, EventArgs e)
        {
        
        }

        private void btnDisCon485_Click(object sender, EventArgs e)
        {
            try
            {
                if (text485Status.Text == "连接成功")
                {
                    RecvRs485Thread.Abort();

                    com.Close();
                    text485Status.Text = "断开连接";
                }
            }
            catch
            {

            }
        }

        byte Crc16(byte[] data,byte startIndex, int count)
        {
            //  函数功能   计算CRC校验
            //	参数说明   *ptr  :起始地址,count :被校验信息长度
            byte  crc;
            crc = 0;

            for (byte i = startIndex; i < startIndex+count; i++)
            {
                crc += data[i];
            }

            return crc;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //byte[] emsData = new byte[6];
            //byte[] crcresult = new byte[2];
            //byte[] data = new byte[10];
            //if ((text485Status.Text == "连接成功"))
            //{
                   
            //        byte p = 0;
            //        emsData[p++] = 0x02;
            //        emsData[p++] = 0x03;
            //        emsData[p++] = 0x53;
            //        emsData[p++] = 0x30;
            //        emsData[p++] = 0x30;
            //        emsData[p++] = Crc16(emsData,2, 3);
            //        try
            //        {
                    

            //            com.SendStringData(emsData);
            //        }
            //        catch
            //        { }
            //}
        }

        private void HdL5000SendCmd()
        {
            byte[] emsData = new byte[6];
            byte[] crcresult = new byte[2];
            byte[] data = new byte[10];
            if ((text485Status.Text == "连接成功"))
            {

                byte p = 0;
                emsData[p++] = 0x02;
                emsData[p++] = 0x03;
                emsData[p++] = 0x53;
                emsData[p++] = 0x30;
                emsData[p++] = 0x30;
                emsData[p++] = Crc16(emsData, 2, 3);
                try
                {
                    com.SendStringData(emsData);
                }
                catch
                { }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (text485Status.Text == "连接成功")
            {
                RecvRs485Thread.Abort();
                com.Close();
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                LogIn registerForm = new LogIn();
                registerForm.ShowDialog();
                if (registerForm.DialogResult == DialogResult.OK)
                {

                }
                else
                {
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btnSaveRef_Click(object sender, EventArgs e)
        {

            try
            {
                double temp = double.Parse(textBoxTestId.Text);
            }
            catch
            {
                MessageBox.Show("TestID请设置为数值");
                return;
            }


            //try
            //{
            //    double temp = double.Parse(textBoxUploadTime.Text);
            //}
            //catch
            //{
            //    MessageBox.Show("上传倒计时值请设置为数值");
            //    return;
            //}

            //try
            //{
            //    double temp = double.Parse(textBoxRef.Text);
            //}
            //catch
            //{
            //    MessageBox.Show("参考值请设置为数值");
            //    return;
          //  }


            string fName = Directory.GetCurrentDirectory().ToString() + "\\Ref.ini";
            FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Close();

            try
            {
                iniFile.IniWriteValue(fName, "CONFIG", "IP", encode( textBoxIP.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "DataBase", encode(textBoxDataBase.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "User", encode(textBoxUserName.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "Password", encode(textBoxPassword.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "Table", encode(textBoxTableName.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "RefValue", encode(textBoxRef.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "TestID", encode(textBoxTestId.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "TestMachine", encode(textBoxTestMachine.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "SQLPort", encode(textBoxSQLPort.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "AuxTable", encode(textBoxAuxTable.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "TimeToUpload", encode(textBoxUploadTime.Text));

                iniFile.IniWriteValue(fName, "CONFIG", "AUTO", encode(textBoxRunModeTime.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "STANDBY", encode(textBoxStandbyTime.Text));

                iniFile.IniWriteValue(fName, "CONFIG", "IP1", encode(textBoxIP1.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "DataBase1", encode(textBoxDataBase1.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "User1", encode(textBoxUserName1.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "Password1", encode(textBoxPassword1.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "Table1", encode(textBoxTableName1.Text));
                iniFile.IniWriteValue(fName, "CONFIG", "SQLPort1", encode(textBoxSQLPort1.Text));

                MessageBox.Show("保存成功");
            }
            catch
            {
                MessageBox.Show("保存失败");
                return;
            }


            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           byte result = InsertSQLData(textBoxTableName.Text); //(textBoxTableName.Text);
           if (result == 1)
           {
               textBoxUpToServer.Text = "成功";
               textBoxUpToServer.BackColor = Color.Green;
           }
           else 
           {
               textBoxUpToServer.Text = "失败";
               textBoxUpToServer.BackColor = Color.White;
           }


        }

        public byte GetLocalData(string TableName)
        {
            int Max_num;



            try
            {
                string maxid = "select max(ID) as sid from " + TableName;
                SqlCommand cmd2 = new SqlCommand(maxid, sqlConn1);

                //将最大值复制给变量mid
                sqlConn1.Open();


                textLocalId.Text = cmd2.ExecuteScalar().ToString();

                sqlConn1.Close();




                sqlConn1.Open();

                string cmd = @"select * from " + TableName + " where ID=" + textLocalId.Text;

                SqlDataAdapter sda = new SqlDataAdapter(cmd, sqlConn1);
                sqlConn1.Close();
                DataSet ds = new DataSet();
                sda.Fill(ds);
                textLocalId.Text = ds.Tables[0].Rows[0][0].ToString();
                textDateTime.Text = ds.Tables[0].Rows[0][1].ToString();
                textSN.Text = ds.Tables[0].Rows[0][2].ToString();

                if (ds.Tables[0].Rows[0][4].ToString() == "15")
                {
                    textResult.Text = "OK";
                }
                else
                {
                    textResult.Text = "NG";
                }


                textLocalStatus.Text = "获取成功";
                textLocalStatus.BackColor = Color.Green;

                // tempValuedouble = ChangeDataToD(read.Substring(0, len - 1));
                //textBoxValue.Text = tempValuedouble.ToString();

                try
                {
                    textValue.Text = ChangeDataToD(ds.Tables[0].Rows[0][7].ToString()).ToString();

                }
                catch
                {

                }
                if (textLocalId.Text != tempId)
                {
                    tempId = textLocalId.Text;
                    InsertSQLData(textBoxTableName.Text);
                }

            }
            catch
            {
                textLocalStatus.Text = "";
                textLocalStatus.BackColor = Color.White;

                textBoxUpToServer.Text = "";
                textBoxUpToServer.BackColor = Color.White;
            }



            return 1;


        }

        public byte InsertSQLData(string TableName)
        {
            string TestID = textBoxTestId.Text;

            string fName = Directory.GetCurrentDirectory().ToString() + "\\Ref.ini";
            FileStream fs = new FileStream(fName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Close();

            try
            {
                iniFile.IniWriteValue(fName, "CONFIG", "MAXID", encode(tempId));

            }
            catch
            {

            }


            string SerialNo = textSN.Text;
            byte TestResult;
            double TestValue;
            try
            {
                TestValue = double.Parse(textValue.Text);

                if (TestValue < 0.0001)
                {
                    TestValue = 0;
                }
            }
            catch
            {
                //textBoxMsg.Text += "测试值异常\r\n";
               // return 0;
                TestValue = 0;
            }
            

            if (textResult.Text == "OK")
            {
                TestResult = 1;
            }
            else
            {
                TestResult = 0;
            }

            if (true)//(textBox2.Text != "")
            {
                String TestTime = Convert.ToDateTime(textDateTime.Text).ToString("yyyy-MM-dd HH:mm:ss");// DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string TestMachine = textBoxTestMachine.Text;
                string tempString1 = @"INSERT INTO dbo." + TableName + @" (TestID,SerialNo ,TestResult ,TestValue ,TestTime,TestMachine)";
                string tempString2 = @" VALUES ('" + TestID + "'," + "'" + SerialNo + "'," + "'" + TestResult.ToString() + "'," + "'" + TestValue.ToString() + "'," + "'" + TestTime + "'," + "'" + TestMachine + "')";
                tempString1 += tempString2;
                SqlCommand MyCommand = new SqlCommand(tempString1, sqlConn);
                try//异常处理
                {
                    sqlConn.Open();
                    MyCommand.ExecuteNonQuery();
                    sqlConn.Close();
                    textBoxMsg.Text += textSN.Text.Trim() + ":Up to Server OK\r\n";

                    textBoxUpToServer.Text = "成功";
                    textBoxUpToServer.BackColor = Color.Green;

                    //string historyData = TestTime + ","
                    //     + textBoxTestId.Text + ","
                    //     + TestMachine + ","
                    //     + SerialNo + ","
                    //     + TestValue.ToString() + ","
                    //     + TestResult.ToString();
                    //try
                    //{
                    //    FileStream fileStream = new FileStream(fPath, FileMode.OpenOrCreate, FileAccess.Write);
                    //    StreamWriter streamWriter = new StreamWriter(fileStream);
                    //    fileStream.Seek(0, SeekOrigin.End);
                    //    streamWriter.WriteLine(historyData);
                    //    streamWriter.Close();
                    //    fileStream.Close();
                    //}
                    //catch { }

                    //try
                    //{
                    //    if (checkBoxUploadAux.Checked == true)
                    //    {
                    //        if (textDetailDesc.Text.Length >= 40)
                    //        {
                    //            textDetailDesc.Text = textDetailDesc.Text.Substring(0, 40);
                    //        }

                    //        if (textBoxData.Text.Length >= 40)
                    //        {
                    //            textBoxData.Text = textBoxData.Text.Substring(0, 40);
                    //        }


                    //        tempString1 = @"INSERT INTO dbo." + "TESTHISTORYDETAIL" + @" (MainID,DetailDesc ,Data)";
                    //        tempString2 = @" VALUES ('" + SQL_ID.ToString() + "'," + "'" + textDetailDesc.Text + "'," + "'" + textBoxData.Text + "')";
                    //        tempString1 += tempString2;
                    //        MyCommand = new SqlCommand(tempString1, sqlConn);
                    //        sqlConn.Open();
                    //        SQL_ID = Convert.ToInt32(MyCommand.ExecuteScalar());
                    //        sqlConn.Close();
                    //        textBoxMsg.Text += SerialNo + ":辅助信息Up to Server OK\r\n";
                    //    }
                    //}
                    //catch
                    //{
                    //    textBoxMsg.Text += SerialNo + ":辅助信息Up to Server Error\r\n";
                    //}

                    return 1;
                }
                catch (Exception ex)
                {
                    sqlConn.Close();
                    textBoxMsg.Text += "Insert New Data Error\r\n";

                    textBoxUpToServer.Text = "失败";
                    textBoxUpToServer.BackColor = Color.Red;
                    return 0;
                }
            }
            {
                textBoxMsg.Text += "请输入产品ID\r\n";
                return 0;
            }
        }

        public byte InsertGprsData(string TableName)
        {
            string TestID = textBoxTestId.Text;
            string SerialNo = textBox2.Text;
            byte TestResult;
            double TestValue;
            try
            {
                TestValue = double.Parse(textBoxValue.Text);
            }
            catch
            {
                textBoxMsg.Text += "测试值异常\r\n" ;
                return 0;
            }
            if (textBoxResult.Text == "正常")
            {
                TestResult = 1;
            }
            else 
            {
                TestResult = 0;
            }

            if (textBox2.Text != "")
            {
                String TestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string TestMachine = textBoxTestMachine.Text;
                string tempString1 = @"INSERT INTO dbo." + TableName + @" (TestID,SerialNo ,TestResult ,TestValue ,TestTime,TestMachine)";
                string tempString2 = @" VALUES ('" + TestID + "'," + "'" + SerialNo + "'," + "'" + TestResult.ToString() + "'," + "'" + TestValue.ToString() + "'," + "'" + TestTime + "'," + "'" + TestMachine + "');select @@IDENTITY";
                tempString1 += tempString2;
                SqlCommand MyCommand = new SqlCommand(tempString1, sqlConn);
                try//异常处理
                {
                    sqlConn.Open();
                    SQL_ID= Convert.ToInt32( MyCommand.ExecuteScalar());
                    sqlConn.Close();
                    textBoxMsg.Text += SerialNo + ":Up to Server OK\r\n";

                    string historyData = TestTime + ","
                         + textBoxTestId.Text + ","
                         + TestMachine + ","
                         + SerialNo + ","
                         + TestValue.ToString() + ","
                         + TestResult.ToString();
                    try
                    {
                        FileStream fileStream = new FileStream(fPath, FileMode.OpenOrCreate, FileAccess.Write);
                        StreamWriter streamWriter = new StreamWriter(fileStream);
                        fileStream.Seek(0, SeekOrigin.End);
                        streamWriter.WriteLine(historyData);
                        streamWriter.Close();
                        fileStream.Close();
                    }
                    catch { }

                    try
                    {
                        if (checkBoxUploadAux.Checked == true)
                        {
                            if (textDetailDesc.Text.Length >= 40)
                            {
                                textDetailDesc.Text = textDetailDesc.Text.Substring(0, 40);
                            }

                            if (textBoxData.Text.Length >= 40)
                            {
                                textBoxData.Text = textBoxData.Text.Substring(0, 40);
                            }


                             tempString1 = @"INSERT INTO dbo." + "TESTHISTORYDETAIL" + @" (MainID,DetailDesc ,Data)";
                             tempString2 = @" VALUES ('" + SQL_ID.ToString() + "'," + "'" + textDetailDesc.Text  + "'," + "'" + textBoxData.Text + "')";
                             tempString1 += tempString2;
                             MyCommand = new SqlCommand(tempString1, sqlConn);
                             sqlConn.Open();
                             SQL_ID = Convert.ToInt32(MyCommand.ExecuteScalar());
                             sqlConn.Close();
                             textBoxMsg.Text += SerialNo + ":辅助信息Up to Server OK\r\n";
                        }
                    }
                    catch
                    {
                             textBoxMsg.Text += SerialNo + ":辅助信息Up to Server Error\r\n";
                    }

                    return 1;
                }
                catch (Exception ex)
                {
                    sqlConn.Close();
                    textBoxMsg.Text += "Insert New Data Error\r\n" ;
                    return 0;
                }
            }
            {
                textBoxMsg.Text += "请输入产品ID\r\n" ;
                return 0;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if ((deviceType == DeviceType.P3000)&&(text485Status.Text=="连接成功"))
            {
                com.SendStringData("*start\r\n");
            }
            textBoxUpToServer.Text = "";
            textBoxUpToServer.BackColor = Color.White;

            timeCnt = 50;
        }

        private void timerAutoUpToServer_Tick(object sender, EventArgs e)
        {

            if(checkBoxAutoUpToServer.Checked == true)
            {
                try
                {
                    GetLocalData(textBoxTableName1.Text);
                }
                catch
                { }
            }
            //double value=0;
            //double tempValue=0;
            //bool valueStable = false;

            //try
            //{
            //  value=double.Parse(textBoxValue.Text);
            //  if (Math.Abs(value - valueBackUp) <= 0.1)//简单处理，跳动大时，可取一组数判断最大最小平均值
            //  {
            //      valueStable = true;
            //  }
            //  else
            //  {
            //      valueStable = false;
            //  }

            //  valueBackUp = value;
            //}
            //catch
            //{
            //    value=0;
            //}

            //if ((valueStable==true)&&(checkBoxAutoUpToServer.Checked == true) && (value > 0) && (textBoxUpToServer.Text == "") && (textBox2.Text != "") && (text485Status.Text == "连接成功"))
            //{
            //    if (timeCnt >= 1)
            //    {
            //        timeCnt--;
            //    }
            //     tempValue=timeCnt/10;
            //     labelUpdateToServer.Text = tempValue.ToString();
            //    if (timeCnt == 0)
            //    {
            //        byte result = InsertGprsData(textBoxTableName.Text);
            //        if (result == 1)
            //        {
            //            textBoxUpToServer.Text = "成功";
            //            textBoxUpToServer.BackColor = Color.Green;
            //        }
            //        else
            //        {
            //            textBoxUpToServer.Text = "失败";
            //            textBoxUpToServer.BackColor = Color.White;
            //        }
            //        timeCnt = 50;
            //        labelUpdateToServer.Text = "5";
            //    }
            //}
            //else
            //{
            //    timeCnt=50;
            //    labelUpdateToServer.Text = "5";
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int time = 1000;

            try
            {
                time = int.Parse(textBoxRunModeTime.Text);
            }
            catch
            {
                time = 1000;
            }

            timerAutoUpToServer.Interval = time;
            timerAutoUpToServer.Enabled = true;

        }

        private void textBoxMsg_TextChanged(object sender, EventArgs e)
        {
            textBoxMsg.SelectionStart = textBoxMsg.Text.Length;
            textBoxMsg.ScrollToCaret(); 
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void splitContainer4_Panel1_Paint(object sender, PaintEventArgs e)
        {
           
        }

        //strin加密
        public static string encode(string str)
        {
            string htext = str;

            //for (int i = 0; i < str.Length; i++)
            //{
            //    htext = htext + (char)(str[i] + 10 - 1 * 2);
            //}
            return htext;
        }

        //strin解密
        public static string decode(string str)
        {
            string dtext = str;

            //for (int i = 0; i < str.Length; i++)
            //{
            //    dtext = dtext + (char)(str[i] - 10 + 1 * 2);
            //}
            return dtext;
        }

        private void splitContainer7_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void textBoxUpToServer_TextChanged(object sender, EventArgs e)
        {

        }

        //public static string byteToHexStr(byte[] bytes)
        //{
        //    string returnStr = "";
        //    if (bytes != null)
        //    {
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            returnStr += bytes[i].ToString("X2");
        //        }
        //    }
        //    return returnStr;
        //}

        //public static string byteToHexStr(byte[] bytes, int len)
        //{
        //    string returnStr = "";
        //    if (bytes != null)
        //    {
        //        for (int i = 0; i < len; i++)
        //        {
        //            returnStr += bytes[i].ToString("X2");
        //        }
        //    }
        //    return returnStr;
        //}


        //public static byte[] StringToHexByte(string str)
        //{
        //    return StringToHexByte(str, false);
        //}


        // public static byte[] StringToHexByte(string str, bool isFilterChinese)
        //{
        //    string hex = isFilterChinese ? FilterChinese(str) : ConvertChinese(str);

        //    //清除所有空格
        //    hex = hex.Replace(" ", "");
        //    //若字符个数为奇数，补一个0
        //    hex += hex.Length % 2 != 0 ? "0" : "";

        //    byte[] result = new byte[hex.Length / 2];
        //    for (int i = 0, c = result.Length; i < c; i++)
        //    {
        //        result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        //    }
        //    return result;
        //}

        //        private static string ConvertChinese(string str)
        //{
        //    StringBuilder s = new StringBuilder();
        //    foreach (short c in str.ToCharArray())
        //    {
        //        if (c <= 0 || c >= 127)
        //        {
        //            s.Append(c.ToString("X4"));
        //        }
        //        else
        //        {
        //            s.Append((char)c);
        //        }
        //    }
        //    return s.ToString();
        //}

        //private static string FilterChinese(string str)
        //{
        //    StringBuilder s = new StringBuilder();
        //    foreach (short c in str.ToCharArray())
        //    {
        //        if (c > 0 && c < 127)
        //        {
        //            s.Append((char)c);
        //        }
        //    }
        //    return s.ToString();
        //}
    }
}

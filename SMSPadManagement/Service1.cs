using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using System.Timers;

namespace SMSPadManagement
{
    public partial class ServiceSMSPad : ServiceBase
    {
        public Socket handler;
        public Socket listener;
        public Int64 IMEI;
        string SerialDevice;
        static string sqlConnection = "Data Source=.; Initial catalog=Client_SMSPad; User id= sa; Password=Hasan@1020;";
        SqlConnection connetDB = new SqlConnection(sqlConnection);
        public ServiceSMSPad()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
        public void StarttoListen()
        {
            Timer t = new Timer
            {
                Interval = 600
            };


            string SNumber;
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = IPAddress.Any; //  host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8572);
            try
            {
                while (true)
                {
                    try
                    {
                        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(localEndPoint);
                        listener.Listen(10);
                        handler = listener.Accept();
                        handler.ReceiveTimeout = 10000;
                        string data = null;
                        byte[] bytes = null;
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        t.Enabled = true;
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (bytes[0] == 202 && bytes[1] == 3)
                        {
                            byte[] bIMEI = new byte[8];
                            Buffer.BlockCopy(bytes, 2, bIMEI, 0, 8);
                            //Array.Reverse(bIMEI);
                            IMEI = BitConverter.ToInt64(bIMEI, 0);
                            SerialDevice =
                                (bIMEI[0] - 48).ToString() +
                                (bIMEI[1] - 48).ToString() +
                                (bIMEI[2] - 48).ToString() +
                                (bIMEI[3] - 48).ToString() +
                                (bIMEI[4] - 48).ToString() +
                                (bIMEI[5] - 48).ToString() +
                                (bIMEI[6] - 48).ToString() +
                                (bIMEI[7] - 48).ToString();

                            byte[] bVersion = new byte[3];
                            Buffer.BlockCopy(bytes, 10, bVersion, 0, 3);
                            string VersionD = bVersion[0].ToString() + bVersion[1].ToString() + bVersion[2].ToString();

                            byte[] bNumber = new byte[16];
                            Buffer.BlockCopy(bytes, 13, bNumber, 0, 16);
                            SNumber =
                                (bNumber[0] - 48).ToString() +
                                (bNumber[1] - 48).ToString() +
                                (bNumber[2] - 48).ToString() +
                                (bNumber[3] - 48).ToString() +
                                (bNumber[4] - 48).ToString() +
                                (bNumber[5] - 48).ToString() +
                                (bNumber[6] - 48).ToString() +
                                (bNumber[7] - 48).ToString() +
                                (bNumber[8] - 48).ToString() +
                                (bNumber[9] - 48).ToString() +
                                (bNumber[10] - 48).ToString() +
                                (bNumber[11] - 48).ToString() +
                                (bNumber[12] - 48).ToString() +
                                (bNumber[13] - 48).ToString() +
                                (bNumber[14] - 48).ToString() +
                                (bNumber[15] - 48).ToString();

                            byte[] messagesendDevice = Encoding.ASCII.GetBytes("TCOK");
                            handler.Send(messagesendDevice);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            listener.Close();
                            if (SNumber.Substring(0,SNumber.IndexOf("-48")).Length > 5)
                            {
                                SendMessage(SNumber.Substring(0, SNumber.IndexOf("-48")));
                            }
                            



                        }
                        else
                        {
                            byte[] ERRORmessagesendDevice = Encoding.ASCII.GetBytes("Not Allowed");
                            handler.Send(ERRORmessagesendDevice);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            listener.Close();
                        }
                    }
                    catch(Exception ex)
                    {
                        byte[] TimeoutMessage = Encoding.ASCII.GetBytes("Timeout");
                        handler.Send(TimeoutMessage);
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        listener.Close();
                    }

                    }
                //CA03E81E6F705F1203005E070B3039303232333634303034000000000003   ---30393032323336343030340000000000 000000000000000000000000000

            }
            catch (Exception e)
            {
            }
        }
        public int SendMessage(string PhoneNumber)
        {
            try
            {
                WebRequest request = WebRequest.Create("http://ippanel.com/services.jspd");
                string[] rcpts = new string[] { PhoneNumber };
                string json = JsonConvert.SerializeObject(rcpts);
                request.Method = "POST";
                string postData = "op=send&uname=tcco&pass=9124077223&message=به باشگاه مشتریان شرکت طراحان کنترل شرق خوش آمدید www.tcco.ir & to=" + json + "&from=+9810000385";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();
                System.Diagnostics.Debug.WriteLine(responseFromServer);
                try
                {
                    string Filelog = "IMEI: " + IMEI + " SerialDevice:" + SerialDevice + " PanelData: " + postData + " responseFromServer: " + responseFromServer + " SendTime: " + DateTime.Now.ToString() + Environment.NewLine;
                    string FileDirectory = System.IO.Directory.GetCurrentDirectory() + "\\" + PhoneNumber + ".txt";
                    if (File.Exists(FileDirectory))
                    {
                        File.AppendAllText(FileDirectory, Filelog);
                    }
                    else
                    {
                        File.Create(FileDirectory).Close();
                        File.AppendAllText(FileDirectory, Filelog);
                    }
                }
                catch (Exception ex)
                {
                }
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }


        }
    }

}

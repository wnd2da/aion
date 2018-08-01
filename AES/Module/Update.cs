﻿using System;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace AES
{
    public class Update
    {
        private MainForm mainForm;
        private Thread updateThread;

        public Update(MainForm main)
        {
            this.mainForm = main;
        }

        private int Method;
        public void Start(int method)
        {
            this.Method = method;
            updateThread = new Thread(new ThreadStart(ThreadMethod));
            updateThread.Start();
        }

        public void Stop()
        {
            if (updateThread != null) updateThread.Abort();
        }

        public void ThreadMethod()
        {
            try
            {
                this.mainForm.SetUpdateLabel("버전 정보 확인중");
                int ret = VersionCheck();
                //ret = 1;
                if (ret == 0)
                {
                    this.mainForm.SetUpdateLabel("최신 버전입니다");
                }
                else if (ret == -1)
                {
                    this.mainForm.SetUpdateLabel("버전 정보를 확인하지 못하였습니다.");
                }
                else if (ret == 1)
                {
                    // 2013-05-14
                    // 개심각한 버그
                    //if (Method != 0)
                    if (Method != 2) // 업데이트후 시작해서 버전체크만
                    {
                        if (Method == 0)
                        {
                            // 정보 탭으로 가도록 한다.
                            this.mainForm.InsertUpdateProgress();
                            this.mainForm.ShowInformationTab();
                        }
                        this.mainForm.SetUpdateLabel("최신 버전 다운로드중");
                        Thread.Sleep(1000);
                        int downloadRet = Download();
                        if (downloadRet == 0)
                        {
                            //성공
                            int time = 3;
                            this.mainForm.DeleteUpdateProgress();
                            do
                            {
                                this.mainForm.SetUpdateLabel("다운로드 완료. " + time + "초뒤 재시작됩니다.");
                                Thread.Sleep(1000);
                            } while (time-- > 0);


                            Restart();
                        }
                        else if (downloadRet == -1)
                        {
                            this.mainForm.SetUpdateLabel("업데이트 오류");
                            this.mainForm.DeleteUpdateProgress();
                        }
                    }
                    else
                    {
                        // 오면 안된다.
                        this.mainForm.SetUpdateLabel("업데이트 후 자동시작으로 실행. 또 업데이트 하라고 함. 업데이트 오류");
                        this.mainForm.DeleteUpdateProgress();
                    }

                }
                //if (Method != 1 && Notice != null)
                // 2013-05-24
                if (Notice != null && Notice.Trim() != "")
                {
                    this.mainForm.ShowNotice(Notice);
                }
                Thread.Sleep(1000);
            }
            catch { }
            finally
            {
                this.mainForm.DeleteUpdateProgress();
            }
        }


public string batchFilename = "up.bat";
        private int VersionCheck()
        {
            String url = "";
            try
            {
                String name = "";
                String server = "";
                //자동업데이트면
                if (Method == 0 || Method == 2)
                {
                    // 웹에서 카운트 할수있도록 한다
                    //url = url + "?auto=1&userserver=" + server + "&username=" + name;
                    //2013-05-13 버전정보 추가
                    url = url + "?auto=1&userserver=" + server + "&username=" + name + "&version=" + this.mainForm.Version;
                }
                else
                {
                    // 이미 자동 업데이트에서 카운트했기때문에 다시 카운트 하지 않도록하고 버전만 받아온다
                    url = url + "?auto=0";
                }

                WebRequest myWebRequest = WebRequest.Create(url);
                WebResponse myWebResponse = myWebRequest.GetResponse();
                StreamReader reader = new StreamReader(myWebResponse.GetResponseStream());
                string temp = reader.ReadLine();
                Notice = reader.ReadToEnd();
                this.mainForm.SetUpdateLabel("버전 정보 분석중");

                String version = temp.Split(':')[1].Trim();
                String currentVersion = this.mainForm.Version;

                if (version.CompareTo(currentVersion) == 0) return 0;
                else return 1;
            }
            catch
            {
                return -1;
            }
        }

        public String Notice;

        private String tempFilename = "";
        private int Download()
        {
            String url = "";
            try
            {
                WebRequest myWebRequest = WebRequest.Create(url);
                WebResponse myWebResponse = myWebRequest.GetResponse();
                myWebResponse.Close();

                long filesize = myWebResponse.ContentLength;

                int iRunningByteTotal = 0;

                using (WebClient client = new WebClient())
                {

                    using (Stream streamRemote = client.OpenRead(new Uri(url)))
                    {
                        using (Stream streamLocal = new FileStream(tempFilename, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            int iByteSize = 0;
                            byte[] byteBuffer = new byte[filesize];
                            while ((iByteSize = streamRemote.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                            {
                                streamLocal.Write(byteBuffer, 0, iByteSize);
                                iRunningByteTotal += iByteSize;

                                double dIndex = (double)(iRunningByteTotal);
                                double dTotal = (double)byteBuffer.Length;
                                double dProgressPercentage = (dIndex / dTotal);
                                int iProgressPercentage = (int)(dProgressPercentage * 100);
                                this.mainForm.SetUpdateProgress(iProgressPercentage);
                            }
                        }
                    }
                }
                return 0;
            }
            catch (Exception ee)
            {
                ee.ToString();
                return -1;
            }
        }

        private String batchFilename = "update.bat";
        private void Restart()
        {
            Process CurrentProcess = Process.GetCurrentProcess();
            string ExecuteFilePath = System.Reflection.Assembly.GetCallingAssembly().Location;

            String executeFilename = System.Reflection.Assembly.GetCallingAssembly().GetName().Name + ".exe";
            // 2013-05-08 ver0.2.2.0 
            // 버전이 업데이트 되면 이전 버전 세팅에 접근할수 없다.
            // 먼저 세팅에 저장한다.

            //this.mainForm.BeginInvoke(new EventHandler(delegate(object s, EventArgs ee)
            //{
            this.mainForm.SettingsSave();
            //}));

            // 세팅파일을 args로 전달한다..
            // 현재까지의 모든 리스트를 key=value &
            StringBuilder sb = new StringBuilder();
            sb.Append("WindowWidth=" + Properties.Settings.Default.WindowWidth + "&");
            sb.Append("WindowHeight=" + Properties.Settings.Default.WindowHeight + "&");
            sb.Append("WindowX=" + Properties.Settings.Default.WindowX + "&");
            sb.Append("WindowY=" + Properties.Settings.Default.WindowY + "&");
            sb.Append("FlagCommon=" + Properties.Settings.Default.FlagCommon + "&");
            sb.Append("FlagRare=" + Properties.Settings.Default.FlagRare + "&");
            sb.Append("FlagLegend=" + Properties.Settings.Default.FlagLegend + "&");
            sb.Append("FlagUnique=" + Properties.Settings.Default.FlagUnique + "&");
            sb.Append("FlagEpic=" + Properties.Settings.Default.FlagEpic + "&");
            sb.Append("FlagMythic=" + Properties.Settings.Default.FlagMythic + "&");
            sb.Append("ItemLevel=" + Properties.Settings.Default.ItemLevel);

            string BatchText = string.Format(@":Repeat
del ""{0}""
if exist ""{0}"" goto :Repeat
ren ""{1}"" ""{0}""
""{0}"" -update ""{2}""
", executeFilename, tempFilename, sb.ToString());

            RemoveBatchFile();
            Encoding enc = Encoding.GetEncoding("euc-kr");
            File.WriteAllText(batchFilename, BatchText, enc);
            ProcessStartInfo BatchProcess = new ProcessStartInfo(batchFilename);
            BatchProcess.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(BatchProcess);
            //CurrentProcess.Kill();
            this.mainForm.Exit();
        }

        public void RemoveBatchFile()
        {
            if (File.Exists(batchFilename)) File.Delete(batchFilename);
        }
    }
}

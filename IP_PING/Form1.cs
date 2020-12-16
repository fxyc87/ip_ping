using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace IP_PING
{
    public partial class Form1 : Form
    {
        List<Thread> threads = new List<Thread>();
        static List<List<byte[]>> ips = new List<List<byte[]>>();
        static List<string> msg = new List<string>();
        static List<int> jd = new List<int>();
        static List<string> msg2 = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "停止")
            {
                threads.ForEach(n =>
                {
                    try
                    {
                        n.Abort();
                    }
                    catch
                    {

                    }
                });
                button1.Text = "查找";
                return;
            }
            else
            {
                button1.Text = "停止";
            }
            int num = (int)numericUpDown1.Value;

            var r1 = IPAddress.TryParse(maskedTextBox1.Text, out IPAddress ip1);
            var r2 = IPAddress.TryParse(maskedTextBox2.Text, out IPAddress ip2);
            if (r1 == false || r2 == false) {
                MessageBox.Show("IP转换失败,无效IP地址！");
                return;
            }
            for (int i = 0; i < ips.Count; i++)
            {
                ips[i].Clear();
            }
            ips.Clear();
            for (int i = 0; i < num; i++)
            {
                ips.Add(new List<byte[]>());
            }
            

            var s1 = ip1.GetAddressBytes().Select(n=>(int)n).ToArray();
            var s2 = ip2.GetAddressBytes().Select(n => (int)n).ToArray();

            //10组 IP分配任务列表
            //169.254.0.0    169.254.1.0
            int index = 0;
            for (int i1 = s1[0]; i1 <= s2[0]; i1++)
            {
                for (int i2 = s1[1]; i2 <= s2[1]; i2++)
                {
                    for (int i3= s1[2]; i3 <= s2[2]; i3++)
                    {
                        for (int i4 = s1[3]; i4 <= s2[3]; i4++)
                        {
                            ips[index].Add(new byte[] { (byte)i1, (byte)i2, (byte)i3, (byte)i4 });
                            index++;
                            if (index >= num)
                                index = 0;
                            //System.Diagnostics.Debug.WriteLine(string.Join(",", new int[] { i1,i2,i3,i4}));
                        }
                    }
                }
            }

            int[] index2 = new int[4];
            bool[] exit = new bool[4];
            listBox1.Items.Clear();
            jd.Clear();
            msg2.Clear();
            threads.Clear();
            for (int i = 0; i < num; i++) {
                listBox1.Items.Add("线程" + i);
                jd.Add(0);
                msg2.Add("初始化中");
                var thread = new Thread(Ping);
                thread.Start(Tuple.Create<int, int>(i, (int)numericUpDown2.Value));
                threads.Add(thread);
            }
        }
        static void Ping(object arg) {
            try
            {
                int index = 0;
                var arg2 = (Tuple<int, int>)arg;
                Ping ping = new Ping();

                while (true)
                {
                    var ip = new IPAddress(ips[arg2.Item1][index]);
                    var r = ping.Send(ip, arg2.Item2);
                    if (r.Status == IPStatus.Success)
                    {
                        msg.Add("IP:" + r.Address.ToString() + " 耗时:" + r.RoundtripTime + "ms");
                        //System.Diagnostics.Debug.WriteLine(r.Address.ToString() + " 耗时:" + r.RoundtripTime);
                    }
                    else
                    {

                    }
                    msg2[arg2.Item1] = ip.ToString();
                    index++;
                    jd[arg2.Item1] = (int)((float)index / (float)ips[arg2.Item1].Count * 100.0f);
                    if (index >= ips[arg2.Item1].Count)
                    {
                        return;
                    }
                }

            }
            catch (Exception e) { 
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < listBox1.Items.Count; i++) {
                listBox1.Items[i] = "线程" + i +" \t" +msg2[i] + " \t" + jd[i]+"%";
            }
            lock (msg)
            {
                if (msg.Count > 0)
                {
                    listBox2.Items.Add(msg.First());
                    msg.RemoveAt(0);
                }
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }
    }
}

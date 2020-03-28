using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Drawing.Design;

namespace RunCopy
{
    public partial class Form1 : Form
    {

       /*
        int totalSize;
        int position;
        const int BUFFER_SIZE = 4096;
        byte[] buffer;
        Stream stream;
       */

        string Startformopen = "0";
        string strFile = "";
        string Restoredirectory = "";
        string Backupdirectory = "";
        string[] items;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Activated(object sender, EventArgs e)
        {

                                 string[] items = INIOperationClass.INIGetAllItems(Application.StartupPath + "/setting.ini", "Start");

                                 foreach (string value in items){

                                         string[] words = value.Split('=');

                                         if (words[0] == "Startform" && words[1] == "0") {
                                                        // this.Hide();
                                                         Startformopen = words[1];
                                         } else if (words[0] == "Title" && words[1] != "") { 
                                                         this.Text = words[1];
                                         }


                                         //Console.WriteLine(words[0]);


                                 }



        }

        private void Form1_Load(object sender, EventArgs e) {


                        Restoredirectory = INIOperationClass.INIGetStringValue(Application.StartupPath + "/setting.ini", "Restoredirectory", "directory", "");

                        textBox1.Text = Restoredirectory;

                        Backupdirectory = INIOperationClass.INIGetStringValue(Application.StartupPath + "/setting.ini", "Backupdirectory", "directory", ""); ;

                        textBox2.Text = Backupdirectory;

        }

        private void button3_Click(object sender, EventArgs e)
        {
                          folderBrowserDialog1.DirectoryPath = textBox2.Text;

                          if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK){
                                   textBox2.Text = folderBrowserDialog1.DirectoryPath;

                          } else {

                                   return;

                          }
        }
        private void ctrlCopy_Click(object sender, EventArgs e){

                    folderBrowserDialog1.DirectoryPath = textBox1.Text;
                    
                    if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK){
                            textBox1.Text = folderBrowserDialog1.DirectoryPath;
                    } else{
                            return;
                    }
        }


        private void button2_Click(object sender, EventArgs e){

                          items = INIOperationClass.INIGetAllItems(Application.StartupPath + "/setting.ini", "File");

                          backgroundWorker1.RunWorkerAsync();

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e){

            try{

                string str = Backupdirectory;// System.Windows.Forms.Application.StartupPath;
                string directoryPath = Restoredirectory;//Path.GetDirectoryName(Restoredirectory);
                if (!Directory.Exists(directoryPath)){
                    DirectoryInfo di = Directory.CreateDirectory(directoryPath);
                }
                // Console.WriteLine(items.Length);

                int sum = 0;

                foreach (string value in items) {

                                         sum++;

                                         string[] words = value.Split('=');

                                         UpdateUI(words[1]);

                                         //Console.WriteLine(directoryPath + "\\" + words[1]);

                                         File.Copy(str + "\\" + words[1], directoryPath + "\\" + words[1], true);


                                         backgroundWorker1.ReportProgress((int)(sum * 100 / items.Length));

                                         System.Threading.Thread.Sleep(100);

                                         //Console.WriteLine((int)(sum * 100 / items.Length));

                }

            } catch(Exception ex){

                MessageBox.Show(ex.Message.ToString(), "訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        delegate void bar_up_v(string v);
        private void UpdateUI(string strReceive){
                          if (this.InvokeRequired) {
                                     bar_up_v d = new bar_up_v(UpdateUI);
                                     this.Invoke(d, strReceive);
                          } else{
                                    label2.Text = strReceive;
                          }
        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBarX1.Value = e.ProgressPercentage;

            this.progressBarX1.Text = Convert.ToString(e.ProgressPercentage) + "%";
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                          this.progressBarX1.Value = 0;

                          this.progressBarX1.Text = "0%";

                          label2.Text = "等待..."; 

                          System.GC.Collect();
        }


        //解析位元組 轉換
        private string formatSizeBinary(Int64 size, Int32 decimals = 2){

                          string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                          double formattedSize = size;
                          Int32 sizeIndex = 0;
                          while (formattedSize >= 1024 & sizeIndex < sizes.Length){

                                     formattedSize /= 1024;
                                     sizeIndex += 1;
                          }
                          return string.Format("{0} {1}", Math.Round(formattedSize, decimals).ToString(), sizes[sizeIndex]);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {


            backgroundWorker1.CancelAsync();

            this.progressBarX1.Value = 0;

            this.progressBarX1.Text = "0%";

            label2.Text = "等待...";

            System.GC.Collect();

            System.Environment.Exit(0);

        }

        private void button4_Click(object sender, EventArgs e){

            items = INIOperationClass.INIGetAllItems(Application.StartupPath + "/setting.ini", "File");

            backgroundWorker2.RunWorkerAsync();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                string str = Backupdirectory;// System.Windows.Forms.Application.StartupPath;
                string directoryPath = Restoredirectory;//Path.GetDirectoryName(Restoredirectory);


                if (!Directory.Exists(Backupdirectory))
                {
                    Directory.CreateDirectory(Backupdirectory);
                }


                // Console.WriteLine(items.Length);

                int sum = 0;

                foreach (string value in items)
                {
                    sum++;

                    string[] words = value.Split('=');

                    UpdateUI(words[1]);

                    //Console.WriteLine(directoryPath + "\\" + words[1]);

                    if (File.Exists(Restoredirectory + "\\" + words[1])) { 

                        File.Copy(directoryPath + "\\" + words[1], str + "\\" + words[1], true);

                    }

                    backgroundWorker2.ReportProgress((int)(sum * 100 / items.Length));

                    System.Threading.Thread.Sleep(100);

                    //Console.WriteLine((int)(sum * 100 / items.Length));

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message.ToString(), "訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
                   this.progressBarX1.Value = e.ProgressPercentage;

                   this.progressBarX1.Text = Convert.ToString(e.ProgressPercentage) + "%";
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                          this.progressBarX1.Value = 0;

                          this.progressBarX1.Text = "0%";

                          label2.Text = "等待...";

                          System.GC.Collect();
        }
    }

}

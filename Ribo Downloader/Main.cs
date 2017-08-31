using Ribo_Downloader.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Ribo_Downloader
{
    public partial class Main : Form
    {
        public string pathToSave;
        public List<string> UrlToDown;

        System.Windows.Forms.Timer timer;
        bool exitFlag = false;
        Downloader downloader;
        bool isClose = false;
        public bool isTurnOffComputer = false;

        int Queue = 0, rowAt, curDown = 0, rowSelect = -1;

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Height;
            Classes.DB db = new Classes.DB();
            DataTable dt = db.get_all();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dataGridView1.Rows.Add(i + 1, dt.Rows[i]["name"], dt.Rows[i]["fsize"], dt.Rows[i]["dtime"], dt.Rows[i]["status"], "", "", "", "", dt.Rows[i]["link"], dt.Rows[i]["localpath"]);
            }
            db.Close();
            btnOpen.Enabled = false;
            btnPause.Enabled = false;
            btnPlay.Enabled = false;
            btnTrash.Enabled = false;
            dataGridView1.ClearSelection();
        }
        private void btn_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Enabled)
            {
                if (btn.Name == "btnAdd")
                    btn.BackgroundImage = Properties.Resources.btnPlus_white;
                if (btn.Name == "btnPlay")
                    btn.BackgroundImage = Properties.Resources.btnPlay_white;
                if (btn.Name == "btnPause")
                    btn.BackgroundImage = Properties.Resources.btnPause_white;
                if (btn.Name == "btnOpen")
                    btn.BackgroundImage = Properties.Resources.btnOpen_white;
                if (btn.Name == "btnTrash")
                    btn.BackgroundImage = Properties.Resources.btnTrash_white;
            }
        }

        private void btn_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Enabled)
            {
                if (btn.Name == "btnAdd")
                    btn.BackgroundImage = Properties.Resources.btnPlus_gray;
                if (btn.Name == "btnPlay")
                    btn.BackgroundImage = Properties.Resources.btnPlay_gray;
                if (btn.Name == "btnPause")
                    btn.BackgroundImage = Properties.Resources.btnPause_gray;
                if (btn.Name == "btnOpen")
                    btn.BackgroundImage = Properties.Resources.btnOpen_gray;
                if (btn.Name == "btnTrash")
                    btn.BackgroundImage = Properties.Resources.btnTrash_gray;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isClose)
            {
                e.Cancel = true;
                Hide();
                this.ShowInTaskbar = false;
            }
            else
            {
                if (downloader != null) downloader.Abort();
                Application.Exit();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.ShowInTaskbar = true;
        }

        private void menuExitTray_Click(object sender, EventArgs e)
        {
            isClose = true;
            Close();
            Application.Exit();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Add_Link al = new Add_Link() ;
            al.main = this;
            al.ShowDialog(this);
        }
        
        public void doAddLink()
        {
            string time = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            rowAt = dataGridView1.Rows.Count-1;
            btnAdd.Enabled = false;
            foreach (string str in UrlToDown)
            {
                dataGridView1.Rows.Add(dataGridView1.Rows.Count + 1, str, 0, time,  "Pending","0 KB", "0%", 0, 0, str, "");
            }
            Queue = UrlToDown.Count -1;
            getNameFile(curDown);
        }
        private void getNameFile(int idx)
        {
            new Thread((dwnlSource) =>
            {
                try
                {
                    rowAt++;
                    dataGridView1.Rows[rowAt].Cells[4].Value = "Naming";
                    if (timer != null) { timer.Stop(); }
                    if (downloader != null) { downloader.Abort().Join(); }
                    timer = new System.Windows.Forms.Timer();
                    timer.Interval = 100;
                    timer.Tick += new EventHandler(Tracker);
                    timer.Start();

                    downloader = new Downloader(UrlToDown[idx], VAR.SavePath, timer);
                    string name = downloader.FileName;
                    dataGridView1.Rows[rowAt].Cells[1].Value = name;
                    dataGridView1.Rows[rowAt].Cells[10].Value = VAR.SavePath + name;
                    string[] ext = name.Split(new char[] { '.' }, StringSplitOptions.None);
                    ShowInfor(Classes.FileIcon.GetLargeIcon("*."+ ext[ext.Length-1]), name, VAR.SavePath + name, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), downloader.FormatBytes(downloader.FileSize));
                    downloader.Start();
                    while (exitFlag == false)
                    {
                        Application.DoEvents();
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Download Failed");
                    ResetUI();
                }
            }).Start(UrlToDown[idx]);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            rowSelect = e.RowIndex;
            btnOpen.Enabled = true;
            btnPause.Enabled = true;
            btnPlay.Enabled = true;
            btnTrash.Enabled = true;
            string[] ext = dataGridView1.Rows[rowSelect].Cells[1].Value.ToString().Split(new char[] { '.' }, StringSplitOptions.None);
            ShowInfor(Classes.FileIcon.GetLargeIcon("*." + ext[ext.Length - 1]),
                dataGridView1.Rows[rowSelect].Cells[1].Value.ToString(),
                dataGridView1.Rows[rowSelect].Cells[10].Value.ToString(),
                dataGridView1.Rows[rowSelect].Cells[3].Value.ToString(),
                dataGridView1.Rows[rowSelect].Cells[2].Value.ToString(),
                dataGridView1.Rows[rowSelect].Cells[4].Value.ToString());
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (rowSelect < 0)
            {
                MessageBox.Show("Select file to pause download");
                return;
            }
            if(downloader != null)
                downloader.Abort();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (rowSelect < 0)
            {
                MessageBox.Show("Select file to open");
                return;
            }
            System.Diagnostics.Process.Start(dataGridView1.Rows[rowSelect].Cells[10].Value.ToString());
        }

        private void btnTrash_Click(object sender, EventArgs e)
        {
            if (rowSelect < 0)
            {
                MessageBox.Show("Select file to remove");
                return;
            }
            string l = dataGridView1.Rows[rowSelect].Cells[9].Value.ToString();
            dataGridView1.Rows.RemoveAt(rowSelect);
            Classes.DB db = new Classes.DB();
            db.delete_by_link(l);
            db.Close();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (rowSelect < 0)
            {
                MessageBox.Show("Select file to redownload");
                return;
            }
            UrlToDown = new List<string>();
            UrlToDown.Add(dataGridView1.Rows[rowSelect].Cells[9].Value.ToString());
            curDown = Queue = 0;
            rowAt = rowSelect-1;
            getNameFile(0);
        }

        
        private void Tracker(object sender, EventArgs e)
        {
            Classes.DB db = new Classes.DB();
            DataGridViewRow r = dataGridView1.Rows[rowAt];
            string strCompleted = downloader.FormatBytes(downloader.Completed);
            string strSpeed = "0.00 KBps";
            string strProgress = "Pending";
            string valProgress = "0%";

            switch (downloader.State)
            {
                case State.Create:
                    strProgress = "Pending";

                    break;
                case State.Idle:
                    strProgress = "Paused";
                    timer.Stop();
                    db.insert(r.Cells[1].Value.ToString(), r.Cells[3].Value.ToString(), downloader.FormatBytes(downloader.FileSize), strProgress, r.Cells[9].Value.ToString(), r.Cells[10].Value.ToString());
                    if (curDown < Queue)
                    {
                        curDown++;
                        getNameFile(curDown);
                    }
                    else
                        ResetUI();
                    break;
                case State.Start:
                   strProgress = "Starting";

                    break;
                case State.Download:
                    strSpeed = downloader.FormatBytes(downloader.Speed) + "ps";
                    valProgress = string.Format("{0:f2}%", downloader.Progress);
                    strProgress = "Downloading";
                    
                    break;
                case State.Append:
                    strProgress = "Appending";

                    break;
                case State.Complete:
                    valProgress = "100%";
                    strProgress = "Completed";
                    db.insert(r.Cells[1].Value.ToString(), r.Cells[3].Value.ToString(), downloader.FormatBytes(downloader.FileSize), strProgress, r.Cells[9].Value.ToString(), r.Cells[10].Value.ToString());
                    timer.Stop();
                    if (curDown < Queue)
                    {
                        curDown++;
                        getNameFile(curDown);
                    }
                    else
                    {
                        if (isTurnOffComputer)
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo("shutdown", "/s /t 1");
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            System.Diagnostics.Process.Start(psi);
                        }
                        ResetUI();
                    }

                    break;
                case State.Error:
                    strProgress = "Error";
                    timer.Stop();
                    db.insert(r.Cells[1].Value.ToString(), r.Cells[3].Value.ToString(), downloader.FormatBytes(downloader.FileSize), strProgress, r.Cells[9].Value.ToString(), r.Cells[10].Value.ToString());

                    if (curDown < Queue)
                    {
                        curDown++;
                        getNameFile(curDown);
                    }else
                    ResetUI();

                    break;
                case State.Abort:
                    strProgress = "Pausing";
                    break;

            }
            dataGridView1.Rows[rowAt].Cells[4].Value = strProgress;
            dataGridView1.Rows[rowAt].Cells[5].Value = valProgress;
            dataGridView1.Rows[rowAt].Cells[8].Value = strSpeed;
            dataGridView1.Rows[rowAt].Cells[6].Value = strCompleted; 
            dataGridView1.Rows[rowAt].Cells[7].Value = downloader.scheduler.ThreadLength;
            db.Close();
            ShowInfor(null, null, null, null, null, strProgress);
        }
        private void ResetUI()
        {
            MethodInvoker inv = delegate
            {
                splitContainer2.SplitterDistance = splitContainer2.Height;
                btnAdd.Enabled = true;
            };
            this.Invoke(inv);
        }
        private void ShowInfor(Icon ico = null, string fname = null, string saved = null, string time = null, string fsize = null, string status = "Pending")
        {
            MethodInvoker inv = delegate
            {
                if (fname != null)
                {
                    lbName.Text = fname;
                }
                if (saved != null)
                {
                    lbSaved.Text = "Local path: " + saved;
                }
                if (time != null)
                {
                    lbTime.Text = "Time: " + time;
                }
                if (fsize != null)
                {
                    lbSize.Text = "Size: " + fsize;
                    dataGridView1.Rows[rowAt].Cells[2].Value = fsize;
                }
                if (ico != null)
                {
                    ptbIcon.Image = Bitmap.FromHicon(ico.Handle);
                }
                lbStatus.Text = "Status: " + status;
                splitContainer2.SplitterDistance = 286;
            };
            this.Invoke(inv);
        }
    }
}

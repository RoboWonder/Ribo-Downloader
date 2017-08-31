using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ribo_Downloader
{
    public partial class Add_Link : Form
    {
        public Main main;
        public Add_Link()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Add_Link_Load(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string url = Clipboard.GetText(TextDataFormat.Text);
                if (ValidateUrl(url))
                    txtUrl.Text = url;
                txtPath.Text = VAR.SavePath;
                // Do whatever you need to do with clipboardText
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    txtPath.Text = VAR.SavePath = fbd.SelectedPath + "\\";
                    System.IO.File.WriteAllText(VAR.LocalData + "save_path.txt", VAR.SavePath);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            main.isTurnOffComputer = cbTurnOff.Checked;
            List<string> arr = new List<string>();
            foreach (string url in txtUrl.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None))
            {
                if (!string.IsNullOrEmpty(url) && ValidateUrl(url))
                {
                    arr.Add(url);
                }
            }
            if (arr.Count()>0)
                main.UrlToDown = arr;
            else
            {
                MessageBox.Show("Has url not match!");
                return;
            }
            main.doAddLink();
            Close();
        }

        private void txtPath_MouseClick(object sender, MouseEventArgs e)
        {
            this.ActiveControl = null;
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }
        private bool ValidateUrl(string str)
        {
            Uri uriResult;
            return (bool) Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}

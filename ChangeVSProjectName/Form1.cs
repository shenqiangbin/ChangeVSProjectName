using ChangeVSProjectName.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChangeVSProjectName
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            var dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                textBoxSelectPath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxSelectPath.Text))
            {
                MessageBox.Show("请选择文件夹");
                return;
            }
            if (string.IsNullOrEmpty(txtKeyWords.Text))
            {
                MessageBox.Show("请输入要替换的文件");
                return;
            }

            rtxResult.Text = "";
            string dir = textBoxSelectPath.Text;
            if (!Directory.Exists(dir))
            {
                rtxResult.Text += ("\r\n" + dir + "-不存在");
                return;
            }
            FindTextInDir(new DirectoryInfo(dir), txtKeyWords.Text);

            if (string.IsNullOrEmpty(rtxResult.Text))
                rtxResult.Text += "\r\n 没有查到";
            rtxResult.Text += "\r\n 查找完成";
        }

        private void FindTextInDir(DirectoryInfo dir, string text)
        {
            FileInfo[] files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                if (Path.GetExtension(item.FullName) == ".dll")
                    continue;

                if (item.Name.Contains(text))
                    ShowMsg(item.FullName);

                using (StreamReader reader = new StreamReader(item.FullName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.Contains(text))
                            ShowMsg(line);
                    }
                }
            }

            DirectoryInfo[] children = dir.GetDirectories();
            foreach (var item in children)
            {
                if (IsIgnoreThis(item.Name))
                    continue;

                FindTextInDir(item, text);
            }

            if (dir.Name.Contains(text))
            {
                ShowMsg(dir.FullName);
            }
        }

        private bool IsIgnoreThis(string fileName)
        {
            if (fileName == "bin" || fileName == "obj" || fileName.Contains(".git"))
                return true;
            else
                return false;
        }

        private void ShowMsg(string msg)
        {
            rtxResult.AppendText(msg + "\r\n");
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            rtxResult.Text = "";
            btnExport.Enabled = false;
            btnExport.Text = "替换中...";

            new Thread(() =>
            {
                try
                {

                    string dir = textBoxSelectPath.Text;
                    FindTextInDirAndReplace(new DirectoryInfo(dir), txtKeyWords.Text, txtNewText.Text);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (btnExport.InvokeRequired)
                    {
                        btnExport.Invoke(new Action<string>((m) =>
                        {
                            btnExport.Enabled = true;
                            btnExport.Text = "开始替换";
                        }), "");
                    }
                    else
                    {
                        btnExport.Enabled = true;
                        btnExport.Text = "开始替换";
                    }

                    if (rtxResult.InvokeRequired)
                    {
                        rtxResult.Invoke(new Action<string>((m) =>
                        {
                            rtxResult.Text += "\r\n完成";
                        }), "");
                    }
                }
            }).Start();
        }

        private void FindTextInDirAndReplace(DirectoryInfo dir, string text, string newText)
        {

            FileInfo[] files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                if (Path.GetExtension(item.FullName) == ".dll")
                    continue;

                string content = File.ReadAllText(item.FullName);
                File.WriteAllText(item.FullName, content.Replace(text, newText));

                if (item.Name.Contains(text))
                {
                    var newPath = Path.Combine(Path.GetDirectoryName(item.FullName), item.Name.Replace(text, newText));
                    File.Move(item.FullName, newPath);
                }

            }

            DirectoryInfo[] children = dir.GetDirectories();
            foreach (var item in children)
            {
                if (IsIgnoreThis(item.Name))
                    continue;

                FindTextInDirAndReplace(item, text, newText);
            }

            if (dir.Name.Contains(text))
            {
                var newPath = Path.Combine(Path.GetDirectoryName(dir.FullName), dir.Name.Replace(text, newText));
                Directory.Move(dir.FullName, newPath);
            }
        }
    }




}

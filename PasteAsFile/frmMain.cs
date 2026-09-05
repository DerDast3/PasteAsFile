using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasteAsFile
{
    public partial class frmMain : Form
    {
        public string CurrentLocation { get; set; }
        public bool IsText { get; set; }
        public frmMain()
        {
            InitializeComponent();
        }
        public frmMain(string location)
        {
            InitializeComponent();
            this.CurrentLocation = location;
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            txtFilename.Text = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");
            txtCurrentLocation.Text = CurrentLocation ?? @"C:\";

            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\Paste As File\command", "", null) == null)
            {
                if (MessageBox.Show("Seems that you are running this application for the first time,\nDo you want to Register it with your system Context Menu ?", "Paste As File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Program.RegisterApp();
                }
            }

            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                IsText = true;
                txtContent.Text = clipboardText;

                if (LooksLikeMarkdown(clipboardText))
                {
                    lblType.Text = "Markdown File";
                    comExt.SelectedItem = "md";
                }
                else
                {
                    lblType.Text = "Text File";
                    comExt.SelectedItem = "txt";
                }
                return;
            }

            if (Clipboard.ContainsImage())
            {
                lblType.Text = "Image";
                comExt.SelectedItem = "png";
                imgContent.Image = Clipboard.GetImage();
                return;
            }

            lblType.Text = "Unknown File";
            btnSave.Enabled = false;
            
            
        }

        private static bool LooksLikeMarkdown(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            int score = 0;
            int nonEmptyLines = 0;

            foreach (string line in lines)
            {
                string trimmed = line.TrimStart();
                if (trimmed.Length == 0)
                    continue;

                nonEmptyLines++;

                if (Regex.IsMatch(trimmed, @"^#{1,6}\s+\S"))
                    score += 3;
                else if (Regex.IsMatch(trimmed, @"^```"))
                    score += 3;
                else if (Regex.IsMatch(trimmed, @"^[-*+]\s+\[[ xX]\]\s+\S"))
                    score += 3;
                else if (Regex.IsMatch(trimmed, @"^(-{3,}|\*{3,}|_{3,})\s*$"))
                    score += 2;
                else if (Regex.IsMatch(trimmed, @"^>\s?\S"))
                    score += 2;
                else if (Regex.IsMatch(trimmed, @"^[-*+]\s+\S"))
                    score += 1;
                else if (Regex.IsMatch(trimmed, @"^\d+[.)]\s+\S"))
                    score += 1;
                else if (Regex.IsMatch(trimmed, @"^\|.*\|\s*$"))
                    score += 2;
            }

            if (Regex.IsMatch(text, @"!\[[^\]]*\]\([^)]+\)"))
                score += 2;
            if (Regex.IsMatch(text, @"\[[^\]]+\]\([^)]+\)"))
                score += 2;
            if (Regex.IsMatch(text, @"\*\*[^*\n]+\*\*"))
                score += 2;
            if (Regex.IsMatch(text, @"`[^`\n]+`"))
                score += 1;

            return score >= Math.Max(3, nonEmptyLines / 2);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string location = txtCurrentLocation.Text;
            location = location.EndsWith("\\") ? location : location + "\\";
            string filename = txtFilename.Text + "." + comExt.SelectedItem.ToString() ;
            if (IsText)
            {

                File.WriteAllText(location+filename,txtContent.Text,Encoding.UTF8);
                this.Text += " : File Saved :)";
            }
            else
            {
                switch (comExt.SelectedItem.ToString())
                {
                    case "png":
                        imgContent.Image.Save(location + filename, ImageFormat.Png);
                        break;
                    case "ico":
                        imgContent.Image.Save(location + filename, ImageFormat.Icon);
                        break;
                    case "jpg":
                        imgContent.Image.Save(location + filename, ImageFormat.Jpeg);
                        break;
                    case "bmp":
                        imgContent.Image.Save(location + filename, ImageFormat.Bmp);
                        break;
                    case "gif":
                        imgContent.Image.Save(location + filename, ImageFormat.Gif);
                        break;
                    default:
                        imgContent.Image.Save(location + filename, ImageFormat.Png);
                        break;
                }
                
                this.Text += " : Image Saved :)";
            }

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Environment.Exit(0);
            });
        }

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a folder for saving this file ";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtCurrentLocation.Text = fbd.SelectedPath;
            }
        }

        private void lblWebsite_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://eslamx.com") { UseShellExecute = true });
        }

        private void lblMe_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/DerDast3/PasteAsFile") { UseShellExecute = true });
        }

        private void lblHelp_Click(object sender, EventArgs e)
        {
            string msg = "Paste As File helps you paste any text or images in your system clipboard into a file directly instead of creating new file yourself";
            msg += "\n--------------------\nTo Register the application to your system Context Menu (per user, no administrator rights needed) run the application with this argument : /reg";
            msg += "\nto Unregister the application use this argument : /unreg\n";
            msg += "\n--------------------\nMarkdown content is detected automatically and offered as .md\n";
            msg += "\n--------------------\nEnhanced fork by madeByDast : https://github.com/DerDast3/PasteAsFile\n";
            msg += "\n--------------------\nSend Feedback to : EslaMx7@Gmail.Com\n\nThanks :)";
            MessageBox.Show(msg,"Paste As File Help",MessageBoxButtons.OK,MessageBoxIcon.Information);


           
        }
    }
}

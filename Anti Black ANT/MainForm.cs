using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;

using WindowsInput;
using EventArguments;
using Security;
using Zipper;


namespace Anti_Black_ANT
{
    public partial class MainForm : Form
    {
        string Black_ANT_LogPATH;
        string UnZipPATH;
        DateTime ModifiDateTime = DateTime.UtcNow;
        ZipEncryption zipE;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        static System.Threading.CancellationTokenSource CTS = new System.Threading.CancellationTokenSource();
        static ReflectorEventList MainReflector = new ReflectorEventList();


        public MainForm()
        {
            InitializeComponent();

            timer.Interval = 1000;
            timer.Tick += timer_Tick;

            zipE = new ZipEncryption(Environment.CurrentDirectory, AppDataManager.GetMainHashPassword);
            zipE.ReportOccurrence += zipE_ReportOccurrence;
            zipE.ErrorOccurrence += zipE_ErrorOccurrence;
            zipE.ExtractProgress += zipE_ExtractProgress;
            zipE.ReadProgress += zipE_ReadProgress;
            zipE.AddProgress += zipE_AddProgress;
            zipE.ZipError += zipE_ZipError;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text += ComponentAssembly.ComponentController.IsAdmin() ? "   (Run as Administrator)" : "   (Run as Current User)";


            txtExecuteFilePath.Text = AppDataManager.ReadData("ExecuteClusterLoc", AppDataManager.GetMainHashPassword);
            this.Black_ANT_LogPATH = txtExecuteFilePath.Text + "\\Black ANT.log";

            if (!string.IsNullOrEmpty(txtExecuteFilePath.Text)) LogAnalyze();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!File.Exists(Black_ANT_LogPATH)) { txtLog.Text = ""; return; }

            if (File.GetLastWriteTimeUtc(Black_ANT_LogPATH) == ModifiDateTime)
                return;

            this.timer.Enabled = false;
            //
            ModifiDateTime = File.GetLastWriteTimeUtc(Black_ANT_LogPATH);

            txtLog.Text = File.ReadAllText(Black_ANT_LogPATH);;

            int LineIndex = 0;

            foreach (string line in txtLog.Lines)
            {
                txtLog.Select(LineIndex, line.Length);

                if (line.Contains("(#EXP#)"))
                    txtLog.SelectionBackColor = Color.Red;
                else if (line.Contains("Kernel"))
                    txtLog.SelectionBackColor = Color.Yellow;
                else if (line.Contains("========="))
                    txtLog.SelectionBackColor = Color.Green;
                else
                    txtLog.SelectionBackColor = txtLog.BackColor;

                LineIndex += line.Length + 1;
            }
            //
            // Scroll To End Line
            //
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            //
            this.timer.Enabled = true;
        }

        private void btnThrowPassKey_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 5000; // 5sec
            t.Tick += (s, ea) => { ((Button)sender).Enabled = true; t.Stop(); t.Dispose(); };


            InputSimulator.SimulateKeyDown(VirtualKeyCode.RETURN);        // 'Enter'
            InputSimulator.SimulateTextEntry(@"God_H\,g,d@13");
            InputSimulator.SimulateKeyDown(VirtualKeyCode.RETURN);        // 'Enter'

            t.Start();
        }

        private void LogAnalyze()
        {
            if (timer.Enabled == false)
            {
                timer.Start();
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(Black_ANT_LogPATH))
            {
                File.Delete(Black_ANT_LogPATH);
            }
        }

        private void btnUnzip_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "Black ANT files *.bant";
            ofd.Multiselect = true;
            ofd.SupportMultiDottedExtensions = false;
            ofd.Title = "Please Select a .bant file to Decoded that's";

            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FolderBrowserDialog FBD = new FolderBrowserDialog())
                {
                    if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        UnZipPATH = Path.Combine(FBD.SelectedPath + "\\UnZipped Files\\");

                        UnZipFilesAsync(ofd.FileNames);
                    }
                }
            }
        }

        public void UnZipFilesAsync(string[] FileNames)
        {
            Task.Factory.StartNew(() =>
                {
                    CreateReportForm(MainReflector);

                    int bantFileConter = 0;

                    foreach (string filePath in FileNames)
                    {
                        grbProcessBar.Invoke(new Action(delegate
                        {
                            grbProcessBar.Text = string.Format("UnZip Process  {0}/{1}", ++bantFileConter, FileNames.Count());
                        }));

                        try
                        {
                            if (File.Exists(filePath))
                            {
                                txtBantPath.Invoke(new Action(delegate { txtBantPath.Text = filePath; }));

                                string JustFileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                                JustFileName = JustFileName.Substring(0, JustFileName.LastIndexOf('.'));
                                string UnZipFolderPath = UnZipPATH + JustFileName + "\\";

                                txtUnzipFolderPath.Invoke(new Action(delegate { txtUnzipFolderPath.Text = UnZipFolderPath; }));

                                prbMaster.Invoke(new Action(delegate { prbMaster.Value = 0; }));
                                string Zip2Path = filePath;

                                zipE.UnZip(Zip2Path, txtUnzipFolderPath.Text, CTS);
                            }
                        }
                        catch (Exception ex) { MainReflector.CallReporter("UnZipFilesAsync", new ReportEventArgs("UnZipFilesAsync (#EXP#)", ex.Message)); }
                    }
                });
        }

        private void CreateReportForm(ReflectorEventList MainReflector)
        {
            Task.Factory.StartNew(() =>
                {
                    Form ReportForm = new Form();
                    ReportForm.Size = new Size(500, 500);
                    ReportForm.Text = "Reports";
                    //
                    // Add a Docked MultiLine TextBox
                    //
                    TextBox txtReports = new TextBox();
                    txtReports.ReadOnly = true;
                    txtReports.Multiline = true;
                    txtReports.Location = new Point(0, 0);
                    txtReports.Dock = DockStyle.Fill;
                    txtReports.Cursor = Cursors.No;
                    txtReports.ScrollBars = ScrollBars.Both;
                    txtReports.TextChanged += (s, e) =>
                    {
                        txtReports.SelectionStart = txtReports.Text.Length;
                        txtReports.ScrollToCaret();
                    };
                    ReportForm.Controls.Add(txtReports);
                    //
                    // Reflect this class reports to report form text box
                    //
                    MainReflector.ReflectedReports += (s, e) => txtReports.Invoke(new Action(delegate { txtReports.Text += e.Message + Environment.NewLine; }));
                    //
                    ReportForm.ShowDialog();
                });
        }

        void zipE_ZipError(object sender, Ionic.Zip.ZipErrorEventArgs e)
        {
            MainReflector.CallReporter(sender, new ReportEventArgs("ZipE.ZipError", e.Exception.Message));
        }
        void zipE_AddProgress(object sender, Ionic.Zip.AddProgressEventArgs e)
        {
            string msg = string.Format(@"Add Progress[ Archive Name: {0}  ,   Current Entry: {1}  ,   Transferred Bytes: {2}  ,   TotalEntries: {3}   ,   Total Bytes to Transfer: {4} ]",
                    e.ArchiveName, e.CurrentEntry, e.BytesTransferred, e.EntriesTotal, e.TotalBytesToTransfer);

            MainReflector.CallReporter(sender, new ReportEventArgs("ZipE.zipE_AddProgress", msg));
        }
        void zipE_ExtractProgress(object sender, Ionic.Zip.ExtractProgressEventArgs e)
        {
            try
            {
                prbSlave.Invoke(new Action(delegate
                {
                    prbSlave.Value = (int)zipE.TotalTransferredPercentForCurrentEntry;
                }));


                prbMaster.Invoke(new Action(delegate
                {
                    prbMaster.Value = (int)zipE.TotalTransferredPercentForAllEntry;
                }));
            }
            catch { }
        }
        void zipE_ReadProgress(object sender, Ionic.Zip.ReadProgressEventArgs e)
        {
            string msg = string.Format(@"Read Progress[ Archive Name: {0}  ,   Current Entry: {1}  ,   Transferred Bytes: {2}  ,   TotalEntries: {3}   ,   Total Bytes to Transfer: {4} ]",
                    e.ArchiveName, e.CurrentEntry, e.BytesTransferred, e.EntriesTotal, e.TotalBytesToTransfer);

            MainReflector.CallReporter(sender, new ReportEventArgs("ZipE.zipE_ReadProgress", msg));
        }
        void zipE_ErrorOccurrence(object sender, System.IO.ErrorEventArgs e)
        {
            MainReflector.CallReporter(sender, new ReportEventArgs("ZipE.zipE_ErrorOccurrence", string.Format(@"Exception Message: {0}", e.GetException().Message)));
        }
        void zipE_ReportOccurrence(object sender, EventArguments.ReportEventArgs e)
        {
            MainReflector.CallReporter(sender, new ReportEventArgs("ZipE.zipE_ErrorOccurrence", e.Message));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CTS.Cancel();
        }

        private void btnLogAnalyzer_Click(object sender, EventArgs e)
        {
            txtExecuteFilePath.Text = AppDataManager.ReadData("ExecuteClusterLoc", AppDataManager.GetMainHashPassword);
            this.Black_ANT_LogPATH = txtExecuteFilePath.Text + "\\Black ANT.log";

            if (!string.IsNullOrEmpty(txtExecuteFilePath.Text)) LogAnalyze();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtExecuteFilePath.Text))
            {
                string appExecutePath = Path.Combine(txtExecuteFilePath.Text, "Black ANT.exe");
                if (File.Exists(appExecutePath))
                {
                    try
                    {
                        foreach (var proc in System.Diagnostics.Process.GetProcesses())
                        {
                            if (proc.ProcessName == "Black ANT")
                            { proc.Kill(); break; }
                        }

                        FileInfo file = new FileInfo(appExecutePath);
                        file.NormalAttributer();
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        timer.Stop(); 
                        
                        if(ex.Message.StartsWith("Access to the path")) // Message: "Access to the path 'C:\Program Files\Common Files\Behzad.Kh\Black ANT.exe' is denied."
                        {
                            // Access Created but file is running, so
                            // ReRun 
                            btnDelete_Click(sender, e);
                        }
                        else if(ex.Message == "Object reference not set to an instance of an object.")
                        {
                            MessageBox.Show("The Black ANT running as Administrator!\n\r" +
                                "So first this app closed and run again as Administrator.\n\rFinally retry to do it.",
                                "Administrator Running", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ComponentAssembly.ComponentController.RestartElevated(Application.ExecutablePath);
                        }
                    }
                    finally
                    {
                        btnClearLog_Click(sender, e);
                        MessageBox.Show("Black ANT Deleted Successfully");
                    }
                }
                else MessageBox.Show("File Not Found");
            }
        }

        private void btnShowStartUpPath_Click(object sender, EventArgs e)
        {
            timer.Stop();
            txtLog.Text = "Black ANT Start Up Paths:" + Environment.NewLine;

            try
            {
                //
                // Search Registry.LocalMachine
                //
                using (RegistryKey main = Registry.LocalMachine)
                {
                    using (RegistryKey key = main.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                    {
                        if (key.GetValue("Black ANT") != null)
                            txtLog.Text += "Registry ---> HKLM:Run" + Environment.NewLine;
                    }
                }
                //
                // Search Registry.CurrentUser
                //
                using (RegistryKey main = Registry.CurrentUser)
                {
                    using (RegistryKey key = main.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                    {
                        if (key.GetValue("Black ANT") != null)
                            txtLog.Text += "Registry ---> HKCU:Run" + Environment.NewLine;
                    }
                }
                //
                // Search CommonStartup
                //
                String fileDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Black ANT.lnk");

                if (File.Exists(fileDestination))
                    txtLog.Text += fileDestination + Environment.NewLine;
                //
                // Search Startup
                //
                fileDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Black ANT.lnk");

                if (File.Exists(fileDestination))
                    txtLog.Text += fileDestination + Environment.NewLine;
            }
            catch { }
        }

        private void btnDeleteFromStartUp_Click(object sender, EventArgs e)
        {
            timer.Stop();
            txtLog.Text = "Black ANT Start Up Paths:" + Environment.NewLine;
            
            if (!ComponentAssembly.ComponentController.IsAdmin())
            {
                MessageBox.Show("The Black ANT running as Administrator!\n\r" +
                "So first this app closed and run again as Administrator.\n\rFinally retry to do it.",
                "Administrator Running", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ComponentAssembly.ComponentController.RestartElevated(Application.ExecutablePath);
            }

            try
            {
                //
                // Search Registry.LocalMachine
                //
                using (RegistryKey main = Registry.LocalMachine)
                {
                    using (RegistryKey key = main.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) 
                    {
                        if (key.GetValue("Black ANT") != null)
                        {
                            txtLog.Text += "Registry ---> HKLM:Run" + Environment.NewLine;
                            key.DeleteValue("Black ANT");
                        }
                    }
                }
                //
                // Search Registry.CurrentUser
                //
                using (RegistryKey main = Registry.CurrentUser)
                {
                    using (RegistryKey key = main.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (key.GetValue("Black ANT") != null)
                        {
                            txtLog.Text += "Registry ---> HKCU:Run" + Environment.NewLine;
                            key.DeleteValue("Black ANT");
                        }
                    }
                }
                //
                // Search CommonStartup
                //
                String fileDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Black ANT.lnk");

                if (File.Exists(fileDestination))
                {
                    txtLog.Text += fileDestination + Environment.NewLine;
                    File.Delete(fileDestination);
                }
                //
                // Search Startup
                //
                fileDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Black ANT.lnk");

                if (File.Exists(fileDestination))
                {
                    txtLog.Text += fileDestination + Environment.NewLine;
                    File.Delete(fileDestination);
                }
            }
            catch { }
            finally
            {
                txtLog.Text += Environment.NewLine + "Delete Startup Paths Completed!";
            }
        }

        private void btnOpenWorkingFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtExecuteFilePath.Text)) System.Diagnostics.Process.Start(txtExecuteFilePath.Text);
        }
    }
}

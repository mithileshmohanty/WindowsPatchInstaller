using log4net;
using Microsoft.Win32;
using NReco.PdfGenerator;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Resources;
using System.Security;
using System.Windows;
using WindowsPatchInstallerUI.Properties;


namespace WindowsPatchInstallerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ResourceManager StringManager;
        private const string PdfFooterText = "WindowsPachInstallerUI Version 1.0.0.0 beta-1 © GE Healthcare Pvt.Ltd.";
        private const string InitialText = @"
        <html>
            <head>
            <style>
            table, th, td {
              border: 1px solid black;
              border-collapse: collapse;
            }

            </style>
            </head>
            <body>
                <div align='center'>
                    <div style = 'float:left; align:center; padding-top: 0px; padding-right: 30px; padding-bottom: 20px; padding-left: 20px;'>
                        <img src='..\..\image\icon.jpg' width='80' height='80'>
                    </div>
                    <div style = 'float:left; align:center;'>
                        <h1>OS Patch Installation Report</h1>
                    </div>
                </div>";
        private const string EnvironmentText = @"
<table style = 'width:100%;border: 3px dashed black;border-collapse: collapse;'>
	<tr>
		<td><b>Machine Name:</b></td>
		<td>{0}</td>
	</tr>
	<tr>
		<td><b>Serial Number:</b></td>
		<td>{1}</td>
	</tr>
	<tr>
		<td><b>IP Address:</b></td>
		<td>{2}</td>
	</tr>
	<tr>
		<td><b>MAC Address:</b></td>
		<td>{3}</td>
	</tr>
	<tr>
		<td><b>Internal OS Version:</b></td>
		<td>{4}</td>
	</tr>
	<tr>
		<td><b>Internal OS Patch Version:</b></td>
		<td>{5}</td>
	</tr>
	<tr>
		<td><b>Date:</b></td>
		<td>{6}</td>
	</tr>
	<tr>
		<td><b>Operation By:</b></td>
		<td>{7}</td>
	</tr>
	<tr>
		<td><b>Operator SSO:</b></td>
		<td>{8}</td>
	</tr>
</table>";
        private const string PatchListText = @"
<table style = 'width:100%;border: 1px black;'>";
        private const string PatchListElementText = @"
	<tr>
		<b>Patch Name : </b> {0}
        <b>Patch Code : </b> {1}
        <b>Release Date : </b> {2}
        <b>Support Link : </b> {3}
        <b>Classification : </b> {4}
        <b>Severity : </b> {5}
        <b>Product Type : </b> {6}
	</tr>
";
        private const string SignatoriesText = @"
<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                <div align='center'>
                            <div style = 'float:left; align:center; padding-top: 0px; padding-right: 30px; padding-bottom: 20px; padding-left: 20px;'>
                                 <p>------------------------------------</p>
                                <h4>{0}</h1>
                            </div>
                            <div style = 'float:right; align:center;'>
                              <p>------------------------------------</p>
                                <h4>{1}</h1>
                            </div>
                 </div>";
        private const string EndText = @"
            </body>
        </html>";
        private const string OsEnvironmentText = @"
<table style = 'width:100%;border: 3px dashed black;border-collapse: collapse;'>
	<tr>
		<td><b>OS Name:</b></td>
		<td>{0}</td>
	</tr>
    <tr>
		<td><b>OS Edition:</b></td>
		<td>{1}</td>
	</tr>
    <tr>
		<td><b>OS Version:</b></td>
		<td>{2}</td>
	</tr>
    <tr>
		<td><b>OS Build:</b></td>
		<td>{3}</td>
	</tr>
	<tr>
		<td><b>OS Architecture:</b></td>
		<td>{4}</td>
	</tr>
    <tr>
		<td><b>Internal OS Version:</b></td>
		<td>{5}</td>
	</tr>
    <tr>
		<td><b>Internal OS Patch Version:</b></td>
		<td>{6}</td>
	</tr>
</table>
";
        private const string WinDetailsRegKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
        private const string RegKey = "SOFTWARE\\GE_Healthcare_Lifesciences\\Windows";
        private const string RegWin10Key = "SOFTWARE\\WOW6432Node\\GE_Healthcare_Lifesciences\\Windows";
        private string _htmlFileName;
        private string _pdfFileName;
        private string _oldOsEnvironmentText;
        private readonly ILog _log;

        /// <summary>
        /// Default Constructor for MainWindow
        /// </summary>
        public MainWindow()
        {
            _log = LogManager.GetLogger("WindowsPatchInstallerUI");
            InitializeComponent();
            Initialize();
            _log.Debug("Exiting MainWindow Constructor");
        }

        private void Initialize()
        {

            _log.Debug("Entering Initialize");
            NamePage.Visibility = Visibility.Visible;
            InstallPage.Visibility = Visibility.Hidden;
            TxtBlockPath.Text = string.Empty;
            TxtBoxName.Text = string.Empty;
            TxtBoxSso.Text = string.Empty;
            StringManager = Resource.ResourceManager;
            _log.Debug("Exiting Initialize");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _log.Debug("Entering Button_Click");
            _log.Info("EVENT: Browse Button Click");
            IsEnabled = false;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Cab File (*.cab)|*.cab",
                InitialDirectory = @"C:\"
            };
            if ((openFileDialog.ShowDialog() == true))
            {
                _log.Info("File Selected: " + openFileDialog.FileName);
                TxtBlockPath.Text = openFileDialog.FileName;
                StartButton.IsEnabled = true;
                Style style = FindResource("HighlightButtonStyle") as Style;
                StartButton.Style = style;
                Style style1 = FindResource("NormalButtonStyle") as Style;
                BrowseButton.Style = style1;
            }
            IsEnabled = true;
            _log.Debug("Exiting Button_Click");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _log.Debug("Entering Button_Click_1");
            IsEnabled = false;
            if (NamePage.Visibility == Visibility.Visible)
            {
                _log.Info("EVENT: Reset Button Click");
                TxtBoxName.Text = string.Empty;
                TxtBoxSso.Text = string.Empty;
                IsEnabled = true;
                CmbBoxOperation.SelectedIndex = 0;
            }
            else
            {
                _log.Info("EVENT: Export Button Click");
                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "Pdf File (*.pdf)|*.pdf",
                    FileName = "PatchInstallationReport.pdf"
                };
                if (dialog.ShowDialog() == true)
                {
                    _log.Info("File saved: " + dialog.FileName);
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage8", CultureInfo.InvariantCulture);
                    File.Delete(_htmlFileName);
                    if (Directory.Exists(Path.GetTempPath() + "\\Patches\\"))
                    {
                        Directory.Delete(Path.GetTempPath() + "\\Patches\\", true);
                    }
                    if (File.Exists(dialog.FileName))
                    {
                        _log.Debug("PDF File Exists.");
                        File.SetAttributes(dialog.FileName, FileAttributes.Normal);
                        File.Delete(dialog.FileName);
                    }
                    File.Move(_pdfFileName, dialog.FileName);
                    File.Delete(_pdfFileName);
                    Close();
                    _log.Info("Application Closed");
                }
            }
            _log.Debug("Exiting Button_Click_1");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _log.Debug("Entering Button_Click_2");
            if (NamePage.Visibility == Visibility.Visible)
            {
                _log.Info("EVENT: Next Button Click");
                if (string.IsNullOrEmpty(TxtBoxName.Text) || string.IsNullOrEmpty(TxtBoxSso.Text))
                {
                    _log.Warn("WARN: Name or SSO Empty.");
                    MessageBox.Show(StringManager.GetString("NameWarning", CultureInfo.InvariantCulture), StringManager.GetString("NameWarningCaption", CultureInfo.InvariantCulture), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _log.Debug("Moved to Install Page");
                    _log.Info("INPUT: Name: " + TxtBoxName.Text);
                    _log.Info("INPUT: SSO: " + TxtBoxSso.Text);
                    _log.Info("INPUT: Operation : " + CmbBoxOperation.SelectedValue);
                    BrowseButton.IsEnabled = true;
                    StartButton.IsEnabled = false;
                    ExportButton.IsEnabled = false;
                    NamePage.Visibility = Visibility.Hidden;
                    InstallPage.Visibility = Visibility.Visible;
                    ProgressBar.Value = 1.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                _log.Info("EVENT: Start Button Click");
                if (!string.IsNullOrEmpty(TxtBlockPath.Text))
                {
                    BrowseButton.IsEnabled = false;
                    StartButton.IsEnabled = false;
                    ProgressBar.Value = 5.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage1", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    _oldOsEnvironmentText = GetCurrentOSInfo();
                    ProgressBar.Value = 10.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage3", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    GenerateUpdateList();
                    ProgressBar.Value = 15.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage2", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
#if !NO_INSTALL

                    InstallPatches();
#endif
                    ProgressBar.Value = 90.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage4", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    ProgressBar.Value = 95.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage5", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    Tuple<string, string> file = GenerateFinalReport();
                    _htmlFileName = file.Item1;
                    _pdfFileName = file.Item2;
                    ProgressBar.Value = 100.0;
                    TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage6", CultureInfo.InvariantCulture);
                    _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    ExportButton.IsEnabled = true;
                    Style style = FindResource("HighlightButtonStyle") as Style;
                    ExportButton.Style = style;
                    Style style1 = FindResource("NormalButtonStyle") as Style;
                    StartButton.Style = style1;
                }
                else
                {
                    _log.Warn("WARN: Cab file not selected.");
                    MessageBox.Show(StringManager.GetString("SelectCabFile", CultureInfo.InvariantCulture), StringManager.GetString("SelectCabFileCaption", CultureInfo.InvariantCulture), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            _log.Debug("Exiting Button_Click_2");
        }

        private string GetCurrentOSInfo()
        {
            _log.Debug("Entered GetCurrentOSInfo");
            string osName = "NA", osEdition = "NA", osCurrentVersion = "NA", osCurrentBuild = "NA", osInternalVersion = "NA", osInternalPatchVersion = "NA";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinDetailsRegKey))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("ProductName");
                        if (o != null)
                        {
                            osName = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osName = "NA";
            }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinDetailsRegKey))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("EditionID");
                        if (o != null)
                        {
                            osEdition = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osEdition = "NA";
            }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinDetailsRegKey))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("CurrentVersion");
                        if (o != null)
                        {
                            osCurrentVersion = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osCurrentVersion = "NA";
            }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinDetailsRegKey))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("CurrentBuild");
                        if (o != null)
                        {
                            osCurrentBuild = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osCurrentBuild = "NA";
            }
            var architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            string keyToCheck = RegKey;
            if (osName != null && osName.Contains("10"))
                keyToCheck = RegWin10Key;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyToCheck))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("WindowsImageVersion");
                        if (o != null)
                        {
                            osInternalVersion = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osInternalVersion = "NA";
            }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyToCheck))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("WindowsPatchVersion");
                        if (o != null)
                        {
                            osInternalPatchVersion = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osInternalPatchVersion = "NA";
            }
            _log.Debug("Exited GetCurrentOSInfo");
            return string.Format(CultureInfo.InvariantCulture, OsEnvironmentText, osName, osEdition, osCurrentVersion, osCurrentBuild, architecture, osInternalVersion, osInternalPatchVersion);
        }

        private void InstallPatches()
        {
            _log.Debug("Entered InstallPatches");
            TxtBlockProgress.Text = StringManager.GetString("ProgressBarMessage7", CultureInfo.InvariantCulture);
            string[] dirs = Directory.GetDirectories(Path.GetTempPath() + "Patches");
            int exeFiles = Directory.GetFiles(Path.GetTempPath() + "Patches", "*.exe", SearchOption.AllDirectories).Length;
            int msuFiles = Directory.GetFiles(Path.GetTempPath() + "Patches", "*.msu", SearchOption.AllDirectories).Length;
            _log.Debug("Number of MSU Files: " + msuFiles.ToString(CultureInfo.InvariantCulture));
            _log.Debug("Number of EXE Files: " + exeFiles.ToString(CultureInfo.InvariantCulture));
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "*.exe");
                foreach (string file in files)
                {
                    using (Process process = new Process())
                    {
                        _log.Info("Installing Exe File: " + file);
                        TxtBoxDetails.Text += StringManager.GetString("Installing", CultureInfo.InvariantCulture) + file.Substring(file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1, file.Length - file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) - 1) + Environment.NewLine;
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            FileName = file,
                            Arguments = "/q /norestart"
                        };
                        process.StartInfo = startInfo;
                        process.OutputDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                        process.ErrorDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                        process.Start();
                        process.WaitForExit();
                        TxtBoxDetails.Text += StringManager.GetString("Installed", CultureInfo.InvariantCulture) + file.Substring(file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1, file.Length - file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) - 1) + Environment.NewLine;
                        _log.Info("Exe File Installed.");
                        TxtBoxDetails.ScrollToEnd();
                        ProgressBar.Value += 75.0 / (exeFiles + msuFiles);
                        _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }
                string[] msu = Directory.GetFiles(dir, "*.msu");
                foreach (string file in msu)
                {
                    using (Process process = new Process())
                    {
                        _log.Info("Installing Msu File: " + file);
                        TxtBoxDetails.Text += StringManager.GetString("Installing", CultureInfo.InvariantCulture) + file.Substring(file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1, file.Length - file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) - 1) + Environment.NewLine;
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            FileName = @"C:\Windows\System32\wusa.exe",
                            Arguments = string.Format(CultureInfo.InvariantCulture, @"{0} /quiet /norestart", file)
                        };
                        process.StartInfo = startInfo;
                        process.OutputDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                        process.ErrorDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                        process.Start();
                        process.WaitForExit();
                        TxtBoxDetails.Text += StringManager.GetString("Installed", CultureInfo.InvariantCulture) + file.Substring(file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1, file.Length - file.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) - 1) + Environment.NewLine;
                        _log.Info("MSU File Installed.");
                        TxtBoxDetails.ScrollToEnd();
                        ProgressBar.Value += 75.0 / (exeFiles + msuFiles);
                        _log.Debug("New Progressbar Value: " + ProgressBar.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            _log.Debug("Exited InstallPatches");
        }

        private Tuple<string, string> GenerateFinalReport()
        {
            _log.Debug("Entered GenerateFinalReport");
            string tableEntry = InitialText;
            string serialNumber = "NA";
            string machineName = Environment.MachineName;
            string iPAddress = LocalIpAddress;
            string macAddress = GetMacAddress();
            string date = DateTime.Now.ToString("dddd, dd MMMM yyyy h:mm tt", CultureInfo.InvariantCulture);
            string osName = "NA", osInternalVersion = "NA", osInternalPatchVersion = "NA";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WinDetailsRegKey))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("ProductName");
                        if (o != null)
                        {
                            osName = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osName = "NA";
            }
            string keyToCheck = RegKey;
            if (osName != null && osName.Contains("10"))
                keyToCheck = RegWin10Key;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyToCheck))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("WindowsImageVersion");
                        if (o != null)
                        {
                            osInternalVersion = o as string;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osInternalVersion = "NA";
            }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyToCheck, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("WindowsPatchVersion");
                        if (o != null)
                        {
                            osInternalPatchVersion = o.ToString();
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                osInternalPatchVersion = "NA";
            }
            string environment = string.Format(CultureInfo.InvariantCulture, EnvironmentText, machineName, serialNumber, iPAddress, macAddress, osInternalVersion, osInternalPatchVersion, date, TxtBoxName.Text, TxtBoxSso.Text);
            tableEntry += environment;
            tableEntry += "<h2> Previous OS Information</h2>";
            tableEntry += _oldOsEnvironmentText;
            tableEntry += "<h2> New OS Information</h2>";
            tableEntry += GetCurrentOSInfo();
            var signatoryString = CmbBoxOperation.SelectedIndex == 0 ? string.Format(CultureInfo.InvariantCulture, SignatoriesText, "Customer", "GE Service Professional") : string.Format(CultureInfo.InvariantCulture, SignatoriesText, "GE Maintenance Supervisor", "GE Maintenance Professional");
            tableEntry += signatoryString;
            tableEntry += EndText;
            _log.Debug("Html File Creating.");
            string fileName = Path.GetTempFileName();
            using (StreamWriter file = new StreamWriter(fileName, false))
            {
                file.WriteLine(tableEntry);
            }
            _log.Debug("Html File Created.");
            _log.Debug("Pdf File Creating.");
            var htmlToPdf = new HtmlToPdfConverter
            {
                PageFooterHtml = PdfFooterText,
                Margins = new PageMargins { Bottom = 20, Top = 18 }
            };
            var pdfBytes = htmlToPdf.GeneratePdf(tableEntry);
            string fileName1 = Path.GetTempFileName();
            File.WriteAllBytes(fileName1, pdfBytes);
            _log.Debug("Pdf File Created.");
            Tuple<string, string> files = new Tuple<string, string>(fileName, fileName1);
            _log.Debug("Pdf File Path: " + fileName1);
            _log.Debug("Html File Path: " + fileName);
            _log.Debug("Exited GenerateFinalReport");
            return files;
        }

        private void GenerateUpdateList()
        {
            _log.Debug("Entered GenerateUpdateList");
            using (Process process = new Process())
            {
                _log.Info("Unpacking the Update Files.");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = AppDomain.CurrentDomain.BaseDirectory + "cabarc.exe",
                    Arguments = string.Format(CultureInfo.InvariantCulture, @"-p x {0} {1}", TxtBlockPath.Text,
                        Path.GetTempPath()),
                    CreateNoWindow = true
                };
                process.StartInfo = startInfo;
                process.OutputDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                process.ErrorDataReceived += (s, e) => Console.WriteLine(TxtBoxDetails.Text += e.Data + Environment.NewLine);
                process.Start();
                process.WaitForExit();
                _log.Info("Update Files Unpacked.");
            }
            _log.Info("Calculating eligible Update Files.");

            //string content = File.ReadAllText(Path.GetTempPath() + "\\Patches\\Patches.def");
            //string[] patches= content.Substring(0, content.Length-3).Split(',');
            //foreach (string name in patches)
            //{
            //    if (Directory.Exists(Path.GetTempPath() + "\\Patches\\" + name))
            //    {
            //        _log.Debug("Update already installed: " + name);
            //        Directory.Delete(Path.GetTempPath() + "\\Patches\\" + name, true);
            //        _log.Debug("Patch Exists Deleting: " + name);
            //    }

            //    _log.Debug("Patch does not Exist: " + name);
            //}

            _log.Info("Eligible Update Files Calculated.");
            _log.Debug("Exited GenerateUpdateList");
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            _log.Debug("Entered Expander_Expanded");
            _log.Info("EVENT: Expander Expanded");
            Height = 550;
            _log.Debug("Exited Expander_Expanded");
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            _log.Debug("Entered Expander_Collapsed");
            _log.Info("EVENT: Expander Collapsed");
            Height = 350;
            _log.Debug("Exited Expander_Collapsed");
        }

        private string GetMacAddress()
        {
            _log.Debug("Entered GetMacAddress");
            const int minMacAddressLength = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed && !string.IsNullOrEmpty(tempMac) && tempMac.Length >= minMacAddressLength)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }
            _log.Debug("Mac Address Calculated: " + macAddress);
            _log.Debug("Exited GetMacAddress");
            return macAddress;
        }

        private string LocalIpAddress
        {
            get
            {
                _log.Debug("Entered GetMacAddress");
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _log.Debug("LocalIPAddress Address Calculated: " + ip);
                        _log.Debug("Exited GetMacAddress");
                        return ip.ToString();
                    }
                }
                _log.Debug("No Network Endpoints Found");
                _log.Debug("Exited GetMacAddress");
                return "No Network Endpoints Found";
            }
        }
    }
}

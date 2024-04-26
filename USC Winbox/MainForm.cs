using NLog;

namespace USC_Winbox
{
    public partial class MainForm : Form
    {
        private static Logger _logger;

        private string _binaryFilesPath = Path.Combine(Application.StartupPath, "BinaryFiles");
        private string _ExtractedShadersPath = Path.Combine(Application.StartupPath, "Shaders");

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Welcome!");
            CheckDirectories();
        }

        private async void btnExtract_Click(object sender, EventArgs e)
        {
            //string[] args = [ rtbBundleName.Text, rtbAssetFileName.Text, rtbAssetPathID.Text ];
            btnExtract.Enabled = false;
            btnExtract.Text = "Extracting Shaders...";
            string[] args = [rtbBundleName.Text, rtbAssetFileName.Text, "-1"];
            await Task.Factory.StartNew(() => USCSandbox.ConsoleProgram.ConsoleMain(args));
            await Task.Delay(1000);
            btnExtract.Text = "Extract Shaders";
            btnExtract.Enabled = true;
            _logger.Info("Extraction complete.");
        }

        private void btbnClearLog_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        private void btnBrowseFolders_Click(object sender, EventArgs e)
        {
            _logger.Info("Browsing folders for asset bundle.");
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                rtbBundleName.Text = ofd.FileName;
            }
        }

        private void CheckDirectories()
        {
            CreateDir(_binaryFilesPath);
            CreateDir(_ExtractedShadersPath);
        }

        private void CreateDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                _logger.Info($"Creating folder: {dir}");
            }
        }
    }
}

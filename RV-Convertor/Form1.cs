using RV_Convertor.Controls;
using RV_Convertor.Convertor;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace RV_Convertor
{
    public partial class mainForm : ShadowedForm
    {
        #region - Variable -

        // Admin
        private bool isAdmin = false;
        
        // Path to files
        private string inputFilePath;
        private string outputFilePath;
        private string pathToLog = "Logs\\LogConv.txt";

        // Formats
        private string[] formatImg = { "jpg", "bmp", "jpeg" };
        private string[] formatText = { "txt", "rtf" };

        // Class Objects
        MainConvertor convevertor = new MainConvertor();
        OpenFileDialog openFileDialogSettings;
        #endregion

        /* So that the LogIn form can send us the information */
        public bool IsAdmin { get { return isAdmin; } set { this.isAdmin = value; } }

        /* Form Initialization */
        public mainForm()
        {
            InitializeComponent();

            /* Hint */
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(label5, "You need to log in to make this feature available");

            /* OpenFileDialog params */
            openFileDialogSettings = new OpenFileDialog()
            {
                InitialDirectory = @"C:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                /* We have: txt, rtf, jpg, jpeg, bmp */
                Filter = "Text (*.txt;*.rtf)|*.txt;*.rtf|Image (*.jpg;*.jpeg;*.bmp)|*.jpg;*.jpeg;*.bmp",
                /* Selected filter - jpg, jpeg, bmp */
                FilterIndex = 2,
                /* Save the previous opened dir */
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true,
                Multiselect = false
            };

            openFileDialog = openFileDialogSettings;

        }

        /* Reset all values in the mainForm */
        private void RessetForm()
        {
            labInpFormat.Text = "None";
            labOutFormat.Text = "None";

            labInpSize.Text = "NaN";
            labOutSize.Text = "NaN";

            comboBoxType.Items.Clear();
            comboBoxType.ResetText();
            comboBoxType.Enabled = false;
        }

        /* File selection method */
        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            // Reset parameters
            RessetForm();

            // Select the file
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            // Save path to file
            inputFilePath = openFileDialog.FileName;

            // Read info about the input file
            string typeInpFile = inputFilePath.Split('.')[inputFilePath.Split('.').Length - 1];
            double sizeInpFile = new System.IO.FileInfo(inputFilePath).Length;
            
            labInpFormat.Text = typeInpFile;
            labInpSize.Text = (sizeInpFile / 1024).ToString("0.000");

            // Selection of available methods
            if (formatImg.Contains(typeInpFile))
            {
                comboBoxType.Items.Add("jpg (jpeg)");
                comboBoxType.Items.Add("bmp");

                comboBoxType.Enabled = true;
            }
            else if (formatText.Contains(typeInpFile))
            {
                comboBoxType.Items.Add("txt");
                comboBoxType.Items.Add("rtf");

                comboBoxType.Enabled = true;
            }
            else MessageBox.Show("Error. The file is in the wrong format.");

        }

        /* File conversion method */
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            // We set the path to the input file in MainConvertor.
            convevertor.InputFilePath = inputFilePath;

            DateTime startConv = DateTime.Now;

            // Reading information from ComboBox
            if (comboBoxType.SelectedItem == null || labInpFormat.Text == comboBoxType.SelectedItem.ToString())
            {
                MessageBox.Show("You try convert to same format, or format don't selected");

                if (checkBoxLog.Checked)
                {
                    StreamWriter sw = new StreamWriter(pathToLog, true);
                    sw.WriteLine("File conversion error: Same format, or format don't selected");
                    sw.Close();
                }
                return;
            }

            /* If we have Turn on the log */
            if (checkBoxLog.Checked)
            {
                /* To read all the basic information from the file */
                FileInfo fileInpInfo = new FileInfo(convevertor.InputFilePath);

                /* If there is no folder - create */
                if (!Directory.Exists("Logs"))
                    Directory.CreateDirectory("Logs");

                /* If there is no file - create */
                if (!File.Exists(pathToLog))
                    File.Create(pathToLog).Close(); ;

                /* Create stream from write into the file */
                StreamWriter sw = new StreamWriter(pathToLog, true);

                /* Formated info about input file and Formatted information about the file and when we started formatting */
                sw.WriteLine("\n");
                sw.WriteLine(new String('-', 36));
                sw.WriteLine("Start conevrt time: ".PadLeft(35) + "| " + DateTime.Now.ToString());
                sw.WriteLine("InpFile name: ".PadLeft(35) + "| " + fileInpInfo.Name);
                sw.WriteLine("InpFile size (byte): ".PadLeft(35) + "| " + fileInpInfo.Length);
                sw.WriteLine("InpFile creation time: ".PadLeft(35) + "| " + fileInpInfo.CreationTime);
                sw.WriteLine("InpFile type: ".PadLeft(35) + "| " + labInpFormat.Text);
                sw.WriteLine("InpFile path: ".PadLeft(35) + "| " + convevertor.InputFilePath);

                sw.Close();
            }

            if (comboBoxType.SelectedItem.ToString() == "rtf") convevertor.StartTxtToRtf();
            else if (comboBoxType.SelectedItem.ToString() == "txt") convevertor.StartRtfToTxt();
            else if (comboBoxType.SelectedItem.ToString() == "bmp") convevertor.StartJpgToBmp();
            else convevertor.StartBmpToJpg();

            TimeSpan totalConvTime = DateTime.Now - startConv;

            // Getting the path to the output file
            outputFilePath = convevertor.OutputFilePath;

            // Read info about the output file
            string typeInpFile = outputFilePath.Split('.')[outputFilePath.Split('.').Length - 1];
            double sizeInpFile = new System.IO.FileInfo(outputFilePath).Length;

            labOutFormat.Text = typeInpFile;
            labOutSize.Text = (sizeInpFile / 1024).ToString("0.000");

            /* If we have Turn on the log */
            if (checkBoxLog.Checked)
            {
                /* To read all the basic information from the output file */
                FileInfo fileOutInfo = new FileInfo(convevertor.OutputFilePath);

                /* Create stream from write into the file */
                StreamWriter sw = new StreamWriter(pathToLog, true);

                /* Formated info about output file and Formatted information about the file and time spent on conversion */
                sw.WriteLine("\n\n");
                sw.WriteLine("Time spent on conversion: ".PadLeft(35) + "| " + totalConvTime.ToString());
                sw.WriteLine("OutFile name: ".PadLeft(35) + "| " + fileOutInfo.Name);
                sw.WriteLine("OutFile size (byte): ".PadLeft(35) + "| " + fileOutInfo.Length);
                sw.WriteLine("OutFile creation time: ".PadLeft(35) + "| " + fileOutInfo.CreationTime);
                sw.WriteLine("OutFile type: ".PadLeft(35) + "| " + labOutFormat.Text);
                sw.WriteLine("OutFile path: ".PadLeft(35) + "| " + convevertor.OutputFilePath);
                sw.WriteLine(new String('-', 36));

                sw.Close();
            }
        }

        /* Open the LogIn form */
        private void ButtonLogIn_Click(object sender, EventArgs e)
        {
            LogForm logForm = new LogForm();
            logForm.ShowDialog();

            isAdmin = logForm.IsAdmin;
            CheckIsAdmin();
        }

        /* View information from the authorization window */
        public void CheckIsAdmin ()
        {
            if (isAdmin)
                checkBoxLog.Enabled = true;
        }
    }

}

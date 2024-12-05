using System.ComponentModel;
using System.Globalization;
using System.Ini;
using System.Linq;
using System.Management.WMI;
using System.Security;
using System.Security.Activation;
using System.Text;

namespace System.Windows.Forms
{
	internal class GenLicForm : Form
	{
		private static class Settings
		{
			[Resource("Expiration", -1)]
			public static long Date
			{
				get;
				set;
			}

			[Resource("Application", "MYAPP")]
			public static string AppName
			{
				get;
				set;
			}

			[Resource("Data", "")]
			public static string Data
			{
				get;
				set;
			}

			[Resource("License", "")]
			public static string License
			{
				get;
				set;
			}
		}

		private IContainer components;

		private TextControl textAppName;

		private TextControl textMachineID;

		private DateTimePicker dateExpiration;

		private TextBox textLicense;

		private Button btnGenerate;

		private Label lblAppName;

		private Label lblMachineID;

		private Label lblExpDate;

		private Label lblInfo;

		private TextControl textOptions;

		private Label lblOptions;
        private Label lblEncoding;
        private ComboBox cmbEncoding;
        internal const int BUF_SIZE = 1024;

		[ModuleString("user32.dll", 800u)]
		private static string OKText
		{
			get;
			set;
		}

		[ModuleString("user32.dll", 801u)]
		private static string CancelText
		{
			get;
			set;
		}

		public SecureString Password
		{
			get;
			set;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.dateExpiration = new System.Windows.Forms.DateTimePicker();
            this.textLicense = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblAppName = new System.Windows.Forms.Label();
            this.lblMachineID = new System.Windows.Forms.Label();
            this.lblExpDate = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.textMachineID = new System.Windows.Forms.TextControl();
            this.textAppName = new System.Windows.Forms.TextControl();
            this.textOptions = new System.Windows.Forms.TextControl();
            this.lblOptions = new System.Windows.Forms.Label();
            this.lblEncoding = new System.Windows.Forms.Label();
            this.cmbEncoding = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // dateExpiration
            // 
            this.dateExpiration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateExpiration.CustomFormat = "dd.MM.yy";
            this.dateExpiration.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateExpiration.Location = new System.Drawing.Point(202, 91);
            this.dateExpiration.Name = "dateExpiration";
            this.dateExpiration.Size = new System.Drawing.Size(225, 20);
            this.dateExpiration.TabIndex = 2;
            // 
            // textLicense
            // 
            this.textLicense.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textLicense.Location = new System.Drawing.Point(15, 145);
            this.textLicense.Multiline = true;
            this.textLicense.Name = "textLicense";
            this.textLicense.ReadOnly = true;
            this.textLicense.Size = new System.Drawing.Size(412, 92);
            this.textLicense.TabIndex = 4;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(352, 243);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "Generate!";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lblAppName
            // 
            this.lblAppName.AutoSize = true;
            this.lblAppName.Location = new System.Drawing.Point(12, 15);
            this.lblAppName.Name = "lblAppName";
            this.lblAppName.Size = new System.Drawing.Size(88, 13);
            this.lblAppName.TabIndex = 100;
            this.lblAppName.Text = "Application name";
            // 
            // lblMachineID
            // 
            this.lblMachineID.AutoSize = true;
            this.lblMachineID.Location = new System.Drawing.Point(12, 42);
            this.lblMachineID.Name = "lblMachineID";
            this.lblMachineID.Size = new System.Drawing.Size(78, 13);
            this.lblMachineID.TabIndex = 101;
            this.lblMachineID.Text = "Workstation ID";
            // 
            // lblExpDate
            // 
            this.lblExpDate.AutoSize = true;
            this.lblExpDate.Location = new System.Drawing.Point(12, 97);
            this.lblExpDate.Name = "lblExpDate";
            this.lblExpDate.Size = new System.Drawing.Size(77, 13);
            this.lblExpDate.TabIndex = 102;
            this.lblExpDate.Text = "Expiration date";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 248);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(91, 13);
            this.lblInfo.TabIndex = 103;
            this.lblInfo.Text = "Fill in all the fields ";
            // 
            // textMachineID
            // 
            this.textMachineID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textMachineID.BackColor = System.Drawing.SystemColors.Window;
            this.textMachineID.DisplayName = null;
            this.textMachineID.HelpText = "Введите идентификатор оборудования";
            this.textMachineID.Location = new System.Drawing.Point(202, 39);
            this.textMachineID.MinLength = 0;
            this.textMachineID.Name = "textMachineID";
            this.textMachineID.Required = false;
            this.textMachineID.Size = new System.Drawing.Size(225, 20);
            this.textMachineID.TabIndex = 1;
            // 
            // textAppName
            // 
            this.textAppName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAppName.BackColor = System.Drawing.SystemColors.Window;
            this.textAppName.DisplayName = null;
            this.textAppName.HelpText = "Enter the name of the application (at least 5 characters) ";
            this.textAppName.Location = new System.Drawing.Point(202, 12);
            this.textAppName.MinLength = 5;
            this.textAppName.Name = "textAppName";
            this.textAppName.Required = true;
            this.textAppName.Size = new System.Drawing.Size(225, 20);
            this.textAppName.TabIndex = 0;
            // 
            // textOptions
            // 
            this.textOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textOptions.BackColor = System.Drawing.SystemColors.Window;
            this.textOptions.DisplayName = null;
            this.textOptions.HelpText = "Enter the workstation ID ";
            this.textOptions.Location = new System.Drawing.Point(202, 65);
            this.textOptions.MinLength = 0;
            this.textOptions.Name = "textOptions";
            this.textOptions.Required = false;
            this.textOptions.Size = new System.Drawing.Size(225, 20);
            this.textOptions.TabIndex = 104;
            // 
            // lblOptions
            // 
            this.lblOptions.AutoSize = true;
            this.lblOptions.Location = new System.Drawing.Point(12, 68);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.Size = new System.Drawing.Size(83, 13);
            this.lblOptions.TabIndex = 105;
            this.lblOptions.Text = "Application data";
            // 
            // lblEncoding
            // 
            this.lblEncoding.AutoSize = true;
            this.lblEncoding.Location = new System.Drawing.Point(12, 122);
            this.lblEncoding.Name = "lblEncoding";
            this.lblEncoding.Size = new System.Drawing.Size(52, 13);
            this.lblEncoding.TabIndex = 106;
            this.lblEncoding.Text = "Encoding";
            // 
            // cmbEncoding
            // 
            this.cmbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEncoding.FormattingEnabled = true;
            this.cmbEncoding.Location = new System.Drawing.Point(202, 118);
            this.cmbEncoding.Name = "cmbEncoding";
            this.cmbEncoding.Size = new System.Drawing.Size(225, 21);
            this.cmbEncoding.TabIndex = 107;
            // 
            // GenLicForm
            // 
            this.AcceptButton = this.btnGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 273);
            this.Controls.Add(this.cmbEncoding);
            this.Controls.Add(this.lblEncoding);
            this.Controls.Add(this.lblOptions);
            this.Controls.Add(this.textOptions);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblExpDate);
            this.Controls.Add(this.lblMachineID);
            this.Controls.Add(this.lblAppName);
            this.Controls.Add(this.textLicense);
            this.Controls.Add(this.dateExpiration);
            this.Controls.Add(this.textMachineID);
            this.Controls.Add(this.textAppName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenLicForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Activation Key Generator 5.0";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private static string GetHardwareID()
		{
			NetworkAdapterConfigurationInfo networkAdapterConfigurationInfo = NetworkAdapterConfigurationInfo.GetNetworkAdapterConfigurationInfo().FirstOrDefault((NetworkAdapterConfigurationInfo n) => !string.IsNullOrEmpty(n.MACAddress));
			ProcessorInfo processorInfo = ProcessorInfo.GetProcessorInfo()[0];
			string mac = (networkAdapterConfigurationInfo?.MACAddress ?? "00:00:00:00:00:00").Replace(":", "");
			return processorInfo.ProcessorId + mac;
		}

		public GenLicForm()
		{
			InitializeComponent();
			textMachineID.Text = GetHardwareID();
			textOptions.KeyPress += HexhInput_KeyPress;
			textMachineID.KeyPress += HexhInput_KeyPress;
			textMachineID.TextChanged += TextMachineID_TextChanged;
			textMachineID.MinLength = 1;
			textAppName.Text = Settings.AppName;
			textOptions.Text = Settings.Data;
			textLicense.Text = Settings.License;
			textAppName.MinLength = 5;
			textAppName.MaxLength = 15;
			textAppName.TextChanged += TextAppName_TextChanged;
			textAppName.KeyPress += TextAppName_KeyPress;
			dateExpiration.TextChanged += DateExpiration_TextChanged;
			dateExpiration.Value = ((Settings.Date > 0) ? DateTime.FromBinary(Settings.Date) : DateTime.Today.AddMonths(1));
            cmbEncoding.DataSource = Enum.GetValues(typeof(PrintableEncoding));
        }

		public GenLicForm(SecureString password)
			: this()
		{
			Password = password;
		}

		private void TextAppName_KeyPress(object sender, KeyPressEventArgs e)
		{
			char c = e.KeyChar;
			c = char.ToUpper(c, CultureInfo.InvariantCulture);
			bool flag = (c >= 'A' && c <= 'Z') || char.IsControl(c);
			if (!flag)
			{
				e.Handled = !flag;
			}
			else
			{
				e.KeyChar = c;
			}
		}

		private void DateExpiration_TextChanged(object sender, EventArgs e)
		{
			lblInfo.Text = dateExpiration.Text;
		}

		private void TextAppName_TextChanged(object sender, EventArgs e)
		{
			lblInfo.Text = textAppName;
		}

		private static byte[] GetHexVal(string text)
        {
            return ActivationKeyTextParser.HexadeciamlEncoding.GetBytes(text);
		}

		private static int GetHexVal(char c)
		{
			return c - ((c < ':') ? 48 : ((c < 'a') ? 55 : 87));
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			if (textAppName.Complete && textMachineID.Complete)
			{
				string options = textOptions.Text;
				string appName = textAppName.Text;
				string machineID = textMachineID.Text;
				ActivationKey activationKey = ActivationKey
                    .CreateEncryptor(dateExpiration.Value.Date, Password, appName, GetHexVal(machineID))
                    .Generate(GetHexVal(options));
                PrintableEncoding encoding = (PrintableEncoding) Enum.Parse(typeof(PrintableEncoding), cmbEncoding.Text);
                string licenseText = activationKey.ToString(encoding);
				textLicense.Text = licenseText;
				Clipboard.SetDataObject(licenseText);
				Settings.Data = textOptions;
				Settings.AppName = textAppName;
				Settings.License = activationKey.ToString();
				Settings.Date = dateExpiration.Value.ToBinary();
				lblInfo.Text = "Copied to clipboard ";
			}
		}

		private void HexhInput_KeyPress(object sender, KeyPressEventArgs e)
		{
			_ = textMachineID;
			char c = e.KeyChar;
			c = char.ToUpperInvariant(c);
			bool flag = (c > '@' && c < 'G') || (c >= '0' && c <= '9') || char.IsControl(c);
			if (!flag)
			{
				e.KeyChar = '0';
			}
			else
			{
				e.KeyChar = c;
			}
			e.Handled = !flag;
		}

		private void TextMachineID_TextChanged(object sender, EventArgs e)
		{
			lblInfo.Text = textMachineID;
		}
	}
}

using System.ComponentModel;
using System.Ini;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Windows.Forms
{
	public class PasswordForm : DialogBox
	{
		private const char PASSWORD_CHAR = '•';

		private readonly SecureString _secureString = new SecureString();

		private IContainer components;

		protected Label lblPass;

		protected TextBox txtPassword;

		[ModuleString("user32.dll", 800u)]
		private static string OKText {get; set; } = "&OK";

        [ModuleString("user32.dll", 801u)]
		private static string CancelText {get; set; } = "&Cancel";

        public SecureString SecurePassword => _secureString;

		public string Password => ToString(_secureString);

		[DefaultValue("Please enter password:")]
		[Localizable(true)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string LabelText
		{
			get
			{
				return lblPass.Text;
			}
			set
			{
				lblPass.Text = value;
			}
		}

		private void InitializeComponent()
		{
			lblPass = new System.Windows.Forms.Label();
			txtPassword = new System.Windows.Forms.TextBox();
			SuspendLayout();
			btnOk.Location = new System.Drawing.Point(171, 61);
			btnOk.Click += new System.EventHandler(btnOk_Click);
			btnCancel.Location = new System.Drawing.Point(252, 61);
			btnCancel.Click += new System.EventHandler(btnCancel_Click);
			lblPass.AutoSize = true;
			lblPass.Location = new System.Drawing.Point(13, 13);
			lblPass.Name = "lblPass";
			lblPass.Size = new System.Drawing.Size(91, 13);
			lblPass.TabIndex = 0;
			lblPass.Text = "Please enter password:";
			txtPassword.Location = new System.Drawing.Point(13, 30);
			txtPassword.MaxLength = 16;
			txtPassword.Name = "txtPassword";
			txtPassword.PasswordChar = '•';
			txtPassword.Size = new System.Drawing.Size(314, 20);
			txtPassword.TabIndex = 1;
			txtPassword.KeyPress += new System.Windows.Forms.KeyPressEventHandler(InputBox_KeyPress);
			txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(InputBox_KeyDown);
			txtPassword.ReadOnly = true;
			txtPassword.AcceptsReturn = true;
			base.AcceptButton = btnOk;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = btnCancel;
			base.ClientSize = new System.Drawing.Size(339, 96);
			base.Controls.Add(txtPassword);
			base.Controls.Add(lblPass);
			base.Controls.Add(btnCancel);
			base.Controls.Add(btnOk);
			base.ControlBox = true;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "PasswordForm";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			base.TopMost = true;
			ResumeLayout(false);
			PerformLayout();
			base.Load += new System.EventHandler(OnLoad);
			base.MouseLeave += new System.EventHandler(OnMouseLeave);
			base.MouseEnter += new System.EventHandler(OnMouseEnter);
			btnOk.MouseEnter += new System.EventHandler(OnMouseEnter);
			btnCancel.MouseEnter += new System.EventHandler(OnMouseEnter);
			txtPassword.MouseEnter += new System.EventHandler(OnMouseEnter);
		}

		private void OnLoad(object sender, EventArgs e)
		{
			OnMouseLeave(this, EventArgs.Empty);
		}

		public PasswordForm()
		{
			InitializeComponent();
			lblPass.Text = "Enter password:";
			txtPassword.Select();
		}

		private static string ToString(SecureString secureString)
		{
			IntPtr stringPointer = Marshal.SecureStringToBSTR(secureString);
			try
			{
				return Marshal.PtrToStringBSTR(stringPointer);
			}
			finally
			{
				Marshal.ZeroFreeBSTR(stringPointer);
			}
		}

		private unsafe static byte[] ToByteArray(SecureString secureString, Encoding encoding = null)
		{
			if (secureString == null)
			{
				throw new ArgumentNullException("secureString");
			}
			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}
			int maxLength = encoding.GetMaxByteCount(secureString.Length);
			IntPtr bytes = Marshal.AllocHGlobal(maxLength);
			IntPtr str = Marshal.SecureStringToBSTR(secureString);
			try
			{
				char* chars = (char*)str.ToPointer();
				byte* bptr = (byte*)bytes.ToPointer();
				int len = encoding.GetBytes(chars, secureString.Length, bptr, maxLength);
				byte[] _bytes = new byte[len];
				for (int i = 0; i < len; i++)
				{
					_bytes[i] = *bptr;
					bptr++;
				}
				return _bytes;
			}
			finally
			{
				Marshal.FreeHGlobal(bytes);
				Marshal.ZeroFreeBSTR(str);
			}
		}

		private void OnMouseLeave(object sender, EventArgs e)
		{
			base.Opacity = 0.99;
		}

		private void OnMouseEnter(object sender, EventArgs e)
		{
			base.Opacity = 1.0;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			_secureString.Clear();
			txtPassword.Clear();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InputBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\b')
			{
				ProcessBackspace();
			}
			else
			{
				ProcessNewCharacter(e.KeyChar);
			}
			e.Handled = true;
		}

		private void ProcessNewCharacter(char character)
		{
			if (txtPassword.SelectionLength > 0)
			{
				RemoveSelectedCharacters();
			}
			_secureString.InsertAt(txtPassword.SelectionStart, character);
			ResetDisplayCharacters(txtPassword.SelectionStart + 1);
		}

		private void RemoveSelectedCharacters()
		{
			for (int i = 0; i < txtPassword.SelectionLength; i++)
			{
				_secureString.RemoveAt(txtPassword.SelectionStart);
			}
		}

		private void ResetDisplayCharacters(int caretPosition)
		{
			txtPassword.Text = new string('•', _secureString.Length);
			txtPassword.SelectionStart = caretPosition;
		}

		private void ProcessBackspace()
		{
			if (txtPassword.SelectionLength > 0)
			{
				RemoveSelectedCharacters();
				ResetDisplayCharacters(txtPassword.SelectionStart);
			}
			else if (txtPassword.SelectionStart > 0)
			{
				_secureString.RemoveAt(txtPassword.SelectionStart - 1);
				ResetDisplayCharacters(txtPassword.SelectionStart - 1);
			}
		}

		private void InputBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
			{
				ProcessDelete();
				e.Handled = true;
			}
			else if (IsIgnorableKey(e.KeyCode))
			{
				e.Handled = true;
			}
		}

		private static bool IsIgnorableKey(Keys key)
		{
			if (key != Keys.Escape)
			{
				return key == Keys.Return;
			}
			return true;
		}

		private void ProcessDelete()
		{
			if (txtPassword.SelectionLength > 0)
			{
				RemoveSelectedCharacters();
			}
			else if (txtPassword.SelectionStart < txtPassword.Text.Length)
			{
				_secureString.RemoveAt(txtPassword.SelectionStart);
			}
			ResetDisplayCharacters(txtPassword.SelectionStart);
		}
	}
}

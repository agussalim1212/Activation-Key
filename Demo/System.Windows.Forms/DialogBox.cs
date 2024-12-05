using System.ComponentModel;
using System.Ini;

namespace System.Windows.Forms
{
	public class DialogBox : Form
	{
		private IContainer components;

		protected Button btnCancel;

		protected Button btnOk;

		[ModuleString("user32.dll", 800u)]
		private static string OKText { get; set; } = "&OK";


		[ModuleString("user32.dll", 801u)]
		private static string CancelText { get; set; } = "&Cancel";


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
			btnCancel = new System.Windows.Forms.Button();
			btnOk = new System.Windows.Forms.Button();
			SuspendLayout();
			btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			btnCancel.Location = new System.Drawing.Point(376, 142);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new System.Drawing.Size(75, 23);
			btnCancel.TabIndex = 102;
			btnCancel.Text = "&Cancel";
			btnCancel.UseVisualStyleBackColor = true;
			btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			btnOk.Location = new System.Drawing.Point(295, 142);
			btnOk.Name = "btnOk";
			btnOk.Size = new System.Drawing.Size(75, 23);
			btnOk.TabIndex = 101;
			btnOk.Text = "&OK";
			btnOk.UseVisualStyleBackColor = true;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(463, 177);
			base.Controls.Add(btnCancel);
			base.Controls.Add(btnOk);
			base.Name = "DialogBox";
			Text = "";
			ResumeLayout(false);
		}

		public DialogBox()
		{
			InitializeComponent();
			btnOk.Text = OKText;
			btnOk.Click += Ok_Click;
			btnCancel.Text = CancelText;
			btnCancel.Click += Cancel_Click;
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}

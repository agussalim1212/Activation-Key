using System;
using System.ComponentModel;
using System.IO;
using System.Resourses;
using System.Windows.Forms;

namespace GenLic5
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			string dbPath = Path.GetFileNameWithoutExtension(PathsInfo.AppFileName) + ".db";
			using (ResourceFile resourceFile = new ResourceFile(dbPath))
			{
				resourceFile.ReadSettings();
			}
			using (ModuleMessageFormatter user32 = new ModuleMessageFormatter("user32.dll"))
			{
				user32.ReadSettings();
			}
			Form mainForm;
			using (PasswordForm pass = new PasswordForm())
			{
				if (pass.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				mainForm = new GenLicForm(pass.SecurePassword);
			}
			Application.Run(mainForm);
			using (ResourceFile dbFile = new ResourceFile(dbPath))
			{
				dbFile.WriteSettings();
			}
		}
	}
}

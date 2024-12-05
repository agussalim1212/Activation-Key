using System.Collections.ObjectModel;
using System.Drawing;

namespace System.Windows.Forms
{
	internal class TextControl : TextBox, IUserControl
	{
		private static readonly ToolTip tool = new ToolTip();

		public sealed override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		public string DisplayName {get; set;}
		public string HelpText {get; set;}
		public bool Required {get; set;}
		public int MinLength {get; set;}
		public bool Complete
		{
			get
			{
				if (!Required)
				{
					return true;
				}
				if (MinLength <= 0)
				{
					return Text.Length > 0;
				}
				return Text.Length >= MinLength;
			}
		}

		public TextControl()
		{
			base.TextChanged += _text_TextChanged;
			base.LostFocus += TextControl_LostFocus;
		}

		public TextControl(bool required)
			: this()
		{
			Required = required;
		}

		public TextControl(string text)
			: this(false)
		{
			Text = text;
		}

		public TextControl(string text, bool required)
			: this(required)
		{
			Text = text;
		}

		public static implicit operator string(TextControl value)
		{
			return value.Text;
		}

		internal void _text_TextChanged(object sender, EventArgs e)
		{
			BackColor = SystemColors.Window;
		}

		private void TextControl_LostFocus(object sender, EventArgs e)
		{
			Light();
		}

		public void Parse(string value)
		{
			Text = value;
		}

		public bool Allert(ref Collection<IUserControl> controlNames)
		{
			if (controlNames == null)
			{
				controlNames = new Collection<IUserControl>();
			}
			if (Complete)
			{
				return true;
			}
			controlNames.Add(this);
			return false;
		}

		public void Light()
		{
			BackColor = (Complete ? SystemColors.Window : Color.LightCoral);
			if (!Complete && !base.DesignMode)
			{
				tool.Show(HelpText, this, 3000);
			}
		}

		public void Reset()
		{
			Clear();
		}

		/*void IUserControl.TextChanged(EventHandler value)
		{
			base.TextChanged += value;
		}

		void IUserControl.TextChanged(EventHandler value)
		{
			base.TextChanged -= value;
		}*/
	}
}

using System.Collections.ObjectModel;

namespace System.Windows.Forms
{
	internal interface IUserControl
	{
		string DisplayName {get; set;}
		bool Required {get; set;}
		bool Complete { get; }

		string Text {get; set;}
		event EventHandler TextChanged;

		bool Allert(ref Collection<IUserControl> controlNames);

		void Light();

		void Reset();
	}
}

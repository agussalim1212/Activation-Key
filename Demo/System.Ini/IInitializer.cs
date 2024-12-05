using System.Reflection;

namespace System.Ini
{
	public interface IInitializer
	{
		void ReadSettings(Assembly assembly = null);

		void WriteSettings(Assembly assembly = null);
	}
}

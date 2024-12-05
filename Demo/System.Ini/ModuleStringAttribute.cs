namespace System.Ini
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class ModuleStringAttribute : Attribute
	{
		private static readonly ModuleStringAttribute _defaultAttribute = new ModuleStringAttribute("user32.dll", 0u);

		public string Name {get; set;}
		public uint MessageId {get; set;}
		public string[] Arguments {get; set;}

		public override bool Match(object obj)
		{
			ModuleStringAttribute iniEntryAttribute;
			if ((iniEntryAttribute = obj as ModuleStringAttribute) != null && iniEntryAttribute.MessageId == MessageId)
			{
				return iniEntryAttribute.Name == Name;
			}
			return false;
		}

		public override bool IsDefaultAttribute()
		{
			return Match(_defaultAttribute);
		}

		public ModuleStringAttribute(string name, uint msgCode, params string[] args)
		{
			Name = name;
			MessageId = msgCode;
			Arguments = args;
		}
	}
}

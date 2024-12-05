namespace System.Ini
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class AssemblyMessageAttribute : Attribute
	{
		private static readonly AssemblyMessageAttribute _defaultAttribute = new AssemblyMessageAttribute("UnknownError");

		public string MessageId {get; set;}
		public string[] Arguments {get; set;}

		public override bool Match(object obj)
		{
			AssemblyMessageAttribute iniEntryAttribute;
			if ((iniEntryAttribute = obj as AssemblyMessageAttribute) != null)
			{
				return iniEntryAttribute.MessageId == MessageId;
			}
			return false;
		}

		public override bool IsDefaultAttribute()
		{
			return Match(_defaultAttribute);
		}

		public AssemblyMessageAttribute(string msgCode, params string[] args)
		{
			MessageId = msgCode;
			Arguments = args;
		}
	}
}

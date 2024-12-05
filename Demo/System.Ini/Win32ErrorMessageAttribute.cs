namespace System.Ini
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class Win32ErrorMessageAttribute : Attribute
	{
		private static readonly Win32ErrorMessageAttribute _defaultAttribute = new Win32ErrorMessageAttribute(0u);

		public uint MessageId {get; set;}
		public string[] Arguments {get; set;}

		public override bool Match(object obj)
		{
			Win32ErrorMessageAttribute iniEntryAttribute;
			if ((iniEntryAttribute = obj as Win32ErrorMessageAttribute) != null)
			{
				return iniEntryAttribute.MessageId == MessageId;
			}
			return false;
		}

		public override bool IsDefaultAttribute()
		{
			return Match(_defaultAttribute);
		}

		public Win32ErrorMessageAttribute(uint msgCode, params string[] args)
		{
			MessageId = msgCode;
			Arguments = args;
		}
	}
}

namespace System.Ini
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class ResourceAttribute : Attribute
	{
		private static readonly ResourceAttribute _defaultAttribute = new ResourceAttribute();

		public string Name
		{
			get;
			set;
		}

		public object DefaultValue
		{
			get;
			set;
		}

		public override bool Match(object obj)
		{
			ResourceAttribute iniEntryAttribute;
			if ((iniEntryAttribute = obj as ResourceAttribute) != null)
			{
				return iniEntryAttribute.Name == Name;
			}
			return false;
		}

		public override bool IsDefaultAttribute()
		{
			return Match(_defaultAttribute);
		}

		public ResourceAttribute(string name = null, object defaultValue = null)
		{
			Name = name;
			DefaultValue = defaultValue;
		}
	}
}

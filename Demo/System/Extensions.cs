using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
	public static class Extensions
	{
		public static string GetMessages(this Exception e)
		{
			string result = $"{e.Message} in {e.TargetSite}\n";
			if (e.InnerException != null)
			{
				result += e.InnerException.GetMessages();
			}
			return result;
		}

		public static IEnumerable<KeyValuePair<string, object>> GetProperties(this object obj)
		{
			return from p in obj.GetType().GetProperties()
				let key = p.Name
				let result = p.GetValue(obj, null)
				select new KeyValuePair<string, object>(key, result);
		}

		public static IEnumerable<KeyValuePair<string, object>> GetProperties<TAttr>(this object obj, Func<TAttr, string> selector = null) where TAttr : Attribute
		{
			Type type = obj.GetType();
			if (!typeof(TAttr).IsSubclassOf(typeof(Attribute)))
			{
				throw new InvalidOperationException();
			}
			return from p in type.GetProperties()
				from a in p.GetCustomAttributes(false)
				where a is TAttr
				let key = selector?.Invoke((TAttr)a) ?? p.Name
				let result = p.GetValue(obj, null)
				select new KeyValuePair<string, object>(key, result);
		}

		public static IEnumerable<KeyValuePair<string, TResult>> GetProperties<TAttr, TResult>(this object obj, Func<TAttr, string> selector = null) where TAttr : Attribute
		{
			return from p in obj.GetType().GetProperties()
				from a in p.GetCustomAttributes(false)
				let result = p.GetValue(obj, null)
				where result is TResult && a is TAttr
				let key = selector?.Invoke((TAttr)a) ?? p.Name
				select new KeyValuePair<string, TResult>(key, (TResult)result);
		}

		public static IEnumerable<KeyValuePair<Tkey, TResult>> GetProperties<TAttr, Tkey, TResult>(this object obj, Func<TAttr, Tkey> selector = null) where TAttr : Attribute
		{
			return from p in obj.GetType().GetProperties()
				from a in p.GetCustomAttributes(false)
				let result = p.GetValue(obj, null)
				where result is TResult && a is TAttr
				let key = selector((TAttr)a)
				select new KeyValuePair<Tkey, TResult>(key, (TResult)result);
		}

		public static IEnumerable<KeyValuePair<TAttr, object>> GetProperties<TAttr>(this object obj) where TAttr : Attribute
		{
			return from p in obj.GetType().GetProperties()
				from a in p.GetCustomAttributes(false)
				where a is TAttr
				let result = p.GetValue(obj, null)
				select new KeyValuePair<TAttr, object>((TAttr)a, result);
		}

		public static IEnumerable<KeyValuePair<TAttr, TResult>> GetProperties<TAttr, TResult>(this object obj) where TAttr : Attribute
		{
			return from p in obj.GetType().GetProperties()
				from a in p.GetCustomAttributes(false)
				let result = p.GetValue(obj, null)
				where result is TResult && a is TAttr
				select new KeyValuePair<TAttr, TResult>((TAttr)a, (TResult)result);
		}

		public static IEnumerable<KeyValuePair<Attribute, object>> GetProperties(this object obj, params Type[] types)
		{
			return from p in obj.GetType().GetProperties()
				from a in p.GetCustomAttributes(false)
				let result = p.GetValue(obj, null)
				where types.Any((Type t) => t.IsInstanceOfType(a))
				select new KeyValuePair<Attribute, object>((Attribute)a, result);
		}

		internal static void SetPropertyValue<T>(this PropertyInfo property, T value)
		{
			if (value != null)
			{
				property?.SetValue(null, value, null);
			}
		}

		internal static void SetPropertyValue(this PropertyInfo property, object value)
		{
			if (value != null)
			{
				property?.SetValue(null, value, null);
			}
		}

		internal static T GetPropertyValue<T>(this PropertyInfo property)
		{
			if (!(property == null))
			{
				return (T)property.GetValue(null, null);
			}
			return default(T);
		}

		internal static object GetPropertyValue(this PropertyInfo property)
		{
			if (!(property == null))
			{
				return property.GetValue(null, null);
			}
			return null;
		}

		internal static TypeConverter GetPropertyConverter(this PropertyInfo property, bool createDefault = false)
		{
			Type propertyType = property.GetType();
			TypeConverterAttribute propertyConverter;
			if ((propertyConverter = property.GetCustomAttributes(typeof(TypeConverterAttribute), false).FirstOrDefault() as TypeConverterAttribute) != null)
			{
				Type converterType = Type.GetType(propertyConverter.ConverterTypeName);
				TypeConverter converter;
				if (converterType != null && (converter = Activator.CreateInstance(converterType) as TypeConverter) != null)
				{
					return converter;
				}
			}
			TypeConverter typeConverter = TypeDescriptor.GetConverter(propertyType);
			if (!createDefault)
			{
				return null;
			}
			return typeConverter;
		}

		public static bool ImplementsInterface(this Type type, Type interfaceType)
		{
			if (type == null || (!type.IsClass && !type.IsValueType))
			{
				throw new ArgumentException(null, "type");
			}
			if (interfaceType == null || !interfaceType.IsInterface)
			{
				throw new ArgumentException(null, "interfaceType");
			}
			return type.GetInterfaces().Any((Type i) => i == interfaceType);
		}

		public static bool ImplementsInterface<T>(this Type type) where T : class
		{
			return type.ImplementsInterface(typeof(T));
		}
	}
}

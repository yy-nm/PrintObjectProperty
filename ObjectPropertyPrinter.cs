
/*
* ObjectPropertyPrinter.cs: print all property in object via reflect as json format
* ***
* finish feature now:
*	1. support simple type
*	2. support custom class use base type
*	3. use object filter avoid dead loop, object will not be print twice in same subbranch
*	4. support simple generic container class which T is not generic:
*		- Dictionary<K, V>
*		- HashSet<T>
*		- List<T>
*		- LinkedList<T>
*		- Queue<T>
*		- Stack<T>
*		- SortedList<K, V>
*	5. support generic container which T can be generic container
*		- Dictionary<K, Dictionary<K, V>>
*		- Dictionary<K, Dictionary<K, Dictionary<K, V>>>
*		- List<Dictionary<K, V>>
*	6. if class has IEnumerable interface or others inherit from IEnumerable will have a array taged with ArrayValues(may same as others) as extend part, 
*		if you donot want it, comment USE_EXTEND_PART definer
*	7. can print property in struct, but has some limit, only print **direct** info about property of struct.
*	8. **well known bugs**: return new same class/struct instance in property of the class/struct which will cause infinite loop util StringBuiler
*		run out of your memory!
*
* Authors:
* mardyu<michealyxd@hotmail.com>
*
* Copyright 2016 mardyu<michealyxd@hotmail.com>
* Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/

#define USE_EXTEND_PART

using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mard.Tools
{
	public class ObjectPropertyPrinter
	{

		private const string cPrintTemplate = ", \"{0}\":\"{1}\"";
		private const string cPrintTemplateNotQuote = ", \"{0}\":{1}";
		private const string cPrintTemplateWithStructure = ", \"{0}\": ";
		private const string cPrintTemplateWithArray = ", \"{0}\": [";
		private const string cPrintTemplateValue = "\"{0}\"";
		private const string cPrintTemplateValueNotQuote = "{0}";
		private const string cPrintEmpty = "{}";

		private const string cCustomValueNameInClass = "ArrayValues";

		private const int cIteratorCountInfinite = 2;
		private const int cIteratorCountLastOne = 1;
		private const int cIteratorCountNone = 0;

		private static readonly HashSet<Type> rPrimaryType = new HashSet<Type>()
		{
			typeof(string),
			typeof(double),
			typeof(float),
			typeof(Int16),
			typeof(Int32),
			typeof(Int64),
			typeof(UInt16),
			typeof(UInt32),
			typeof(UInt64),
			typeof(bool),
		};

		private static readonly HashSet<Type> rPrimaryTypeWithoutQuote = new HashSet<Type>()
		{
			typeof(double),
			typeof(float),
			typeof(Int16),
			typeof(Int32),
			typeof(Int64),
			typeof(UInt16),
			typeof(UInt32),
			typeof(UInt64),
		};

		private static readonly Type rIteratorType = typeof(IEnumerable);

		// TODO if use .net higher version or non unity, uncomment below line
		private static readonly Regex rReadClassNameFromGeneric = new Regex(@"(?<mainclassname>[^`]*)`\d+\[(?<subclassname>.+)\]"
//		, RegexOptions.Compiled
		);

		private static bool IsBaseType(Type _t)
		{
			return rPrimaryType.Contains (_t) || _t.IsEnum;
		}

		private static bool IsNeedQuote(Type _t)
		{
			return !rPrimaryTypeWithoutQuote.Contains(_t);
		}

		private static void FormatProperty(StringBuilder _sb, PropertyInfo _pi, System.Object _o, HashSet<object> _alreadyUsed, int iterlevel = cIteratorCountInfinite)
		{
			if (IsBaseType(_pi.PropertyType))
			{
				_sb.AppendFormat(IsNeedQuote(_pi.PropertyType) ? cPrintTemplate : cPrintTemplateNotQuote, _pi.Name, _o);
			}
			else
			{
				_sb.AppendFormat(cPrintTemplateWithStructure, _pi.Name);
				_sb.Append (PrintProperty (_pi.PropertyType, _o, _alreadyUsed, iterlevel));
			}
		}

		private static void FormatProperty(StringBuilder _sb, FieldInfo _fi, System.Object _o, HashSet<object> _alreadyUsed, int iterlevel = cIteratorCountInfinite)
		{
			if (IsBaseType(_fi.FieldType))
			{
				_sb.AppendFormat(IsNeedQuote(_fi.FieldType) ? cPrintTemplate : cPrintTemplateNotQuote, _fi.Name, _o);
			}
			else
			{
				_sb.AppendFormat(cPrintTemplateWithStructure, _fi.Name);
				_sb.Append (PrintProperty (_fi.FieldType, _o, _alreadyUsed, iterlevel));
			}
		}

		private static void FormatType(StringBuilder _sb, Type _t, System.Object _o, HashSet<object> _alreadyUsed, int iterlevel = cIteratorCountInfinite)
		{
			if (IsBaseType(_t))
			{
				_sb.AppendFormat(IsNeedQuote(_t) ? cPrintTemplateValue : cPrintTemplateValueNotQuote, _o);
			}
			else
			{
				_sb.Append(PrintProperty(_t, _o, _alreadyUsed, iterlevel));
			}
		}

		public static string GetRightClassName(Type _t)
		{
			StringBuilder sb = new StringBuilder();
			if (_t == null)
				return sb.ToString();

			if (_t.IsGenericType)
			{
				string tmp = _t.UnderlyingSystemType.ToString();
				if (!string.IsNullOrEmpty(_t.Namespace))
				{
					tmp = tmp.Substring(_t.Namespace.Length + 1);
				}
				if (rReadClassNameFromGeneric.IsMatch(tmp))
				{
					Match m = rReadClassNameFromGeneric.Match(tmp);
					sb.Append(m.Groups["mainclassname"]);
					sb.Append("<");
					Type[] subs = _t.GetGenericArguments();
					sb.Append(GetRightClassName(subs[0]));
					if (subs.Length > 1)
					{
						for (int i = 1; i < subs.Length; i++)
						{
							sb.Append(",");
							sb.Append(GetRightClassName(subs[i]));
						}
					}
					sb.Append(">");
				}
				else
				{
					sb.Append(tmp);
				}
			}
			else
			{
				sb.Append(_t.Name);
			}

			return sb.ToString();
		}

		public static string GetRightClassNameWithNameSpace(Type _t)
		{
			if (_t == null)
				return "";
			if (string.IsNullOrEmpty(_t.Namespace))
			{
				return GetRightClassName(_t);
			}
			else
			{
				return _t.Namespace + "." + GetRightClassName(_t);
			}
		}

		public static bool IsObsolete(Type _t)
		{
			object[] attrs = _t.GetCustomAttributes(true);

			for (int j = 0; j < attrs.Length; j++)
			{
				Type t = attrs[j].GetType();

				if (t == typeof(System.ObsoleteAttribute))
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsObsolete(PropertyInfo _pi)
		{
			object[] attrs = _pi.GetCustomAttributes(true);

			for (int j = 0; j < attrs.Length; j++)
			{
				Type t = attrs[j].GetType();

				if (t == typeof(System.ObsoleteAttribute))
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsObsolete(FieldInfo _fi)
		{
			object[] attrs = _fi.GetCustomAttributes(true);

			for (int j = 0; j < attrs.Length; j++)
			{
				Type t = attrs[j].GetType();

				if (t == typeof(System.ObsoleteAttribute))
				{
					return true;
				}
			}

			return false;
		}

		public static string PrintProperty(Type _t, System.Object _o)
		{
			return PrintProperty(_t, _o, null);
		}

		public static string PrintProperty(Type _t, System.Object _o, HashSet<object> _alreadyUsed, int iterlevel = cIteratorCountInfinite)
		{
			if (null == _o || null == _t || iterlevel == cIteratorCountNone)
				return cPrintEmpty;

			if (IsBaseType(_t)) 
			{
				return string.Format(IsNeedQuote(_t) ? cPrintTemplateValue : cPrintTemplateValueNotQuote, _o);
			}

			if (_alreadyUsed == null)
				_alreadyUsed = new HashSet<object>();

			PropertyInfo[] pis = _t.GetProperties();
			StringBuilder sb = new StringBuilder();
			sb.Append("{");

			if (_alreadyUsed.Contains(_o) && !rPrimaryType.Contains(_t))
			{
				sb.AppendFormat("\"class_name\":\"{0}\"", GetRightClassName(_o.GetType()));
				sb.Append("}");
				return sb.ToString();
			}
			else if (_alreadyUsed.Contains(_o))
			{
				sb.AppendFormat(rPrimaryTypeWithoutQuote.Contains(_t) ? cPrintTemplateValueNotQuote
					: cPrintTemplateValue, _o);
				sb.Append("}");
				return sb.ToString();
			}
			else
			{
				sb.AppendFormat("\"class_name\":\"{0}\"", GetRightClassName(_o.GetType()));
			}
			_alreadyUsed.Add(_o);
#if USE_EXTEND_PART
			List<Type> o_interfaces = new List<Type> (_t.GetInterfaces ());
			if (null != _o && o_interfaces.Contains(rIteratorType))
			{
				sb.AppendFormat(cPrintTemplateWithArray, cCustomValueNameInClass);
				foreach (var it in (IEnumerable)_o)
				{
					FormatType(sb, it.GetType(), it, _alreadyUsed);
					sb.Append(',');
				}
				if (sb[sb.Length - 1] == ',')
					sb.Length -= 1;
				sb.Append("]");
			}
#endif
			if (_t.IsValueType) {
				switch (iterlevel) {
				case cIteratorCountInfinite:
					iterlevel = cIteratorCountLastOne;
					break;
				case cIteratorCountLastOne:
				default:
					iterlevel = cIteratorCountNone;
					break;
				}
			}
			foreach (var pi in pis)
			{
				if (pi.GetIndexParameters().Length > 0 || IsObsolete(pi))
					continue;
				var value = pi.GetValue(_o, null);
				if (pi.PropertyType.IsGenericType)
				{
					List<Type> interfaces = new List<Type>(pi.PropertyType.GetInterfaces());
					if (null != value && interfaces.Contains(rIteratorType))
					{
						sb.AppendFormat(cPrintTemplateWithArray, pi.Name);
						foreach (var it in (IEnumerable)value)
						{
							FormatType(sb, it.GetType(), it, _alreadyUsed, iterlevel);
							sb.Append(',');
						}
						if (sb[sb.Length - 1] == ',')
							sb.Length -= 1;
						sb.Append("]");
					}
					else if (null != value)
					{
						sb.AppendFormat(cPrintTemplateNotQuote, pi.Name, PrintProperty(pi.PropertyType, value, _alreadyUsed, iterlevel));
					}
					else
						sb.AppendFormat(cPrintTemplateNotQuote, pi.Name, "null");
				}
				else
				{
					FormatProperty(sb, pi, value, _alreadyUsed, iterlevel);
				}
			}

			FieldInfo[] fis = _t.GetFields();
			foreach (var fi in fis)
			{
				if (IsObsolete(fi))
					continue;
				var value = fi.GetValue(_o);
				if (fi.FieldType.IsGenericType)
				{
					List<Type> interfaces = new List<Type>(fi.FieldType.GetInterfaces());
					if (null != value && interfaces.Contains(rIteratorType))
					{
						sb.AppendFormat(cPrintTemplateWithArray, fi.Name);
						foreach (var it in (IEnumerable)value)
						{
							FormatType(sb, it.GetType(), it, _alreadyUsed);
							sb.Append(',');
						}
						if (sb[sb.Length - 1] == ',')
							sb.Length -= 1;
						sb.Append("]");
					}
					else if (null != value)
					{
						sb.AppendFormat(cPrintTemplateNotQuote, fi.Name, PrintProperty(fi.FieldType, value, _alreadyUsed));
					}
					else
						sb.AppendFormat(cPrintTemplateNotQuote, fi.Name, "null");
				}
				else
				{
					FormatProperty(sb, fi, value, _alreadyUsed);
				}
			}
			sb.Append("}");
			_alreadyUsed.Remove(_o);
			return sb.ToString();
		}

		public static string PrintProperty<T>(T _t)
		{
			return PrintProperty(typeof(T), _t);
		}
	}
}


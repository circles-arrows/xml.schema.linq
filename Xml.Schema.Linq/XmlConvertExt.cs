using System;

namespace Xml.Schema.Linq
{
	internal class XmlConvertExt
	{
		internal static char[] crt;

		internal readonly static char[] WhitespaceChars;

		static XmlConvertExt()
		{
			XmlConvertExt.crt = new char[] { '\n', '\r', '\t' };
			XmlConvertExt.WhitespaceChars = new char[] { ' ', '\t', '\n', '\r' };
		}

		public XmlConvertExt()
		{
		}

		internal static string[] SplitString(string value)
		{
			return value.Split(XmlConvertExt.WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);
		}

		internal static Uri ToUri(string s)
		{
			Uri uri;
			if ((s == null ? false : s.Length > 0))
			{
				s = XmlConvertExt.TrimString(s);
				if ((s.Length == 0 ? true : s.IndexOf("##", StringComparison.Ordinal) != -1))
				{
					throw new FormatException();
				}
			}
			if (!Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out uri))
			{
				throw new FormatException();
			}
			return uri;
		}

		internal static string TrimString(string value)
		{
			return value.Trim(XmlConvertExt.WhitespaceChars);
		}

		internal static Exception TryToUri(string s, out Uri result)
		{
			Exception formatException;
			result = null;
			if ((s == null ? false : s.Length > 0))
			{
				s = XmlConvertExt.TrimString(s);
				if ((s.Length == 0 ? true : s.IndexOf("##", StringComparison.Ordinal) != -1))
				{
					formatException = new FormatException();
					return formatException;
				}
			}
			if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out result))
			{
				formatException = null;
			}
			else
			{
				formatException = new FormatException();
			}
			return formatException;
		}

		internal static Exception VerifyNormalizedString(string str)
		{
			Exception linqToXsdException;
			if (str.IndexOfAny(XmlConvertExt.crt) == -1)
			{
				linqToXsdException = null;
			}
			else
			{
				linqToXsdException = new LinqToXsdException(string.Concat("Failed to Verify Normalized String: ", str));
			}
			return linqToXsdException;
		}
	}
}
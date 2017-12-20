using System;
using System.Text;

namespace Xml.Schema.Linq
{
	internal static class XmlComplianceUtil
	{
		public static string CDataNormalize(string value)
		{
			string str;
			bool flag;
			bool flag1;
			int len = value.Length;
			if (len > 0)
			{
				int i = 0;
				int startPos = 0;
				StringBuilder norValue = null;
				while (i < len)
				{
					char ch = value[i];
					if (ch >= ' ')
					{
						flag = false;
					}
					else
					{
						flag = (ch == '\t' || ch == '\n' ? true : ch == '\r');
					}
					if (flag)
					{
						if (norValue == null)
						{
							norValue = new StringBuilder(len);
						}
						if (startPos < i)
						{
							norValue.Append(value, startPos, i - startPos);
						}
						norValue.Append(' ');
						if (ch != '\r')
						{
							flag1 = true;
						}
						else
						{
							flag1 = (i + 1 >= len ? true : value[i + 1] != '\n');
						}
						if (flag1)
						{
							i++;
						}
						else
						{
							i += 2;
						}
						startPos = i;
					}
					else
					{
						i++;
					}
				}
				if (norValue != null)
				{
					if (i > startPos)
					{
						norValue.Append(value, startPos, i - startPos);
					}
					str = norValue.ToString();
				}
				else
				{
					str = value;
				}
			}
			else
			{
				str = string.Empty;
			}
			return str;
		}

		internal static bool IsWhiteSpace(char c)
		{
			bool flag;
			char chr = c;
			switch (chr)
			{
				case '\t':
				case '\n':
				case '\r':
				{
					flag = true;
					break;
				}
				case '\v':
				case '\f':
				{
					flag = false;
					break;
				}
				default:
				{
					if (chr == ' ')
					{
						goto case '\r';
					}
					goto case '\f';
				}
			}
			return flag;
		}

		public static string NonCDataNormalize(string value)
		{
			string str;
			int len = value.Length;
			if (len > 0)
			{
				int startPos = 0;
				StringBuilder norValue = null;
				while (XmlComplianceUtil.IsWhiteSpace(value[startPos]))
				{
					startPos++;
					if (startPos == len)
					{
						str = " ";
						return str;
					}
				}
				int i = startPos;
				while (i < len)
				{
					if (XmlComplianceUtil.IsWhiteSpace(value[i]))
					{
						int j = i + 1;
						while (true)
						{
							if ((j >= len ? true : !XmlComplianceUtil.IsWhiteSpace(value[j])))
							{
								break;
							}
							j++;
						}
						if (j == len)
						{
							if (norValue != null)
							{
								norValue.Append(value, startPos, i - startPos);
								str = norValue.ToString();
								return str;
							}
							else
							{
								str = value.Substring(startPos, i - startPos);
								return str;
							}
						}
						else if ((j > i + 1 ? false : value[i] == ' '))
						{
							i++;
						}
						else
						{
							if (norValue == null)
							{
								norValue = new StringBuilder(len);
							}
							norValue.Append(value, startPos, i - startPos);
							norValue.Append(' ');
							startPos = j;
							i = j;
						}
					}
					else
					{
						i++;
					}
				}
				if (norValue == null)
				{
					str = (startPos <= 0 ? value : value.Substring(startPos, len - startPos));
				}
				else
				{
					if (startPos < i)
					{
						norValue.Append(value, startPos, i - startPos);
					}
					str = norValue.ToString();
				}
			}
			else
			{
				str = string.Empty;
			}
			return str;
		}

		public static string StripSpaces(string value)
		{
			int i;
			string str;
			int len = value.Length;
			if (len > 0)
			{
				int startPos = 0;
				StringBuilder norValue = null;
				while (value[startPos] == ' ')
				{
					startPos++;
					if (startPos == len)
					{
						str = " ";
						return str;
					}
				}
				for (i = startPos; i < len; i++)
				{
					if (value[i] == ' ')
					{
						int j = i + 1;
						while (true)
						{
							if ((j >= len ? true : value[j] != ' '))
							{
								break;
							}
							j++;
						}
						if (j == len)
						{
							if (norValue != null)
							{
								norValue.Append(value, startPos, i - startPos);
								str = norValue.ToString();
								return str;
							}
							else
							{
								str = value.Substring(startPos, i - startPos);
								return str;
							}
						}
						else if (j > i + 1)
						{
							if (norValue == null)
							{
								norValue = new StringBuilder(len);
							}
							norValue.Append(value, startPos, i - startPos + 1);
							startPos = j;
							i = j - 1;
						}
					}
				}
				if (norValue != null)
				{
					if (i > startPos)
					{
						norValue.Append(value, startPos, i - startPos);
					}
					str = norValue.ToString();
				}
				else
				{
					str = (startPos == 0 ? value : value.Substring(startPos, len - startPos));
				}
			}
			else
			{
				str = string.Empty;
			}
			return str;
		}

		public static void StripSpaces(char[] value, int index, ref int len)
		{
			if (len > 0)
			{
				int startPos = index;
				int endPos = index + len;
				while (value[startPos] == ' ')
				{
					startPos++;
					if (startPos == endPos)
					{
						len = 1;
						return;
					}
				}
				int offset = startPos - index;
				for (int i = startPos; i < endPos; i++)
				{
					char chr = value[i];
					char ch = chr;
					if (chr == ' ')
					{
						int j = i + 1;
						while (true)
						{
							if ((j >= endPos ? true : value[j] != ' '))
							{
								break;
							}
							j++;
						}
						if (j == endPos)
						{
							offset = offset + (j - i);
							break;
						}
						else if (j > i + 1)
						{
							offset = offset + (j - i - 1);
							i = j - 1;
						}
					}
					value[i - offset] = ch;
				}
				len -= offset;
			}
		}
	}
}
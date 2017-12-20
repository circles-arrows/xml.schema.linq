using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Xml.Schema.Linq
{
	internal class XObjectsNamespaceList
	{
		private XObjectsNamespaceList.ListType type = XObjectsNamespaceList.ListType.Any;

		private Hashtable @set = null;

		private string targetNamespace;

		private string namespaces;

		public ICollection Enumerate
		{
			get
			{
				switch (this.type)
				{
					case XObjectsNamespaceList.ListType.Any:
					case XObjectsNamespaceList.ListType.Other:
					{
						throw new InvalidOperationException();
					}
					case XObjectsNamespaceList.ListType.Set:
					{
						return this.@set.Keys;
					}
					default:
					{
						throw new InvalidOperationException();
					}
				}
			}
		}

		public string Excluded
		{
			get
			{
				return this.targetNamespace;
			}
		}

		internal string Namespaces
		{
			get
			{
				return this.namespaces;
			}
		}

		internal string TargetNamespace
		{
			get
			{
				return this.targetNamespace;
			}
		}

		public XObjectsNamespaceList.ListType Type
		{
			get
			{
				return this.type;
			}
		}

		public XObjectsNamespaceList()
		{
		}

		public XObjectsNamespaceList(string namespaces, string targetNamespace)
		{
			this.targetNamespace = targetNamespace;
			this.namespaces = namespaces;
			if (!(namespaces == "##any" ? false : namespaces.Length != 0))
			{
				this.type = XObjectsNamespaceList.ListType.Any;
			}
			else if (!(namespaces == "##other"))
			{
				this.type = XObjectsNamespaceList.ListType.Set;
				this.@set = new Hashtable();
				string[] strArrays = XmlConvertExt.SplitString(namespaces);
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string ns = strArrays[i];
					if (ns == "##local")
					{
						this.@set[string.Empty] = string.Empty;
					}
					else if (!(ns == "##targetNamespace"))
					{
						XmlConvertExt.ToUri(ns);
						this.@set[ns] = ns;
					}
					else
					{
						this.@set[targetNamespace] = targetNamespace;
					}
				}
			}
			else
			{
				this.type = XObjectsNamespaceList.ListType.Other;
			}
		}

		public virtual bool Allows(string ns)
		{
			bool item;
			switch (this.type)
			{
				case XObjectsNamespaceList.ListType.Any:
				{
					item = true;
					break;
				}
				case XObjectsNamespaceList.ListType.Other:
				{
					item = (ns == this.targetNamespace ? false : ns.Length != 0);
					break;
				}
				case XObjectsNamespaceList.ListType.Set:
				{
					item = this.@set[ns] != null;
					break;
				}
				default:
				{
					Debug.Assert(false);
					item = false;
					break;
				}
			}
			return item;
		}

		public bool Allows(XmlQualifiedName qname)
		{
			return this.Allows(qname.Namespace);
		}

		public XObjectsNamespaceList Clone()
		{
			XObjectsNamespaceList nsl = (XObjectsNamespaceList)this.MemberwiseClone();
			if (this.type == XObjectsNamespaceList.ListType.Set)
			{
				Debug.Assert(this.@set != null);
				nsl.@set = (Hashtable)this.@set.Clone();
			}
			return nsl;
		}

		private XObjectsNamespaceList CompareSetToOther(XObjectsNamespaceList other)
		{
			XObjectsNamespaceList nslist = null;
			if (this.@set.Contains(other.targetNamespace))
			{
				nslist = (!this.@set.Contains(string.Empty) ? new XObjectsNamespaceList("##other", string.Empty) : new XObjectsNamespaceList());
			}
			else if (!this.@set.Contains(string.Empty))
			{
				nslist = other.Clone();
			}
			else
			{
				nslist = null;
			}
			return nslist;
		}

		public static XObjectsNamespaceList Intersection(XObjectsNamespaceList o1, XObjectsNamespaceList o2, bool v1Compat)
		{
			XObjectsNamespaceList xObjectsNamespaceList;
			XObjectsNamespaceList nslist = null;
			Debug.Assert(o1 != o2);
			if (o1.type == XObjectsNamespaceList.ListType.Any)
			{
				nslist = o2.Clone();
			}
			else if (o2.type == XObjectsNamespaceList.ListType.Any)
			{
				nslist = o1.Clone();
			}
			else if (!(o1.type != XObjectsNamespaceList.ListType.Set ? true : o2.type != XObjectsNamespaceList.ListType.Other))
			{
				nslist = o1.Clone();
				nslist.RemoveNamespace(o2.targetNamespace);
				if (!v1Compat)
				{
					nslist.RemoveNamespace(string.Empty);
				}
			}
			else if (!(o1.type != XObjectsNamespaceList.ListType.Other ? true : o2.type != XObjectsNamespaceList.ListType.Set))
			{
				nslist = o2.Clone();
				nslist.RemoveNamespace(o1.targetNamespace);
				if (!v1Compat)
				{
					nslist.RemoveNamespace(string.Empty);
				}
			}
			else if (!(o1.type != XObjectsNamespaceList.ListType.Set ? true : o2.type != XObjectsNamespaceList.ListType.Set))
			{
				nslist = o1.Clone();
				nslist = new XObjectsNamespaceList()
				{
					type = XObjectsNamespaceList.ListType.Set,
					@set = new Hashtable()
				};
				foreach (string ns in o1.@set.Keys)
				{
					if (o2.@set.Contains(ns))
					{
						nslist.@set.Add(ns, ns);
					}
				}
			}
			else if ((o1.type != XObjectsNamespaceList.ListType.Other ? false : o2.type == XObjectsNamespaceList.ListType.Other))
			{
				if (o1.targetNamespace == o2.targetNamespace)
				{
					nslist = o1.Clone();
					xObjectsNamespaceList = nslist;
					return xObjectsNamespaceList;
				}
				if (!v1Compat)
				{
					if (o1.targetNamespace == string.Empty)
					{
						nslist = o2.Clone();
					}
					else if (o2.targetNamespace == string.Empty)
					{
						nslist = o1.Clone();
					}
				}
			}
			xObjectsNamespaceList = nslist;
			return xObjectsNamespaceList;
		}

		public bool IsEmpty()
		{
			bool flag;
			if (this.type != XObjectsNamespaceList.ListType.Set)
			{
				flag = false;
			}
			else
			{
				flag = (this.@set == null ? true : this.@set.Count == 0);
			}
			return flag;
		}

		public static bool IsSubset(XObjectsNamespaceList sub, XObjectsNamespaceList super)
		{
			bool flag;
			if (super.type == XObjectsNamespaceList.ListType.Any)
			{
				flag = true;
			}
			else if (!(sub.type != XObjectsNamespaceList.ListType.Other ? true : super.type != XObjectsNamespaceList.ListType.Other))
			{
				flag = super.targetNamespace == sub.targetNamespace;
			}
			else if (sub.type != XObjectsNamespaceList.ListType.Set)
			{
				flag = false;
			}
			else if (super.type != XObjectsNamespaceList.ListType.Other)
			{
				Debug.Assert(super.type == XObjectsNamespaceList.ListType.Set);
				foreach (string ns in sub.@set.Keys)
				{
					if (!super.@set.Contains(ns))
					{
						flag = false;
						return flag;
					}
				}
				flag = true;
			}
			else
			{
				flag = !sub.@set.Contains(super.targetNamespace);
			}
			return flag;
		}

		private void RemoveNamespace(string tns)
		{
			if (this.@set[tns] != null)
			{
				this.@set.Remove(tns);
			}
		}

		public override string ToString()
		{
			string str;
			switch (this.type)
			{
				case XObjectsNamespaceList.ListType.Any:
				{
					str = "##any";
					break;
				}
				case XObjectsNamespaceList.ListType.Other:
				{
					str = "##other";
					break;
				}
				case XObjectsNamespaceList.ListType.Set:
				{
					StringBuilder sb = new StringBuilder();
					bool first = true;
					foreach (string s in this.@set.Keys)
					{
						if (!first)
						{
							sb.Append(" ");
						}
						else
						{
							first = false;
						}
						if (s == this.targetNamespace)
						{
							sb.Append("##targetNamespace");
						}
						else if (s.Length != 0)
						{
							sb.Append(s);
						}
						else
						{
							sb.Append("##local");
						}
					}
					str = sb.ToString();
					break;
				}
				default:
				{
					Debug.Assert(false);
					str = string.Empty;
					break;
				}
			}
			return str;
		}

		public static XObjectsNamespaceList Union(XObjectsNamespaceList o1, XObjectsNamespaceList o2, bool v1Compat)
		{
			XObjectsNamespaceList nslist = null;
			Debug.Assert(o1 != o2);
			if (o1.type == XObjectsNamespaceList.ListType.Any)
			{
				nslist = new XObjectsNamespaceList();
			}
			else if (o2.type == XObjectsNamespaceList.ListType.Any)
			{
				nslist = new XObjectsNamespaceList();
			}
			else if (!(o1.type != XObjectsNamespaceList.ListType.Set ? true : o2.type != XObjectsNamespaceList.ListType.Set))
			{
				nslist = o1.Clone();
				foreach (string ns in o2.@set.Keys)
				{
					nslist.@set[ns] = ns;
				}
			}
			else if (!(o1.type != XObjectsNamespaceList.ListType.Other ? true : o2.type != XObjectsNamespaceList.ListType.Other))
			{
				nslist = (!(o1.targetNamespace == o2.targetNamespace) ? new XObjectsNamespaceList("##other", string.Empty) : o1.Clone());
			}
			else if ((o1.type != XObjectsNamespaceList.ListType.Set ? true : o2.type != XObjectsNamespaceList.ListType.Other))
			{
				if ((o2.type != XObjectsNamespaceList.ListType.Set ? false : o1.type == XObjectsNamespaceList.ListType.Other))
				{
					if (v1Compat)
					{
						nslist = (!o2.@set.Contains(o2.targetNamespace) ? o1.Clone() : new XObjectsNamespaceList());
					}
					else if (!(o1.targetNamespace != string.Empty))
					{
						nslist = (!o2.@set.Contains(string.Empty) ? new XObjectsNamespaceList("##other", string.Empty) : new XObjectsNamespaceList());
					}
					else
					{
						nslist = o2.CompareSetToOther(o1);
					}
				}
			}
			else if (v1Compat)
			{
				nslist = (!o1.@set.Contains(o2.targetNamespace) ? o2.Clone() : new XObjectsNamespaceList());
			}
			else if (!(o2.targetNamespace != string.Empty))
			{
				nslist = (!o1.@set.Contains(string.Empty) ? new XObjectsNamespaceList("##other", string.Empty) : new XObjectsNamespaceList());
			}
			else
			{
				nslist = o1.CompareSetToOther(o2);
			}
			return nslist;
		}

		public enum ListType
		{
			Any,
			Other,
			Set
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class XNamespaceResolver : IXmlNamespaceResolver
	{
		private XElement element;

		public XNamespaceResolver(XElement element)
		{
			this.element = element;
		}

		private Dictionary<string, string> GetNamespaceDecls(XElement outOfScope, bool excludeXml)
		{
			Dictionary<string, string> namespaceDecls = new Dictionary<string, string>();
			do
			{
				foreach (XAttribute att in this.element.Attributes())
				{
					if (att.IsNamespaceDeclaration)
					{
						string prefix = att.Name.LocalName;
						if (prefix == "xmlns")
						{
							prefix = string.Empty;
						}
						if (!namespaceDecls.ContainsKey(prefix))
						{
							namespaceDecls.Add(prefix, att.Value);
						}
					}
				}
				this.element = this.element.Parent;
			}
			while (this.element != outOfScope);
			if (!excludeXml)
			{
				if (!namespaceDecls.ContainsKey("xml"))
				{
					namespaceDecls.Add("xml", XNamespace.Xml.NamespaceName);
				}
			}
			return namespaceDecls;
		}

		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
		{
			IDictionary<string, string> namespaceDecls;
			switch (scope)
			{
				case XmlNamespaceScope.All:
				{
					namespaceDecls = this.GetNamespaceDecls(null, false);
					break;
				}
				case XmlNamespaceScope.ExcludeXml:
				{
					namespaceDecls = this.GetNamespaceDecls(null, true);
					break;
				}
				case XmlNamespaceScope.Local:
				{
					namespaceDecls = this.GetNamespaceDecls(this.element.Parent, false);
					break;
				}
				default:
				{
					namespaceDecls = null;
					break;
				}
			}
			return namespaceDecls;
		}

		public string LookupNamespace(string prefix)
		{
			string str;
			Debug.Assert(prefix != null);
			str = (prefix.Length != 0 ? this.element.GetNamespaceOfPrefix(prefix).NamespaceName : this.element.GetDefaultNamespace().NamespaceName);
			return str;
		}

		public string LookupPrefix(string namespaceName)
		{
			Debug.Assert(namespaceName != null);
			return this.element.GetPrefixOfNamespace(namespaceName);
		}
	}
}
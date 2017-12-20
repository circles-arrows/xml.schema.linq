using System;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class WildCard
	{
		public readonly static WildCard DefaultWildCard;

		private XObjectsNamespaceList nsList;

		internal XObjectsNamespaceList NsList
		{
			get
			{
				return this.nsList;
			}
		}

		static WildCard()
		{
			WildCard.DefaultWildCard = new WildCard("##any", "");
		}

		public WildCard(string namespaces, string targetNamespace)
		{
			if (targetNamespace == null)
			{
				targetNamespace = "";
			}
			if (namespaces == null)
			{
				namespaces = "##any";
			}
			this.nsList = new XObjectsNamespaceList(namespaces, targetNamespace);
		}

		internal bool Allows(XName symbol)
		{
			return this.NsList.Allows(symbol.Namespace.ToString());
		}

		public override bool Equals(object obj)
		{
			bool flag;
			WildCard symbol = obj as WildCard;
			flag = (symbol == null ? false : symbol.NsList.Equals(this.NsList));
			return flag;
		}

		public override int GetHashCode()
		{
			return this.NsList.GetHashCode();
		}

		public override string ToString()
		{
			return string.Concat("<ANY> : ", this.NsList.ToString());
		}
	}
}
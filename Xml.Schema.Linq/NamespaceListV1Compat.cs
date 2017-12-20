using System;

namespace Xml.Schema.Linq
{
	internal class NamespaceListV1Compat : XObjectsNamespaceList
	{
		public NamespaceListV1Compat(string namespaces, string targetNamespace) : base(namespaces, targetNamespace)
		{
		}

		public override bool Allows(string ns)
		{
			bool flag;
			flag = (base.Type != XObjectsNamespaceList.ListType.Other ? base.Allows(ns) : ns != base.Excluded);
			return flag;
		}
	}
}
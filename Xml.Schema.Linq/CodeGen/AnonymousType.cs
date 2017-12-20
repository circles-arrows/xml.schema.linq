using System;
using System.Xml.Schema;

namespace Xml.Schema.Linq.CodeGen
{
	internal class AnonymousType
	{
		public string identifier;

		public XmlSchemaElement parentElement;

		public XmlSchemaComplexType wrappingType;

		public ClrTypeReference typeRefence;

		public AnonymousType()
		{
		}
	}
}
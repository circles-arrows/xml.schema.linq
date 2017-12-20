using System;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public class ExtractType : IFxtTransformation
	{
		internal XmlSchemaElement element;

		public ExtractType()
		{
		}

		public void Run()
		{
			this.element.SchemaType.Name = this.element.Name;
			this.element.XmlSchema().Add(this.element.SchemaType);
			this.element.SchemaType = null;
			this.element.SchemaTypeName = new XmlQualifiedName(this.element.Name, this.element.XmlSchema().TargetNamespace);
		}
	}
}
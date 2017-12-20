using System;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public class NamedContentModelEntity : ContentModelEntity
	{
		internal XName name;

		private int elementPosition = -1;

		private SchemaAwareContentModelEntity parentContentModel;

		internal int ElementPosition
		{
			get
			{
				return this.elementPosition;
			}
			set
			{
				this.elementPosition = value;
			}
		}

		internal XName Name
		{
			get
			{
				return this.name;
			}
		}

		internal SchemaAwareContentModelEntity ParentContentModel
		{
			get
			{
				return this.parentContentModel;
			}
			set
			{
				this.parentContentModel = value;
			}
		}

		public NamedContentModelEntity(XName name)
		{
			this.name = name;
		}

		public override void AddElementToParent(XName name, object value, XElement parentElement, bool addToExisting, XmlSchemaDatatype datatype)
		{
			throw new InvalidOperationException();
		}
	}
}
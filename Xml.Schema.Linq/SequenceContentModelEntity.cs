using System;

namespace Xml.Schema.Linq
{
	public class SequenceContentModelEntity : SchemaAwareContentModelEntity
	{
		internal override Xml.Schema.Linq.ContentModelType ContentModelType
		{
			get
			{
				return Xml.Schema.Linq.ContentModelType.Sequence;
			}
		}

		public SequenceContentModelEntity(params ContentModelEntity[] items) : base(items)
		{
		}
	}
}
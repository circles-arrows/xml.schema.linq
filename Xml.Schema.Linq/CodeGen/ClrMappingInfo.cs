using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Xml.Schema.Linq.CodeGen
{
	public class ClrMappingInfo
	{
		private List<ClrTypeInfo> types;

		private Dictionary<XmlSchemaObject, string> nameMappings;

		internal Dictionary<XmlSchemaObject, string> NameMappings
		{
			get
			{
				if (this.nameMappings == null)
				{
					this.nameMappings = new Dictionary<XmlSchemaObject, string>();
				}
				return this.nameMappings;
			}
			set
			{
				this.nameMappings = value;
			}
		}

		internal List<ClrTypeInfo> Types
		{
			get
			{
				if (this.types == null)
				{
					this.types = new List<ClrTypeInfo>();
				}
				return this.types;
			}
		}

		public ClrMappingInfo()
		{
		}
	}
}
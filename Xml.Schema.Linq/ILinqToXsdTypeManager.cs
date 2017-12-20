using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	public interface ILinqToXsdTypeManager
	{
		Dictionary<XName, Type> GlobalElementDictionary
		{
			get;
		}

		Dictionary<XName, Type> GlobalTypeDictionary
		{
			get;
		}

		Dictionary<Type, Type> RootContentTypeMapping
		{
			get;
		}

		XmlSchemaSet Schemas
		{
			get;
			set;
		}
	}
}
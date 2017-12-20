using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class TypeManager : ILinqToXsdTypeManager
	{
		private static XmlSchemaSet defaultSchemaSet;

		internal static TypeManager Default;

		Dictionary<XName, Type> Xml.Schema.Linq.ILinqToXsdTypeManager.GlobalElementDictionary
		{
			get
			{
				return XTypedServices.EmptyDictionary;
			}
		}

		Dictionary<XName, Type> Xml.Schema.Linq.ILinqToXsdTypeManager.GlobalTypeDictionary
		{
			get
			{
				return XTypedServices.EmptyDictionary;
			}
		}

		Dictionary<Type, Type> Xml.Schema.Linq.ILinqToXsdTypeManager.RootContentTypeMapping
		{
			get
			{
				return XTypedServices.EmptyTypeMappingDictionary;
			}
		}

		XmlSchemaSet Xml.Schema.Linq.ILinqToXsdTypeManager.Schemas
		{
			get
			{
				if (TypeManager.defaultSchemaSet == null)
				{
					XmlSchemaSet tempSet = new XmlSchemaSet();
					Interlocked.CompareExchange<XmlSchemaSet>(ref TypeManager.defaultSchemaSet, tempSet, null);
				}
				return TypeManager.defaultSchemaSet;
			}
			set
			{
				TypeManager.defaultSchemaSet = value;
			}
		}

		static TypeManager()
		{
			TypeManager.defaultSchemaSet = null;
			TypeManager.Default = new TypeManager();
		}

		public TypeManager()
		{
		}
	}
}
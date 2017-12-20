using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public interface IXMetaData
	{
		XTypedElement Content
		{
			get;
		}

		Dictionary<XName, Type> LocalElementsDictionary
		{
			get;
		}

		XName SchemaName
		{
			get;
		}

		ILinqToXsdTypeManager TypeManager
		{
			get;
		}

		SchemaOrigin TypeOrigin
		{
			get;
		}

		ContentModelEntity GetContentModel();

		FSM GetValidationStates();
	}
}
using System;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public class FxtScope
	{
		public FxtScope()
		{
		}

		public bool Test(XmlSchemaElement el)
		{
			return true;
		}

		public bool Test(XmlSchemaAttribute at)
		{
			return true;
		}

		public bool Test(XmlSchemaType ty)
		{
			return true;
		}
	}
}
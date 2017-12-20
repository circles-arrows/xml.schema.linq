using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public static class FxtLinq2XsdInterpreter
	{
		private static void interpret(XmlSchemaSet schemas, XElement trafo, FxtLog log, List<IFxtTransformation> trafos)
		{
			foreach (XElement child in trafo.Elements())
			{
				if ((child.Name != (FxtInterpreter.FxtNs + "Deanonymize") ? true : child.Elements().Count<XElement>() != 0))
				{
					throw FxtInterpreter.ex;
				}
				bool? nullable = (bool?)child.Attribute("strict");
				foreach (IFxtTransformation x in schemas.Deanonymize(new FxtScope(), (nullable.HasValue ? nullable.GetValueOrDefault() : false), log))
				{
					trafos.Add(x);
				}
			}
		}

		public static FxtLog Run(XmlSchemaSet schemas, XElement trafo)
		{
			FxtLog fxtLog = FxtInterpreter.Run(schemas, trafo, new interpreter(FxtLinq2XsdInterpreter.interpret));
			return fxtLog;
		}
	}
}
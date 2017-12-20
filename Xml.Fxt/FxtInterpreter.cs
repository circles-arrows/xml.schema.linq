using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public static class FxtInterpreter
	{
		internal static XNamespace FxtNs;

		internal static FxtException ex;

		static FxtInterpreter()
		{
			FxtInterpreter.FxtNs = "http://www.microsoft.com/FXT";
			FxtInterpreter.ex = new FxtInterpreterException("Requested Xsd transformation not understood.");
		}

		public static FxtLog Run(XmlSchemaSet schemas, XElement trafo, interpreter i)
		{
			FxtLog fxtLog;
			List<IFxtTransformation> trafos = new List<IFxtTransformation>();
			FxtLog log = new FxtLog();
			if (!schemas.IsCompiled)
			{
				schemas.Compile();
			}
			if (trafo != null)
			{
				i(schemas, trafo, log, trafos);
				foreach (IFxtTransformation x in trafos)
				{
					x.Run();
				}
				foreach (XmlSchema x in schemas.XmlSchemas())
				{
					schemas.Reprocess(x);
				}
				schemas.Compile();
				fxtLog = log;
			}
			else
			{
				fxtLog = log;
			}
			return fxtLog;
		}
	}
}
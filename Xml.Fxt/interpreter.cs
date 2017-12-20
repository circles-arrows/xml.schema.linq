using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Xml.Fxt
{
	public delegate void interpreter(XmlSchemaSet schemas, XElement trafo, FxtLog log, List<IFxtTransformation> trafos);
}
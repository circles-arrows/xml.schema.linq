using System;

namespace Xml.Schema.Linq
{
	internal class XTypedElementAnnotation
	{
		internal XTypedElement typedElement;

		internal XTypedElementAnnotation(XTypedElement typedElement)
		{
			this.typedElement = typedElement;
		}
	}
}
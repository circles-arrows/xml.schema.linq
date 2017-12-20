using System;

namespace Xml.Schema.Linq
{
	internal class XTypedElementWrapperAnnotation
	{
		internal XTypedElement typedElement;

		internal XTypedElementWrapperAnnotation(XTypedElement typedElement)
		{
			this.typedElement = typedElement;
		}
	}
}
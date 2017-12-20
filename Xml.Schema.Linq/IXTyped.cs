using System.Collections.Generic;

namespace Xml.Schema.Linq
{
	public interface IXTyped
	{
		IEnumerable<T> Ancestors<T>()
		where T : XTypedElement;

		IEnumerable<T> Descendants<T>()
		where T : XTypedElement, new();

		IEnumerable<T> SelfAndAncestors<T>()
		where T : XTypedElement;

		IEnumerable<T> SelfAndDescendants<T>()
		where T : XTypedElement, new();
	}
}
using System;
using System.Diagnostics;

namespace Xml.Schema.Linq
{
	[DebuggerDisplay("Count = {((ICountAndCopy)((object)this)).Count}")]
	[DebuggerTypeProxy(typeof(XListVisualizable.XListDebugVisualizer))]
	public abstract class XListVisualizable
	{
		protected XListVisualizable()
		{
		}

		internal class XListDebugVisualizer
		{
			private object _xList;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public XTypedElement[] Items
			{
				get
				{
					XTypedElement[] tArray = new XTypedElement[((ICountAndCopy)this._xList).Count];
					((ICountAndCopy)this._xList).CopyTo(tArray, 0);
					return tArray;
				}
			}

			public XListDebugVisualizer(object xList)
			{
				this._xList = xList;
			}
		}
	}
}
using System;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class SingleTransition
	{
		internal XName nameLabel;

		internal WildCard wcLabel;

		internal int nextState;

		public SingleTransition(XName name, int newState)
		{
			this.nameLabel = name;
			this.wcLabel = null;
			this.nextState = newState;
		}

		public SingleTransition(WildCard wildCard, int newState)
		{
			this.wcLabel = wildCard;
			this.nameLabel = null;
			this.nextState = newState;
		}
	}
}
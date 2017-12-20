using System;

namespace Xml.Schema.Linq.CodeGen
{
	internal class StateNameSource
	{
		private int nextName = 1;

		public StateNameSource()
		{
		}

		internal int Next()
		{
			StateNameSource stateNameSource = this;
			int num = stateNameSource.nextName;
			int num1 = num;
			stateNameSource.nextName = num + 1;
			return num1;
		}

		internal void Reset()
		{
			this.nextName = 1;
		}
	}
}
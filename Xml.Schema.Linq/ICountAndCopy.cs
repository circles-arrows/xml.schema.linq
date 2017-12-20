using System;

namespace Xml.Schema.Linq
{
	internal interface ICountAndCopy
	{
		int Count
		{
			get;
		}

		void CopyTo(Array valuesArray, int arrayIndex);
	}
}
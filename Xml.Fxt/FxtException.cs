using System;

namespace Xml.Fxt
{
	public class FxtException : Exception
	{
		public FxtException()
		{
		}

		public FxtException(string msg) : base(msg)
		{
		}
	}
}
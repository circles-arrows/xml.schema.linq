using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace Xml.Schema.Linq
{
	internal class UnionMemberTypeNotFoundException : LinqToXsdException
	{
		public UnionMemberTypeNotFoundException(object value, UnionSimpleTypeValidator typeDef) : base(LinqToXsdException.CreateMessage("Union Type: No Matching Member Type Was Found. Valid Types ", UnionMemberTypeNotFoundException.GetMemberTypeCodes(typeDef), value))
		{
		}

		private static List<string> GetMemberTypeCodes(UnionSimpleTypeValidator typeDef)
		{
			List<string> codes = new List<string>();
			SimpleTypeValidator[] memberTypes = typeDef.MemberTypes;
			for (int i = 0; i < (int)memberTypes.Length; i++)
			{
				SimpleTypeValidator type = memberTypes[i];
				codes.Add(type.DataType.TypeCode.ToString());
			}
			return codes;
		}
	}
}
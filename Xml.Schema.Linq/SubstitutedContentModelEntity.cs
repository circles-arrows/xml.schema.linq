using System;
using System.Xml.Linq;

namespace Xml.Schema.Linq
{
	public class SubstitutedContentModelEntity : NamedContentModelEntity
	{
		private XName[] members;

		internal XName[] Members
		{
			get
			{
				return this.members;
			}
		}

		public SubstitutedContentModelEntity(params XName[] names) : base(names[(int)names.Length - 1])
		{
			this.members = names;
		}
	}
}
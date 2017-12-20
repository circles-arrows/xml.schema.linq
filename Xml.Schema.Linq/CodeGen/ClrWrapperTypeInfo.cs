using System;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrWrapperTypeInfo : ClrTypeInfo
	{
		private ClrTypeReference innerType;

		internal string fixedDefaultValue;

		private bool hasBaseContentType;

		internal string DefaultValue
		{
			get
			{
				string str;
				if ((this.clrTypeFlags & ClrTypeFlags.HasDefaultValue) == ClrTypeFlags.None)
				{
					str = null;
				}
				else
				{
					str = this.fixedDefaultValue;
				}
				return str;
			}
			set
			{
				if (value != null)
				{
					this.clrTypeFlags |= ClrTypeFlags.HasDefaultValue;
					this.fixedDefaultValue = value;
				}
			}
		}

		internal string FixedValue
		{
			get
			{
				string str;
				if ((this.clrTypeFlags & ClrTypeFlags.HasFixedValue) == ClrTypeFlags.None)
				{
					str = null;
				}
				else
				{
					str = this.fixedDefaultValue;
				}
				return str;
			}
			set
			{
				if (value != null)
				{
					this.clrTypeFlags |= ClrTypeFlags.HasFixedValue;
					this.fixedDefaultValue = value;
				}
			}
		}

		internal override bool HasBaseContentType
		{
			get
			{
				return this.hasBaseContentType;
			}
		}

		internal ClrTypeReference InnerType
		{
			get
			{
				return this.innerType;
			}
			set
			{
				this.innerType = value;
			}
		}

		internal override bool IsWrapper
		{
			get
			{
				return true;
			}
		}

		internal ClrWrapperTypeInfo()
		{
		}

		internal ClrWrapperTypeInfo(bool hasBaseContentType)
		{
			this.hasBaseContentType = hasBaseContentType;
		}
	}
}
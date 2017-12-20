using System;
using System.CodeDom;
using System.Collections.Generic;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	internal abstract class TypePropertyBuilder
	{
		protected CodeTypeDeclItems declItems;

		protected CodeTypeDeclaration decl;

		public virtual bool IsRepeating
		{
			get
			{
				return false;
			}
		}

		public TypePropertyBuilder(CodeTypeDeclaration decl, CodeTypeDeclItems declItems)
		{
			this.decl = decl;
			this.declItems = declItems;
		}

		public static TypePropertyBuilder Create(GroupingInfo groupingInfo, CodeTypeDeclaration decl, CodeTypeDeclItems declItems)
		{
			TypePropertyBuilder defaultPropertyBuilder;
			switch (groupingInfo.ContentModelType)
			{
				case ContentModelType.None:
				case ContentModelType.All:
				{
					defaultPropertyBuilder = new DefaultPropertyBuilder(decl, declItems);
					break;
				}
				case ContentModelType.Sequence:
				{
					if (!groupingInfo.IsComplex)
					{
						defaultPropertyBuilder = new SequencePropertyBuilder(groupingInfo, decl, declItems);
						break;
					}
					else
					{
						defaultPropertyBuilder = new DefaultPropertyBuilder(decl, declItems);
						break;
					}
				}
				case ContentModelType.Choice:
				{
					if (!groupingInfo.IsComplex)
					{
						defaultPropertyBuilder = new ChoicePropertyBuilder(groupingInfo, decl, declItems);
						break;
					}
					else
					{
						defaultPropertyBuilder = new DefaultPropertyBuilder(decl, declItems);
						break;
					}
				}
				default:
				{
					throw new InvalidOperationException();
				}
			}
			return defaultPropertyBuilder;
		}

		public static TypePropertyBuilder Create(CodeTypeDeclaration decl, CodeTypeDeclItems declItems)
		{
			return new DefaultPropertyBuilder(decl, declItems);
		}

		public virtual void EndCodeGen()
		{
		}

		public virtual void GenerateCode(ClrBasePropertyInfo property, List<ClrAnnotation> annotations)
		{
			property.AddToType(this.decl, annotations);
		}

		public virtual void StartCodeGen()
		{
		}
	}
}
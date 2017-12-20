using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Xml.Schema.Linq.CodeGen
{
	internal class XSimpleTypedElementBuilder : TypeBuilder
	{
		private string simpleTypeName;

		private bool isSchemaList;

		public XSimpleTypedElementBuilder()
		{
		}

		internal override CodeConstructor CreateFunctionalConstructor(List<ClrAnnotation> annotations)
		{
			string parameterName = "content";
			CodeConstructor constructor = CodeDomHelper.CreateConstructor(MemberAttributes.Public);
			CodeTypeReference returnType = null;
			if (!this.isSchemaList)
			{
				returnType = new CodeTypeReference(this.simpleTypeName);
			}
			else
			{
				CodeTypeReference[] codeTypeReference = new CodeTypeReference[] { new CodeTypeReference(this.simpleTypeName) };
				returnType = new CodeTypeReference("IList", codeTypeReference);
			}
			constructor.Parameters.Add(new CodeParameterDeclarationExpression(returnType, parameterName));
			constructor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(CodeDomHelper.This(), "TypedValue"), new CodeVariableReferenceExpression(parameterName)));
			TypeBuilder.ApplyAnnotations(constructor, annotations, null);
			this.decl.Members.Add(constructor);
			return constructor;
		}

		internal override void CreateProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			propertyInfo.AddToType(this.decl, annotations);
		}

		internal void Init(string simpleTypeName, bool isSchemaList)
		{
			base.InnerInit();
			this.simpleTypeName = simpleTypeName;
			this.isSchemaList = isSchemaList;
		}
	}
}
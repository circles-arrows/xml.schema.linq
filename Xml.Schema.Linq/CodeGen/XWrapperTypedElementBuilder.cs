using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Xml.Schema.Linq.CodeGen
{
	internal class XWrapperTypedElementBuilder : TypeBuilder
	{
		private string innerTypeName;

		private string innerTypeNs;

		private string memberName;

		private TypeAttributes innerTypeAttributes;

		protected override string InnerType
		{
			get
			{
				return this.innerTypeName;
			}
		}

		public XWrapperTypedElementBuilder()
		{
		}

		internal void AddTypeToTypeManager(CodeStatementCollection elementDictionaryStatements, CodeStatementCollection wrapperDictionaryStatements)
		{
			base.AddTypeToTypeManager(elementDictionaryStatements, "elementDictionary");
			string innerTypeFullName = null;
			if (!this.innerTypeName.Contains(this.innerTypeNs))
			{
				innerTypeFullName = string.Concat("global::", this.innerTypeNs, ".", this.innerTypeName);
			}
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.Typeof(this.clrTypeInfo.clrFullTypeName), CodeDomHelper.Typeof(innerTypeFullName) };
			wrapperDictionaryStatements.Add(CodeDomHelper.CreateMethodCallFromField("wrapperDictionary", "Add", codeExpressionArray));
		}

		internal override void CreateDefaultConstructor(List<ClrAnnotation> annotations)
		{
			CodeMemberField typeField = CodeDomHelper.CreateMemberField(this.memberName, this.innerTypeName, MemberAttributes.Private, false);
			CodeFieldReferenceExpression fieldRef = CodeDomHelper.CreateFieldReference("this", this.memberName);
			CodeConstructor emptyConstructor = CodeDomHelper.CreateConstructor(MemberAttributes.Public);
			if ((this.innerTypeAttributes & TypeAttributes.Abstract) != TypeAttributes.NotPublic)
			{
				emptyConstructor.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression("InvalidOperationException", new CodeExpression[0])));
			}
			else
			{
				CodeStatementCollection statements = emptyConstructor.Statements;
				CodeExpression[] codeObjectCreateExpression = new CodeExpression[] { new CodeObjectCreateExpression(typeField.Type, new CodeExpression[0]) };
				statements.Add(CodeDomHelper.CreateMethodCall(null, "SetInnerType", codeObjectCreateExpression));
			}
			CodeConstructor dummyConstructor = null;
			if (this.clrTypeInfo.IsSubstitutionHead)
			{
				dummyConstructor = CodeDomHelper.CreateConstructor(MemberAttributes.Family);
				dummyConstructor.Parameters.Add(new CodeParameterDeclarationExpression("System.Boolean", "setNull"));
				this.decl.Members.Add(dummyConstructor);
			}
			if (this.clrTypeInfo.IsSubstitutionMember())
			{
				emptyConstructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(true));
				if (dummyConstructor != null)
				{
					dummyConstructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(true));
				}
			}
			TypeBuilder.ApplyAnnotations(emptyConstructor, annotations, null);
			this.decl.Members.Add(typeField);
			this.decl.Members.Add(emptyConstructor);
			this.decl.Members.Add(this.CreateUntypedProperty(fieldRef));
			this.decl.Members.Add(this.InnerTypeProperty());
			this.decl.Members.Add(this.SetInnerType());
			if (this.clrTypeInfo.IsSubstitutionHead)
			{
				this.decl.Members.Add(this.SetSubstitutionMember());
			}
		}

		internal override CodeConstructor CreateFunctionalConstructor(List<ClrAnnotation> annotations)
		{
			CodeConstructor constructor = CodeDomHelper.CreateConstructor(MemberAttributes.Public);
			if (this.clrTypeInfo.IsSubstitutionMember())
			{
				constructor.BaseConstructorArgs.Add(new CodePrimitiveExpression(true));
			}
			constructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.innerTypeName), "content"));
			CodeStatementCollection statements = constructor.Statements;
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression("content") };
			statements.Add(CodeDomHelper.CreateMethodCall(null, "SetInnerType", codeVariableReferenceExpression));
			TypeBuilder.ApplyAnnotations(constructor, annotations, null);
			this.decl.Members.Add(constructor);
			return constructor;
		}

		internal override void CreateProperty(ClrBasePropertyInfo propertyInfo, List<ClrAnnotation> annotations)
		{
			((ClrWrappingPropertyInfo)propertyInfo).WrappedFieldName = this.memberName;
			propertyInfo.AddToType(this.decl, annotations);
		}

		private CodeMemberProperty CreateUntypedProperty(CodeFieldReferenceExpression fieldRef)
		{
			CodeMemberProperty xElementProperty = CodeDomHelper.CreateProperty(new CodeTypeReference("XElement"), true);
			xElementProperty.Name = "Untyped";
			xElementProperty.Attributes = MemberAttributes.Override | MemberAttributes.FamilyAndAssembly | MemberAttributes.FamilyOrAssembly | MemberAttributes.Public;
			CodePropertyReferenceExpression baseUntyped = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), "Untyped");
			xElementProperty.GetStatements.Add(new CodeMethodReturnStatement(baseUntyped));
			xElementProperty.SetStatements.Add(new CodeAssignStatement(baseUntyped, CodeDomHelper.SetValue()));
			if (!this.clrTypeInfo.IsSubstitutionHead)
			{
				xElementProperty.SetStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(fieldRef, "Untyped"), CodeDomHelper.SetValue()));
			}
			else
			{
				CodeStatementCollection setStatements = xElementProperty.SetStatements;
				CodeBinaryOperatorExpression codeBinaryOperatorExpression = new CodeBinaryOperatorExpression(fieldRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
				CodeStatement[] codeAssignStatement = new CodeStatement[] { new CodeAssignStatement(new CodePropertyReferenceExpression(fieldRef, "Untyped"), CodeDomHelper.SetValue()) };
				setStatements.Add(new CodeConditionStatement(codeBinaryOperatorExpression, codeAssignStatement));
			}
			return xElementProperty;
		}

		protected override void ImplementCommonIXMetaData()
		{
			CodeMemberProperty localElementDictionary = CodeDomHelper.CreateInterfaceImplProperty("LocalElementsDictionary", "IXMetaData", CodeDomHelper.CreateDictionaryType("XName", "System.Type"));
			localElementDictionary.GetStatements.Add(CodeDomHelper.CreateCastToInterface("IXMetaData", "schemaMetaData", "Content"));
			localElementDictionary.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("schemaMetaData"), "LocalElementsDictionary")));
			CodeMemberProperty contentProperty = CodeDomHelper.CreateInterfaceImplProperty("Content", "IXMetaData", new CodeTypeReference("XTypedElement"));
			contentProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Content")));
			this.decl.Members.Add(localElementDictionary);
			this.decl.Members.Add(contentProperty);
		}

		protected override void ImplementContentModelMetaData()
		{
			this.decl.Members.Add(TypeBuilder.DefaultContentModel());
		}

		internal void Init(string innerTypeFullName, string innerTypeNs, TypeAttributes innerTypeAttributes)
		{
			base.InnerInit();
			this.memberName = NameGenerator.ChangeClrName("Content", NameOptions.MakeField);
			this.innerTypeName = innerTypeFullName;
			this.innerTypeNs = innerTypeNs;
			this.innerTypeAttributes = innerTypeAttributes;
		}

		private CodeMemberProperty InnerTypeProperty()
		{
			CodeMemberProperty innerTypeProperty = CodeDomHelper.CreateProperty("Content", new CodeTypeReference(this.innerTypeName), MemberAttributes.Public);
			innerTypeProperty.HasSet = false;
			if (this.clrTypeInfo.IsSubstitutionMember())
			{
				CodeMemberProperty attributes = innerTypeProperty;
				attributes.Attributes = attributes.Attributes | MemberAttributes.New;
			}
			innerTypeProperty.GetStatements.Add(new CodeMethodReturnStatement(CodeDomHelper.CreateFieldReference(null, this.memberName)));
			return innerTypeProperty;
		}

		private CodeMemberMethod SetInnerType()
		{
			CodeMemberMethod setInnerType = CodeDomHelper.CreateMethod("SetInnerType", MemberAttributes.Private, null);
			setInnerType.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.innerTypeName), this.memberName));
			CodeStatementCollection statements = setInnerType.Statements;
			CodeFieldReferenceExpression codeFieldReferenceExpression = CodeDomHelper.CreateFieldReference("this", this.memberName);
			string str = this.innerTypeName;
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
			CodeExpression[] codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression(this.memberName) };
			statements.Add(new CodeAssignStatement(codeFieldReferenceExpression, new CodeCastExpression(str, new CodeMethodInvokeExpression(codeTypeReferenceExpression, "GetCloneIfRooted", codeVariableReferenceExpression))));
			setInnerType.Statements.Add(this.SetNameMethodCall());
			if (this.clrTypeInfo.IsSubstitutionMember())
			{
				CodeStatementCollection codeStatementCollection = setInnerType.Statements;
				CodeBaseReferenceExpression codeBaseReferenceExpression = new CodeBaseReferenceExpression();
				codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression(this.memberName) };
				codeStatementCollection.Add(CodeDomHelper.CreateMethodCall(codeBaseReferenceExpression, "SetSubstitutionMember", codeVariableReferenceExpression));
			}
			return setInnerType;
		}

		private CodeMethodInvokeExpression SetNameMethodCall()
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression("XTypedServices");
			CodeExpression[] codeExpressionArray = new CodeExpression[] { CodeDomHelper.This(), CodeDomHelper.CreateFieldReference("this", this.memberName) };
			return new CodeMethodInvokeExpression(codeTypeReferenceExpression, "SetName", codeExpressionArray);
		}

		private CodeMemberMethod SetSubstitutionMember()
		{
			CodeMemberMethod setSubstMember = CodeDomHelper.CreateMethod("SetSubstitutionMember", MemberAttributes.Family, null);
			setSubstMember.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(this.innerTypeName), this.memberName));
			setSubstMember.Statements.Add(new CodeAssignStatement(CodeDomHelper.CreateFieldReference("this", this.memberName), new CodeVariableReferenceExpression(this.memberName)));
			if (this.clrTypeInfo.IsSubstitutionMember())
			{
				CodeStatementCollection statements = setSubstMember.Statements;
				CodeBaseReferenceExpression codeBaseReferenceExpression = new CodeBaseReferenceExpression();
				CodeExpression[] codeVariableReferenceExpression = new CodeExpression[] { new CodeVariableReferenceExpression(this.memberName) };
				statements.Add(CodeDomHelper.CreateMethodCall(codeBaseReferenceExpression, "SetSubstitutionMember", codeVariableReferenceExpression));
			}
			return setSubstMember;
		}
	}
}
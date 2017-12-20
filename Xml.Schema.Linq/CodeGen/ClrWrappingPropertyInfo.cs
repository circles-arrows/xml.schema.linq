using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xml.Schema.Linq.CodeGen
{
	internal class ClrWrappingPropertyInfo : ClrBasePropertyInfo
	{
		private const int propertySuffixIndex = 1;

		private string wrappedFieldName;

		private MemberAttributes wrappedPropertyAttributes;

		private CodeCommentStatementCollection codeCommentStatementCollection;

		internal string WrappedFieldName
		{
			get
			{
				return this.wrappedFieldName;
			}
			set
			{
				this.wrappedFieldName = value;
			}
		}

		public ClrWrappingPropertyInfo()
		{
			this.contentType = Xml.Schema.Linq.CodeGen.ContentType.Property;
		}

		private void AddGetStatements(CodeStatementCollection getStatements)
		{
			getStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.wrappedFieldName), this.propertyName)));
		}

		private void AddSetStatements(CodeStatementCollection setStatements)
		{
			setStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.wrappedFieldName), this.propertyName), CodeDomHelper.SetValue()));
		}

		internal override void AddToConstructor(CodeConstructor functionalConstructor)
		{
		}

		internal override void AddToContentModel(CodeObjectCreateExpression contentModelExpression)
		{
			throw new InvalidOperationException();
		}

		internal override CodeMemberProperty AddToType(CodeTypeDeclaration typeDecl, List<ClrAnnotation> annotations)
		{
			CodeMemberProperty wrapperProperty = CodeDomHelper.CreateProperty(this.returnType, this.hasSet);
			wrapperProperty.Name = this.CheckPropertyName(typeDecl.Name);
			wrapperProperty.Attributes = this.wrappedPropertyAttributes;
			this.AddGetStatements(wrapperProperty.GetStatements);
			if (this.hasSet)
			{
				this.AddSetStatements(wrapperProperty.SetStatements);
			}
			this.ApplyAnnotations(wrapperProperty, annotations);
			typeDecl.Members.Add(wrapperProperty);
			return wrapperProperty;
		}

		internal override void ApplyAnnotations(CodeMemberProperty propDecl, List<ClrAnnotation> annotations)
		{
			foreach (CodeCommentStatement comm in this.codeCommentStatementCollection)
			{
				propDecl.Comments.Add(new CodeCommentStatement(comm.Comment.Text, comm.Comment.DocComment));
			}
		}

		private string CheckPropertyName(string className)
		{
			string str;
			if (!this.propertyName.Equals(className))
			{
				str = this.propertyName;
			}
			else
			{
				int num = 1;
				str = string.Concat(this.propertyName, num.ToString(CultureInfo.InvariantCulture.NumberFormat));
			}
			return str;
		}

		public void Init(CodeMemberProperty property)
		{
			this.propertyName = property.Name;
			this.returnType = (XCodeTypeReference)property.Type;
			if ((this.returnType.fullTypeName == null ? false : this.returnType.fullTypeName != this.returnType.BaseType))
			{
				this.returnType = new XCodeTypeReference(this.returnType.fullTypeName);
			}
			this.hasSet = property.HasSet;
			this.wrappedPropertyAttributes = property.Attributes;
			this.codeCommentStatementCollection = property.Comments;
		}
	}
}
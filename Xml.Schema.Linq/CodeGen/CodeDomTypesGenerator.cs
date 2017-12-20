using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	public class CodeDomTypesGenerator
	{
		private LinqToXsdSettings settings;

		private TypeBuilder typeBuilder;

		private TypeBuilder emptyTypeBuilder;

		private XmlQualifiedName rootElementName = XmlQualifiedName.Empty;

		private CodeNamespace codeNamespace;

		private Dictionary<string, CodeNamespace> codeNamespacesTable;

		private Dictionary<XmlSchemaObject, string> nameMappings;

		private Dictionary<CodeNamespace, List<CodeTypeDeclaration>> xroots;

		private List<ClrWrapperTypeInfo> wrapperRootElements;

		private string currentNamespace;

		private string currentFullTypeName;

		private static CodeStatementCollection typeDictionaryAddStatements;

		private static CodeStatementCollection elementDictionaryAddStatements;

		private static CodeStatementCollection wrapperDictionaryAddStatements;

		public CodeDomTypesGenerator(bool nameMangler2) : this(new LinqToXsdSettings(nameMangler2))
		{
		}

		public CodeDomTypesGenerator(LinqToXsdSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException("Argument setttings should not be null.");
			}
			this.settings = settings;
			this.codeNamespacesTable = new Dictionary<string, CodeNamespace>();
			this.xroots = new Dictionary<CodeNamespace, List<CodeTypeDeclaration>>();
			CodeDomTypesGenerator.typeDictionaryAddStatements = new CodeStatementCollection();
			CodeDomTypesGenerator.elementDictionaryAddStatements = new CodeStatementCollection();
			CodeDomTypesGenerator.wrapperDictionaryAddStatements = new CodeStatementCollection();
		}

		private void AddDefaultImports(CodeNamespace newCodeNamespace)
		{
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.IO"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Xml"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
			if (this.settings.EnableServiceReference)
			{
				newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
			}
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("System.Xml.Linq"));
			newCodeNamespace.Imports.Add(new CodeNamespaceImport("Xml.Schema.Linq"));
		}

		private void CreateTypeManager()
		{
			string rootClrNamespace = this.settings.GetClrNamespace(this.rootElementName.Namespace);
			CodeNamespace rootCodeNamespace = null;
			if (!this.codeNamespacesTable.TryGetValue(rootClrNamespace, out rootCodeNamespace))
			{
				rootCodeNamespace = this.codeNamespacesTable.Values.FirstOrDefault<CodeNamespace>();
			}
			if (rootCodeNamespace != null)
			{
				rootCodeNamespace.Types.Add(TypeBuilder.CreateTypeManager(this.rootElementName, this.settings.EnableServiceReference, CodeDomTypesGenerator.typeDictionaryAddStatements, CodeDomTypesGenerator.elementDictionaryAddStatements, CodeDomTypesGenerator.wrapperDictionaryAddStatements));
				CodeNamespaceImport rootImport = new CodeNamespaceImport(rootCodeNamespace.Name);
				foreach (CodeNamespace cns in this.codeNamespacesTable.Values)
				{
					if (cns != rootCodeNamespace)
					{
						if (rootCodeNamespace.Name.Length > 0)
						{
							cns.Imports.Add(rootImport);
						}
						if (cns.Name.Length > 0)
						{
							rootCodeNamespace.Imports.Add(new CodeNamespaceImport(cns.Name));
						}
					}
				}
			}
		}

		private void CreateXRoot(CodeNamespace codeNamespace, string rootName, List<CodeTypeDeclaration> elements, List<CodeNamespace> namespaces)
		{
			if (codeNamespace != null)
			{
				LocalSymbolTable lst = new LocalSymbolTable(this.settings);
				CodeTypeDeclaration xroot = CodeDomHelper.CreateTypeDeclaration(rootName, null);
				CodeMemberField docField = CodeDomHelper.CreateMemberField("doc", "XDocument", MemberAttributes.Private, false);
				CodeMemberField rootField = CodeDomHelper.CreateMemberField("rootObject", "XTypedElement", MemberAttributes.Private, false);
				xroot.Members.Add(docField);
				xroot.Members.Add(rootField);
				lst.Init(rootName);
				lst.RegisterMember("doc");
				lst.RegisterMember("rootObject");
				lst.RegisterMember("Load");
				lst.RegisterMember("Parse");
				lst.RegisterMember("Save");
				lst.RegisterMember("XDocument");
				xroot.Members.Add(CodeDomHelper.CreateConstructor(MemberAttributes.Private));
				CodeTypeMemberCollection members = xroot.Members;
				string[][] strArrays = new string[1][];
				string[] strArrays1 = new string[] { "System.String", "xmlFile" };
				strArrays[0] = strArrays1;
				members.Add(CodeDomHelper.CreateXRootMethod(rootName, "Load", strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection = xroot.Members;
				strArrays = new string[2][];
				strArrays1 = new string[] { "System.String", "xmlFile" };
				strArrays[0] = strArrays1;
				strArrays1 = new string[] { "LoadOptions", "options" };
				strArrays[1] = strArrays1;
				codeTypeMemberCollection.Add(CodeDomHelper.CreateXRootMethod(rootName, "Load", strArrays));
				CodeTypeMemberCollection members1 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "TextReader", "textReader" };
				strArrays[0] = strArrays1;
				members1.Add(CodeDomHelper.CreateXRootMethod(rootName, "Load", strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection1 = xroot.Members;
				strArrays = new string[2][];
				strArrays1 = new string[] { "TextReader", "textReader" };
				strArrays[0] = strArrays1;
				strArrays1 = new string[] { "LoadOptions", "options" };
				strArrays[1] = strArrays1;
				codeTypeMemberCollection1.Add(CodeDomHelper.CreateXRootMethod(rootName, "Load", strArrays));
				CodeTypeMemberCollection members2 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "XmlReader", "xmlReader" };
				strArrays[0] = strArrays1;
				members2.Add(CodeDomHelper.CreateXRootMethod(rootName, "Load", strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection2 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "System.String", "text" };
				strArrays[0] = strArrays1;
				codeTypeMemberCollection2.Add(CodeDomHelper.CreateXRootMethod(rootName, "Parse", strArrays));
				CodeTypeMemberCollection members3 = xroot.Members;
				strArrays = new string[2][];
				strArrays1 = new string[] { "System.String", "text" };
				strArrays[0] = strArrays1;
				strArrays1 = new string[] { "LoadOptions", "options" };
				strArrays[1] = strArrays1;
				members3.Add(CodeDomHelper.CreateXRootMethod(rootName, "Parse", strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection3 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "System.String", "fileName" };
				strArrays[0] = strArrays1;
				codeTypeMemberCollection3.Add(CodeDomHelper.CreateXRootSave(strArrays));
				CodeTypeMemberCollection members4 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "TextWriter", "textWriter" };
				strArrays[0] = strArrays1;
				members4.Add(CodeDomHelper.CreateXRootSave(strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection4 = xroot.Members;
				strArrays = new string[1][];
				strArrays1 = new string[] { "XmlWriter", "writer" };
				strArrays[0] = strArrays1;
				codeTypeMemberCollection4.Add(CodeDomHelper.CreateXRootSave(strArrays));
				CodeTypeMemberCollection members5 = xroot.Members;
				strArrays = new string[2][];
				strArrays1 = new string[] { "TextWriter", "textWriter" };
				strArrays[0] = strArrays1;
				strArrays1 = new string[] { "SaveOptions", "options" };
				strArrays[1] = strArrays1;
				members5.Add(CodeDomHelper.CreateXRootSave(strArrays));
				CodeTypeMemberCollection codeTypeMemberCollection5 = xroot.Members;
				strArrays = new string[2][];
				strArrays1 = new string[] { "System.String", "fileName" };
				strArrays[0] = strArrays1;
				strArrays1 = new string[] { "SaveOptions", "options" };
				strArrays[1] = strArrays1;
				codeTypeMemberCollection5.Add(CodeDomHelper.CreateXRootSave(strArrays));
				CodeMemberProperty prop = CodeDomHelper.CreateProperty("XDocument", "XDocument", docField, MemberAttributes.Public, false);
				xroot.Members.Add(prop);
				for (int i = 0; i < elements.Count; i++)
				{
					string typeName = elements[i].Name;
					string fqTypeName = (namespaces == null || namespaces[i].Name == string.Empty ? typeName : string.Concat("global::", namespaces[i].Name, ".", typeName));
					xroot.Members.Add(CodeDomHelper.CreateXRootFunctionalConstructor(fqTypeName));
					xroot.Members.Add(CodeDomHelper.CreateXRootGetter(typeName, fqTypeName, lst));
				}
				codeNamespace.Types.Add(xroot);
			}
		}

		private void CreateXRoots()
		{
			List<CodeTypeDeclaration> allTypes = new List<CodeTypeDeclaration>();
			List<CodeNamespace> allNamespaces = new List<CodeNamespace>();
			string rootClrNamespace = this.settings.GetClrNamespace(this.rootElementName.Namespace);
			CodeNamespace rootCodeNamespace = null;
			if (!this.codeNamespacesTable.TryGetValue(rootClrNamespace, out rootCodeNamespace))
			{
				rootCodeNamespace = this.codeNamespacesTable.Values.FirstOrDefault<CodeNamespace>();
			}
			foreach (CodeNamespace codeNamespace in this.xroots.Keys)
			{
				if (rootCodeNamespace == null)
				{
					rootCodeNamespace = codeNamespace;
				}
				List<CodeTypeDeclaration> xRoot = this.xroots[codeNamespace];
				foreach (CodeTypeDeclaration i in xRoot)
				{
					allTypes.Add(i);
					allNamespaces.Add(codeNamespace);
				}
				this.CreateXRoot(codeNamespace, "XRootNamespace", xRoot, null);
			}
			this.CreateXRoot(rootCodeNamespace, "XRoot", allTypes, allNamespaces);
		}

		private bool ForwardProperty(CodeMemberProperty property)
		{
			return (property == null ? false : property.ImplementationTypes.Count == 0);
		}

		public IEnumerable<CodeNamespace> GenerateTypes(ClrMappingInfo binding)
		{
			List<CodeTypeDeclaration> types;
			if (binding == null)
			{
				throw new ArgumentException("binding");
			}
			this.nameMappings = binding.NameMappings;
			Debug.Assert(this.nameMappings != null);
			foreach (ClrTypeInfo type in binding.Types)
			{
				if (!type.IsWrapper)
				{
					this.codeNamespace = this.GetCodeNamespace(type.clrtypeNs);
					ClrSimpleTypeInfo stInfo = type as ClrSimpleTypeInfo;
					if (stInfo == null)
					{
						CodeTypeDeclaration decl = this.ProcessType(type as ClrContentTypeInfo, null, true);
						this.codeNamespace.Types.Add(decl);
						if (type.IsRootElement)
						{
							if (!this.xroots.TryGetValue(this.codeNamespace, out types))
							{
								types = new List<CodeTypeDeclaration>();
								this.xroots.Add(this.codeNamespace, types);
							}
							types.Add(decl);
						}
					}
					else
					{
						this.codeNamespace.Types.Add(TypeBuilder.CreateSimpleType(stInfo, this.nameMappings, this.settings));
					}
				}
				else
				{
					if (this.wrapperRootElements == null)
					{
						this.wrapperRootElements = new List<ClrWrapperTypeInfo>();
					}
					this.wrapperRootElements.Add(type as ClrWrapperTypeInfo);
				}
			}
			this.ProcessWrapperTypes();
			this.CreateTypeManager();
			this.CreateXRoots();
			return this.codeNamespacesTable.Values;
		}

		private CodeNamespace GetCodeNamespace(string clrNamespace)
		{
			CodeNamespace codeNamespace;
			if ((this.codeNamespace == null ? true : !(this.codeNamespace.Name == clrNamespace)))
			{
				CodeNamespace currentCodeNamespace = null;
				if (!this.codeNamespacesTable.TryGetValue(clrNamespace, out currentCodeNamespace))
				{
					currentCodeNamespace = new CodeNamespace(clrNamespace);
					this.AddDefaultImports(currentCodeNamespace);
					this.codeNamespacesTable.Add(clrNamespace, currentCodeNamespace);
				}
				codeNamespace = currentCodeNamespace;
			}
			else
			{
				codeNamespace = this.codeNamespace;
			}
			return codeNamespace;
		}

		private CodeTypeDeclaration GetCodeTypeDeclaration(string typeName, CodeNamespace innerTypeCodeNamespace)
		{
			CodeTypeDeclaration codeTypeDeclaration;
			if (innerTypeCodeNamespace != null)
			{
				foreach (CodeTypeDeclaration decl in innerTypeCodeNamespace.Types)
				{
					if (decl.Name.Equals(typeName))
					{
						codeTypeDeclaration = decl;
						return codeTypeDeclaration;
					}
				}
				codeTypeDeclaration = null;
			}
			else
			{
				codeTypeDeclaration = null;
			}
			return codeTypeDeclaration;
		}

		private TypeBuilder GetEmptyTypeBuilder()
		{
			if (this.emptyTypeBuilder != null)
			{
				this.emptyTypeBuilder.Init();
			}
			else
			{
				this.emptyTypeBuilder = new XEmptyTypedElementBuilder();
			}
			return this.emptyTypeBuilder;
		}

		private TypeBuilder GetTypeBuilder()
		{
			if (this.typeBuilder != null)
			{
				this.typeBuilder.Init();
			}
			else
			{
				this.typeBuilder = new XTypedElementBuilder();
			}
			return this.typeBuilder;
		}

		private ClrPropertyInfo InitializeTypedValuePropertyInfo(ClrTypeInfo typeInfo, ClrPropertyInfo typedValPropertyInfo, ClrTypeReference innerType)
		{
			if (typedValPropertyInfo != null)
			{
				typedValPropertyInfo.Reset();
			}
			else
			{
				typedValPropertyInfo = new ClrPropertyInfo("TypedValue", string.Empty, "TypedValue", Occurs.One)
				{
					Origin = SchemaOrigin.Text
				};
			}
			typedValPropertyInfo.TypeReference = innerType;
			if (typeInfo.IsSubstitutionMember())
			{
				typedValPropertyInfo.IsNew = true;
			}
			typedValPropertyInfo.UpdateTypeReference(this.currentFullTypeName, this.currentNamespace, this.nameMappings);
			return typedValPropertyInfo;
		}

		private void ProcessComplexGroupProperties(GroupingInfo grouping, List<ClrAnnotation> annotations)
		{
			foreach (ContentInfo child in grouping.Children)
			{
				if (child.ContentType == ContentType.Property)
				{
					ClrPropertyInfo propertyInfo = child as ClrPropertyInfo;
					propertyInfo.UpdateTypeReference(this.currentFullTypeName, this.currentNamespace, this.nameMappings);
					this.typeBuilder.CreateProperty(propertyInfo, annotations);
				}
				else if (child.ContentType != ContentType.WildCardProperty)
				{
					Debug.Assert(child.ContentType == ContentType.Grouping);
					this.ProcessComplexGroupProperties(child as GroupingInfo, annotations);
				}
				else
				{
					this.typeBuilder.CreateProperty(child as ClrWildCardPropertyInfo, annotations);
				}
			}
		}

		private void ProcessGroup(GroupingInfo grouping, List<ClrAnnotation> annotations)
		{
			this.typeBuilder.StartGrouping(grouping);
			foreach (ContentInfo child in grouping.Children)
			{
				if (child.ContentType == ContentType.Property)
				{
					ClrPropertyInfo propertyInfo = child as ClrPropertyInfo;
					propertyInfo.UpdateTypeReference(this.currentFullTypeName, this.currentNamespace, this.nameMappings);
					this.typeBuilder.CreateProperty(propertyInfo, annotations);
				}
				else if (child.ContentType != ContentType.WildCardProperty)
				{
					Debug.Assert(child.ContentType == ContentType.Grouping);
					this.ProcessGroup(child as GroupingInfo, annotations);
				}
				else
				{
					this.typeBuilder.CreateProperty(child as ClrWildCardPropertyInfo, annotations);
				}
			}
			this.typeBuilder.EndGrouping();
		}

		private void ProcessNestedTypes(List<ClrTypeInfo> anonymousTypes, CodeTypeDeclaration parentTypeDecl, string parentIdentifier)
		{
			foreach (ClrTypeInfo nestedType in anonymousTypes)
			{
				ClrSimpleTypeInfo stInfo = nestedType as ClrSimpleTypeInfo;
				CodeTypeDeclaration decl = null;
				if (stInfo == null)
				{
					decl = this.ProcessType(nestedType as ClrContentTypeInfo, parentIdentifier, false);
				}
				else
				{
					decl = TypeBuilder.CreateSimpleType(stInfo, this.nameMappings, this.settings);
					decl.TypeAttributes = TypeAttributes.NestedPrivate;
				}
				parentTypeDecl.Members.Add(decl);
			}
		}

		private void ProcessProperties(IEnumerable<ContentInfo> properties, List<ClrAnnotation> annotations)
		{
			foreach (ContentInfo child in properties)
			{
				if (child.ContentType != ContentType.Property)
				{
					GroupingInfo rootGroup = child as GroupingInfo;
					if (!rootGroup.IsComplex)
					{
						this.ProcessGroup(rootGroup, annotations);
					}
					else
					{
						this.typeBuilder.StartGrouping(rootGroup);
						this.ProcessComplexGroupProperties(rootGroup, annotations);
						this.typeBuilder.EndGrouping();
					}
				}
				else
				{
					(child as ClrPropertyInfo).UpdateTypeReference(this.currentFullTypeName, this.currentNamespace, this.nameMappings);
					this.typeBuilder.CreateAttributeProperty(child as ClrPropertyInfo, null);
				}
			}
		}

		private CodeTypeDeclaration ProcessType(ClrContentTypeInfo typeInfo, string parentIdentifier, bool globalType)
		{
			this.SetFullTypeName(typeInfo, parentIdentifier);
			if (globalType)
			{
				this.currentNamespace = typeInfo.clrtypeNs;
			}
			this.typeBuilder = this.GetTypeBuilder();
			this.typeBuilder.CreateTypeDeclaration(typeInfo);
			this.ProcessProperties(typeInfo.Content, typeInfo.Annotations);
			this.typeBuilder.CreateFunctionalConstructor(typeInfo.Annotations);
			this.typeBuilder.ImplementInterfaces(this.settings.EnableServiceReference);
			this.typeBuilder.ApplyAnnotations(typeInfo);
			if (globalType)
			{
				if (typeInfo.typeOrigin != SchemaOrigin.Fragment)
				{
					this.typeBuilder.AddTypeToTypeManager(CodeDomTypesGenerator.elementDictionaryAddStatements, "elementDictionary");
				}
				else
				{
					this.typeBuilder.AddTypeToTypeManager(CodeDomTypesGenerator.typeDictionaryAddStatements, "typeDictionary");
				}
			}
			CodeTypeDeclaration builtType = this.typeBuilder.TypeDeclaration;
			this.ProcessNestedTypes(typeInfo.NestedTypes, builtType, typeInfo.clrFullTypeName);
			return builtType;
		}

		private void ProcessWrapperTypes()
		{
			List<CodeTypeDeclaration> types;
			if (this.wrapperRootElements != null)
			{
				XWrapperTypedElementBuilder wrapperBuilder = new XWrapperTypedElementBuilder();
				XSimpleTypedElementBuilder simpleTypeBuilder = new XSimpleTypedElementBuilder();
				TypeBuilder builder = null;
				ClrPropertyInfo typedValPropertyInfo = null;
				foreach (ClrWrapperTypeInfo typeInfo in this.wrapperRootElements)
				{
					this.SetFullTypeName(typeInfo, null);
					ClrTypeReference innerType = typeInfo.InnerType;
					if (!innerType.IsSimpleType)
					{
						string innerTypeName = null;
						string innerTypeFullName = innerType.GetClrFullTypeName(typeInfo.clrtypeNs, this.nameMappings, out innerTypeName);
						string innerTypeNs = innerType.Namespace;
						CodeTypeDeclaration innerTypeDecl = this.GetCodeTypeDeclaration(innerTypeName, this.GetCodeNamespace(innerTypeNs));
						TypeAttributes innerTypeAttributes = TypeAttributes.NotPublic;
						if (innerTypeDecl != null)
						{
							innerTypeAttributes = innerTypeDecl.TypeAttributes;
						}
						else if (innerTypeName != "XTypedElement")
						{
							continue;
						}
						this.currentNamespace = typeInfo.clrtypeNs;
						wrapperBuilder.Init(innerTypeFullName, innerTypeNs, innerTypeAttributes);
						wrapperBuilder.CreateTypeDeclaration(typeInfo);
						wrapperBuilder.CreateFunctionalConstructor(typeInfo.Annotations);
						wrapperBuilder.ApplyAnnotations(typeInfo);
						wrapperBuilder.AddTypeToTypeManager(CodeDomTypesGenerator.elementDictionaryAddStatements, CodeDomTypesGenerator.wrapperDictionaryAddStatements);
						if (!typeInfo.HasBaseContentType)
						{
							ClrWrappingPropertyInfo wrappingPropertyInfo = null;
							if (innerTypeName != "XTypedElement")
							{
								wrappingPropertyInfo = new ClrWrappingPropertyInfo();
								foreach (CodeTypeMember member in innerTypeDecl.Members)
								{
									CodeMemberProperty memberProperty = member as CodeMemberProperty;
									if (this.ForwardProperty(memberProperty))
									{
										wrappingPropertyInfo.Init(memberProperty);
										wrapperBuilder.CreateProperty(wrappingPropertyInfo, typeInfo.Annotations);
									}
								}
							}
						}
						builder = wrapperBuilder;
					}
					else
					{
						typedValPropertyInfo = this.InitializeTypedValuePropertyInfo(typeInfo, typedValPropertyInfo, innerType);
						simpleTypeBuilder.Init(typedValPropertyInfo.ClrTypeName, innerType.IsSchemaList);
						simpleTypeBuilder.CreateTypeDeclaration(typeInfo);
						simpleTypeBuilder.CreateFunctionalConstructor(typeInfo.Annotations);
						typedValPropertyInfo.SetFixedDefaultValue(typeInfo);
						simpleTypeBuilder.CreateProperty(typedValPropertyInfo, typeInfo.Annotations);
						simpleTypeBuilder.AddTypeToTypeManager(CodeDomTypesGenerator.elementDictionaryAddStatements, "elementDictionary");
						simpleTypeBuilder.ApplyAnnotations(typeInfo);
						builder = simpleTypeBuilder;
					}
					builder.ImplementInterfaces(this.settings.EnableServiceReference);
					this.codeNamespace = this.GetCodeNamespace(typeInfo.clrtypeNs);
					this.codeNamespace.Types.Add(builder.TypeDeclaration);
					this.codeNamespace = this.GetCodeNamespace(typeInfo.clrtypeNs);
					if (!this.xroots.TryGetValue(this.codeNamespace, out types))
					{
						types = new List<CodeTypeDeclaration>();
						this.xroots.Add(this.codeNamespace, types);
					}
					types.Add(builder.TypeDeclaration);
				}
			}
		}

		private void SetFullTypeName(ClrTypeInfo typeInfo, string parentIdentifier)
		{
			bool flag;
			if (parentIdentifier != null)
			{
				this.currentFullTypeName = string.Concat(parentIdentifier, ".", typeInfo.clrtypeName);
			}
			else if (!(typeInfo.clrtypeNs == string.Empty))
			{
				this.currentFullTypeName = string.Concat(typeInfo.clrtypeNs, ".", typeInfo.clrtypeName);
			}
			else
			{
				this.currentFullTypeName = typeInfo.clrtypeName;
			}
			typeInfo.clrFullTypeName = this.currentFullTypeName;
			XmlQualifiedName baseTypeName = typeInfo.BaseTypeName;
			if (baseTypeName != XmlQualifiedName.Empty)
			{
				string clrNamespace = this.settings.GetClrNamespace(baseTypeName.Namespace);
				string baseTypeIdentifier = null;
				if (this.nameMappings.TryGetValue(typeInfo.baseType, out baseTypeIdentifier))
				{
					typeInfo.baseTypeClrName = baseTypeIdentifier;
					typeInfo.baseTypeClrNs = clrNamespace;
				}
			}
			if (typeInfo.typeOrigin != SchemaOrigin.Element)
			{
				flag = true;
			}
			else
			{
				flag = (this.rootElementName.IsEmpty ? false : !typeInfo.IsRoot);
			}
			if (!flag)
			{
				this.rootElementName = new XmlQualifiedName(typeInfo.schemaName, typeInfo.schemaNs);
			}
		}
	}
}
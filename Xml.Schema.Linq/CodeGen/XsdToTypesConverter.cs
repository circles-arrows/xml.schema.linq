using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Xml.Fxt;
using Xml.Schema.Linq;

namespace Xml.Schema.Linq.CodeGen
{
	public class XsdToTypesConverter
	{
		private XmlSchemaSet schemas;

		private int schemaErrorCount;

		private LinqToXsdSettings configSettings;

		private GlobalSymbolTable symbolTable;

		private LocalSymbolTable localSymbolTable;

		private ClrMappingInfo binding;

		private Stack<XsdToTypesConverter.ParticleData> particleStack;

		private Dictionary<string, ClrPropertyInfo> propertyNameTypeTable;

		private Dictionary<XmlSchemaType, ClrPropertyInfo> textPropInheritanceTracker;

		private Dictionary<XmlQualifiedName, ArrayList> substitutionGroups;

		public XsdToTypesConverter(bool nameMangler2) : this(new LinqToXsdSettings(nameMangler2))
		{
		}

		public XsdToTypesConverter(LinqToXsdSettings configSettings)
		{
			this.configSettings = configSettings;
			this.symbolTable = new GlobalSymbolTable(configSettings);
			this.localSymbolTable = new LocalSymbolTable(configSettings);
			this.binding = new ClrMappingInfo();
			this.textPropInheritanceTracker = new Dictionary<XmlSchemaType, ClrPropertyInfo>();
			this.substitutionGroups = new Dictionary<XmlQualifiedName, ArrayList>();
		}

		internal void AddSimpleType(XmlQualifiedName name, XmlSchemaSimpleType simpleType)
		{
			SymbolEntry symbol = this.symbolTable.AddType(name, simpleType);
			string xsdNamespace = simpleType.QualifiedName.Namespace;
			ClrSimpleTypeInfo typeInfo = ClrSimpleTypeInfo.CreateSimpleTypeInfo(simpleType);
			typeInfo.IsAbstract = false;
			typeInfo.clrtypeName = symbol.identifierName;
			typeInfo.clrtypeNs = symbol.clrNamespace;
			typeInfo.schemaName = symbol.symbolName;
			typeInfo.schemaNs = xsdNamespace;
			typeInfo.typeOrigin = SchemaOrigin.Fragment;
			this.BuildAnnotationInformation(typeInfo, simpleType);
			this.binding.Types.Add(typeInfo);
		}

		private void AppendCardinalityInformation(List<ClrAnnotation> annotations, ClrBasePropertyInfo basePropertyInfo, XmlSchemaObject schemaObject, bool isInChoice, bool isInNestedGroup)
		{
			ClrPropertyInfo propertyInfo = basePropertyInfo as ClrPropertyInfo;
			string text = string.Empty;
			text = string.Concat(text, "Occurrence: ");
			text = (!propertyInfo.IsOptional ? string.Concat(text, "required") : string.Concat(text, "optional"));
			if ((propertyInfo.IsStar ? true : propertyInfo.IsPlus))
			{
				text = string.Concat(text, ", repeating");
			}
			if (isInChoice)
			{
				text = string.Concat(text, ", choice");
			}
			this.AppendMessage(annotations, "summary", text);
			if (isInNestedGroup)
			{
				this.AppendMessage(annotations, "summary", "Setter: Appends");
			}
			if (propertyInfo.IsSubstitutionHead)
			{
				bool fComma = false;
				text = "Substitution members: ";
				foreach (XmlSchemaElement xse in propertyInfo.SubstitutionMembers)
				{
					if (fComma)
					{
						text = string.Concat(text, ", ");
					}
					else
					{
						fComma = true;
					}
					text = string.Concat(text, xse.Name);
				}
				this.AppendMessage(annotations, "summary", text);
			}
		}

		private void AppendMessage(List<ClrAnnotation> annotations, string section, string message)
		{
			Debug.Assert((message.Length == 0 ? false : section.Length != 0));
			ClrAnnotation clrAnn = new ClrAnnotation()
			{
				Section = section,
				Text = message
			};
			annotations.Add(clrAnn);
		}

		private void AppendOccurenceToRegex(ContentInfo contentInfo, StringBuilder regEx)
		{
			if (contentInfo.IsStar)
			{
				regEx.Append("*");
			}
			else if (contentInfo.IsPlus)
			{
				regEx.Append("+");
			}
			else if (contentInfo.IsQMark)
			{
				regEx.Append("?");
			}
		}

		private void AppendRegExInformation(ClrTypeInfo typeInfo)
		{
			if ((typeInfo.ContentModelRegEx == null ? false : typeInfo.ContentModelRegEx.Length > 0))
			{
				string text = string.Concat("Regular expression: ", typeInfo.ContentModelRegEx);
				this.AppendMessage(typeInfo.Annotations, "summaryRegEx", text);
			}
		}

		private void AppendXsdDocumentationInformation(List<ClrAnnotation> annotations, XmlSchemaObject schemaObject)
		{
			XmlNode[] markup;
			XmlSchemaAnnotated annotatedObject = schemaObject as XmlSchemaAnnotated;
			if ((annotatedObject == null ? false : annotatedObject.Annotation != null))
			{
				foreach (XmlSchemaObject annot in annotatedObject.Annotation.Items)
				{
					XmlSchemaDocumentation doc = annot as XmlSchemaDocumentation;
					markup = (doc == null ? ((XmlSchemaAppInfo)annot).Markup : doc.Markup);
					string text = string.Empty;
					XmlNode[] xmlNodeArrays = markup;
					for (int i = 0; i < (int)xmlNodeArrays.Length; i++)
					{
						XmlNode xn = xmlNodeArrays[i];
						text = string.Concat(text, xn.InnerText);
					}
					if (text.Length > 0)
					{
						this.AppendMessage(annotations, "summary", text);
					}
				}
			}
		}

		internal void AttributesToTypes()
		{
			foreach (XmlSchemaAttribute a in this.schemas.GlobalAttributes.Values)
			{
				if ((!a.AttributeSchemaType.QualifiedName.IsEmpty ? false : a.AttributeSchemaType.IsOrHasUnion()))
				{
					this.AddSimpleType(a.QualifiedName, a.AttributeSchemaType);
				}
			}
		}

		private XmlSchemaType BaseType(XmlSchemaComplexType ct)
		{
			XmlSchemaType xmlSchemaType;
			XmlSchemaType baseType = ct.BaseXmlSchemaType;
			XmlQualifiedName baseTypeName = baseType.QualifiedName;
			if ((baseType.TypeCode == XmlTypeCode.Item || baseTypeName.Equals(ct.QualifiedName) ? false : !(baseType is XmlSchemaSimpleType)))
			{
				xmlSchemaType = baseType;
			}
			else
			{
				xmlSchemaType = null;
			}
			return xmlSchemaType;
		}

		private void BuildAnnotationInformation(ClrTypeInfo typeInfo, XmlSchemaObject schemaObject)
		{
			this.AppendXsdDocumentationInformation(typeInfo.Annotations, schemaObject);
			this.AppendRegExInformation(typeInfo);
		}

		private void BuildAnnotationInformation(ClrBasePropertyInfo propertyInfo, XmlSchemaObject schemaObject, bool isInChoice, bool isInNestedGroup)
		{
			this.AppendXsdDocumentationInformation(propertyInfo.Annotations, schemaObject);
			this.AppendCardinalityInformation(propertyInfo.Annotations, propertyInfo, schemaObject, isInChoice, isInNestedGroup);
		}

		private ClrWildCardPropertyInfo BuildAnyProperty(XmlSchemaAny any, bool addToTypeDef)
		{
			ClrWildCardPropertyInfo property = new ClrWildCardPropertyInfo(any.Namespace, any.GetTargetNS(), addToTypeDef, this.GetOccurence(any))
			{
				PropertyName = "Any"
			};
			return property;
		}

		private ClrPropertyInfo BuildComplexTypeTextProperty(XmlSchemaElement parentElement, XmlSchemaComplexType schemaType)
		{
			string identifier;
			ClrPropertyInfo clrPropertyInfo;
			Debug.Assert(schemaType != null);
			Debug.Assert(schemaType.GetContentType() == XmlSchemaContentType.TextOnly);
			ClrPropertyInfo textProperty = new ClrPropertyInfo("TypedValue", string.Empty, "TypedValue", Occurs.One)
			{
				Origin = SchemaOrigin.Text
			};
			ClrTypeReference typeRef = null;
			bool anonymous = false;
			XmlSchemaType baseType = schemaType.BaseXmlSchemaType;
			if (!(baseType is XmlSchemaSimpleType))
			{
				if (schemaType.HasFacetRestrictions())
				{
					goto Label1;
				}
				clrPropertyInfo = null;
				return clrPropertyInfo;
			}
			else
			{
				typeRef = this.BuildTypeReference(baseType, baseType.QualifiedName, false, true);
				anonymous = false;
				if (!this.textPropInheritanceTracker.ContainsKey(schemaType))
				{
					this.textPropInheritanceTracker.Add(schemaType, textProperty);
				}
			}
			if (anonymous)
			{
				if (parentElement == null)
				{
					this.localSymbolTable.AddComplexRestrictedContentType(schemaType, typeRef);
				}
				else
				{
					identifier = this.localSymbolTable.AddLocalElement(parentElement);
					this.localSymbolTable.AddAnonymousType(identifier, parentElement, typeRef);
				}
			}
			textProperty.TypeReference = typeRef;
			clrPropertyInfo = textProperty;
			return clrPropertyInfo;
		Label1:
			XmlSchemaSimpleType st = schemaType.GetBaseSimpleType();
			Debug.Assert(st != null);
			typeRef = this.BuildTypeReference(st, st.QualifiedName, true, true);
			typeRef.Validate = true;
			anonymous = true;
			ClrPropertyInfo baseProp = null;
			if (this.textPropInheritanceTracker.TryGetValue(baseType, out baseProp))
			{
				textProperty.IsOverride = true;
				if (!baseProp.IsOverride)
				{
					baseProp.IsVirtual = true;
				}
			}
			if (anonymous)
			{
				if (parentElement == null)
				{
					this.localSymbolTable.AddComplexRestrictedContentType(schemaType, typeRef);
				}
				else
				{
					identifier = this.localSymbolTable.AddLocalElement(parentElement);
					this.localSymbolTable.AddAnonymousType(identifier, parentElement, typeRef);
				}
			}
			textProperty.TypeReference = typeRef;
			clrPropertyInfo = textProperty;
			return clrPropertyInfo;
		}

		private void BuildNestedTypes(ClrContentTypeInfo typeInfo)
		{
			ClrSimpleTypeInfo clrSimpleTypeInfo;
			foreach (AnonymousType at in this.localSymbolTable.GetAnonymousTypes())
			{
				XmlQualifiedName qname = null;
				XmlSchemaComplexType complexType = null;
				XmlSchemaElement elem = at.parentElement;
				if (elem != null)
				{
					qname = elem.QualifiedName;
					complexType = elem.ElementSchemaType as XmlSchemaComplexType;
				}
				else
				{
					complexType = at.wrappingType;
					qname = complexType.QualifiedName;
				}
				if (complexType != null)
				{
					if ((complexType.GetContentType() != XmlSchemaContentType.TextOnly ? true : !complexType.IsDerivedByRestriction()))
					{
                        ClrContentTypeInfo clrContentTypeInfo = new ClrContentTypeInfo();
						this.localSymbolTable.Init(at.identifier);
						clrContentTypeInfo.clrtypeName = at.identifier;
						clrContentTypeInfo.clrtypeNs = this.configSettings.GetClrNamespace(qname.Namespace);
						clrContentTypeInfo.schemaName = qname.Name;
						clrContentTypeInfo.schemaNs = qname.Namespace;
						clrContentTypeInfo.typeOrigin = SchemaOrigin.Fragment;
						clrContentTypeInfo.IsNested = true;
						clrContentTypeInfo.baseType = this.BaseType(complexType);
						this.BuildProperties(elem, complexType, clrContentTypeInfo);
						this.BuildNestedTypes(clrContentTypeInfo);
						this.BuildAnnotationInformation(clrContentTypeInfo, complexType);
						typeInfo.NestedTypes.Add(clrContentTypeInfo);
					}
					else
					{
						clrSimpleTypeInfo = ClrSimpleTypeInfo.CreateSimpleTypeInfo(complexType);
						clrSimpleTypeInfo.clrtypeName = at.identifier;
						clrSimpleTypeInfo.clrtypeNs = this.configSettings.GetClrNamespace(qname.Namespace);
						clrSimpleTypeInfo.schemaName = qname.Name;
						clrSimpleTypeInfo.schemaNs = qname.Namespace;
						clrSimpleTypeInfo.typeOrigin = SchemaOrigin.Fragment;
						clrSimpleTypeInfo.IsNested = true;
						this.BuildAnnotationInformation(clrSimpleTypeInfo, complexType);
						typeInfo.NestedTypes.Add(clrSimpleTypeInfo);
					}
				}
				XmlSchemaSimpleType simpleType = null;
				if (elem != null)
				{
					simpleType = elem.ElementSchemaType as XmlSchemaSimpleType;
				}
				if (simpleType != null)
				{
					clrSimpleTypeInfo = ClrSimpleTypeInfo.CreateSimpleTypeInfo(simpleType);
					clrSimpleTypeInfo.clrtypeName = at.identifier;
					clrSimpleTypeInfo.clrtypeNs = this.configSettings.GetClrNamespace(qname.Namespace);
					clrSimpleTypeInfo.schemaName = qname.Name;
					clrSimpleTypeInfo.schemaNs = qname.Namespace;
					clrSimpleTypeInfo.typeOrigin = SchemaOrigin.Fragment;
					clrSimpleTypeInfo.IsNested = true;
					this.BuildAnnotationInformation(clrSimpleTypeInfo, simpleType);
					typeInfo.NestedTypes.Add(clrSimpleTypeInfo);
				}
			}
		}

		private void BuildProperties(XmlSchemaElement parentElement, XmlSchemaType schemaType, ClrContentTypeInfo typeInfo)
		{
			XmlSchemaComplexType ct = schemaType as XmlSchemaComplexType;
			if (ct == null)
			{
				typeInfo.AddMember(this.BuildSimpleTypeTextProperty(parentElement, schemaType as XmlSchemaSimpleType));
			}
			else if (ct.TypeCode != XmlTypeCode.Item)
			{
				XmlSchemaParticle particleToProperties = ct.ContentTypeParticle;
				XmlSchemaComplexType baseType = ct.BaseXmlSchemaType as XmlSchemaComplexType;
				if (schemaType.GetContentType() != XmlSchemaContentType.TextOnly)
				{
					Debug.Assert(baseType != null);
					if (ct.IsDerivedByRestriction())
					{
						return;
					}
					if (particleToProperties.GetParticleType() != ParticleType.Empty)
					{
						this.TraverseParticle(particleToProperties, baseType, typeInfo, ct.DerivedBy);
					}
					this.TraverseAttributes(ct.AttributeUses, baseType.AttributeUses, typeInfo);
				}
				else
				{
					ClrPropertyInfo property = this.BuildComplexTypeTextProperty(parentElement, ct);
					if (property != null)
					{
						typeInfo.AddMember(property);
					}
					if (baseType == null)
					{
						this.TraverseAttributes(ct.AttributeUses, typeInfo);
					}
					else if (!ct.IsDerivedByRestriction())
					{
						this.TraverseAttributes(ct.AttributeUses, baseType.AttributeUses, typeInfo);
					}
				}
			}
		}

		private ClrPropertyInfo BuildProperty(XmlSchemaElement elem, bool fromBaseType)
		{
			string identifierName = this.localSymbolTable.AddLocalElement(elem);
			XmlSchemaType schemaType = elem.ElementSchemaType;
			XmlQualifiedName schemaTypeName = schemaType.QualifiedName;
			string schemaName = elem.QualifiedName.Name;
			string schemaNs = elem.QualifiedName.Namespace;
			string clrNs = (elem.FormResolved() == XmlSchemaForm.Qualified ? this.configSettings.GetClrNamespace(schemaNs) : string.Empty);
			SchemaOrigin typeRefOrigin = SchemaOrigin.Fragment;
			bool isTypeRef = false;
			bool anonymousType = schemaTypeName.IsEmpty;
			XmlSchemaObject schemaObject = schemaType;
			ArrayList substitutionMembers = null;
			if (elem.IsGlobal())
			{
				substitutionMembers = this.IsSubstitutionGroupHead(elem);
				schemaTypeName = elem.QualifiedName;
				isTypeRef = true;
				typeRefOrigin = SchemaOrigin.Element;
				schemaObject = this.schemas.GlobalElements[schemaTypeName];
				anonymousType = false;
			}
			ClrTypeReference typeRef = this.BuildTypeReference(schemaObject, schemaTypeName, anonymousType, true);
			typeRef.Origin = typeRefOrigin;
			typeRef.IsTypeRef = isTypeRef;
			if ((!anonymousType ? false : !fromBaseType))
			{
				this.localSymbolTable.AddAnonymousType(identifierName, elem, typeRef);
			}
			ClrPropertyInfo propertyInfo = new ClrPropertyInfo(identifierName, schemaNs, schemaName, this.GetOccurence(elem))
			{
				Origin = SchemaOrigin.Element,
				FromBaseType = fromBaseType,
				TypeReference = typeRef
			};
			if (substitutionMembers != null)
			{
				propertyInfo.SubstitutionMembers = substitutionMembers;
			}
			return propertyInfo;
		}

		private ClrPropertyInfo BuildProperty(XmlSchemaAttribute attribute, bool fromBaseType, bool isNew)
		{
			string schemaName = attribute.QualifiedName.Name;
			string schemaNs = attribute.QualifiedName.Namespace;
			string propertyName = this.localSymbolTable.AddAttribute(attribute);
			ClrPropertyInfo propertyInfo = new ClrPropertyInfo(propertyName, schemaNs, schemaName, (attribute.Use == XmlSchemaUse.Required ? Occurs.One : Occurs.ZeroOrOne))
			{
				Origin = SchemaOrigin.Attribute,
				FromBaseType = fromBaseType,
				IsNew = isNew,
				VerifyRequired = this.configSettings.VerifyRequired
			};
			XmlSchemaSimpleType schemaType = attribute.AttributeSchemaType;
			XmlQualifiedName qName = schemaType.QualifiedName;
			if (qName.IsEmpty)
			{
				qName = attribute.QualifiedName;
			}
			propertyInfo.TypeReference = this.BuildTypeReference(schemaType, qName, false, true);
			Debug.Assert(schemaType.Datatype != null);
			this.SetFixedDefaultValue(attribute, propertyInfo);
			return propertyInfo;
		}

		private ClrPropertyInfo BuildSimpleTypeTextProperty(XmlSchemaElement parentElement, XmlSchemaSimpleType schemaType)
		{
			Debug.Assert(schemaType != null);
			ClrPropertyInfo textProperty = new ClrPropertyInfo("TypedValue", string.Empty, "TypedValue", Occurs.One)
			{
				Origin = SchemaOrigin.Text
			};
			bool anonymous = schemaType.QualifiedName.IsEmpty;
			ClrTypeReference typeRef = this.BuildTypeReference(schemaType, schemaType.QualifiedName, anonymous, true);
			textProperty.TypeReference = typeRef;
			if ((!anonymous ? false : parentElement != null))
			{
				string idenfitier = this.localSymbolTable.AddLocalElement(parentElement);
				this.localSymbolTable.AddAnonymousType(idenfitier, parentElement, typeRef);
			}
			return textProperty;
		}

		internal void BuildSubstitutionGroups()
		{
			foreach (XmlSchemaElement element in this.schemas.GlobalElements.Values)
			{
				if (!element.SubstitutionGroup.IsEmpty)
				{
					this.WalkSubstitutionGroup(element, element);
				}
			}
		}

		private ClrTypeReference BuildTypeReference(XmlSchemaObject schemaObject, XmlQualifiedName typeQName, bool anonymousType, bool setVariety)
		{
			string typeName = typeQName.Name;
			string typeNs = typeQName.Namespace;
			if (!anonymousType)
			{
				typeNs = this.configSettings.GetClrNamespace(typeNs);
			}
			return new ClrTypeReference(typeName, typeNs, schemaObject, anonymousType, setVariety);
		}

		private bool CheckUnhandledAttributes(XmlSchemaAnnotated annotated)
		{
			bool flag;
			if (annotated.UnhandledAttributes != null)
			{
				XmlAttribute[] unhandledAttributes = annotated.UnhandledAttributes;
				int num = 0;
				while (num < (int)unhandledAttributes.Length)
				{
					XmlAttribute att = unhandledAttributes[num];
					if ((att.LocalName != "root" ? true : !(att.NamespaceURI == "http://www.microsoft.com/xml/schema/linq")))
					{
						num++;
					}
					else
					{
						flag = true;
						return flag;
					}
				}
			}
			flag = false;
			return flag;
		}

		private bool ElementExists(XmlQualifiedName name)
		{
			return this.schemas.GlobalElements[name] != null;
		}

		internal void ElementsToTypes()
		{
			bool isRoot = false;
			int rootElementsCount = this.schemas.GlobalElements.Count;
			foreach (XmlSchemaElement elem in this.schemas.GlobalElements.Values)
			{
				SymbolEntry symbol = this.symbolTable.AddElement(elem);
				XmlSchemaType schemaType = elem.ElementSchemaType;
				string xsdNamespace = elem.QualifiedName.Namespace;
				ClrTypeInfo typeInfo = null;
				XmlSchemaElement headElement = null;
				if (!elem.SubstitutionGroup.IsEmpty)
				{
					headElement = (XmlSchemaElement)this.schemas.GlobalElements[elem.SubstitutionGroup];
				}
				if (!schemaType.IsGlobal())
				{
					ClrContentTypeInfo ctypeInfo = new ClrContentTypeInfo();
					this.localSymbolTable.Init(symbol.identifierName);
					ctypeInfo.baseType = headElement;
					this.BuildProperties(elem, schemaType, ctypeInfo);
					this.BuildNestedTypes(ctypeInfo);
					typeInfo = ctypeInfo;
				}
				else
				{
					ClrWrapperTypeInfo wtypeInfo = new ClrWrapperTypeInfo((headElement == null ? false : headElement.ElementSchemaType == schemaType));
					ClrTypeReference typeDef = this.BuildTypeReference(schemaType, schemaType.QualifiedName, false, true);
					wtypeInfo.InnerType = typeDef;
					typeInfo = wtypeInfo;
					typeInfo.baseType = headElement;
				}
				if (!isRoot)
				{
					if ((rootElementsCount == 1 ? true : this.CheckUnhandledAttributes(elem)))
					{
						typeInfo.IsRoot = true;
						isRoot = true;
					}
				}
				typeInfo.IsSubstitutionHead = this.IsSubstitutionGroupHead(elem) != null;
				typeInfo.IsAbstract = elem.IsAbstract;
				typeInfo.clrtypeName = symbol.identifierName;
				typeInfo.clrtypeNs = symbol.clrNamespace;
				typeInfo.schemaName = symbol.symbolName;
				typeInfo.schemaNs = xsdNamespace;
				typeInfo.typeOrigin = SchemaOrigin.Element;
				this.BuildAnnotationInformation(typeInfo, schemaType);
				this.binding.Types.Add(typeInfo);
			}
		}

		public ClrMappingInfo GenerateMapping(XmlSchemaSet schemas)
		{
			ClrMappingInfo clrMappingInfo;
			if (schemas == null)
			{
				throw new ArgumentNullException("schemas");
			}
			schemas.ValidationEventHandler += new ValidationEventHandler(this.Validationcallback);
			schemas.Compile();
			this.schemas = schemas;
			if (this.schemaErrorCount <= 0)
			{
				try
				{
					FxtLinq2XsdInterpreter.Run(schemas, this.configSettings.trafo);
				}
				catch (FxtException fxtException)
				{
					Console.WriteLine("Schema cannot be transformed. Class generation aborted");
					clrMappingInfo = null;
					return clrMappingInfo;
				}
				clrMappingInfo = this.GenerateMetaModel();
			}
			else
			{
				Console.WriteLine("Schema cannot be compiled. Class generation aborted");
				clrMappingInfo = null;
			}
			return clrMappingInfo;
		}

		private ClrMappingInfo GenerateMetaModel()
		{
			this.BuildSubstitutionGroups();
			this.ElementsToTypes();
			this.AttributesToTypes();
			this.TypesToTypes();
			this.binding.NameMappings = this.symbolTable.schemaNameToIdentifiers;
			return this.binding;
		}

		private Occurs GetOccurence(XmlSchemaParticle particle)
		{
			Occurs occur;
			if (particle.MinOccurs == new decimal(1))
			{
				occur = (!(particle.MaxOccurs == new decimal(1)) ? Occurs.OneOrMore : Occurs.One);
			}
			else if (!(particle.MinOccurs == new decimal(0)))
			{
				Debug.Assert(particle.MinOccurs > new decimal(1));
				occur = Occurs.OneOrMore;
			}
			else
			{
				occur = (!(particle.MaxOccurs == new decimal(1)) ? Occurs.ZeroOrMore : Occurs.ZeroOrOne);
			}
			return occur;
		}

		private ArrayList IsSubstitutionGroupHead(XmlSchemaElement element)
		{
			ArrayList memberList;
			XmlQualifiedName elementName = element.QualifiedName;
			this.substitutionGroups.TryGetValue(elementName, out memberList);
			return memberList;
		}

		private void SetFixedDefaultValue(XmlSchemaAttribute attribute, ClrPropertyInfo propertyInfo)
		{
			if ((attribute.RefName == null ? true : attribute.RefName.IsEmpty))
			{
				propertyInfo.FixedValue = attribute.FixedValue;
				propertyInfo.DefaultValue = attribute.DefaultValue;
			}
			else
			{
				XmlSchemaAttribute globalAtt = (XmlSchemaAttribute)this.schemas.GlobalAttributes[attribute.RefName];
				propertyInfo.FixedValue = globalAtt.FixedValue;
				propertyInfo.DefaultValue = globalAtt.DefaultValue;
			}
			if (attribute.AttributeSchemaType.DerivedBy == XmlSchemaDerivationMethod.Union)
			{
				string value = propertyInfo.FixedValue;
				if (value == null)
				{
					value = propertyInfo.DefaultValue;
				}
				if (value != null)
				{
					propertyInfo.unionDefaultType = attribute.AttributeSchemaType.Datatype.ParseValue(value, new NameTable(), null).GetType();
				}
			}
		}

		private void SetPropertyFlags(ClrPropertyInfo propertyInfo, GroupingInfo currentGroupingInfo, XmlSchemaType propertyType)
		{
			ClrPropertyInfo isNullable = propertyInfo;
			isNullable.IsNullable = isNullable.IsNullable | (currentGroupingInfo.ContentModelType == ContentModelType.Choice ? true : currentGroupingInfo.IsOptional);
			propertyInfo.VerifyRequired = this.configSettings.VerifyRequired;
			if (currentGroupingInfo.IsRepeating)
			{
				propertyInfo.IsList = true;
			}
			string propertyName = propertyInfo.PropertyName;
			ClrPropertyInfo prevProperty = null;
			if (!this.propertyNameTypeTable.TryGetValue(propertyName, out prevProperty))
			{
				this.propertyNameTypeTable.Add(propertyName, propertyInfo);
			}
			else
			{
				currentGroupingInfo.HasRecurrentElements = true;
				propertyInfo.IsDuplicate = true;
				prevProperty.IsList = true;
			}
		}

		private void TraverseAttributes(XmlSchemaObjectTable derivedAttributes, ClrContentTypeInfo typeInfo)
		{
			foreach (XmlSchemaAttribute derivedAttribute in derivedAttributes.Values)
			{
				Debug.Assert(derivedAttribute.AttributeSchemaType != null);
				ClrBasePropertyInfo propertyInfo = this.BuildProperty(derivedAttribute, false, false);
				this.BuildAnnotationInformation(propertyInfo, derivedAttribute, false, false);
				typeInfo.AddMember(propertyInfo);
			}
		}

		private void TraverseAttributes(XmlSchemaObjectTable derivedAttributes, XmlSchemaObjectTable baseAttributes, ClrContentTypeInfo typeInfo)
		{
			ClrBasePropertyInfo propertyInfo;
			foreach (XmlSchemaAttribute derivedAttribute in derivedAttributes.Values)
			{
				if (derivedAttribute.Use != XmlSchemaUse.Prohibited)
				{
					XmlSchemaAttribute baseAttribute = baseAttributes[derivedAttribute.QualifiedName] as XmlSchemaAttribute;
					if ((baseAttribute == null ? true : baseAttribute != derivedAttribute))
					{
						propertyInfo = this.BuildProperty(derivedAttribute, false, baseAttribute != null);
						this.BuildAnnotationInformation(propertyInfo, derivedAttribute, false, false);
						typeInfo.AddMember(propertyInfo);
					}
					else
					{
						propertyInfo = this.BuildProperty(derivedAttribute, typeInfo.IsDerived, false);
						this.BuildAnnotationInformation(propertyInfo, derivedAttribute, false, false);
						typeInfo.AddMember(propertyInfo);
					}
				}
			}
		}

		private void TraverseParticle(XmlSchemaParticle particle, XmlSchemaComplexType baseType, ClrContentTypeInfo typeInfo, XmlSchemaDerivationMethod derivationMethod)
		{
			if (this.particleStack != null)
			{
				this.particleStack.Clear();
			}
			else
			{
				this.particleStack = new Stack<XsdToTypesConverter.ParticleData>();
			}
			if (this.propertyNameTypeTable != null)
			{
				this.propertyNameTypeTable.Clear();
			}
			else
			{
				this.propertyNameTypeTable = new Dictionary<string, ClrPropertyInfo>();
			}
			XmlSchemaParticle baseParticle = baseType.ContentTypeParticle;
			GroupingInfo parentGroupInfo = null;
			StringBuilder regEx = new StringBuilder();
			XmlSchemaGroupBase currentGroupBase = null;
			GroupingInfo currentGroupingInfo = null;
			int currentIndex = 0;
			while (true)
			{
				if ((currentGroupBase == null ? true : currentIndex <= currentGroupBase.Items.Count))
				{
					ParticleType particleType = particle.GetParticleType();
					switch (particleType)
					{
						case ParticleType.Sequence:
						case ParticleType.Choice:
						case ParticleType.All:
						{
							regEx.Append("(");
							if (currentGroupBase != null)
							{
								this.particleStack.Push(new XsdToTypesConverter.ParticleData(currentGroupBase, currentGroupingInfo, currentIndex));
								currentIndex = 0;
							}
							parentGroupInfo = currentGroupingInfo;
							currentGroupBase = particle as XmlSchemaGroupBase;
							Debug.Assert(currentGroupBase != null);
							currentGroupingInfo = new GroupingInfo((ContentModelType)particleType, this.GetOccurence(currentGroupBase));
							if (parentGroupInfo != null)
							{
								parentGroupInfo.AddChild(currentGroupingInfo);
								parentGroupInfo.HasChildGroups = true;
								currentGroupingInfo.IsNested = true;
								if (parentGroupInfo.IsRepeating)
								{
									currentGroupingInfo.IsRepeating = true;
								}
								if (currentGroupingInfo.IsRepeating)
								{
									parentGroupInfo.HasRepeatingGroups = true;
								}
							}
							else
							{
								typeInfo.AddMember(currentGroupingInfo);
								parentGroupInfo = currentGroupingInfo;
							}
							break;
						}
						case ParticleType.Element:
						{
							XmlSchemaElement elem = particle as XmlSchemaElement;
							ClrPropertyInfo propertyInfo = null;
							bool fromBaseType = false;
							if ((derivationMethod != XmlSchemaDerivationMethod.Extension ? false : typeInfo.IsDerived))
							{
								if (baseParticle.ContainsElement(elem))
								{
									fromBaseType = true;
								}
								else if ((typeInfo.InlineBaseType ? false : baseType.ContainsName(elem.QualifiedName)))
								{
									typeInfo.InlineBaseType = true;
								}
							}
							propertyInfo = this.BuildProperty(elem, fromBaseType);
							regEx.Append(propertyInfo.PropertyName);
							this.AppendOccurenceToRegex(propertyInfo, regEx);
							if (currentGroupingInfo != null)
							{
								this.BuildAnnotationInformation(propertyInfo, elem, currentGroupingInfo.ContentModelType == ContentModelType.Choice, currentGroupingInfo.IsNested);
								currentGroupingInfo.AddChild(propertyInfo);
								this.SetPropertyFlags(propertyInfo, currentGroupingInfo, elem.ElementSchemaType);
							}
							else
							{
								this.BuildAnnotationInformation(propertyInfo, elem, false, false);
								typeInfo.AddMember(propertyInfo);
							}
							break;
						}
						case ParticleType.Any:
						{
							regEx.Append("any");
							XmlSchemaAny any = particle as XmlSchemaAny;
							if ((derivationMethod != XmlSchemaDerivationMethod.Extension ? false : typeInfo.IsDerived))
							{
								if (baseParticle.ContainsWildCard(any))
								{
									typeInfo.HasElementWildCard = true;
								}
							}
							ClrWildCardPropertyInfo wcPropertyInfo = this.BuildAnyProperty(any, !typeInfo.HasElementWildCard);
							if (currentGroupingInfo != null)
							{
								currentGroupingInfo.AddChild(wcPropertyInfo);
							}
							else
							{
								typeInfo.AddMember(wcPropertyInfo);
							}
							if (!typeInfo.HasElementWildCard)
							{
								typeInfo.HasElementWildCard = true;
							}
							break;
						}
					}
				}
				if ((currentGroupBase == null ? true : currentIndex >= currentGroupBase.Items.Count))
				{
					if (currentGroupBase != null)
					{
						regEx.Append(")");
						this.AppendOccurenceToRegex(currentGroupingInfo, regEx);
					}
					if (this.particleStack.Count <= 0)
					{
						break;
					}
					bool childGroupHasRecurringElements = currentGroupingInfo.HasRecurrentElements;
					bool childGroupHasRepeatingGroups = currentGroupingInfo.HasRepeatingGroups;
					XsdToTypesConverter.ParticleData particleData = this.particleStack.Pop();
					currentGroupBase = particleData.currentGroupBase;
					currentGroupingInfo = particleData.currentGroupingInfo;
					currentGroupingInfo.HasRecurrentElements = childGroupHasRecurringElements;
					currentGroupingInfo.HasRepeatingGroups = childGroupHasRepeatingGroups;
					currentIndex = particleData.currentIndex;
					if (currentIndex >= currentGroupBase.Items.Count)
					{
						currentIndex++;
					}
					else
					{
						int num = currentIndex;
						currentIndex = num + 1;
						particle = (XmlSchemaParticle)currentGroupBase.Items[num];
						regEx.Append((currentGroupingInfo.ContentModelType == ContentModelType.Choice ? "|" : ", "));
					}
				}
				else
				{
					if (currentIndex > 0)
					{
						regEx.Append((currentGroupingInfo.ContentModelType == ContentModelType.Choice ? " | " : ", "));
					}
					int num1 = currentIndex;
					currentIndex = num1 + 1;
					particle = (XmlSchemaParticle)currentGroupBase.Items[num1];
				}
			}
			if (regEx.Length != 0)
			{
				typeInfo.ContentModelRegEx = regEx.ToString();
			}
		}

		internal void TypesToTypes()
		{
			foreach (XmlSchemaType st in this.schemas.GlobalTypes.Values)
			{
				this.TypeToType(st);
			}
		}

		internal void TypeToType(XmlSchemaType st)
		{
			XmlSchemaSimpleType simpleType = st as XmlSchemaSimpleType;
			if (simpleType == null)
			{
				XmlSchemaComplexType ct = st as XmlSchemaComplexType;
				if ((ct == null ? false : ct.TypeCode != XmlTypeCode.Item))
				{
					SymbolEntry symbol = this.symbolTable.AddType(ct.QualifiedName, ct);
					string xsdNamespace = ct.QualifiedName.Namespace;
					this.localSymbolTable.Init(symbol.identifierName);
					ClrContentTypeInfo typeInfo = new ClrContentTypeInfo()
					{
						IsAbstract = ct.IsAbstract,
						IsSealed = ct.IsFinal(),
						clrtypeName = symbol.identifierName,
						clrtypeNs = symbol.clrNamespace,
						schemaName = symbol.symbolName,
						schemaNs = xsdNamespace,
						typeOrigin = SchemaOrigin.Fragment,
						baseType = this.BaseType(ct)
					};
					this.BuildProperties(null, ct, typeInfo);
					this.BuildNestedTypes(typeInfo);
					this.BuildAnnotationInformation(typeInfo, ct);
					this.binding.Types.Add(typeInfo);
				}
			}
			else
			{
				this.AddSimpleType(simpleType.QualifiedName, simpleType);
			}
		}

		private void Validationcallback(object sender, ValidationEventArgs args)
		{
			if (args.Severity == XmlSeverityType.Error)
			{
				this.schemaErrorCount++;
			}
			Console.WriteLine(string.Concat("Exception Severity: ", args.Severity));
			Console.WriteLine(args.Message);
		}

		private void WalkSubstitutionGroup(XmlSchemaElement element, XmlSchemaElement leafElement)
		{
			XmlQualifiedName subsName = element.SubstitutionGroup;
			XmlSchemaElement head = this.schemas.GlobalElements[subsName] as XmlSchemaElement;
			if ((head.Block & XmlSchemaDerivationMethod.Substitution) == XmlSchemaDerivationMethod.Empty)
			{
				ArrayList groupMembers = null;
				if (!this.substitutionGroups.TryGetValue(subsName, out groupMembers))
				{
					groupMembers = new ArrayList();
					groupMembers.Add(head);
					this.substitutionGroups.Add(subsName, groupMembers);
				}
				groupMembers.Add(leafElement);
			}
			if (!head.SubstitutionGroup.IsEmpty)
			{
				this.WalkSubstitutionGroup(head, leafElement);
			}
		}

		private struct ParticleData
		{
			internal XmlSchemaGroupBase currentGroupBase;

			internal GroupingInfo currentGroupingInfo;

			internal int currentIndex;

			public ParticleData(XmlSchemaGroupBase groupBase, GroupingInfo gInfo, int index)
			{
				this.currentGroupBase = groupBase;
				this.currentGroupingInfo = gInfo;
				this.currentIndex = index;
			}
		}
	}
}
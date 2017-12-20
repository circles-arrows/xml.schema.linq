using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Xml.Fxt
{
    public static class XmlSchemaQueryExtensions
    {
        public static IEnumerable<XmlSchemaElement> AllXsdElements(this XmlSchemaSet set)
        {
            IEnumerable<XmlSchemaElement> xmlSchemaElements = set.GlobalXsdElements().Union<XmlSchemaElement>(set.LocalXsdElements());
            return xmlSchemaElements;
        }

        public static IEnumerable<XmlSchemaType> AllXsdTypes(this XmlSchemaSet set)
        {
            IEnumerable<XmlSchemaType> xmlSchemaTypes = set.GlobalXsdTypes().Union<XmlSchemaType>(set.AnonymousXsdTypes());
            return xmlSchemaTypes;
        }

        public static IEnumerable<XmlSchemaType> AnonymousXsdTypes(this XmlSchemaSet set)
        {
            IEnumerable<XmlSchemaType> xmlSchemaTypes =
                from gel in set.AllXsdElements()
                where gel.SchemaType != null
                select gel.SchemaType;
            return xmlSchemaTypes;
        }

        public static bool DefinesXsdAttribute(this XmlSchemaSet schemas, XmlQualifiedName name)
        {
            bool flag = schemas.GlobalXsdAttributes().QNames().Contains<XmlQualifiedName>(name);
            return flag;
        }

        public static bool DefinesXsdElement(this XmlSchemaSet schemas, XmlQualifiedName name)
        {
            bool flag = schemas.GlobalXsdElements().QNames().Contains<XmlQualifiedName>(name);
            return flag;
        }

        public static bool DefinesXsdType(this XmlSchemaSet schemas, XmlQualifiedName name)
        {
            bool flag = schemas.GlobalXsdTypes().QNames().Contains<XmlQualifiedName>(name);
            return flag;
        }

        public static IEnumerable<XmlSchemaAttribute> GlobalXsdAttributes(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.XmlSchemas())
            {
                foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchema.GlobalXsdAttributes())
                {
                    yield return xmlSchemaAttribute;
                }
            }
        }

        public static IEnumerable<XmlSchemaAttribute> GlobalXsdAttributes(this XmlSchema schema)
        {
            foreach (XmlSchemaAttribute value in schema.Attributes.Values)
            {
                yield return value;
            }
        }

        public static IEnumerable<XmlSchemaElement> GlobalXsdElements(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.XmlSchemas())
            {
                foreach (XmlSchemaElement xmlSchemaElement in xmlSchema.GlobalXsdElements())
                {
                    yield return xmlSchemaElement;
                }
            }
        }

        public static IEnumerable<XmlSchemaElement> GlobalXsdElements(this XmlSchema schema)
        {
            foreach (XmlSchemaElement value in schema.Elements.Values)
            {
                yield return value;
            }
        }

        public static IEnumerable<XmlSchemaType> GlobalXsdTypes(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.XmlSchemas())
            {
                foreach (XmlSchemaType xmlSchemaType in xmlSchema.GlobalXsdTypes())
                {
                    yield return xmlSchemaType;
                }
            }
        }

        public static IEnumerable<XmlSchemaType> GlobalXsdTypes(this XmlSchema schema)
        {
            foreach (XmlSchemaType value in schema.SchemaTypes.Values)
            {
                yield return value;
            }
        }

        public static bool IsDerivedComplexType(this XmlSchemaType ty)
        {
            bool flag;
            XmlSchemaComplexType ct = ty as XmlSchemaComplexType;
            flag = (ct == null ? false : ct.ContentModel is XmlSchemaComplexContent);
            return flag;
        }

        public static bool IsGlobal(this XmlSchemaElement el)
        {
            return el.Parent is XmlSchema;
        }

        public static bool IsLocal(this XmlSchemaElement el)
        {
            return !el.IsGlobal();
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.XmlSchemas())
            {
                foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchema.LocalXsdAttributes())
                {
                    yield return xmlSchemaAttribute;
                }
            }
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchema schema)
        {
            IEnumerator enumerator = schema.Elements.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XmlSchemaElement current = (XmlSchemaElement)enumerator.Current;
                foreach (XmlSchemaAttribute iteratorVariable1 in current.LocalXsdAttributes())
                {
                    yield return iteratorVariable1;
                }
            }
            IEnumerator iteratorVariable9 = schema.SchemaTypes.Values.GetEnumerator();
            while (iteratorVariable9.MoveNext())
            {
                XmlSchemaType ty = (XmlSchemaType)iteratorVariable9.Current;
                foreach (XmlSchemaAttribute iteratorVariable3 in ty.LocalXsdAttributes())
                {
                    yield return iteratorVariable3;
                }
            }
            IEnumerator iteratorVariable12 = schema.Groups.Values.GetEnumerator();
            while (iteratorVariable12.MoveNext())
            {
                XmlSchemaGroup gr = (XmlSchemaGroup)iteratorVariable12.Current;
                foreach (XmlSchemaAttribute iteratorVariable5 in gr.LocalXsdAttributes())
                {
                    yield return iteratorVariable5;
                }
            }
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchemaElement el)
        {
            return el.SchemaType.LocalXsdAttributes();
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchemaType ty)
        {
            XmlSchemaComplexType xmlSchemaComplexType = ty as XmlSchemaComplexType;
            if (xmlSchemaComplexType != null)
            {
                foreach (XmlSchemaAttribute attribute in xmlSchemaComplexType.Attributes)
                {
                    yield return attribute;
                }
                if (xmlSchemaComplexType.ContentModel != null)
                {
                    XmlSchemaComplexContent contentModel = xmlSchemaComplexType.ContentModel as XmlSchemaComplexContent;
                    if (contentModel != null)
                    {
                        XmlSchemaComplexContentExtension content = contentModel.Content as XmlSchemaComplexContentExtension;
                        if (content == null)
                        {
                            goto Label0;
                        }
                        foreach (XmlSchemaAttribute xmlSchemaAttribute in content.Attributes)
                        {
                            yield return xmlSchemaAttribute;
                        }
                        foreach (XmlSchemaAttribute xmlSchemaAttribute1 in content.Particle.LocalXsdAttributes())
                        {
                            yield return xmlSchemaAttribute1;
                        }
                    }
                    else
                    {
                        goto Label0;
                    }
                }
                else
                {
                    foreach (XmlSchemaAttribute xmlSchemaAttribute2 in xmlSchemaComplexType.Particle.LocalXsdAttributes())
                    {
                        yield return xmlSchemaAttribute2;
                    }
                }
            }
            Label0:
            yield break;
            goto Label0;
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchemaGroup gr)
        {
            return gr.Particle.LocalXsdAttributes();
        }

        public static IEnumerable<XmlSchemaAttribute> LocalXsdAttributes(this XmlSchemaParticle pa)
        {
            if (pa != null)
            {
                XmlSchemaGroupBase xmlSchemaGroupBase = pa as XmlSchemaGroupBase;
                if (xmlSchemaGroupBase == null)
                {
                    XmlSchemaElement xmlSchemaElement = pa as XmlSchemaElement;
                    if (xmlSchemaElement != null)
                    {
                        foreach (XmlSchemaAttribute xmlSchemaAttribute in xmlSchemaElement.SchemaType.LocalXsdAttributes())
                        {
                            yield return xmlSchemaAttribute;
                        }
                    }
                }
                else
                {
                    foreach (XmlSchemaParticle item in xmlSchemaGroupBase.Items)
                    {
                        foreach (XmlSchemaAttribute xmlSchemaAttribute1 in item.LocalXsdAttributes())
                        {
                            yield return xmlSchemaAttribute1;
                        }
                    }
                }
            }
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.XmlSchemas())
            {
                foreach (XmlSchemaElement xmlSchemaElement in xmlSchema.LocalXsdElements())
                {
                    yield return xmlSchemaElement;
                }
            }
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchema schema)
        {
            IEnumerator enumerator = schema.Elements.Values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XmlSchemaElement current = (XmlSchemaElement)enumerator.Current;
                foreach (XmlSchemaElement iteratorVariable1 in current.LocalXsdElements())
                {
                    yield return iteratorVariable1;
                }
            }
            IEnumerator iteratorVariable9 = schema.SchemaTypes.Values.GetEnumerator();
            while (iteratorVariable9.MoveNext())
            {
                XmlSchemaType ty = (XmlSchemaType)iteratorVariable9.Current;
                foreach (XmlSchemaElement iteratorVariable3 in ty.LocalXsdElements())
                {
                    yield return iteratorVariable3;
                }
            }
            IEnumerator iteratorVariable12 = schema.Groups.Values.GetEnumerator();
            while (iteratorVariable12.MoveNext())
            {
                XmlSchemaGroup gr = (XmlSchemaGroup)iteratorVariable12.Current;
                foreach (XmlSchemaElement iteratorVariable5 in gr.LocalXsdElements())
                {
                    yield return iteratorVariable5;
                }
            }
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchemaElement el)
        {
            return el.SchemaType.LocalXsdElements();
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchemaType ty)
        {
            XmlSchemaComplexType iteratorVariable0 = ty as XmlSchemaComplexType;
            if (iteratorVariable0 != null)
            {
                if (iteratorVariable0.ContentModel != null)
                {
                    XmlSchemaComplexContent contentModel = iteratorVariable0.ContentModel as XmlSchemaComplexContent;
                    if (contentModel != null)
                    {
                        XmlSchemaComplexContentExtension content = contentModel.Content as XmlSchemaComplexContentExtension;
                        if (content != null)
                        {
                            foreach (XmlSchemaElement iteratorVariable4 in content.Particle.LocalXsdElements())
                            {
                                yield return iteratorVariable4;
                            }
                        }
                    }
                }
                else
                {
                    foreach (XmlSchemaElement iteratorVariable1 in iteratorVariable0.Particle.LocalXsdElements())
                    {
                        yield return iteratorVariable1;
                    }
                }
            }
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchemaGroup gr)
        {
            return gr.Particle.LocalXsdElements();
        }

        public static IEnumerable<XmlSchemaElement> LocalXsdElements(this XmlSchemaParticle pa)
        {
            if (pa != null)
            {
                XmlSchemaGroupBase xmlSchemaGroupBase = pa as XmlSchemaGroupBase;
                if (xmlSchemaGroupBase == null)
                {
                    XmlSchemaElement xmlSchemaElement = pa as XmlSchemaElement;
                    if (xmlSchemaElement != null)
                    {
                        yield return xmlSchemaElement;
                        foreach (XmlSchemaElement xmlSchemaElement1 in xmlSchemaElement.SchemaType.LocalXsdElements())
                        {
                            yield return xmlSchemaElement1;
                        }
                    }
                }
                else
                {
                    foreach (XmlSchemaParticle item in xmlSchemaGroupBase.Items)
                    {
                        foreach (XmlSchemaElement xmlSchemaElement2 in item.LocalXsdElements())
                        {
                            yield return xmlSchemaElement2;
                        }
                    }
                }
            }
        }

        public static IEnumerable<XmlQualifiedName> QNames(this IEnumerable<XmlSchemaType> types)
        {
            IEnumerable<XmlQualifiedName> xmlQualifiedNames =
                from x in types
                select x.QualifiedName;
            return xmlQualifiedNames;
        }

        public static IEnumerable<XmlQualifiedName> QNames(this IEnumerable<XmlSchemaElement> els)
        {
            IEnumerable<XmlQualifiedName> xmlQualifiedNames =
                from x in els
                select x.QualifiedName;
            return xmlQualifiedNames;
        }

        public static IEnumerable<XmlQualifiedName> QNames(this IEnumerable<XmlSchemaAttribute> els)
        {
            IEnumerable<XmlQualifiedName> xmlQualifiedNames =
                from x in els
                select x.QualifiedName;
            return xmlQualifiedNames;
        }

        public static XmlQualifiedName RootElement(this XmlSchemaSet schemas, XmlQualifiedName name)
        {
            XmlQualifiedName xmlQualifiedName;
            XmlQualifiedName bname = (schemas.GlobalElements[name] as XmlSchemaElement).SubstitutionGroup;
            xmlQualifiedName = (!bname.IsEmpty ? schemas.RootElement(bname) : name);
            return xmlQualifiedName;
        }

        public static XmlQualifiedName RootType(this XmlSchemaSet schemas, XmlQualifiedName name)
        {
            XmlQualifiedName xmlQualifiedName;
            XmlSchemaComplexType ct = schemas.GlobalTypes[name] as XmlSchemaComplexType;
            if (ct != null)
            {
                xmlQualifiedName = (ct.BaseXmlSchemaType.TypeCode != XmlTypeCode.Item ? schemas.RootType(ct.BaseXmlSchemaType.QualifiedName) : name);
            }
            else
            {
                xmlQualifiedName = null;
            }
            return xmlQualifiedName;
        }

        public static XmlSchema XmlSchema(this XmlSchemaObject o)
        {
            XmlSchema xmlSchema;
            xmlSchema = (!(o is XmlSchema) ? o.Parent.XmlSchema() : o as XmlSchema);
            return xmlSchema;
        }

        public static IEnumerable<XmlSchema> XmlSchemas(this XmlSchemaSet set)
        {
            foreach (XmlSchema xmlSchema in set.Schemas())
            {
                yield return xmlSchema;
            }
        }

        public static IEnumerable<XmlSchemaAttribute> XsdAttributesInScope(this XmlSchemaElement el)
        {
            return el.SchemaType.XsdAttributesInScope();
        }

        public static IEnumerable<XmlSchemaAttribute> XsdAttributesInScope(this XmlSchemaType ty)
        {
            if (ty != null)
            {
                XmlSchemaComplexType xmlSchemaComplexType = ty as XmlSchemaComplexType;
                if (xmlSchemaComplexType != null)
                {
                    foreach (XmlSchemaAttribute attribute in xmlSchemaComplexType.Attributes)
                    {
                        yield return attribute;
                    }
                    if (xmlSchemaComplexType.ContentModel != null)
                    {
                        XmlSchemaComplexContent contentModel = xmlSchemaComplexType.ContentModel as XmlSchemaComplexContent;
                        if (contentModel != null)
                        {
                            XmlSchemaComplexContentExtension content = contentModel.Content as XmlSchemaComplexContentExtension;
                            if (content == null)
                            {
                                goto Label0;
                            }
                            foreach (XmlSchemaAttribute xmlSchemaAttribute in content.Attributes)
                            {
                                yield return xmlSchemaAttribute;
                            }
                        }
                        else
                        {
                            goto Label0;
                        }
                    }
                }
            }
            Label0:
            yield break;
            goto Label0;
        }
    }
}
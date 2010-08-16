// Copyright (c) 2004-2010 Azavea, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Azavea.Open.Common;
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Exceptions;
using log4net;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// Represents a mapping of a class onto a database table.
    /// </summary>
    public class ClassMapping
    {
        private static readonly ILog log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// The class name that is being mapped.
        /// </summary>
        public string TypeName;
        /// <summary>
        /// The class that is being mapped.
        /// </summary>
        public Type ClassType;
        /// <summary>
        /// The constructor that takes no parameters.
        /// </summary>
        public ConstructorInfo Constructor;
        /// <summary>
        /// The table it is being mapped to.
        /// </summary>
        public string Table;

        /// <summary>
        /// The columns that comprise the ID.
        /// Key:   Class Property/Attribute
        /// Value: DB Column
        /// </summary>
        public readonly IDictionary<string, string> IdDataColsByObjAttrs = new CheckedDictionary<string, string>();
        /// <summary>
        /// All the columns that aren't part of the ID.
        /// Key:   Class Property/Attribute
        /// Value: DB Column name
        /// </summary>
        public readonly IDictionary<string, string> NonIdDataColsByObjAttrs = new CheckedDictionary<string, string>();
        /// <summary>
        /// IDCols + PropertyCols together.
        /// Key:   Class Property/Attribute Name
        /// Value: DB Column name
        /// </summary>
        public IDictionary<string, string> AllDataColsByObjAttrs = new CheckedDictionary<string, string>();

        /// <summary>
        /// The columns that comprise the ID.
        /// Key:   Class Property/Attribute MemberInfo
        /// Value: DB Column name
        /// </summary>
        public IDictionary<MemberInfo, string> IdDataColsByObjMemberInfo = new CheckedDictionary<MemberInfo, string>();
        /// <summary>
        /// All the columns that aren't part of the ID.
        /// Key:   Class Property/Attribute MemberInfo
        /// Value: DB Column name
        /// </summary>
        public IDictionary<MemberInfo, string> NonIdDataColsByObjMemberInfo = new CheckedDictionary<MemberInfo, string>();
        /// <summary>
        /// IDCols + PropertyCols together.
        /// Key:   Class Property/Attribute MemberInfo
        /// Value: DB Column name
        /// </summary>
        public IDictionary<MemberInfo, string> AllDataColsByObjMemberInfo = new CheckedDictionary<MemberInfo, string>();

        /// <summary>
        /// The DB column names in the order they appeared in the mapping file.
        /// </summary>
        public IList<string> AllDataColsInOrder = new List<string>();

        /// <summary>
        /// The MemberInfos keyed by the column name (case insensitive since column names
        /// read from the data source may or may not be in the expected case).
        /// Key:   DB Column name
        /// Value: Class Property/Attribute MemberInfo
        /// </summary>
        public IDictionary<string, MemberInfo> AllObjMemberInfosByDataCol =
            new CheckedDictionary<string, MemberInfo>(new CaseInsensitiveStringComparer());
        /// <summary>
        /// The MemberInfos keyed by the column name.
        /// Key:   Class Property/Attribute Name
        /// Value: Class Property/Attribute MemberInfo
        /// </summary>
        public IDictionary<string, MemberInfo> AllObjMemberInfosByObjAttr =
            new CheckedDictionary<string, MemberInfo>();
        /// <summary>
        /// The class attribute names keyed by the column name (case insensitive since column names
        /// read from the data source may or may not be in the expected case).
        /// Key:   DB Column name
        /// Value: Class Property/Attribute Name
        /// </summary>
        public IDictionary<string, string> AllObjAttrsByDataCol =
            new CheckedDictionary<string, string>(new CaseInsensitiveStringComparer());
        /// <summary>
        /// For each ID column, what type of generator do we use (case insensitive since column names
        /// read from the data source may or may not be in the expected case).
        /// Key:   Column name
        /// Value: Generator type.
        /// </summary>
        public IDictionary<string, GeneratorType> IdGeneratorsByDataCol =
            new CheckedDictionary<string, GeneratorType>(new CaseInsensitiveStringComparer());
        /// <summary>
        /// For each ID column, what sequence (if any) do we use.  Only populated for IDs
        /// with GeneratorType SEQUENCE.
        /// Key:   Column name (case insensitive since column names
        ///        read from the data source may or may not be in the expected case)
        /// Value: Sequence name.
        /// </summary>
        public IDictionary<string, string> IdSequencesByDataCol =
            new CheckedDictionary<string, string>(new CaseInsensitiveStringComparer());
        /// <summary>
        /// Column types, if specified in the mapping file.
        /// Key:   Class Property/Attribute Name
        /// Value: Type string from the mapping file, or null if none was specified.
        /// </summary>
        public IDictionary<string, Type> DataColTypesByObjAttr =
            new CheckedDictionary<string, Type>();
        /// <summary>
        /// Column types, if specified in the mapping file.
        /// Key:   Column name (case insensitive since column names
        ///        read from the data source may or may not be in the expected case)
        /// Value: Type string from the mapping file, or null if none was specified.
        /// </summary>
        public IDictionary<string, Type> DataColTypesByDataCol =
            new CheckedDictionary<string, Type>(new CaseInsensitiveStringComparer());

        /// <summary>
        /// Construct it from an NHibernate config's "class" node.  Will not include
        /// foreign keys by default.
        /// </summary>
        /// <param name="hibConfNode">The XML configuration.</param>
        public ClassMapping(XmlNode hibConfNode) : this(hibConfNode, true) { }
        /// <summary>
        /// Construct it from an NHibernate config's "class" node.
        /// This allows you to skip using reflection to populate the class info,
        /// if for example your Dao doesn't use it for some reason.
        /// Will not include foreign keys by default.
        /// </summary>
        /// <param name="hibConfNode">The XML configuration.</param>
        /// <param name="reflect">Whether or not to use reflection to populate all the class info.</param>
        public ClassMapping(XmlNode hibConfNode, bool reflect)
        {
            ParseNHibernateXML(hibConfNode, false);
            if (reflect)
            {
                ReflectOnTypeInfo();
            }
        }

        /// <summary>
        /// Construct it from an NHibernate config's "class" node.
        /// This allows you to skip using reflection to populate the class info,
        /// if for example your Dao doesn't use it for some reason.
        /// This also allows you to specify whether to include many-to-one and one-to-one
        /// links as properties (may break FastDAO since the type of the attribute on the object
        /// will need to be the foreign key type (I.E. int) rather than the real object type of
        /// the relationship, which is what NHibernate expects).
        /// </summary>
        /// <param name="hibConfNode">The XML configuration.</param>
        /// <param name="reflect">Whether or not to use reflection to populate all the class info.</param>
        /// <param name="includeForeignKeys">If true, many-to-one and one-to-one
        ///                                  links will be included as properties.</param>
        public ClassMapping(XmlNode hibConfNode, bool reflect, bool includeForeignKeys)
        {
            ParseNHibernateXML(hibConfNode, includeForeignKeys);
            if (reflect)
            {
                ReflectOnTypeInfo();
            }
        }

        /// <summary>
        /// A constructor that allows you to explicitly declare the type, table name, and column mappings, 
        /// without using an XML mapping string.  (e.g. if you want to store mapping info in a database)
        /// </summary>
        /// <param name="typeName">The fully-qualified Class,Assembly type name for the object.</param>
        /// <param name="tableName">The data source table name.</param>
        /// <param name="colDefinitions">List of column mappings.</param>
        public ClassMapping(string typeName, string tableName, IEnumerable<ClassMapColDefinition> colDefinitions) : 
            this(typeName, tableName, colDefinitions, true) { }

        /// <summary>
        /// A constructor that allows you to explicitly declare the type, table name, and column mappings, 
        /// without using an XML mapping string.  (e.g. if you want to store mapping info in a database)
        /// </summary>
        /// <param name="typeName">The fully-qualified Class,Assembly type name for the object.</param>
        /// <param name="tableName">The data source table name.</param>
        /// <param name="colDefinitions">List of column mappings.</param>
        /// <param name="reflect">Whether or not to use reflection to populate all the class info.</param>
        public ClassMapping(string typeName, string tableName, IEnumerable<ClassMapColDefinition> colDefinitions, bool reflect)
        {
            TypeName = typeName;
            Table = tableName;
            foreach (ClassMapColDefinition colDef in colDefinitions)
            {
                if (colDef is ClassMapIDColDefinition)
                {
                    IdDataColsByObjAttrs[colDef.Property] = colDef.Column;
                    GeneratorType genType = GeneratorType.NONE;
                    if (((ClassMapIDColDefinition)colDef).Generator)
                    {
                        genType = GeneratorType.AUTO;
                        if (String.IsNullOrEmpty(((ClassMapIDColDefinition)colDef).Sequence) == false)
                        {
                            genType = GeneratorType.SEQUENCE;
                            IdSequencesByDataCol[colDef.Column] = ((ClassMapIDColDefinition)colDef).Sequence;
                        }
                    }
                    IdGeneratorsByDataCol[colDef.Column] = genType;
                }
                AllDataColsInOrder.Add(colDef.Column);
                AllDataColsByObjAttrs[colDef.Property] = colDef.Column;
                AllObjAttrsByDataCol[colDef.Column] = colDef.Property;
                DataColTypesByObjAttr[colDef.Property] = ParseColumnType(colDef.Type);
                DataColTypesByDataCol[colDef.Column] = ParseColumnType(colDef.Type);
                NonIdDataColsByObjAttrs[colDef.Property] = colDef.Column;
            }
            if (reflect)
            {
                ReflectOnTypeInfo();
            }
        }

        private static Type ParseColumnType(string input)
        {
            // For speed, put the most common types at the top.
            if (input == null)
            {
                return null;
            }
            string lowerInput = input.ToLower();
            if (lowerInput.Equals("string"))
            {
                return typeof (string);
            }
            if (lowerInput.Equals("int") || lowerInput.Equals("integer") || lowerInput.Equals("int32"))
            {
                return typeof (int);
            }
            if (lowerInput.Equals("long") || lowerInput.Equals("int64"))
            {
                return typeof (long);
            }
            if (lowerInput.Equals("double"))
            {
                return typeof (double);
            }
            if (lowerInput.Equals("date") || lowerInput.Equals("datetime"))
            {
                return typeof (DateTime);
            }
            if (lowerInput.Equals("bool") || lowerInput.Equals("boolean"))
            {
                return typeof (bool);
            }
            if (lowerInput.Equals("short") || lowerInput.Equals("int16"))
            {
                return typeof (short);
            }
            if (lowerInput.Equals("byte"))
            {
                return typeof (byte);
            }
            if (lowerInput.Equals("char"))
            {
                return typeof (char);
            }
            if (lowerInput.Equals("float"))
            {
                return typeof (float);
            }
            if (lowerInput.Equals("bytearray"))
            {
                return typeof (byte[]);
            }
            Type colType = Type.GetType(input, false);
            if (colType != null)
            {
                return colType;
            }
            throw new BadDaoConfigurationException("Type " + input + " does not parse either as a primitive or as a valid class type.");
        }

        private void ParseNHibernateXML(XmlNode hibConfNode, bool includeForeignKeys)
        {
            TypeName = hibConfNode.Attributes["name"].Value;
            Table = hibConfNode.Attributes["table"].Value;
            foreach (XmlNode node in hibConfNode.ChildNodes)
            {
                try
                {
                    if ("id".Equals(node.Name))
                    {
                        string prop = node.Attributes["name"].Value;
                        string col = node.Attributes["column"].Value;
                        XmlAttribute typeAttr = node.Attributes["type"];
                        string type = typeAttr == null ? null : typeAttr.Value;
                        IdDataColsByObjAttrs[prop] = col;
                        AllDataColsByObjAttrs[prop] = col;
                        AllDataColsInOrder.Add(col);
                        AllObjAttrsByDataCol[col] = prop;
                        Type colType = ParseColumnType(type);
                        DataColTypesByObjAttr[prop] = colType;
                        DataColTypesByDataCol[col] = colType;

                        GeneratorType genType = GeneratorType.NONE;
                        // Check for generator type.
                        foreach (XmlNode idChild in node.ChildNodes)
                        {
                            if ("generator".Equals(idChild.Name))
                            {
                                // ok it is an autogenerated ID of some sort.
                                genType = GeneratorType.AUTO;

                                // See if there is a sequence.
                                foreach (XmlNode idGrandChild in idChild.ChildNodes)
                                {
                                    if ("param".Equals(idGrandChild.Name))
                                    {
                                        if ("sequence".Equals(idGrandChild.Attributes["name"].Value))
                                        {
                                            // We have to load it from a sequence.
                                            genType = GeneratorType.SEQUENCE;
                                            IdSequencesByDataCol[col] = idGrandChild.FirstChild.Value;
                                            // Only support one sequence def per ID.
                                            break;
                                        }
                                    }
                                }
                                // Only support one generator per id.
                                break;
                            }
                        }
                        // Save the generator type.
                        IdGeneratorsByDataCol[col] = genType;
                    }
                    else if ("property".Equals(node.Name) || 
                             ("many-to-one".Equals(node.Name) && includeForeignKeys) ||
                             ("one-to-one".Equals(node.Name) && includeForeignKeys))
                    {
                        string prop = node.Attributes.GetNamedItem("name").Value;
                        string col = node.Attributes.GetNamedItem("column").Value;
                        XmlAttribute typeAttr = node.Attributes["type"];
                        string type = typeAttr == null ? null : typeAttr.Value;
                        NonIdDataColsByObjAttrs[prop] = col;
                        AllDataColsByObjAttrs[prop] = col;
                        AllDataColsInOrder.Add(col);
                        Type colType = ParseColumnType(type);
                        DataColTypesByObjAttr[prop] = colType;
                        DataColTypesByDataCol[col] = colType;
                        AllObjAttrsByDataCol[col] = prop;
                    }
                    else if ("composite-id".Equals(node.Name))
                    {
                        // We support two ways of making composite ids.  The "easy" way
                        // and the "nhibernate" way.  The "easy" way is just put more than
                        // one "id" field in the mapping.  The "nhibernate" way is use
                        // this composite-id tag.
                        foreach (XmlNode compositeCol in node.ChildNodes)
                        {
                            if ("key-property".Equals(compositeCol.Name))
                            {
                                // This is one of the composite columns.
                                string prop = compositeCol.Attributes["name"].Value;
                                string col = compositeCol.Attributes["column"].Value;
                                XmlAttribute typeAttr = compositeCol.Attributes["type"];
                                string type = typeAttr == null ? null : typeAttr.Value;
                                IdDataColsByObjAttrs[prop] = col;
                                AllDataColsByObjAttrs[prop] = col;
                                AllDataColsInOrder.Add(col);
                                AllObjAttrsByDataCol[col] = prop;
                                Type colType = ParseColumnType(type);
                                DataColTypesByObjAttr[prop] = colType;
                                DataColTypesByDataCol[col] = colType;
                                // NHibernate doesn't support generators on composite keys.
                                IdGeneratorsByDataCol[col] = GeneratorType.NONE;
                            }
                        }
                    } // else ignore it.
                }
                catch (Exception e)
                {
                    throw new BadDaoConfigurationException("Error while parsing class map XML for type " +
                                                  TypeName + ", table " + Table + ": " + node.OuterXml, e);
                }
            }
        }

        private void ReflectOnTypeInfo()
        {
            ClassType = Type.GetType(TypeName);
            if (ClassType == null)
            {
                throw new NullReferenceException("No class type found for data mapping: " + TypeName);
            }
            // Get the constructor that takes no parameters (default constructor).
            Constructor = ClassType.GetConstructor(new Type[0]);
            foreach (MemberInfo info in ClassType.GetMembers())
            {
                if ((info.MemberType == MemberTypes.Field) ||
                    (info.MemberType == MemberTypes.Property))
                {
                    string memberName = info.Name;
                    AllObjMemberInfosByObjAttr[memberName] = info;
                    if (IdDataColsByObjAttrs.ContainsKey(memberName))
                    {
                        string colName = IdDataColsByObjAttrs[memberName];
                        AllObjMemberInfosByDataCol[colName] = info;
                        IdDataColsByObjMemberInfo[info] = colName;
                        AllDataColsByObjMemberInfo[info] = colName;
                    }
                    else
                    {
                        if (NonIdDataColsByObjAttrs.ContainsKey(memberName))
                        {
                            string colName = NonIdDataColsByObjAttrs[memberName];
                            AllObjMemberInfosByDataCol[colName] = info;
                            NonIdDataColsByObjMemberInfo[info] = colName;
                            AllDataColsByObjMemberInfo[info] = colName;
                        }
                        else
                        {
                            // This isn't necessarily wrong, but it might be if the configuration is
                            // messed up.
                            log.Debug("Member " + memberName + " of type " + TypeName +
                                      " is not mapped to a database column.");
                        }
                    }
                }
            }
            // Sanity check, make sure all cols in the xml mapping actually found object attributes.
            foreach (KeyValuePair<string, string> colByAttr in AllDataColsByObjAttrs)
            {
                string colName = colByAttr.Value;
                bool found = false;
                foreach (string colByMember in AllDataColsByObjMemberInfo.Values)
                {
                    if (colName.Equals(colByMember))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new BadDaoConfigurationException("Column " + colName +
                                                           " was mapped in the mapping file for type " + TypeName +
                                                           " to field " + colByAttr.Key +
                                                           ", but the class doesn't have that field.  It does have: " +
                                                           StringHelper.Join(ClassType.GetMembers()));
                }
            }
        }

        /// <summary>
        /// Returns the class mapping type name and table name.
        /// </summary>
        public override string ToString()
        {
            return "ClassMapping(type " +
                (ClassType == null ? TypeName : ClassType.FullName) +
                " to table " + Table + ")";
        }
    }

    /// <summary>
    /// The purpose of this class is to represent in a mapping file that a .NET (unicode) string
    /// is being mapped to an ASCII varchar column.  Not marking the column as this
    /// type will still work, but may have performance implications because the DB will
    /// be casting the value at query execution time.
    /// This has been demonistrated on SQL Server 2005, and could not be reproduced
    /// on Oracle 10.2.
    /// </summary>
    public class AsciiString { }

    /// <summary>
    /// A class that encapsulates the properties need to define a column mapping in a ClassMapping object.
    /// </summary>
    public class ClassMapColDefinition
    {
        /// <summary>
        /// The name of the property on the data class type.
        /// </summary>
        public string Property;
        /// <summary>
        /// The name of the column in the data source.
        /// </summary>
        public string Column;
        /// <summary>
        /// The string name of the data type (not necessarily a .NET class name).  May be null if you
        /// don't want to exlicitly map the type.
        /// </summary>
        public string Type;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ClassMapColDefinition() { }

        /// <summary>
        /// Constructor to populate all the needed properties.
        /// </summary>
        /// <param name="name">The object property name.</param>
        /// <param name="column">The data source column name.</param>
        /// <param name="type">The data type (optional).</param>
        public ClassMapColDefinition(string name, string column, string type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", "Name cannot be null.");
            }
            if (column == null)
            {
                throw new ArgumentNullException("column", "Column cannot be null.");
            }
            Property = name;
            Column = column;
            Type = type;
        }
    }

    /// <summary>
    /// Extends ClassMapColDefinition, and adds properties needed to define an ID column.
    /// </summary>
    public class ClassMapIDColDefinition : ClassMapColDefinition
    {
        /// <summary>
        /// Whether or not the data source will use a generator to make Auto-IDs
        /// </summary>
        public bool Generator;
        /// <summary>
        /// The sequence name, if one is used to generate the Auto-ID
        /// </summary>
        public string Sequence;

        /// <summary>
        /// Constructor to populate all the needed properties.
        /// </summary>
        /// <param name="name">The object property name.</param>
        /// <param name="column">The data source column name.</param>
        /// <param name="type">The data type (optional).</param>
        /// <param name="generator">Whether or not the data source will use a generator to make Auto-IDs.</param>
        /// <param name="sequence">The sequence name, if one is used to generate the Auto-ID.</param>
        public ClassMapIDColDefinition(string name, string column, string type, bool generator, string sequence)
            : base(name, column, type)
        {
            Generator = generator;
            Sequence = sequence;
        }
    }
}
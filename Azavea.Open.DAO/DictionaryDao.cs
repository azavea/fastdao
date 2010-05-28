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
using System.Xml;
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.Common;
using Azavea.Open.DAO.Exceptions;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// A FastDAO that returns Dictionaries instead of objects.  This is a more
    /// basic DAO that is useful in cases where you don't actually care much about
    /// the object (I.E. are just going to drop it into XML or JSON anyway or
    /// something like that).
    /// </summary>
    public class DictionaryDao : FastDAO<CheckedDictionary<string, object>>
    {
        /// <summary>
        /// This allows you to specify the config name and the section in the config file
        /// used to get the database config info.
        /// </summary>
        /// <param name="mappedType">The type name we're mapping.  Since this class doesn't
        ///                          actually instantiate it, it can just be the key to the
        ///                          config file and doesn't have to be a real class.</param>
        /// <param name="configName">Name used to get the configuration.</param>
        /// <param name="sectionName">Name of the section within the config file.</param>
        public DictionaryDao(string mappedType, string configName, string sectionName)
            : this(mappedType, Config.GetConfig(configName), sectionName, null)
        {
        }

        /// <summary>
        /// This allows you to specify the config name and the section in the config file
        /// used to get the database config info.
        /// </summary>
        /// <param name="mappedType">The type name we're mapping.  Since this class doesn't
        ///                          actually instantiate it, it can just be the key to the
        ///                          config file and doesn't have to be a real class.</param>
        /// <param name="configName">Name used to get the configuration.</param>
        /// <param name="sectionName">Name of the section within the config file.</param>
        /// <param name="decryptionDelegate">The method to call to decrypt passwords or
        ///                                  other encrypted connection info.  May be null.</param>
        public DictionaryDao(string mappedType, string configName, string sectionName,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(mappedType, Config.GetConfig(configName), sectionName, decryptionDelegate)
        {
        }

        /// <summary>
        /// This allows you to give the config object and the section in the config
        /// used to get the database config info.
        /// </summary>
        /// <param name="mappedType">The type name we're mapping.  Since this class doesn't
        ///                          actually instantiate it, it can just be the key to the
        ///                          config file and doesn't have to be a real class.</param>
        /// <param name="config">Configuration object loaded somewhere else.</param>
        /// <param name="sectionName">Name of the section within the config.</param>
        public DictionaryDao(string mappedType, Config config, string sectionName) :
            this(mappedType, config, sectionName, null)
        {
        }

        /// <summary>
        /// This allows you to give the config object and the section in the config
        /// used to get the database config info.
        /// </summary>
        /// <param name="mappedType">The type name we're mapping.  Since this class doesn't
        ///                          actually instantiate it, it can just be the key to the
        ///                          config file and doesn't have to be a real class.</param>
        /// <param name="config">Configuration object loaded somewhere else.</param>
        /// <param name="sectionName">Name of the section within the config.</param>
        /// <param name="decryptionDelegate">The method to call to decrypt passwords or
        ///                                  other encrypted connection info.  May be null.</param>
        public DictionaryDao(string mappedType, Config config, string sectionName,
            ConnectionInfoDecryptionDelegate decryptionDelegate) :
            this(mappedType, ConnectionDescriptor.LoadFromConfig(config, sectionName, decryptionDelegate),
                 config.GetParameterWithSubstitution(sectionName, "MAPPING", false))
        {
        }

        /// <summary>
        /// This allows you to specify the data source connection and the mapping file.
        /// </summary>
        /// <param name="mappedType">The type name we're mapping.  Since this class doesn't
        ///                          actually instantiate it, it can just be the key to the
        ///                          config file and doesn't have to be a real class.</param>
        /// <param name="connDesc">Data source Connection information.</param>
        /// <param name="mappingFileName">Filename (with path) to the mapping file.</param>
        public DictionaryDao(string mappedType, IConnectionDescriptor connDesc, string mappingFileName) :
            this(connDesc, ParseHibernateConfig(mappedType, mappingFileName))
        {
        }

        /// <summary>
        /// If you already have the data source connection and the mapping file, you may use
        /// this constructor.
        /// </summary>
        /// <param name="connDesc">Data source Connection information.</param>
        /// <param name="mapping">ClassMapping describing the class to be mapped and
        ///                       the table to map it to.</param>
        public DictionaryDao(IConnectionDescriptor connDesc, ClassMapping mapping)
            : base(connDesc, mapping)
        {
        }
        /// <summary>
        /// This version of this method looks for a matching string, not a matching type.
        /// TODO: Refactor FastDAO to look for the object type name rather than trying to
        ///       parse the type out of the mapping file.  Then we don't need the other
        ///       version, and this can be moved to FastDAO and made protected.
        /// </summary>
        /// <param name="desiredType">The type we're loading a mapping for.</param>
        /// <param name="fileName">XML File containing an NHibernate configuration.</param>
        /// <returns>The class mapping for the desired type.  If unable to find it, an
        ///          exception is thrown, so you may safely assume this will never return
        ///          null.</returns>
        private static ClassMapping ParseHibernateConfig(string desiredType, string fileName)
        {
            ClassMapping retVal = null;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList list = doc.GetElementsByTagName("class");
            foreach (XmlNode node in list)
            {
                string thisNodeType = node.Attributes["name"].Value;
                if (desiredType.Equals(thisNodeType))
                {
                    // NOTE: The boolean parameters here are not compatible with FastDAO.
                    retVal = new ClassMapping(node, false, true);
                    break;
                }
            }
            if (retVal == null)
            {
                throw new BadDaoConfigurationException("Type " + desiredType +
                                                       " does not appear to be mapped in file " + fileName);
            }
            return retVal;
        }

        /// <summary>
        /// This method takes property/field values from the data object
        /// and puts them into the dictionary 'putEmHere', keyed by the column names.
        /// </summary>
        /// <param name="dataObject">An implementation-specific data object.</param>
        /// <param name="putEmHere">A dictionary to insert values into.  Values will be objects
        ///                         (which may be null) keyed by column names.  The values
        ///                         will be cast to the appropriate type for the db, if necessary.</param>
        /// <param name="classMap">The class map for the object.</param>
        /// <param name="idFields">If true, we copy the ID fields.</param>
        /// <param name="dataFields">If true, we copy the non-ID fields.</param>
        protected override void GetFieldValues(CheckedDictionary<string, object> dataObject,
                                               IDictionary<string, object> putEmHere, ClassMapping classMap, bool idFields, bool dataFields)
        {
            IDictionary<string, string> fieldMapping;
            if (idFields)
            {
                if (dataFields)
                {
                    fieldMapping = classMap.AllDataColsByObjAttrs;
                }
                else
                {
                    fieldMapping = classMap.IdDataColsByObjAttrs;
                }
            }
            else
            {
                if (dataFields)
                {
                    fieldMapping = classMap.NonIdDataColsByObjAttrs;
                }
                else
                {
                    throw new ArgumentNullException("dataFields", "GetFieldValues called for no fields.");
                }
            }
            foreach (KeyValuePair<string, string> kvp in fieldMapping)
            {
                string propertyName = kvp.Key;
                string columnName = kvp.Value;
                // The value not being present is also treated as null.
                object colValue = dataObject.ContainsKey(propertyName) ? dataObject[propertyName] : null;
                if (colValue != null)
                {
                    // All attr names are in the collection, but they have null values
                    // if a type wasn't specified in the mapping.
                    Type desiredType = _classMap.DataColTypesByObjAttr[propertyName];
                    if (desiredType != null)
                    {
                        colValue = _dataAccessLayer.CoerceType(desiredType, colValue);
                    }
                }
                putEmHere[columnName] = colValue;
            }
        }

        /// <summary>
        /// This object gets a value off the data object based on the
        /// name of the field/property.
        /// </summary>
        /// <param name="dataObject">The object to get a value off of.</param>
        /// <param name="fieldName">The name of the field/property to get the value of.</param>
        /// <returns>The value.</returns>
        public override object GetValueFromObject(CheckedDictionary<string, object> dataObject,
                                                  string fieldName)
        {
            return dataObject[fieldName];
        }

        /// <summary>
        /// Given an object and a (data source) column name
        /// set the given memberValue onto the object's property.
        /// 
        /// Since the DictionaryDAO is probably used only for reading,
        /// it will tolerate the same column mapped to more than one property
        /// (normal FastDAO will not work if you try that).
        /// </summary>
        /// <param name="dataObj">Object to set the value upon.</param>
        /// <param name="classMap">Object's mapping.</param>
        /// <param name="colName">Name of the column we got the value from.</param>
        /// <param name="memberValue">Value to set on the field or property.</param>
        protected override void SetValueOnObject(CheckedDictionary<string, object> dataObj,
                                                 ClassMapping classMap, string colName, object memberValue)
        {
            foreach (KeyValuePair<string, string> kvp in classMap.AllDataColsByObjAttrs)
            {
                if (kvp.Value.Equals(colName))
                {
                    // We'll go ahead and coerce the type, although
                    // the db "should" give it to us as the mapped type.
                    // Not all data sources do however, so this is the best
                    // we can do.
                    Type desiredType = classMap.DataColTypesByObjAttr[classMap.AllObjAttrsByDataCol[colName]];
                    if (desiredType != null)
                    {
                        memberValue = _dataAccessLayer.CoerceType(desiredType, memberValue);
                    }
                    dataObj[kvp.Key] = memberValue;
                }
            }
        }

        /// <summary>
        /// Populates a criteria with a bunch of EqualExpressions, one for each
        /// ID on the data object.
        /// </summary>
        /// <param name="dataObject">The object we're concerned with.</param>
        /// <param name="crit">Criteria to add equals expressions to.</param>
        /// <param name="classMap">The mapping of the object to the data source.</param>
        protected override void PopulateIDCriteria(CheckedDictionary<string, object> dataObject,
                                                   DaoCriteria crit, ClassMapping classMap)
        {
            foreach (KeyValuePair<string, string> kvp in classMap.IdDataColsByObjAttrs)
            {
                crit.Expressions.Add(new EqualExpression(kvp.Key, dataObject[kvp.Key]));
            }
        }
    }
}
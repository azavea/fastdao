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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml;
using Azavea.Open.Common;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Joins;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Util;
using log4net;

namespace Azavea.Open.DAO
{
    /// <summary>
    /// This class is built to be a "fast and easy" way of reading/writing objects to/from a
    /// data source.  It meant to have high performance on throughput of large numbers of objects.
    /// It does not support every possible sophistication that an ORM system can have.
    /// </summary>
    public class FastDAO<T> : IFastDaoInserter<T>, IFastDaoUpdater<T>, IFastDaoDeleter<T>, IFastDaoReader<T> where T : class, new()
    {
        /// <summary>
        /// This is the value that will be returned from methods that return a number of rows
        /// affected, if the number is unable to be determined.
        /// </summary>
        public const int UNKNOWN_NUM_ROWS = -123;
        /// <summary>
        /// log4net logger that any child class may use for logging any appropriate messages.
        /// </summary>
        protected ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// The connection descriptor created from the connection config parameters.
        /// </summary>
        private readonly ConnectionDescriptor _connDesc;

        /// <summary>
        /// The ClassMapping object representing the class-to-table mapping 
        /// loaded from the config file.
        /// </summary>
        protected readonly ClassMapping _classMap;

        /// <summary>
        /// The data access layer can be different depending on what kind of data source we're talking to.
        /// </summary>
        protected readonly IDaLayer _dataAccessLayer;

        /// <summary>
        /// The name of the table (or equivalent) in the data storage that this DAO reads/writes to/from.
        /// </summary>
        public string Table
        {
            get { return _classMap.Table; }
        }

        /// <summary>
        /// The class mapping used to map this DAO's data object type to the data storage.
        /// </summary>
        public ClassMapping ClassMap
        {
            get { return _classMap; }
        }

        /// <summary>
        /// The object describing how to connect to and/or interact with the data
        /// source we're reading objects from.
        /// </summary>
        public ConnectionDescriptor ConnDesc
        {
            get { return _connDesc; }
        }

        #region Constructors
        /// <summary>
        /// This allows you to specify the config name and the section in the config file
        /// used to get the database config info.
        /// </summary>
        /// <param name="configName">Name used to get the configuration.</param>
        /// <param name="sectionName">Name of the section within the config file.</param>
        public FastDAO(string configName, string sectionName)
            : this(Config.GetConfig(configName), sectionName, null)
        {
        }

        /// <summary>
        /// This allows you to specify the config name and the section in the config file
        /// used to get the database config info.
        /// </summary>
        /// <param name="configName">Name used to get the configuration.</param>
        /// <param name="sectionName">Name of the section within the config file.</param>
        /// <param name="decryptionDelegate">The method to call to decrypt passwords or
        ///                                  other encrypted connection info.  May be null.</param>
        public FastDAO(string configName, string sectionName,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(Config.GetConfig(configName), sectionName, decryptionDelegate)
        {
        }

        /// <summary>
        /// This allows you to give the config object and the section in the config
        /// used to get the database config info.
        /// </summary>
        /// <param name="config">Configuration object (presumably read ahead of time).</param>
        /// <param name="sectionName">Name of the section within the config.</param>
        public FastDAO(Config config, string sectionName) : this(config, sectionName, null)
        {
        }

        /// <summary>
        /// This allows you to give the config object and the section in the config
        /// used to get the database config info.
        /// </summary>
        /// <param name="config">Configuration object (presumably read ahead of time).</param>
        /// <param name="sectionName">Name of the section within the config.</param>
        /// <param name="decryptionDelegate">The method to call to decrypt passwords or
        ///                                  other encrypted connection info.  May be null.</param>
        public FastDAO(Config config, string sectionName,
            ConnectionInfoDecryptionDelegate decryptionDelegate) :
            this(ConnectionDescriptor.LoadFromConfig(config, sectionName, decryptionDelegate),
                 config.GetParameterWithSubstitution(sectionName, "MAPPING", false))
        {
        }

        /// <summary>
        /// This allows you to specify the data source connection info and the mapping file.
        /// </summary>
        /// <param name="connDesc">DB Connection information.</param>
        /// <param name="mappingFileName">Filename (with path) to the mapping file.</param>
        public FastDAO(ConnectionDescriptor connDesc, string mappingFileName) :
            this(connDesc, ParseHibernateConfig(typeof(T), mappingFileName))
        {
        }

        /// <summary>
        /// If you already have the DB connection and the mapping information, you may use
        /// this constructor.
        /// </summary>
        /// <param name="connDesc">DB Connection information.</param>
        /// <param name="mapping">ClassMapping describing the class to be mapped and
        ///                       the table to map it to.</param>
        public FastDAO(ConnectionDescriptor connDesc, ClassMapping mapping)
        {
            if (connDesc == null)
            {
                throw new ArgumentNullException("connDesc", "Connection descriptor must not be null.");
            }
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping", "Class mapping must not be null.");
            }
            _connDesc = connDesc;
            _dataAccessLayer = _connDesc.CreateDataAccessLayer();
            _classMap = mapping;
        }

        /// <summary>
        /// Loads ClassMappings from NHibernate config xml.  This method is somewhat
        /// fragile (doesn't catch exceptions), since if we can't parse the NHibernate
        /// config, we can't do much.  However it will ignore problems with class mappings
        /// other than for the desired type.
        /// 
        /// Static so we can call it from the constructor.
        /// </summary>
        /// <param name="desiredType">The type we're loading a mapping for.</param>
        /// <param name="fileName">XML File containing an NHibernate configuration.</param>
        /// <returns>The class mapping for the desired type.  If unable to find it, an
        ///          exception is thrown, so you may safely assume this will never return
        ///          null.</returns>
        private static ClassMapping ParseHibernateConfig(Type desiredType, string fileName)
        {
            ClassMapping retVal = null;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList list = doc.GetElementsByTagName("class");
            foreach (XmlNode node in list)
            {
                string name = node.Attributes["name"].Value;
                if (desiredType.Equals(Type.GetType(name)))
                {
                    retVal = new ClassMapping(node);
                    break;
                }
            }
            if (retVal == null)
            {
                throw new BadDaoConfigurationException("Type " + desiredType.FullName +
                                                       " does not appear to be mapped in file " + fileName);
            }
            return retVal;
        }
        #endregion

        #region Methods For Modifying Behavior
        /// <summary>
        /// A method to add a coercion delegate for a type.  This allows you to specify how
        /// to convert an unsupported type to/from the data source.
        /// </summary>
        /// <param name="t">The type to coerce.</param>
        /// <param name="coercionDelegate">How to coerce it.</param>
        public void AddCoercibleType(Type t, TypeCoercionDelegate coercionDelegate)
        {
            _dataAccessLayer.AddCoercibleType(t, coercionDelegate);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the specified object from the data source.
        /// </summary>
        /// <param name="dataObject">An object to delete from the DB.</param>
        public virtual void Delete(T dataObject)
        {
            if (_classMap.IdDataColsByObjAttrs.Count <= 0)
            {
                throw new BadDaoConfigurationException("You cannot delete an object of a type with no ID fields defined: " + typeof(T));
            }

            // Put all the IDs on the criteria.
            DaoCriteria crit = DbCaches.Criteria.Get();
            foreach (string propName in _classMap.IdDataColsByObjAttrs.Keys)
            {
                crit.Expressions.Add(new EqualExpression(propName, GetValueFromObject(dataObject, propName)));
            }
            int numRecs = _dataAccessLayer.Delete(_classMap, crit);

            if ((numRecs != 1) && (numRecs != UNKNOWN_NUM_ROWS))
            {
                throw new UnexpectedResultsException("Delete statement deleted " + numRecs + " rows (should have been 1).",
                    _connDesc);
            }
        }

        /// <summary>
        /// Deletes the specified objects from the data source.  If the objects are not in the
        /// data source, it ignores them.
        /// </summary>
        public virtual void Delete(IEnumerable<T> deleteUs)
        {
            foreach (T obj in deleteUs)
            {
                Delete(obj);
            }
        }

        /// <summary>
        /// Deletes objects from the data source that meet the given criteria.
        /// </summary>
        /// <param name="crit">Criteria for deletion.  NOTE: Only the expressions are observed,
        ///                    other things (like "order" or start / limit) are ignored.
        ///                    Also, null or blank (no expressions) criteria are NOT allowed.
        ///                    If you really wish to delete everything, call DeleteAll().</param>
        /// <returns>The number of rows/objects deleted (or UNKNOWN_NUM_ROWS).</returns>
        public virtual int Delete(DaoCriteria crit)
        {
            if ((crit == null) || (crit.Expressions.Count == 0))
            {
                throw new ArgumentNullException("crit",
                                                "Critera must be non-null and must contain at least one expression.  To delete all records, use the DeleteAll method.");
            }
            return _dataAccessLayer.Delete(_classMap, crit);
        }

        /// <summary>
        /// Deletes all records of this dao's type.
        /// </summary>
        /// <returns>The number of rows/objects deleted.</returns>
        public virtual int DeleteAll()
        {
            return _dataAccessLayer.Delete(_classMap, null);
        }

        /// <summary>
        /// Deletes every row from the table for this DAO.
        /// Performance will be equal to or better than DeleteAll, but may require
        /// that the user (from the connection descriptor) have greater permissions
        /// than necessary for DeleteAll.  Depends on the implementation.
        /// </summary>
        public virtual void Truncate()
        {
            _log.Info("Truncating " + _classMap.Table);
            _dataAccessLayer.Truncate(_classMap);
            _log.Info("Finished truncating " + _classMap.Table);
        }
        #endregion

        #region "Save" (Insert or Update as appropriate)
        /// <summary>
        /// Tries to update the object if it is already in the DB, which it guesses by calling
        /// IsIDValid on it and/or by just trying tye update and seeing if it works, otherwise
        /// inserts it as a new record.
        /// 
        /// NOTE: If you already know whether you're going to be doing an insert or an update,
        /// calling Insert() or Update() will be faster.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        public void Save(T obj)
        {
            Save(obj, false);
        }

        /// <summary>
        /// Overload for Save that allows us to put the generated ID into the object if inserting
        /// to a table with a DB-generated ID column.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="setGeneratedId">Whether to set the generated ID on the
        ///                              object to be saved.</param>
        public void Save(T obj, bool setGeneratedId)
        {
            // If it has a valid ID, it's probably already been saved so we should try and update.
            if (IsIDValid(obj))
            {
                SaveRecord(obj, true, true, setGeneratedId);
            }
            else // otherwise, only try to insert.
            {
                SaveRecord(obj, false, true, setGeneratedId);
            }
        }
        #endregion

        #region Insert
        /// <summary>
        /// Faster than Save if you know this is a new object that is being inserted.
        /// Inserts the object into the data source.  If there are unique constraints on
        /// the data source and this is a duplicate record, this may generate an error.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        /// <param name="setGeneratedId">Update the object with its new ID if the ID was 
        ///                              autogenerated by the database.  That requires a 
        ///                              second DB query to retrieve the ID and "most" of the 
        ///                              time you may not need it.
        ///                              If setGeneratedId is false, this ONLY Updates the 
        ///                              object's ID field(s) IF the mapping contains a sequence, 
        ///                              meaning that FastDAO must query the sequence for the next 
        ///                              value anyway.</param>
        public void Insert(T obj, bool setGeneratedId)
        {
            SaveRecord(obj, false, true, setGeneratedId);
        }

        /// <summary>
        /// Simpler override, same as calling Insert(obj, false);
        /// </summary>
        /// <param name="obj">The object to save.</param>
        public virtual void Insert(T obj)
        {
            Insert(obj, false);
        }

        /// <summary>
        /// Inserts a bunch of records in one transaction, hopefully to be faster than
        /// separate calls to Insert().  Whether it is or not depends on the implementation.
        /// </summary>
        /// <param name="insertUs">List of objects to save.</param>
        public virtual void Insert(IEnumerable<T> insertUs)
        {
            List<IDictionary<string, object>> propValueDictionaries = new List<IDictionary<string, object>>();

            foreach (T obj in insertUs)
            {
                Dictionary<string, object> colsToWrite = DbCaches.StringObjectDicts.Get();
                Dictionary<string, object> idCols = DbCaches.StringObjectDicts.Get();

                GetFieldValues(obj, colsToWrite, _classMap, false, true);
                GetFieldValues(obj, idCols, _classMap, true, false);

                ProcessIdColumnsForInsert(obj, idCols, colsToWrite);

                propValueDictionaries.Add(colsToWrite);
            }

            if (propValueDictionaries.Count > 0)
            {
                _dataAccessLayer.InsertBatch(_classMap, propValueDictionaries);
            }
            else
            {
                _log.Warn("InsertBatch was called without any objects in the collection.");
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Faster than Save if you know this is an existing object that is being updated.
        /// Updates the data source with the new values from this object.  May generate an
        /// error if the object does not in fact exist in the data source yet.
        /// </summary>
        /// <param name="obj">The object to save.</param>
        public virtual void Update(T obj)
        {
            SaveRecord(obj, true, false, false);
        }

        /// <summary>
        /// Updates a bunch of records in one transaction, hopefully to be faster than
        /// separate calls to Update().  Whether it is or not depends on the implementation.
        /// </summary>
        /// <param name="updateUs">List of objects to save.</param>
        public virtual void Update(IEnumerable<T> updateUs)
        {
            List<DaoCriteria> criteriaList = new List<DaoCriteria>();
            List<IDictionary<string, object>> propValueDictionaries = new List<IDictionary<string, object>>();

            foreach (T obj in updateUs)
            {
                DaoCriteria idCrit = DbCaches.Criteria.Get();

                Dictionary<string, object> colsToWrite = DbCaches.StringObjectDicts.Get();

                GetFieldValues(obj, colsToWrite, _classMap, false, true);
                PopulateIDCriteria(obj, idCrit, _classMap);

                criteriaList.Add(idCrit);
                propValueDictionaries.Add(colsToWrite);
            }

            if (criteriaList.Count > 0)
            {
                _dataAccessLayer.UpdateBatch(_classMap, criteriaList, propValueDictionaries);
            }
            else
            {
                _log.Warn("UpdateBatch was called without any objects in the collection.");
            }
        }
        #endregion

        #region Querying
        /// <summary>
        /// Returns all objects of the given type.
        /// </summary>
        /// <returns>A list of objects, or an empty list (not null).</returns>
        public IList<T> Get()
        {
            return Get(null);
        }

        /// <summary>
        /// Queries for objects of the specified type where the property matches the given value.
        /// </summary>
        /// <param name="propName">Property or Field on the object you want to match a value.</param>
        /// <param name="val">Value that the Property or Field should have.</param>
        /// <returns>The first object that matches the criteria.</returns>
        public T GetFirst(string propName, object val)
        {
            DaoCriteria crit = DbCaches.Criteria.Get();
            try
            {
                crit.Expressions.Add(new EqualExpression(propName, val, true));
                IList<T> list = Get(crit);

                T retVal;

                if (list == null || list.Count < 1)
                {
                    // This means null if it's an object.
                    retVal = default(T);
                }
                else
                {
                    retVal = list[0];
                }

                return retVal;
            }
            finally
            {
                DbCaches.Criteria.Return(crit);
            }
        }

        /// <summary>
        /// Queries for objects where the property matches the given value.
        /// </summary>
        /// <param name="propertyName">Property or Field on the object you want to match a value.</param>
        /// <param name="propertyValue">Value that the Property or Field should have.</param>
        /// <returns>All objects that match the criteria, or an empty list (not null).</returns>
        public IList<T> Get(string propertyName, object propertyValue)
        {
            DaoCriteria crit = DbCaches.Criteria.Get();
            try
            {
                crit.Expressions.Add(new EqualExpression(propertyName, propertyValue, true));
                return Get(crit);
            }
            finally
            {
                DbCaches.Criteria.Return(crit);
            }
        }

        /// <summary>
        /// Queries and returns objects matching the criteria.
        /// </summary>
        /// <returns>A list of objects, or an empty list (not null).</returns>
        public virtual IList<T> Get(DaoCriteria crit)
        {
            Hashtable parameters = DbCaches.Hashtables.Get();

            if (crit != null)
            {
                if (crit.Start > 0)
                {
                    parameters.Add("start", crit.Start);
                }
                if (crit.Limit > 0)
                {
                    parameters.Add("limit", crit.Limit);
                }
            }
            IDaQuery query = _dataAccessLayer.CreateQuery(_classMap, crit);
            _dataAccessLayer.ExecuteQuery(_classMap, query, CreateObjectsFromReader, parameters);
            _dataAccessLayer.DisposeOfQuery(query);

            IList<T> items = (IList<T>)parameters["items"];

            DbCaches.Hashtables.Return(parameters);

            return items;
        }

        /// <summary>
        /// Returns the number of objects matching the given criteria.
        /// </summary>
        public virtual int GetCount(DaoCriteria crit)
        {
            return _dataAccessLayer.GetCount(_classMap, crit);
        }

        /// <summary>
        /// Queries for objects, similar to Get, except that this iterates over the resulting
        /// records and invokes the specified delegate for each one.  This allows processing of much
        /// larger result sets since it doesn't attempt to load all the objects into memory at once.
        /// </summary>
        /// <param name="criteria">Any criteria for the query.  May be null for "all records".</param>
        /// <param name="invokeMe">The method to invoke for each object returned by the query.</param>
        /// <param name="parameters">Any parameters that you want to pass to the invokeMe method.
        ///                            This may be null.</param>
        /// <param name="desc">Description of the loop for logging purposes.</param>
        /// <returns>The number of objects iterated over.</returns>
        public int Iterate<P>(DaoCriteria criteria, DaoIterationDelegate<T, P> invokeMe,
                           P parameters, string desc)
        {
            int limit = (criteria == null || criteria.Limit <= 0) ? int.MaxValue : criteria.Limit;

            IDaQuery query = _dataAccessLayer.CreateQuery(_classMap, criteria);
            int retVal = IterateOverQuery(query, invokeMe, parameters, limit, desc);
            _dataAccessLayer.DisposeOfQuery(query);
            return retVal;
        }
        #endregion

        #region Join Support

        /// <summary>
        /// This allows joining between two DAOs.  Assuming both are using the same data
        /// source, and joins are implemented in the data access layer, this will use
        /// native joins (I.E. JOIN keyword in SQL) and be fast.  If not, this will use
        /// the "PseudoJoiner", and be less fast (though not horrible most of the time,
        /// depending on the number of records and the data sources involved).
        /// </summary>
        /// <param name="crit">An object describing how to join the two DAOs.  Includes any
        ///                    criteria that apply to the right or left DAO.</param>
        /// <param name="rightDao">The other DAO we are joining against.</param>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <returns>A list of KeyValuePairs.  The Key is the object from this </returns>
        public List<JoinResult<T,R>> Get<R>(DaoJoinCriteria crit, IFastDaoReader<R> rightDao) where R : class, new()
        {
            // If the two DAOs use the same connection descriptor, and the data
            // access layer supports joins, we can do an actual join via the
            // data source.  Otherwise, we'll fake it by querying first this
            // DAO then the other DAO.
            if ((_dataAccessLayer is IDaJoinableLayer) &&
                ((IDaJoinableLayer)_dataAccessLayer).CanJoin<R>(crit, rightDao.ConnDesc, rightDao.ClassMap))
            {
                Hashtable parameters = DbCaches.Hashtables.Get();
                parameters["rightDao"] = rightDao;
                IDaJoinQuery query = ((IDaJoinableLayer)_dataAccessLayer).CreateJoinQuery(crit,
                        _classMap, rightDao.ClassMap);
                parameters["leftPrefix"] = query.GetLeftColumnPrefix();
                parameters["rightPrefix"] = query.GetRightColumnPrefix();
                _dataAccessLayer.ExecuteQuery(_classMap, query,
                                              CreateJoinObjectsFromReader<R>, parameters);
                _dataAccessLayer.DisposeOfQuery(query);

                List<JoinResult<T,R>> items = (List<JoinResult<T,R>>)parameters["items"];

                DbCaches.Hashtables.Return(parameters);

                return items;
            }
            return PseudoJoiner.Join(crit, this, rightDao);
        }

        /// <summary>
        /// Performs a join using the given join criteria and returns the number of objects that
        /// would result from the join if you called Get.
        /// 
        /// Whether this is faster than calling Get depends on the implementation.
        /// </summary>
        /// <typeparam name="R">The type of object returned by the other DAO.</typeparam>
        /// <param name="crit">An object describing how to join the two DAOs.  Includes any
        ///                    criteria that apply to the right or left DAO.</param>
        /// <param name="rightDao">The other DAO we are joining against.</param>
        /// <returns>The number of join results that matched the criteria.</returns>
        public int GetCount<R>(DaoJoinCriteria crit, IFastDaoReader<R> rightDao) where R : class, new()
        {
            // If the two DAOs use the same connection descriptor, and the data
            // access layer supports joins, we can do an actual join count via the
            // data source.  Otherwise, we'll fake it using the pseudojoiner.
            if ((_dataAccessLayer is IDaJoinableLayer) &&
                ((IDaJoinableLayer)_dataAccessLayer).CanJoin<R>(crit, rightDao.ConnDesc, rightDao.ClassMap))
            {
                return ((IDaJoinableLayer)_dataAccessLayer).GetCount(crit,
                                                                     _classMap, rightDao.ClassMap);
            }
            // This is slow, but does work...
            return PseudoJoiner.Join(crit, this, rightDao).Count;
        }

        private void CreateJoinObjectsFromReader<R>(Hashtable parameters, IDataReader reader) where R : class, new()
        {
            int start = -1;
            int limit = int.MaxValue;
            if (parameters.ContainsKey("start"))
            {
                start = (int)parameters["start"];
            }
            if (parameters.ContainsKey("limit"))
            {
                limit = (int)parameters["limit"];
            }
            string leftPrefix = (string)parameters["leftPrefix"];
            string rightPrefix = (string)parameters["rightPrefix"];
            FastDAO<R> rightDao = (FastDAO<R>) parameters["rightDao"];

            List<JoinResult<T, R>> items = new List<JoinResult<T, R>>();
            int rowNum = 0;
            Dictionary<string, int> colNums = DbCaches.StringIntDicts.Get();
            PopulateColNums(reader, colNums, leftPrefix);
            rightDao.PopulateColNums(reader, colNums, rightPrefix);
            while (reader.Read())
            {
                if (rowNum++ >= start)
                {
                    items.Add(GetJoinObjectFromReader(reader, colNums, leftPrefix, rightPrefix, rightDao));
                }
                if (items.Count >= limit)
                {
                    break;
                }
            }
            DbCaches.StringIntDicts.Return(colNums);
            parameters["items"] = items;
        }

        private JoinResult<T, R> GetJoinObjectFromReader<R>(IDataReader reader,
                                                            IDictionary<string, int> colNumsByName, string leftPrefix, string rightPrefix,
                                                            FastDAO<R> rightDao) where R : class, new()
        {
            T leftObj = IsRowNull(reader, colNumsByName, leftPrefix)
                            ? default(T)
                            : GetDataObjectFromReader(reader, colNumsByName, leftPrefix);
            R rightObj = rightDao.IsRowNull(reader, colNumsByName, rightPrefix)
                             ? default(R)
                             : rightDao.GetDataObjectFromReader(reader, colNumsByName, rightPrefix);
            return new JoinResult<T, R>(leftObj, rightObj);
        }

        private bool IsRowNull(IDataReader reader,
                               IDictionary<string, int> colNumsByName, string colPrefix)
        {
            IEnumerable<string> colNamesToCheck;
            // If there are ID columns, we just need to check if those are null.  ID columns
            // are presumably not allowed to be null.
            if (_classMap.IdDataColsByObjAttrs.Count > 0)
            {
                colNamesToCheck = _classMap.IdDataColsByObjAttrs.Values;
            }
                // Otherwise, we need to check all columns.  Even that is a hacky check
                // since it is entirely possible to have a row with all nulls in it, but
                // it's the best we can do.
            else
            {
                colNamesToCheck = _classMap.AllDataColsByObjAttrs.Values;
            }

            // Now check if they're all null.
            bool allNull = true;
            foreach (string colName in colNamesToCheck)
            {
                if (!reader.IsDBNull(colNumsByName[colPrefix + colName]))
                {
                    allNull = false;
                    break;
                }
            }
            return allNull;
        }

        #endregion

        #region Internal Methods For Object -> DB
        /// <summary>
        /// This object gets a value off the data object based on the
        /// name of the field/property.
        /// </summary>
        /// <param name="dataObject">The object to get a value off of.</param>
        /// <param name="fieldName">The name of the field/property to get the value of.</param>
        /// <returns>The value.</returns>
        public virtual object GetValueFromObject(T dataObject, string fieldName)
        {
            if (dataObject == null)
            {
                throw new ArgumentNullException("dataObject", "Cannot get field '" + fieldName + "' from a null object.");
            }
            object retVal;
            MemberInfo info = _classMap.AllObjMemberInfosByObjAttr[fieldName];
            if (info.MemberType == MemberTypes.Field)
            {
                retVal = ((FieldInfo)info).GetValue(dataObject);
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                retVal = ((PropertyInfo)info).GetValue(dataObject, null);
            }
            else
            {
                throw new BadDaoConfigurationException(
                    "Somehow wound up with a member type in the mapping that wasn't a field or property: " +
                    info);
            }
            if (retVal != null)
            {
                // All attr names are in the collection, but they have null values
                // if a type wasn't specified in the mapping.
                Type desiredType = _classMap.DataColTypesByObjAttr[fieldName];
                if (desiredType != null)
                {
                    retVal = _dataAccessLayer.CoerceType(desiredType, retVal);
                }
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
        protected virtual void GetFieldValues(T dataObject, IDictionary<string, object> putEmHere,
                                              ClassMapping classMap, bool idFields, bool dataFields)
        {
            IDictionary<MemberInfo, string> fieldMapping;
            if (idFields)
            {
                if (dataFields)
                {
                    fieldMapping = classMap.AllDataColsByObjMemberInfo;
                }
                else
                {
                    fieldMapping = classMap.IdDataColsByObjMemberInfo;
                }
            }
            else
            {
                if (dataFields)
                {
                    fieldMapping = classMap.NonIdDataColsByObjMemberInfo;
                }
                else
                {
                    throw new ArgumentNullException("datafields", "GetFieldValues called for no fields.");
                }
            }
            foreach (KeyValuePair<MemberInfo, string> kvp in fieldMapping)
            {
                MemberInfo info = kvp.Key;
                string columnName = kvp.Value;
                object colValue;
                if (info.MemberType == MemberTypes.Field)
                {
                    colValue = ((FieldInfo)info).GetValue(dataObject);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    colValue = ((PropertyInfo)info).GetValue(dataObject, null);
                }
                else
                {
                    throw new BadDaoConfigurationException(
                        "Somehow wound up with a member type in the mapping that wasn't a field or property: " +
                        info);
                }
                if (colValue != null)
                {
                    // All attr names are in the collection, but they have null values
                    // if a type wasn't specified in the mapping.
                    Type desiredType = _classMap.DataColTypesByObjAttr[info.Name];
                    if (desiredType != null)
                    {
                        colValue = _dataAccessLayer.CoerceType(desiredType, colValue);
                    }
                }
                putEmHere[columnName] = colValue;
            }
        }

        /// <summary>
        /// Populates a criteria with a bunch of EqualExpressions, one for each
        /// ID on the data object.
        /// </summary>
        /// <param name="dataObject">The object we're concerned with.</param>
        /// <param name="crit">Criteria to add equals expressions to.</param>
        /// <param name="classMap">The mapping of the object to the data source.</param>
        protected virtual void PopulateIDCriteria(T dataObject, DaoCriteria crit,
                                                  ClassMapping classMap)
        {
            foreach (KeyValuePair<MemberInfo, string> kvp in classMap.IdDataColsByObjMemberInfo)
            {
                MemberInfo info = kvp.Key;
                object colValue;
                if (info.MemberType == MemberTypes.Field)
                {
                    colValue = ((FieldInfo)info).GetValue(dataObject);
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    colValue = ((PropertyInfo)info).GetValue(dataObject, null);
                }
                else
                {
                    throw new BadDaoConfigurationException(
                        "Somehow wound up with a member type in the mapping that wasn't a field or property: " +
                        info);
                }
                crit.Expressions.Add(new EqualExpression(info.Name, colValue));
            }
        }


        /// <summary>
        /// Returns true if the objects id field(s) have non-null values that, if numeric, are greater
        /// than zero.
        /// </summary>
        protected virtual bool IsIDValid(T obj)
        {
            IDictionary<string, object> vals = new Dictionary<string, object>();
            GetFieldValues(obj, vals, _classMap, true, false);
            foreach (object val in vals.Values)
            {
                if (val == null)
                {
                    return false;
                }
                int intVal;
                if (int.TryParse(val.ToString(), out intVal))
                {
                    if (intVal <= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// Updates the column list of the object we plan to insert, based on the id generator type 
        /// in the mpaping file.
        /// </summary>
        /// <param name="dataObj">The object bring inserted.</param>
        /// <param name="idCols">The list of id columns</param>
        /// <param name="cols">The current list of columns to write</param>
        protected virtual void ProcessIdColumnsForInsert(T dataObj,
                                                         IDictionary<string, object> idCols, Dictionary<string, object> cols)
        {
            foreach (string colName in _classMap.IdDataColsByObjAttrs.Values)
            {
                switch (_classMap.IdGeneratorsByDataCol[colName])
                {
                    case GeneratorType.NONE:
                        // Use whatever value has been set on the object.
                        cols[colName] = idCols[colName];
                        break;
                    case GeneratorType.AUTO:
                        // Nothing to do, the DB will handle it.
                        break;
                    case GeneratorType.SEQUENCE:
                        string sequenceName = _classMap.IdSequencesByDataCol[colName];
                        int id = _dataAccessLayer.GetNextSequenceValue(sequenceName);
                        // Use the ID we got from the sequence.
                        cols[colName] = id;
                        // Since we have the ID anyway, save the ID back to the object.
                        SetValueOnObject(dataObj, _classMap, colName, id);
                        break;
                    default:
                        throw new BadDaoConfigurationException("Column " + colName + " of type " +
                                                               _classMap.TypeName + " is supposedly an ID, but no generator type was found for it.");
                }
            }
        }

        /// <summary>
        /// Updates the appropriate record, or inserts a new record if there is no
        /// record with the primary key values of this object, based on the allow flags.
        /// Note that using this with allowUpdate and allowInsert both true can be inefficient,
        /// because if the record is new it will first attempt an update which will fail.
        /// However sometimes it is inconvenient to determine first whether the record exists,
        /// which is why this method exists.
        /// </summary>
        /// <param name="dataObject">An implementation-specific data object.  For example, if the
        ///                            implementation class is "Topic", dataObject would be expected
        ///                            to be type "TopicRecord".  An ArgumentException will be thrown
        ///                            if it is not the correct type.</param>
        ///    <param name="allowUpdate">If this is true, an existing record will be updated if one
        ///                              exists.  If allowInsert is false, only an update will be
        ///                              attempted.</param>
        ///    <param name="allowInsert">If this is true, a new record will be inserted if there is not
        ///                              one already present to update.  If allowUpdate is false, only
        ///                              an insert will be attempted.</param>
        ///    <param name="setGeneratedId">If this is true, if the action is an insert, then the object
        ///                                 that is inserted will have ID fields that were generated by
        ///                                 the database filled in.  This is supported differently on
        ///                                 different databases.</param>
        protected virtual void SaveRecord(T dataObject, bool allowUpdate, bool allowInsert, bool setGeneratedId)
        {
            if (!allowUpdate && !allowInsert)
            {
                throw new ArgumentException("allowUpdate and allowInsert are both false, cannot save record!");
            }
            if (allowUpdate && allowInsert && _dataAccessLayer.SupportsNumRecords() == false)
            {
                throw new ArgumentException("Using SaveRecord with both allowUpdate and allowInsert is not supported for the current dataAccessLayer class, because it does not return \"records affected\".");
            }

            Dictionary<string, object> colsToWrite = DbCaches.StringObjectDicts.Get();
            Dictionary<string, object> idCols = DbCaches.StringObjectDicts.Get();

            GetFieldValues(dataObject, colsToWrite, _classMap, false, true);
            GetFieldValues(dataObject, idCols, _classMap, true, false);

            if (allowUpdate)
            {
                DaoCriteria idCrit = DbCaches.Criteria.Get();
                PopulateIDCriteria(dataObject, idCrit, _classMap);
                int numRecs = _dataAccessLayer.Update(_classMap, idCrit, colsToWrite);
                DbCaches.Criteria.Return(idCrit);

                // If numRecs is zero, there was no record to update.  If allowInsert
                // is false, that's an error.  If allowInsert is true, fall out of here and we'll try to
                // do the insert in the next block.  If there was one record, all is well with the world
                // and there's no need to do an insert, so set allowInsert to false.
                switch (numRecs)
                {
                    case 0:
                        // If numRecs is zero, there was no record to update.  If allowInsert
                        // is false, that's an error.  If allowInsert is true, fall out of here and we'll try to
                        // do the insert down below.
                        if (!allowInsert)
                        {
                            throw new Exception("No record updated");
                        }
                        break;
                    case 1:
                        // If there was one record, all is well with the world
                        // and there's no need to do an insert, so set allowInsert to false.
                        allowInsert = false;
                        break;
                    case UNKNOWN_NUM_ROWS:
                        // The data access layer doesn't know how many rows were updated, but
                        // it didn't throw an exception, so we'll assume it worked.
                        break;
                    default:
                        // If numRecs is greater than 1 (it should never return less than 0) then we
                        // updated more than one record.  Since we should be only updating a single
                        // row, that's a problem.
                        throw new Exception("Too many (" + numRecs + ") records updated (should have been 1)");
                }
            }
            if (allowInsert)
            {
                ProcessIdColumnsForInsert(dataObject, idCols, colsToWrite);

                _dataAccessLayer.Insert(_classMap, colsToWrite);

                if (setGeneratedId)
                {
                    PostInsert(dataObject);
                }
            }

            DbCaches.StringObjectDicts.Return(colsToWrite);
            DbCaches.StringObjectDicts.Return(idCols);
        }

        /// <summary>
        /// Called after an insert, may be overridden to do anything necessary after inserting
        /// this object type.  An example (the default behavior) is to set the auto-generated
        /// ID values back on the object that was saved.
        /// 
        /// NOTE: This appears to only be called if setGeneratedId is true, which means NOT
        ///       after every insert!
        /// </summary>
        /// <param name="insertee">Object that was just inserted into the DB.</param>
        protected virtual void PostInsert(T insertee)
        {
            // Check ID type, only need to do something if it's auto-generated.
            foreach (string idCol in _classMap.IdDataColsByObjAttrs.Values)
            {
                if (_classMap.IdGeneratorsByDataCol[idCol] == GeneratorType.AUTO)
                {
                    SetValueOnObject(insertee, _classMap, idCol,
                                     _dataAccessLayer.GetLastAutoGeneratedId(_classMap, idCol));
                }
            }
        }
        #endregion

        #region Internal Methods For DB -> Object
        /// <summary>
        /// Populates the dictionary of column name to index mappings, so that
        /// we can minimize the number of times we call GetOrdinal.
        /// </summary>
        /// <param name="reader">Reader that has been generated from some query.</param>
        /// <param name="colNums">Mapping dictionary to populate.</param>
        /// <param name="colNamePrefix">The prefix (if any) to use for looking up our
        ///                             columns from the data reader.  I.E. "TableName." or
        ///                             "TableAlias." or whatever.</param>
        private void PopulateColNums(IDataReader reader,
                                     IDictionary<string, int> colNums, string colNamePrefix)
        {
            foreach (string colName in _classMap.AllDataColsByObjAttrs.Values)
            {
                string prefixedName = colNamePrefix + colName;
                try
                {
                    colNums[prefixedName] = reader.GetOrdinal(prefixedName);
                }
                catch (Exception e)
                {
                    throw new LoggingException("The " + _classMap + " has attribute '" +
                                               _classMap.AllObjAttrsByDataCol[colName] + "' mapped to column '" + colName + 
                                               "', but that column was not present in the results of our query.", e);
                }
            }
        }

        /// <summary>
        /// Given an object and a (data source) column name
        /// set the given memberValue onto the object's property.
        /// </summary>
        /// <param name="dataObj">Object to set the value upon.</param>
        /// <param name="classMap">Object's mapping.</param>
        /// <param name="colName">Name of the column we got the value from.</param>
        /// <param name="memberValue">Value to set on the field or property.</param>
        protected virtual void SetValueOnObject(T dataObj, ClassMapping classMap,
                                                string colName, object memberValue)
        {
            MemberInfo info = classMap.AllObjMemberInfosByDataCol[colName];
            // Don't call MemberType getter twice
            MemberTypes type = info.MemberType;
            if (type == MemberTypes.Field)
            {
                FieldInfo fInfo = ((FieldInfo)info);
                object newValue = memberValue == null ? null : _dataAccessLayer.CoerceType(fInfo.FieldType, memberValue);
                fInfo.SetValue(dataObj, newValue);
            }
            else if (type == MemberTypes.Property)
            {
                PropertyInfo pInfo = ((PropertyInfo)info);
                object newValue = memberValue == null ? null : _dataAccessLayer.CoerceType(pInfo.PropertyType, memberValue);
                pInfo.SetValue(dataObj, newValue, null);
            }
        }

        /// <summary>
        /// This method returns an object that has been loaded from the current
        /// row of the data reader.
        /// </summary>
        /// <param name="reader">The reader, which should already be positioned on the row to read.</param>
        /// <param name="colNums">A dictionary of column name to index mappings (faster than calling
        ///                       GetOrdinal over and over again).</param>
        /// <param name="colPrefix">The prefix (if any) to use when looking for columns by name.</param>
        /// <returns>The newly loaded data object.</returns>
        protected virtual T GetDataObjectFromReader(IDataReader reader,
                                                    IDictionary<string, int> colNums, string colPrefix)
        {
            T retVal = new T();
            foreach (string colName in _classMap.AllDataColsByObjAttrs.Values)
            {
                // It is possible for the object to have fields that don't exist
                // in the database (or at least in the cols returned by this query).
                if (colName != null)
                {
                    // Prefix the name with the prefix.
                    int colIndex = colNums[colPrefix + colName];
                    if (!reader.IsDBNull(colIndex))
                    {
                        SetValueOnObject(retVal, _classMap, colName, reader[colIndex]);
                    }
                    else
                    {
                        SetValueOnObject(retVal, _classMap, colName, null);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Reads all records from the reader, creating objects and inserting them into
        /// the parameters hashtable as a collection called "items".  An exception on
        /// any one record will cause this method to fail.
        /// </summary>
        protected virtual void CreateObjectsFromReader(Hashtable parameters, IDataReader reader)
        {
            int start = -1;
            int limit = int.MaxValue;
            if (parameters.ContainsKey("start"))
            {
                start = (int)parameters["start"];
            }
            if (parameters.ContainsKey("limit"))
            {
                limit = (int)parameters["limit"];
            }
            IList<T> items = new List<T>();
            int rowNum = 0;
            Dictionary<string, int> colNums = DbCaches.StringIntDicts.Get();
            PopulateColNums(reader, colNums, null);
            while (reader.Read())
            {
                if (rowNum++ >= start)
                {
                    items.Add(GetDataObjectFromReader(reader, colNums, null));
                }
                if (items.Count >= limit)
                {
                    break;
                }
            }
            DbCaches.StringIntDicts.Return(colNums);
            parameters["items"] = items;
        }

        /// <summary>
        /// Reads records one at a time from the reader, creating objects and calling
        /// the 'invokeMe' delegate for each one.  An exception while processing any
        /// one object will cause this method to fail.
        /// </summary>
        protected virtual void IterateOverObjectsFromReader<P>(Hashtable parameters, IDataReader reader)
        {
            int count = 0;
            int max = (int)parameters["max"];
            string desc = parameters["desc"].ToString();

            DaoIterationDelegate<T, P> invokeMe = (DaoIterationDelegate<T, P>)parameters["invokeMe"];
            P itsParams = (P)parameters["parameters"];
            // Log memory usage.
            System.Diagnostics.PerformanceCounter counter =
                new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            Dictionary<string, int> colNums = DbCaches.StringIntDicts.Get();

            PopulateColNums(reader, colNums, null);
            while (reader.Read())
            {
                count++;
                if (count > max)
                {
                    count--; // make the return value correct.
                    break;
                }
                if (count % 1000 == 0)
                {
                    _log.Info(desc + " iteration: " + count);
                    _log.Info("Memory: " + counter.NextValue() + "Mb");
                }

                invokeMe(itsParams, GetDataObjectFromReader(reader, colNums, null));
            }
            DbCaches.StringIntDicts.Return(colNums);
            parameters["count"] = count;
        }

        /// <summary>
        /// Helper that does the work once the query is created.
        /// </summary>
        /// <param name="query">The query to execute that is expected to return a large
        ///                      number of rows.</param>
        /// <param name="invokeMe">The method to invoke for each object returned by the query.</param>
        /// <param name="parameters">Any parameters that you want to pass to the invokeMe method.
        ///                            This may be null.</param>
        /// <param name="max">maximum number of records to iterate over, int.MaxValue will mean no limit.</param>
        /// <param name="desc">Description of the loop for logging purposes.</param>
        /// <returns>the number of records iterated over.</returns>
        private int IterateOverQuery<P>(IDaQuery query, DaoIterationDelegate<T, P> invokeMe,
                                     P parameters, int max, string desc)
        {
            Hashtable myParameters = DbCaches.Hashtables.Get();
            myParameters["parameters"] = parameters;
            myParameters["invokeMe"] = invokeMe;
            myParameters["max"] = max;
            myParameters["desc"] = desc;

            _dataAccessLayer.ExecuteQuery(_classMap, query,
                                          IterateOverObjectsFromReader<P>, myParameters);

            int retVal = (int)myParameters["count"];
            DbCaches.Hashtables.Return(myParameters);
            return retVal;
        }
        #endregion

        /// <summary>
        /// ToString implementation.
        /// </summary>
        /// <returns>The type name and what table it is mapped to.</returns>
        public override string ToString()
        {
            return "FastDao: " + typeof(T).Name + " -> " + Table;
        }
    }
}
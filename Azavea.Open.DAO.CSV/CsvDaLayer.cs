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
using System.IO;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.Common.Collections;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Criteria.Grouping;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.Unqueryable;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.CSV
{
    /// <summary>
    /// Data layer that implements reading/writing/modifying CSV files.
    /// NOTE: Updates/Deletes will require re-writing the file, so expect some churn
    /// if you're doing a lot of those.  Inserts will be appended to the end of the file.
    /// </summary>
    public class CsvDaLayer : UnqueryableDaLayer
    {
        /// <summary>
        /// We want to treat it as a CSV descriptor rather than a generic connection descriptor.
        /// </summary>
        protected new CsvDescriptor _connDesc;

        /// <summary>
        /// Instantiates the data access layer with the connection descriptor for the DB.
        /// </summary>
        /// <param name="connDesc">The connection descriptor that is being used by this FastDaoLayer.</param>
        public CsvDaLayer(CsvDescriptor connDesc)
            : base(connDesc, true)
        {
            _connDesc = connDesc;
        }

        /// <summary>
        /// Deletes a data object record using the mapping and criteria for what's deleted.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="crit">Criteria for deletion.  NOTE: Only the expressions are observed,
        ///                    other things (like "order" or start / limit) are ignored.
        ///                    WARNING: A null or empty (no expression) criteria will 
        ///                    delete ALL records!</param>
        /// <param name="mapping">The mapping of the table from which to delete.</param>
        /// <returns>The number of records affected.</returns>
        public override int Delete(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support deleting: " + _connDesc);
            }

            // No way to selectively delete text from a text file, so instead we copy all
            // the rows that don't match the criteria into a new file.
            string existingFile = GetFileName(mapping);
            string newFile = existingFile + ".new";
            DaoCriteria inverseCrit = new DaoCriteria();
            foreach (IExpression expr in crit.Expressions)
            {
                inverseCrit.Expressions.Add(expr.Invert());
            }
            CsvDataReader reader = new CsvDataReader(this, mapping, inverseCrit);
            try
            {
                TextWriter newWriter = new StreamWriter(newFile, false);
                try
                {
                    newWriter.WriteLine(MakeHeaderRow(mapping));
                    int numCols = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int x = 0; x < numCols; x++)
                        {
                            if (x > 0)
                            {
                                newWriter.Write(",");
                            }
                            newWriter.Write(QuoteValue(reader.GetValue(x)));
                        }
                        newWriter.WriteLine();
                    }
                }
                finally
                {
                    newWriter.Close();
                }
            }
            finally
            {
                reader.Close();
            }
            // Now move the old file out of the way and replace it with the new one.
            File.Replace(newFile, existingFile, existingFile + ".old", true);
            return FastDAO<object>.UNKNOWN_NUM_ROWS;
        }

        /// <summary>
        /// Blanks the file, leaving nothing but the header row (if there is one).
        /// </summary>
        public override void Truncate(ClassMapping mapping)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support truncating: " + _connDesc);
            }
            // Just open a stream, overwriting the file.
            WriterInfo info = GetWriter(mapping, false);
            try
            {
                if (UseNamedColumns(mapping))
                {
                    info.Writer.WriteLine(MakeHeaderRow(mapping));
                }
            }
            finally
            {
                DoneWithWriter(info);
            }
        }

        /// <summary>
        /// Creates the first row of the CSV file, the header row.
        /// </summary>
        private string MakeHeaderRow(ClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string colName in mapping.AllDataColsInOrder)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                sb.Append(QuoteValue(colName));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a new row for the CSV file.  Will create blanks (",,") if using
        /// numeric indexing and the numbers are not consecutive.
        /// </summary>
        private string MakeDataRow(ClassMapping mapping, IDictionary<string, object> values)
        {
            StringBuilder sb = new StringBuilder();
            if (UseNamedColumns(mapping))
            {
                bool first = true;
                foreach (string colName in mapping.AllDataColsInOrder)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    if (mapping.IdGeneratorsByDataCol.ContainsKey(colName))
                    {
                        // This is an ID column with a generator.  NONE is the only supported value, because
                        // CSVs don't support generating IDs.
                        switch (mapping.IdGeneratorsByDataCol[colName])
                        {
                            case GeneratorType.NONE:
                                // This is OK, nothing to do.
                                break;
                            default:
                                throw new LoggingException("Unsupported generator type " +
                                                           mapping.IdGeneratorsByDataCol[colName] + " for mapping " +
                                                           mapping);
                        }
                    }
                    sb.Append(QuoteValue(values[colName]));
                }
            }
            else
            {
                int last = 1;
                foreach (string colName in mapping.AllDataColsInOrder)
                {
                    int thisNum = int.Parse(colName);
                    if ((thisNum != 1) && (thisNum <= last))
                    {
                        throw new LoggingException(
                            "Column names must be mapped in ascending order, " +
                            thisNum + " came after " + last + " in mapping " + mapping);
                    }
                    while (last < thisNum)
                    {
                        sb.Append(",");
                        last++;
                    }
                    sb.Append(QuoteValue(values[colName]));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Uses the OutputQuoteLevel on the connection descriptor plus the value
        /// itself to determine if we need to quote it, and if so quote it.  Also
        /// converts the value to a string, and if it is already a string, escapes
        /// internal quotes in it.
        /// </summary>
        private string QuoteValue(object val)
        {
            // Nulls don't get anything, they should just appear as ",,"
            if (val == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            bool useQuotes = false;
            switch (_connDesc.OutputQuoteLevel)
            {
                case CsvQuoteLevel.QuoteAlways:
                    useQuotes = true;
                    break;
                case CsvQuoteLevel.QuoteStrings:
                    if (val is string)
                    {
                        useQuotes = true;
                    }
                    break;
                case CsvQuoteLevel.QuoteBareMinimum:
                    // Only quote it if it is a string that will break the CSV format.
                    if (val is string)
                    {
                        string strVal = (string) val;
                        if (strVal.IndexOfAny(new char[] {',', '"', '\n'}) != -1)
                        {
                            useQuotes = true;
                        }
                    }
                    break;
                default:
                    throw new Exception("Unknown quote level: " + _connDesc.OutputQuoteLevel);
            }
            if (useQuotes)
            {
                // open quote.
                sb.Append('"');
                if (val is string)
                {
                    // replace any double quotes with double double quotes, that is how they
                    // are escaped in a CSV.
                    val = ((string)val).Replace("\"", "\"\"");
                }
            }

            // The value itself.
            sb.Append(val);

            if (useQuotes)
            {
                // close quote.
                sb.Append('"');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Checks whether this mapping is using column indexes or named columns.
        /// If named columns, returns true (and there must be a header row).  If
        /// numerically indexed columns, returns false (we assume there is not a header row).
        /// </summary>
        /// <param name="mapping">Mapping for this file.</param>
        /// <returns>Whether or not we're using named columns.</returns>
        protected internal static bool UseNamedColumns(ClassMapping mapping)
        {
            bool allNumeric = true;
            foreach (string colName in mapping.AllDataColsInOrder)
            {
                int unused;
                if (!int.TryParse(colName, out unused))
                {
                    allNumeric = false;
                    break;
                }
            }
            return !allNumeric;
        }

        /// <summary>
        /// Inserts a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValues">A dictionary of "column"/value pairs for the object to insert.</param>
        /// <returns>The number of records affected.</returns>
        public override int Insert(ITransaction transaction, ClassMapping mapping, IDictionary<string, object> propValues)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                case CsvConnectionType.Writer:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support inserting: " + _connDesc);
            }
            WriterInfo info = GetWriter(mapping, true);
            try
            {
                if (info.NeedsHeader)
                {
                    info.Writer.WriteLine(MakeHeaderRow(mapping));
                }
                info.Writer.WriteLine(MakeDataRow(mapping, propValues));
                _connDesc.HasBeenWrittenTo = true;
                return 1;
            }
            finally
            {
                DoneWithWriter(info);
            }
        }

        /// <summary>
        /// Inserts a list of data object records of the same type.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.  
        ///                                     Each item in the list should represent the dictionary of column/value pairs for 
        ///                                     each respective object being inserted.</param>
        public override void InsertBatch(ITransaction transaction, ClassMapping mapping, List<IDictionary<string, object>> propValueDictionaries)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                case CsvConnectionType.Writer:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support inserting in batch: " + _connDesc);
            }
            WriterInfo info = GetWriter(mapping, true);
            try
            {
                if (info.NeedsHeader)
                {
                    info.Writer.WriteLine(MakeHeaderRow(mapping));
                }
                foreach (IDictionary<string, object> propValues in propValueDictionaries)
                {
                    info.Writer.WriteLine(MakeDataRow(mapping, propValues));
                }
                _connDesc.HasBeenWrittenTo = true;
            }
            finally
            {
                DoneWithWriter(info);
            }
        }

        /// <summary>
        /// Updates a data object record using the "table" and a list of column/value pairs.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="crit">All records matching this criteria will be updated per the dictionary of
        ///                    values.</param>
        /// <param name="propValues">A dictionary of column/value pairs for all non-ID columns to be updated.</param>
        /// <returns>The number of records affected.</returns>
        public override int Update(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, IDictionary<string, object> propValues)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support updating: " + _connDesc);
            }
            // No way to selectively update text from a text file, so instead we first copy all
            // the rows that don't match the criteria into a new file.
            string existingFile = GetFileName(mapping);
            string newFile = existingFile + ".new";
            DaoCriteria inverseCrit = new DaoCriteria();
            foreach (IExpression expr in crit.Expressions)
            {
                inverseCrit.Expressions.Add(expr.Invert());
            }
            TextWriter newWriter = new StreamWriter(newFile, false);
            int rowsUpdated = 0;
            try
            {
                newWriter.WriteLine(MakeHeaderRow(mapping));
                // Copy the rows that don't match...
                CsvDataReader reader = new CsvDataReader(this, mapping, inverseCrit);
                try
                {
                    int numCols = reader.FieldCount;
                    while (reader.Read())
                    {
                        for (int x = 0; x < numCols; x++)
                        {
                            if (x > 0)
                            {
                                newWriter.Write(",");
                            }
                            newWriter.Write(QuoteValue(reader.GetValue(x)));
                        }
                        newWriter.WriteLine();
                    }
                }
                finally
                {
                    reader.Close();
                }
                // Copy (modified) the rows that do match...
                reader = new CsvDataReader(this, mapping, crit);
                try
                {
                    IDictionary<int, object> replacements = new CheckedDictionary<int, object>();
                    foreach (KeyValuePair<string, object> kvp in propValues)
                    {
                        replacements[reader.GetColumnIndex(kvp.Key)] = kvp.Value;
                    }
                    int numCols = reader.FieldCount;
                    while (reader.Read())
                    {
                        rowsUpdated++;
                        for (int x = 0; x < numCols; x++)
                        {
                            if (x > 0)
                            {
                                newWriter.Write(",");
                            }
                            // Use the updated value if one was provided.
                            object val = replacements.ContainsKey(x)
                                             ? replacements[x] : reader.GetValue(x);
                            newWriter.Write(QuoteValue(val));
                        }
                        newWriter.WriteLine();
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                newWriter.Close();
            }
            // Now move the old file out of the way and replace it with the new one.
            File.Replace(newFile, existingFile, existingFile + ".old", true);
            return rowsUpdated;
        }

        /// <summary>
        /// Updates a list of data object records of the same type.
        /// NOTE: At the moment this just loops calling Update().
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">The mapping of the table or other data container we're dealing with.</param>
        /// <param name="criteriaList">A list of DaoCriteria.
        ///                            Each item in the list should represent the criteria for 
        ///                            rows that will be updated per the accompanying dictionary.</param>
        /// <param name="propValueDictionaries">A list of dictionaries of column/value pairs.
        ///                                   Each item in the list should represent the dictionary of non-ID column/value pairs for 
        ///                                   each respective object being updated.</param>
        public override void UpdateBatch(ITransaction transaction, ClassMapping mapping, List<DaoCriteria> criteriaList, List<IDictionary<string, object>> propValueDictionaries)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support updating in batch: " + _connDesc);
            }
            base.UpdateBatch(transaction, mapping, criteriaList, propValueDictionaries);
        }

        /// <summary>
        /// Executes a query and invokes a method with a DataReader of results.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">Class mapping for the table we're querying against.  Optional,
        ///                       but not all columns may be properly typed if it is null.</param>
        /// <param name="query">The query to execute, should have come from CreateQuery.</param>
        /// <param name="invokeMe">The method to invoke with the IDataReader results.</param>
        /// <param name="parameters">A hashtable containing any values that need to be persisted through invoked method.
        ///                          The list of objects from the query will be placed here.</param>
        public override void ExecuteQuery(ITransaction transaction, ClassMapping mapping, IDaQuery query, DataReaderDelegate invokeMe, Hashtable parameters)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                case CsvConnectionType.Reader:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support querying: " + _connDesc);
            }
            CsvDataReader reader = new CsvDataReader(this, mapping, ((UnqueryableQuery)query).Criteria);
            try
            {
                invokeMe.Invoke(parameters, reader);
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Finds the last generated id number for a column.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="mapping">The class mapping for the table being queried.</param>
        /// <param name="idCol">The ID column for which to find the last-generated ID.</param>
        public override object GetLastAutoGeneratedId(ITransaction transaction, ClassMapping mapping, string idCol)
        {
            throw new NotImplementedException("CSVs can't autogenerate IDs.");
        }

        /// <summary>
        /// Gets the next id number from a sequence in the data source.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <returns>The next number from the sequence.</returns>
        public override int GetNextSequenceValue(ITransaction transaction, string sequenceName)
        {
            throw new NotImplementedException("CSVs don't have sequences.");
        }

        /// <summary>
        /// Gets a count of records for the given criteria.
        /// </summary>
        /// <param name="transaction">Should be null, transactions are not supported.</param>
        /// <param name="crit">The criteria to use for "where" comparisons.</param>
        /// <param name="mapping">The mapping of the table for which to build the query string.</param>
        /// <returns>The number of results found that matched the criteria.</returns>
        public override int GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                case CsvConnectionType.Reader:
                    // These are OK.
                    break;
                default:
                    throw new LoggingException("Connection does not support counting records: " + _connDesc);
            }
            CsvDataReader reader = new CsvDataReader(this, mapping, crit);
            int retVal = 0;
            try
            {
                while (reader.Read())
                {
                    retVal++;
                }
            }
            finally
            {
                reader.Close();
            }
            return retVal;
        }

        public override List<GroupCountResult> GetCount(ITransaction transaction, ClassMapping mapping, DaoCriteria crit, ICollection<AbstractGroupExpression> groupExpressions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Overridden to handle the case of converting an empty string to
        /// a non-string datatype.  It's a slow check because you have to trim,
        /// and check type, etc, which is why it isn't in the base class.  But CSV
        /// files apparently frequently use "" for blank numerical values which causes
        /// the base class implementation to error out.
        /// </summary>
        /// <param name="desiredType">Type we need the value to be.</param>
        /// <param name="input">Input value, may or may not already be the right type.</param>
        /// <returns>An object of type desiredType whose value is equal to the input.</returns>
        public override object CoerceType(Type desiredType, object input)
        {
            try
            {
                // If we're on a non-string data type,
                // treat a blank string as a null.
                if (!desiredType.Equals(typeof (string)) &&
                    (input != null) &&
                    (input is string) &&
                    (((string) input).Trim().Length == 0))
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new DaoTypeCoercionException(desiredType, input, e);
            }
            return base.CoerceType(desiredType, input);
        }

        private string GetFileName(ClassMapping mapping)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                    // Use the table name as the file name.
                    return Path.Combine(_connDesc.Path, mapping.Table + ".csv");
                case CsvConnectionType.FileName:
                    // Only one file, just open a reader to it.
                    return _connDesc.Path;
                default:
                    throw new LoggingException("Unable to get filename for class " +
                                               mapping + " for unsupported connection type: " + _connDesc.Type);
            }
        }
        /// <summary>
        /// Gets a valid StreamReader that can be used to read CSV data.
        /// If we were configured with a StreamReader, it may be that one.  If we
        /// are accessing a file we will open a new reader.
        /// 
        /// You should not close it yourself, instead you should call DoneWithReader
        /// when you are done with it.
        /// </summary>
        /// <param name="mapping">The mapping for the object we intend to read.</param>
        /// <returns>A reader that can be used to access the CSV data.  DO NOT CLOSE IT YOURSELF.</returns>
        protected internal StreamReader GetReader(ClassMapping mapping)
        {
            switch (_connDesc.Type)
            {
                case CsvConnectionType.Directory:
                case CsvConnectionType.FileName:
                    return new StreamReader(GetFileName(mapping));
                case CsvConnectionType.Reader:
                    // We have a reader specifically set.
                    // Make sure we're back at the beginning.
                    _connDesc.Reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    _connDesc.Reader.DiscardBufferedData();
                    return _connDesc.Reader;
                case CsvConnectionType.Writer:
                    throw new LoggingException("Connection for class " +
                                               mapping + " was configured as read-only: " + _connDesc.Type);
                default:
                    throw new LoggingException("Unable to get reader for class " +
                                               mapping + " for unsupported connection type: " + _connDesc.Type);
            }
        }
        /// <summary>
        /// Returns the reader for closing (or not, if we were configured with it).
        /// </summary>
        /// <param name="reader">The StreamReader obtained from a GetReader call.</param>
        protected internal void DoneWithReader(StreamReader reader)
        {
            // Close it unless we were configured with it.
            if (_connDesc.Type != CsvConnectionType.Reader)
            {
                reader.Close();
            }
        }
        /// <summary>
        /// Gets a valid TextWriter that can be used to output CSV data.
        /// If we were configured with a TextWriter, it may be that one.  If we
        /// are accessing a file we will open a new writer.
        /// 
        /// You should not close it yourself, instead you should call DoneWithWriter
        /// when you are done with it.
        /// </summary>
        /// <param name="mapping">The mapping for the object we intend to write.</param>
        /// <param name="append">Whether to append to, or overwrite, the file if it exists.
        ///                      If it does not exist, this parameter doesn't matter and a new
        ///                      file is created.</param>
        /// <returns>A writer that can be used to output the CSV data.  DO NOT CLOSE IT YOURSELF.</returns>
        protected internal WriterInfo GetWriter(ClassMapping mapping, bool append)
        {
            try
            {
                switch (_connDesc.Type)
                {
                    case CsvConnectionType.Directory:
                    case CsvConnectionType.FileName:
                        return new WriterInfo(GetFileName(mapping), append, UseNamedColumns(mapping));
                    case CsvConnectionType.Reader:
                        throw new LoggingException("Connection for class " +
                                                   mapping + " was configured as write-only: " + _connDesc.Type);
                    case CsvConnectionType.Writer:
                        // We have a writer specifically set.
                        return new WriterInfo(_connDesc.Writer,
                                              (!_connDesc.HasBeenWrittenTo) && UseNamedColumns(mapping));
                    default:
                        throw new LoggingException("Unable to get writer for class " +
                                                   mapping + " for unsupported connection type: " + _connDesc.Type);
                }
            }
            catch (Exception e)
            {
                throw new LoggingException("Unable to get writer for connection " +
                                           _connDesc + ", mapping " + mapping, e);
            }
        }
        /// <summary>
        /// Returns the writer for closing (or not, if we were configured with it).
        /// </summary>
        /// <param name="info">The WriterInfo obtained from a GetWriter call.</param>
        protected internal void DoneWithWriter(WriterInfo info)
        {
            // Close it unless we were configured with it.
            if (_connDesc.Type != CsvConnectionType.Writer)
            {
                info.Writer.Close();
            }
        }
    }

    /// <summary>
    /// We need the writer but we also need to know whether we should write a header to it
    /// or not.
    /// </summary>
    public class WriterInfo
    {
        /// <summary>
        /// The writer to write CSV data to.
        /// </summary>
        public TextWriter Writer;
        /// <summary>
        /// Whether we should write a header line (I.E. false if
        /// there is already data there or we already wrote one).
        /// </summary>
        public bool NeedsHeader;
        /// <summary>
        /// Create the info and initialize the fields.
        /// </summary>
        /// <param name="writer">The writer to write CSV data to.</param>
        /// <param name="needsHeader">Whether we should write a header line (I.E. false if
        ///                           there is already data there or we already wrote one).</param>
        public WriterInfo(TextWriter writer, bool needsHeader)
        {
            Writer = writer;
            NeedsHeader = needsHeader;
        }
        /// <summary>
        /// Create the info and initialize the fields.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="append">True to append to an existing file, false to replace it.</param>
        /// <param name="namedColumns">Are we using named columns or numerical indexes.</param>
        public WriterInfo(string filePath, bool append, bool namedColumns)
        {
            // Start out assuming we need a header if we use named columns.
            NeedsHeader = namedColumns;
            if (namedColumns)
            {
                // Don't bother to do this work if we don't use named columns anyway.
                if (append)
                {
                    FileInfo info = new FileInfo(filePath);
                    if (info.Exists)
                    {
                        if (info.Length > 0)
                        {
                            // If we're appending, and it already exists, and it isn't empty,
                            // then assume we do NOT need to write the header.
                            NeedsHeader = false;
                        }
                    }
                }
            }
            Writer = new StreamWriter(filePath, append);
        }
    }
}
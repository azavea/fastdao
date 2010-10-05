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
using Azavea.Open.DAO.Criteria;
using Azavea.Open.DAO.Unqueryable;

namespace Azavea.Open.DAO.Memory
{
    /// <summary>
    /// A data reader that iterates over the objects from the memory store,
    /// sorting and filtering as necessary to satisfy the criteria.
    /// </summary>
    public class MemoryDataReader : UnqueryableDataReader
    {
        private bool _isClosed;
        private readonly IEnumerator<MemoryObject> _objects;

        /// <summary>
        /// Create the data reader.
        /// </summary>
        /// <param name="layer">Layer creating it.</param>
        /// <param name="mapping">Mapping for the class stored in the data store.</param>
        /// <param name="criteria">Criteria for which instances you want.</param>
        /// <param name="objects">Iterator over the list of objects.</param>
        public MemoryDataReader(UnqueryableDaLayer layer, ClassMapping mapping, DaoCriteria criteria,
            IEnumerator<MemoryObject> objects)
            : base(layer, mapping, criteria, GetConfig(mapping))
        {
            _objects = objects;

            PreProcessSorts();
        }

        private static DataReaderConfig GetConfig(ClassMapping mapping)
        {
            DataReaderConfig retVal = new DataReaderConfig();
            int colNum = 0;
            foreach (string colName in mapping.AllDataColsInOrder)
            {
                retVal.IndexesByName[colName] = colNum++;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <returns>
        /// The name of the field or the empty string (""), if there is no value to return.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override string GetName(int i)
        {
            return Mapping.AllDataColsInOrder[i];
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override Type GetFieldType(int i)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object" /> in the array.
        /// </returns>
        /// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into. </param><filterpriority>2</filterpriority>
        public override int GetValues(object[] values)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation. </param>
        /// <param name="buffer">The buffer into which to read the stream of bytes. </param>
        /// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation. </param>
        /// <param name="length">The number of bytes to read. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <param name="fieldoffset">The index within the row from which to start the read operation. </param>
        /// <param name="buffer">The buffer into which to read the stream of bytes. </param>
        /// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation. </param>
        /// <param name="length">The number of bytes to read. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <returns>
        /// The GUID value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override Guid GetGuid(int i)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader" />.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public override IDataReader GetData(int i)
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader" /> Object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Close()
        {
            _isClosed = true;
        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.IDataReader" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.IDataReader" /> is closed. </exception><filterpriority>2</filterpriority>
        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool NextResult()
        {
            throw new NotImplementedException("Not needed at this time.");
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The level of nesting.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int Depth
        {
            get { throw new NotImplementedException("Not needed at this time."); }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <returns>
        /// true if the data reader is closed; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsClosed
        {
            get { return _isClosed; }
        }

        /// <summary>
        /// Returns the "row" from the data source, which we will then determine whether it
        /// matches the criteria, needs to be sorted, etc.
        /// </summary>
        protected override IList ReadRawRow()
        {
            if (!_objects.MoveNext())
            {
                return null;
            }
            IList retVal = new List<object>();
            MemoryObject obj = _objects.Current;
            for (int x = 0; x < _namesByIndex.Length; x++)
            {
                retVal.Add(obj.ColValues[_namesByIndex[x]]);
            }
            return retVal;
        }

        /// <summary>
        /// Gets the current row as a memory object rather than piecemeal.
        /// </summary>
        /// <returns>An object representing this row.</returns>
        public MemoryObject GetCurrentObject()
        {
            IDictionary<string, object> currentValues = new Dictionary<string, object>();
            for (int x = 0; x < _namesByIndex.Length; x++)
            {
                currentValues[_namesByIndex[x]] = _valsByIndex[x];
            }
            return new MemoryObject(Mapping, currentValues);
        }
    }
}

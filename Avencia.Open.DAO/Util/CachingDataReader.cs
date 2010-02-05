// Copyright (c) 2004-2010 Avencia, Inc.
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
using System.Data;
using Avencia.Open.Common.Collections;
using log4net;

namespace Avencia.Open.DAO.Util
{
    /// <summary>
    /// A base class to save copying a whole lot of common code for different IDataReader
    /// implementations.
    /// </summary>
    public abstract class CachingDataReader : IDataReader
    {
        /// <summary>
        /// Logger that may be used by this class or its children.
        /// </summary>
        protected ILog _log = LogManager.GetLogger(
            new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);
        /// <summary>
        /// The number of columns that the data reader can read.
        /// </summary>
        protected readonly int _numCols;
        /// <summary>
        /// The column indexes, keyed by the column name.
        /// NOTE: This must be populated by the implementing class constructor!
        /// </summary>
        protected readonly IDictionary<string, int> _indexesByName =
            new CheckedDictionary<string, int>(new CaseInsensitiveStringComparer());
        /// <summary>
        /// The column names, in order from the data source.
        /// NOTE: This must be populated by the implementing class constructor!
        /// </summary>
        protected string[] _namesByIndex;
        /// <summary>
        /// The values for this row, in column order.
        /// </summary>
        protected readonly object[] _valsByIndex;

        /// <summary>
        /// Create the data reader.
        /// </summary>
        /// <param name="numCols">How many columns are there, used to set up some of the cache info.</param>
        protected CachingDataReader(int numCols)
        {
            _numCols = numCols;
            _valsByIndex = new object[_numCols];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <returns>
        /// The name of the field or the empty string (""), if there is no value to return.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public abstract string GetName(int i);

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <returns>
        /// The data type information for the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Type" /> information corresponding to the type of <see cref="T:System.Object" /> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)" />.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public abstract Type GetFieldType(int i);

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object" /> which will contain the field value upon return.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual object GetValue(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return GetCachedValue(i);
        }

        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object" /> in the array.
        /// </returns>
        /// <param name="values">An array of <see cref="T:System.Object" /> to copy the attribute fields into. </param><filterpriority>2</filterpriority>
        public abstract int GetValues(object[] values);

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        /// <param name="name">The name of the field to find. </param><filterpriority>2</filterpriority>
        public virtual int GetOrdinal(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", "Name cannot be null.");
            }
            return _indexesByName[name];
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual bool GetBoolean(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToBoolean(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <returns>
        /// The 8-bit unsigned integer value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual byte GetByte(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToByte(GetCachedValue(i));
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
        public abstract long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <returns>
        /// The character value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual char GetChar(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToChar(GetCachedValue(i));
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
        public abstract long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length);

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <returns>
        /// The GUID value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public abstract Guid GetGuid(int i);

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 16-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual short GetInt16(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToInt16(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual int GetInt32(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToInt32(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 64-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual long GetInt64(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToInt64(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <returns>
        /// The single-precision floating point number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual float GetFloat(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return (float)GetCachedValue(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <returns>
        /// The double-precision floating point number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual double GetDouble(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToDouble(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <returns>
        /// The string value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual string GetString(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            object retVal = GetCachedValue(i);
            return (retVal == null ? null : retVal.ToString());
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <returns>
        /// The fixed-position numeric value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual decimal GetDecimal(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToDecimal(GetCachedValue(i));
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <returns>
        /// The date and time data value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual DateTime GetDateTime(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return Convert.ToDateTime(GetCachedValue(i));
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.IDataReader" /> for the specified column ordinal.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader" />.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public abstract IDataReader GetData(int i);

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <returns>
        /// true if the specified field is set to null; otherwise, false.
        /// </returns>
        /// <param name="i">The index of the field to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        public virtual bool IsDBNull(int i)
        {
            if ((i < 0) || (i >= _numCols))
            {
                throw new ArgumentOutOfRangeException("i", "Column index must be >= 0 and < " + _numCols + ".");
            }
            return (GetCachedValue(i) == null);
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <returns>
        /// When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The default is -1.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual int FieldCount
        {
            get { return _numCols; }
        }

        /// <summary>
        /// Gets the column located at the specified index.
        /// </summary>
        /// <returns>
        /// The column located at the specified index as an <see cref="T:System.Object" />.
        /// </returns>
        /// <param name="i">The zero-based index of the column to get. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />. </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <returns>
        /// The column with the specified name as an <see cref="T:System.Object" />.
        /// </returns>
        /// <param name="name">The name of the column to find. </param>
        /// <exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader" /> Object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public abstract void Close();

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable" /> that describes the column metadata of the <see cref="T:System.Data.IDataReader" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable" /> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.IDataReader" /> is closed. </exception><filterpriority>2</filterpriority>
        public abstract DataTable GetSchemaTable();

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract bool NextResult();

        /// <summary>
        /// Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual bool Read()
        {
            // Blow away the old values.
            ClearVals();

            // Move the cursor to the next row.
            bool retVal = FetchNextRow();

            return retVal;
        }

        /// <summary>
        /// Moves the cursor (or whatever the implementation equivilent is) to the next row
        /// if there is one.
        /// </summary>
        /// <returns>Whether or not there was another row to fetch.</returns>
        protected abstract bool FetchNextRow();

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The level of nesting.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract int Depth { get; }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <returns>
        /// true if the data reader is closed; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract bool IsClosed { get; }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public abstract int RecordsAffected { get; }

        /// <summary>
        /// Will only ever look up the value once.
        /// </summary>
        /// <param name="i">0-based column index.</param>
        /// <returns>The value, or null if the column had no value.</returns>
        protected virtual object GetCachedValue(int i)
        {
            object retVal = _valsByIndex[i];
            if (retVal == DBNull.Value)
            {
                retVal = GetDataObject(i);
                _valsByIndex[i] = retVal;
            }
            return retVal;
        }

        /// <summary>
        /// Resets the values in _valsByIndex.
        /// </summary>
        protected virtual void ClearVals()
        {
            for (int i = 0; i < _numCols; i++)
            {
                // Use DBNull because "null" is actually a legitimate value.
                _valsByIndex[i] = DBNull.Value;
            }
        }

        /// <summary>
        /// This method gets the actual data value from the actual data source.
        /// </summary>
        /// <param name="i">Column number to get, zero-based.</param>
        /// <returns>A primitive, string, date, NTS geometry, or null if the column
        ///          had no value.</returns>
        protected abstract object GetDataObject(int i);
    }
}
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
using System.IO;
using System.Text;
using Azavea.Open.DAO.Criteria;
using Azavea.Open.Common;
using Azavea.Open.DAO.Unqueryable;

namespace Azavea.Open.DAO.CSV
{
    /// <summary>
    /// A datareader that reads CSV files.
    /// </summary>
    public class CsvDataReader : UnqueryableDataReader
    {
        private StreamReader _reader;

        /// <summary>
        /// Create the data reader.
        /// </summary>
        /// <param name="layer">Data access layer that will give us the TextReader we need.</param>
        /// <param name="mapping">ClassMapping for the type we're returning.</param>
        /// <param name="criteria">Since there is no way to filter before we read the file,
        ///                     the reader checks each row read to see if it matches the
        ///                     criteria, if not, it is skipped.</param>
        public CsvDataReader(CsvDaLayer layer, ClassMapping mapping, DaoCriteria criteria)
            : base(layer, mapping, criteria, GetConfig(layer, mapping))
        {
            _reader = ((CsvDataReaderConfig) _config).Reader;

            PreProcessSorts();
        }

        private static DataReaderConfig GetConfig(CsvDaLayer layer, ClassMapping mapping)
        {
            CsvDataReaderConfig retVal = new CsvDataReaderConfig();

            try
            {
                retVal.Reader = layer.GetReader(mapping);
                if (CsvDaLayer.UseNamedColumns(mapping))
                {
                    // If the CSV has row headers, read the header row.
                    IList colNameRow = ReadRawCsvRow(retVal.Reader);
                    for (int x = 0; x < colNameRow.Count; x++)
                    {
                        retVal.IndexesByName[colNameRow[x].ToString()] = x;
                    }
                }
                else
                {
                    // No row headers, so we must be mapped to column numbers.
                    // In that case, just map the column number strings to the ints.
                    foreach (string colStr in mapping.AllDataColsInOrder)
                    {
                        // Remember the mapping column numbers are 1-based but
                        // the internal column numbers are 0-based.
                        retVal.IndexesByName[colStr] = int.Parse(colStr) - 1;
                    }
                }
                return retVal;
            }
            catch (Exception e)
            {
                // Problem setting up, close the reader.
                if (retVal.Reader != null)
                {
                    layer.DoneWithReader(retVal.Reader);
                }
                throw new LoggingException("Unable to begin reading from CSV file.", e);
            }
        }

        /// <summary>
        /// Helper so the data access layer can be efficient in reading/rewriting CSVs.
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        protected internal int GetColumnIndex(string colName)
        {
            return _indexesByName[colName];
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
            // We operate entirely based on the mapping, since CSV files have almost no useful metadata.
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
            // Everything that comes back from a CSV is.... a string!
            return typeof (string);
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
            ((CsvDaLayer)Layer).DoneWithReader(_reader);
            _reader = null;
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
        /// This reads the row from the CSV.  It basically just tokenizes it, it
        /// does not verify that there are any particular number of values or anything
        /// else.
        /// </summary>
        /// <returns>A list of the comma-separated strings from this row.</returns>
        protected override IList ReadRawRow()
        {
            return ReadRawCsvRow(_reader);
        }

        private static IList ReadRawCsvRow(TextReader reader)
        {
            IList retVal = new List<string>();
            StringBuilder currentValue = new StringBuilder();
            bool insideQuotes = false;
            bool keepGoing = true;
            bool eof = false;
            bool wasQuoted = false;
            while (keepGoing)
            {
                int intChar = reader.Read();
                if (intChar == -1)
                {
                    // done reading.
                    keepGoing = false;
                    eof = true;
                }
                else
                {
                    char nextChar = (char) intChar;
                    switch (nextChar)
                    {
                        case '"':
                            if (insideQuotes)
                            {
                                // This either means the end of the quotes, or it means an
                                // escaped quote.
                                int peekedChar = reader.Peek();
                                if (peekedChar == -1)
                                {
                                    // No more chars, can't be an escaped quote.
                                    insideQuotes = false;
                                }
                                else
                                {
                                    if (((char)peekedChar) == '"')
                                    {
                                        // Two quotes in a row means an escaped quote.
                                        currentValue.Append('"');
                                        // Since we verified the next char is a quote,
                                        // pull it off the reader.
                                        reader.Read();
                                    }
                                    else
                                    {
                                        // Next char is not another quote, so this is
                                        // the closing quote.
                                        insideQuotes = false;
                                    }
                                }
                            }
                                // If we're NOT inside quotes, we are now.
                            else
                            {
                                insideQuotes = true;
                                wasQuoted = true;
                            }
                            break;
                        case ',':
                            // If inside quotes, it's a normal char.
                            if (insideQuotes)
                            {
                                currentValue.Append(nextChar);
                            }
                            else
                            {
                                // Not inside quotes, it means the break between values.
                                // If the value was quoted, add it exactly as is.
                                if (wasQuoted)
                                {
                                    retVal.Add(currentValue.ToString());
                                }
                                else
                                {
                                    // If not quoted, trim it.
                                    string val = currentValue.ToString().Trim();
                                    if (val.Length == 0)
                                    {
                                        // If it trimmed to nothing, assume null.
                                        retVal.Add(null);
                                    }
                                    else
                                    {
                                        retVal.Add(val);
                                    }
                                }
                                currentValue.Remove(0, currentValue.Length);
                            }
                            break;
                        case '\r':
                            // If we're NOT inside quotes, ignore it as whitespace.
                            if (insideQuotes)
                            {
                                currentValue.Append(nextChar);
                            }
                            break;
                        case '\n':
                            // If we're NOT inside quotes, this indicates the end of a row.
                            if (insideQuotes)
                            {
                                currentValue.Append(nextChar);
                            }
                            else
                            {
                                keepGoing = false;
                            }
                            break;
                        default:
                            // Any other char, just add it to the current token.
                            currentValue.Append(nextChar);
                            break;
                    }
                }
            }
            // Check if we had just finished a value when we ran out of data
            if (currentValue.Length > 0)
            {
                retVal.Add(currentValue.ToString().Trim());
            }
            // If we read nothing and we're at the end of the file, return null
            // to indicate that.
            if (eof && retVal.Count == 0)
            {
                retVal = null;
            }
            return retVal;
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
            get { return _reader == null; }
        }

        /// <summary>
        /// Adds extra config info necessary when setting up a CSV data reader.
        /// </summary>
        public class CsvDataReaderConfig : DataReaderConfig
        {
            /// <summary>
            /// The reader we'll use to get data from the CSV file or stream.
            /// </summary>
            public StreamReader Reader;
        }
    }
}
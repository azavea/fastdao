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
using System.IO;
using Azavea.Open.Common;

namespace Azavea.Open.DAO.CSV
{
    /// <summary>
    /// What level of error tolerance do you want when parsing CSV files with
    /// bad values in certain columns?  For example, a column that is mapped to
    /// an integer field has a non-integer string value in it ("test").
    /// </summary>
    public enum CsvParseErrorTolerance
    {
        /// <summary>
        /// FastDAO will throw an exception as soon as it is unable to parse
        /// the row.
        /// </summary>
        Fail,
        /// <summary>
        /// The row with the offending value will be skipped (logging a Warning message) as
        /// if the row was not there.  This is dangerous because the log
        /// message may not be noticed.
        /// </summary>
        IgnoreRow,
        /// <summary>
        /// The value that does not parse will be ignored, and the field on the result
        /// object will be left with its default value.  This is dangerous because you
        /// can wind up importing invalid data.
        /// </summary>
        IgnoreValue
    }

    /// <summary>
    /// Connection descriptor representing a CSV file that you intend to read/write/modify/etc.
    /// 
    /// For CSV files, your mapping may map either to column names (which assumes the first
    /// row of the CSV is a header row) or column indexes (1-based, since that's what spreadsheets
    /// use).
    /// 
    /// See CsvConnectionType for more information on how CSV descriptors can be configured.
    /// </summary>
    public class CsvDescriptor : ConnectionDescriptor
    {
        /// <summary>
        /// How are we accessing the CSV, is it a single file, a directory full of files,
        /// or a programmatically-specified Stream?
        /// </summary>
        public readonly CsvConnectionType Type;
        /// <summary>
        /// The name of the CSV file, or the directory with all the files,
        /// that we're reading or writing to.  Will be null if a Stream was
        /// directly specified.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// The TextWriter that we were programmatically configured to use.  Will
        /// be null if we were configured to use a file or directory or StreamReader.
        /// </summary>
        public readonly TextWriter Writer;
        /// <summary>
        /// The StreamReader that we were programmatically configured to use.  Will
        /// be null if we were configured to use a file or directory or TextWriter.
        /// Cannot be a TextReader because we need to be able to seek back to the
        /// beginning for subsequent queries, so we need to use the underlying stream
        /// for that operation.
        /// </summary>
        public readonly StreamReader Reader;
        /// <summary>
        /// How verbosely do we quote values that we write to the file.
        /// Default is to quote all strings.
        /// </summary>
        public readonly CsvQuoteLevel OutputQuoteLevel = CsvQuoteLevel.QuoteStrings;

        /// <summary>
        /// This is a hack for the case where we're configured with someone else's
        /// TextWriter... we need to know whether to output a header row or not, and
        /// we only want to do that if we haven't written anything to the writer yet.
        /// This becomes true after anything is written to the TextWriter.  If we
        /// are not using the Writer type this parameter is unused.
        /// </summary>
        protected internal bool HasBeenWrittenTo;

        private readonly string _connectionStr;

        /// <summary>
        /// Populates the descriptor's values from a config file.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public CsvDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(CsvConnectionType.Unknown,
                   config.GetParameter(component, "Path"),
                   null, null,
                   config.ParameterExists(component, "OutputQuoteLevel")
                       ? (CsvQuoteLevel) Enum.Parse(typeof (CsvQuoteLevel),config.GetParameter(component, "OutputQuoteLevel"))
                       : CsvQuoteLevel.QuoteStrings) { }

        /// <summary>
        /// Creates a descriptor for the given path.  If the path may either be a file
        /// or a directory, see CsvConnectionType for descriptions of the behavior in
        /// either case.
        /// </summary>
        /// <param name="path">Path to the CSV file or directory.  Must exist.</param>
        public CsvDescriptor(string path)
            : this(CsvConnectionType.Unknown, path, null, null, CsvQuoteLevel.QuoteStrings) { }

        /// <summary>
        /// Creates a descriptor for the given path.  If the path may either be a file
        /// or a directory, see CsvConnectionType for descriptions of the behavior in
        /// either case.
        /// Allows you to specify the verbosity of quotes when we write the file.
        /// </summary>
        /// <param name="path">Path to the CSV file or directory.  Must exist.</param>
        /// <param name="quoteLevel">How verbosely do we quote values we write.</param>
        public CsvDescriptor(string path, CsvQuoteLevel quoteLevel)
            : this(CsvConnectionType.Unknown, path, null, null, quoteLevel) { }

        /// <summary>
        /// Creates a descriptor for the given path.  If the path may either be a file
        /// or a directory, see CsvConnectionType for descriptions of the behavior in
        /// either case.
        /// </summary>
        /// <param name="type">Which type is it, a file or a directory.</param>
        /// <param name="path">Path to the CSV file or directory.  Will be created if it does not exist.</param>
        public CsvDescriptor(CsvConnectionType type, string path)
            : this(type, path, null, null, CsvQuoteLevel.QuoteStrings) { }

        /// <summary>
        /// Creates a descriptor for the given path.  If the path may either be a file
        /// or a directory, see CsvConnectionType for descriptions of the behavior in
        /// either case.
        /// Allows you to specify the verbosity of quotes when we write the file.
        /// </summary>
        /// <param name="type">Which type is it, a file or a directory.</param>
        /// <param name="path">Path to the CSV file or directory.  Will be created if it does not exist.</param>
        /// <param name="quoteLevel">How verbosely do we quote values we write.</param>
        public CsvDescriptor(CsvConnectionType type, string path, CsvQuoteLevel quoteLevel)
            : this(type, path, null, null, quoteLevel) { }

        /// <summary>
        /// Creates a descriptor using a TextWriter.  See CsvConnectionType for a description
        /// of the behavior when using a TextWriter.
        /// </summary>
        /// <param name="writer">The writer to "insert" to.</param>
        public CsvDescriptor(TextWriter writer)
            : this(CsvConnectionType.Writer, null, writer, null, CsvQuoteLevel.QuoteStrings) { }

        /// <summary>
        /// Creates a descriptor using a TextWriter.  See CsvConnectionType for a description
        /// of the behavior when using a TextWriter.
        /// Allows you to specify the verbosity of quotes when we write to the stream.
        /// </summary>
        /// <param name="writer">The writer to "insert" to.</param>
        /// <param name="quoteLevel">How verbosely do we quote values we write.</param>
        public CsvDescriptor(TextWriter writer, CsvQuoteLevel quoteLevel)
            : this(CsvConnectionType.Writer, null, writer, null, quoteLevel) { }

        /// <summary>
        /// Creates a descriptor using a StreamReader.  See CsvConnectionType for a description
        /// of the behavior when using a StreamReader.
        /// </summary>
        /// <param name="reader">The reader to "query" against.</param>
        public CsvDescriptor(StreamReader reader)
            : this(CsvConnectionType.Writer, null, null, reader, CsvQuoteLevel.QuoteStrings) { }

        /// <summary>
        /// Creates a descriptor using a StreamReader.  See CsvConnectionType for a description
        /// of the behavior when using a StreamReader.
        /// Allows you to specify the verbosity of quotes when we write to the stream.
        /// </summary>
        /// <param name="reader">The reader to "query" against.</param>
        /// <param name="quoteLevel">How verbosely do we quote values we write.</param>
        public CsvDescriptor(StreamReader reader, CsvQuoteLevel quoteLevel)
            : this(CsvConnectionType.Reader, null, null, reader, quoteLevel) { }

        /// <summary>
        /// Creates a descriptor from any possible combination of inputs / configuration.
        /// </summary>
        /// <param name="type">May be InputStream or OutputStream if stream is not null, or
        ///                    Unknown if stream is null.</param>
        /// <param name="path">Path to the CSV file or directory.  One of path, reader, writer
        ///                    must be set.</param>
        /// <param name="writer">The writer to "insert" to.  One of path, reader, writer
        ///                    must be set.</param>
        /// <param name="reader">The reader to "query" against.  One of path, reader, writer
        ///                    must be set.</param>
        /// <param name="quoteLevel">How verbosely do we quote values we write.</param>
        protected CsvDescriptor(CsvConnectionType type, string path,
                                TextWriter writer, StreamReader reader, CsvQuoteLevel quoteLevel)
        {
            if (StringHelper.IsNonBlank(path))
            {
                if (writer != null)
                {
                    throw new LoggingException(
                        "Cannot create a CSV 'connection' using both a file path and a writer.");
                }
                if (reader != null)
                {
                    throw new LoggingException(
                        "Cannot create a CSV 'connection' using both a file path and a reader.");
                }
                switch (type)
                {
                    case CsvConnectionType.Unknown:
                        if ((!File.Exists(path)) && (!Directory.Exists(path)))
                        {
                            throw new LoggingException(
                                "Path '" + path +
                                "' does not exist.  If you want it to be created, you must specify the connection type (FileName or Directory).");
                        }
                        // Check if it's a directory or a file.
                        if ((File.GetAttributes(path) & FileAttributes.Directory)
                            == FileAttributes.Directory)
                        {
                            Type = CsvConnectionType.Directory;
                        }
                        else
                        {
                            Type = CsvConnectionType.FileName;
                        }
                        break;
                    case CsvConnectionType.FileName:
                        if (File.Exists(path))
                        {
                            // Check if it's a directory or a file.
                            if ((File.GetAttributes(path) & FileAttributes.Directory)
                                == FileAttributes.Directory)
                            {
                                throw new LoggingException(
                                    "You specified a type of FileName, but the path you provided (" +
                                    path + ") is a directory.");
                            }
                        }
                        Type = CsvConnectionType.FileName;
                        break;
                    case CsvConnectionType.Directory:
                        if (Directory.Exists(path))
                        {
                            // Check if it's a directory or a file.
                            if ((File.GetAttributes(path) & FileAttributes.Directory)
                                != FileAttributes.Directory)
                            {
                                throw new LoggingException(
                                    "You specified a type of Directory, but the path you provided (" +
                                    path + ") is not a directory.");
                            }
                        }
                        else
                        {
                            // Go ahead and create it.
                            Directory.CreateDirectory(path);
                        }
                        Type = CsvConnectionType.Directory;
                        break;
                    default:
                        throw new LoggingException("Connection type " + type +
                                                   " is invalid when configuring using a path.");
                }
                Path = path;
            }
            else if (writer != null)
            {
                if (reader != null)
                {
                    throw new LoggingException(
                        "Cannot create a CSV 'connection' using both a writer and a reader.");
                }
                switch (type)
                {
                    case CsvConnectionType.Unknown:
                    case CsvConnectionType.Writer:
                        Type = CsvConnectionType.Writer;
                        break;
                    default:
                        throw new LoggingException("Connection type " + type +
                                                   " is invalid when configuring using a writer.");
                }
                Writer = writer;
            }
            else
            {
                if (reader == null)
                {
                    throw new LoggingException(
                        "You must either specify a path, a reader, or a writer to use.");
                }
                switch (type)
                {
                    case CsvConnectionType.Unknown:
                    case CsvConnectionType.Reader:
                        if (!reader.BaseStream.CanRead)
                        {
                            throw new LoggingException(
                                "You passed a reader wrapped around a stream that does not support reading.");
                        }
                        if (!reader.BaseStream.CanSeek)
                        {
                            throw new LoggingException(
                                "You passed a reader wrapped around a stream that does not support seeking.  Support for a single query without seeking has not yet been implemented.");
                        }
                        break;
                    default:
                        throw new LoggingException("Connection type " + type +
                                                   " is invalid when configuring using a reader.");
                }
                Reader = reader;
            }
            OutputQuoteLevel = quoteLevel;
            _connectionStr = "CSV[" + Type + "," + quoteLevel + "," + (Path ?? Writer.ToString() ?? Reader.ToString()) + "]";
        }

        /// <summary>
        /// Since we often need to represent database connection info as strings,
        /// child classes must implement ToCompleteString() such that this.Equals(that) and
        /// this.ToCompleteString().Equals(that.ToCompleteString()) will behave the same.
        /// </summary>
        /// <returns>A string representation of all of the connection info.</returns>
        public override string ToCompleteString()
        {
            return _connectionStr;
        }

        /// <summary>
        /// This method is similar to ToString, except it will not contain any
        /// "sensitive" information, I.E. passwords.
        /// 
        /// This method is intended to be used for logging or error handling, where
        /// we do not want to display passwords to (potentially) just anyone, but
        /// we do want to indicate what DB connection we were using.
        /// </summary>
        /// <returns>A string representation of most of the connection info, except
        ///          passwords or similar items that shouldn't be shown.</returns>
        public override string ToCleanString()
        {
            return _connectionStr;
        }

        /// <summary>
        /// Returns the appropriate data access layer for this connection.
        /// </summary>
        public override IDaLayer CreateDataAccessLayer()
        {
            return new CsvDaLayer(this);
        }
    }
}
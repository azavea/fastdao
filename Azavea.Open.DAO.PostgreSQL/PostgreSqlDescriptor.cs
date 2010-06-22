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
using System.Data;
using System.Data.Common;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Util;
using Npgsql;

namespace Azavea.Open.DAO.PostgreSQL
{
    /// <summary>
    /// This class is a ConnectionDescriptor implementation for using FastDAO
    /// to communicate with a PostGreSQL / PostGIS database.  Thought it is named
    /// "PostGreSqlDescriptor", it supports spatial queries and data if you have
    /// PostGIS installed.
    /// </summary>
    public class PostgreSqlDescriptor : AbstractSqlConnectionDescriptor, ITransactionalConnectionDescriptor
    {
        /// <summary>
        /// This is the "usual" port that PostGreSQL runs on.  Unless you know
        /// your instance is running on a different port, this is the one you should use.
        /// </summary>
        public static readonly string DEFAULT_PORT = "5432";
        /// <summary>
        /// This is the "usual" character encoding that PostGreSQL uses.  Unless you know
        /// your instance is using a different encoding, this is the one you should use.
        /// </summary>
        public static readonly string DEFAULT_ENCODING = "UNICODE";
        private readonly string _server;
        private readonly string _port = DEFAULT_PORT;
        private readonly string _databaseName;
        private readonly string _user;
        private readonly string _password;
        private readonly string _encoding = DEFAULT_ENCODING;
        private readonly string _connectionStr;
        private readonly string _cleanConnStr;

        /// <summary>
        /// Constructor that lets you pass everything as parameters rather than requiring a config.
        /// </summary>
        /// <param name="server">Server name or IP.  May not be null.</param>
        /// <param name="port">Port number on the server.  Null means use the default.</param>
        /// <param name="database">Database name on that server.  May not be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Plain text password for the user.
        ///                        May be null.</param>
        /// <param name="encoding">String encoding to use.  Null means use the default (unicode).</param>
        public PostgreSqlDescriptor(string server, string port, string database,
                                  string user, string password, string encoding)
        {
            if (!StringHelper.IsNonBlank(server))
            {
                throw new ArgumentNullException("server", "Server parameter cannot be null/blank.");
            }
            if (!StringHelper.IsNonBlank(database))
            {
                throw new ArgumentNullException("database", "Database name parameter cannot be null/blank.");
            }
            _server = server;
            if (port != null)
            {
                _port = port;
            }
            _databaseName = database;
            _user = user;
            _password = password;
            if (encoding != null)
            {
                _encoding = encoding;
            }

            _connectionStr = MakeConnectionString(_server, _port, _databaseName, _user, _password, _encoding);
            _cleanConnStr = MakeConnectionString(_server, _port, _databaseName, _user, null, _encoding);
        }

        /// <summary>
        /// This constructor reads all the appropriate values from our standard config file
        /// in the normal format.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public PostgreSqlDescriptor(Config config, string component,
                                  ConnectionInfoDecryptionDelegate decryptionDelegate)
        {
            _server = config.GetParameter(component, "Server");
            _port = config.GetParameter(component, "Port", _port);
            _databaseName = config.GetParameter(component, "Database");
            _user = config.GetParameter(component, "User", null);
            _password = GetDecryptedConfigParameter(config, component, "Password", decryptionDelegate);
            _encoding = config.GetParameter(component, "Encoding", _encoding);

            _connectionStr = MakeConnectionString(_server, _port, _databaseName, _user, _password, _encoding);
            _cleanConnStr = MakeConnectionString(_server, _port, _databaseName, _user, null, _encoding);
        }

        /// <exclude/>
        public override IDaLayer CreateDataAccessLayer()
        {
            return new PostgreSqlDaLayer(this);
        }

        /// <summary>
        /// The port number to communicate with the database server on.
        /// </summary>
        public string Port
        {
            get { return _port; }
        }
        /// <summary>
        /// The server name.
        /// </summary>
        public string Server
        {
            get{return _server;}
        }
        /// <summary>
        /// The database name, since a single PostGreSQL instance may have multiple databases.
        /// </summary>
        public string Database
        {
            get{return _databaseName;}
        }
        /// <summary>
        /// The user name, if necessary to log into the database.
        /// </summary>
        public string User
        {
            get{return _user;}
        }
        /// <summary>
        /// The plain text password for the User.
        /// </summary>
        protected string Password
        {
            get{return _password;}
        }
        /// <summary>
        /// The string encoding to use.
        /// </summary>
        protected string Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Begins the transaction.  Returns a transaction object that you should
        /// use for operations you wish to be part of the transaction.
        /// 
        /// NOTE: You MUST call Commit or Rollback on the returned ITransaction when you are done.
        /// </summary>
        /// <returns>The Transaction object to pass to calls that you wish to have
        ///          happen as part of this transaction.</returns>
        public ITransaction BeginTransaction()
        {
            return new SqlTransaction(this);
        }

        /// <summary>
        /// Assembles a PostGreSQL/PostGIS connection string that can be used to get a database connection.
        /// All the parameters are optional for the purposes of this method, although obviously
        /// it would be possible to create a useless connection string if you leave out important
        /// parameters.
        /// </summary>
        /// <param name="server">Server name that is hosting the database</param>
        /// <param name="port">Port number on the server to use, if you don't know then use DEFAULT_PORT.</param>
        /// <param name="database">Database name, if necessary to specify</param>
        /// <param name="user">User name to use when accessing the db</param>
        /// <param name="password">Password for above user, in plain text.</param>
        /// <param name="encoding">String encoding to use, if you don't know then use DEFAULT_ENCODING.</param>
        /// <returns>A connection string that can be used to create PostGreSQL/PostGIS connections.</returns>
        public static string MakeConnectionString(string server, string port,
                                                  string database, string user, string password, string encoding)
        {
            string connStr = "";

            // Assemble the string.  Only include parameters that have values.
            if (StringHelper.IsNonBlank(server))
            {
                connStr += "Server=" + server + ";";
            }
            if (StringHelper.IsNonBlank(port))
            {
                connStr += "Port=" + port + ";";
            }
            if (StringHelper.IsNonBlank(encoding))
            {
                connStr += "Encoding=" + encoding + ";";
            }
            if (StringHelper.IsNonBlank(database))
            {
                connStr += "Database=" + database + ";";
            }
            if (StringHelper.IsNonBlank(user))
            {
                connStr += "User ID=" + user + ";";
            }
            if (StringHelper.IsNonBlank(password))
            {
                connStr += "Password=" + password + "";
            }
            return connStr;
        }

        /// <exclude/>
        public override string ToCleanString()
        {
            return _cleanConnStr;
        }

        /// <exclude/>
        public override string ToCompleteString()
        {
            return _connectionStr;
        }

        /// <exclude/>
        public override DbConnection CreateNewConnection()
        {
            return new NpgsqlConnection(_connectionStr);
        }

        /// <exclude/>
        public override void SetParametersOnCommand(IDbCommand cmd, IEnumerable parameters)
        {
            IEnumerator enumer = parameters.GetEnumerator();

            if (enumer.MoveNext())
            {
                // There's at least one, so split apart the sql.
                string[] sqlPieces = cmd.CommandText.Split('?');
                StringBuilder sb = DbCaches.StringBuilders.Get();
                for (int x = 0; x < (sqlPieces.Length - 1); x++)
                {
                    // First append the sql fragment.
                    sb.Append(sqlPieces[x]);
                    // Get the value to insert.
                    object param = enumer.Current;
                    if (param != null)
                    {
                        // The name of the param has to be prepended with a : in the sql, but not
                        // on the parameter object.
                        sb.Append(":");
                        string paramName = DbCaches.ParamNames.Get(x);
                        sb.Append(paramName);

                        // Check for enums, Npgsql does not convert them to ints by
                        // default like every other provider does.
                        if (param is Enum)
                        {
                            param = (int) param;
                        }
                        // Construct and add the parameter object.
                        cmd.Parameters.Add(new NpgsqlParameter(paramName, param));
                    }
                    else
                    {
                        // PostGres doesn't seem happy accepting any ol' null param as a null,
                        // and since we don't know what type the parameter should be, we'll cheat
                        // by putting the keyword null into the SQL.
                        sb.Append("null");
                    }
                    // Move the enumerator to the next item, if there should be another one.
                    if ((x + 1) < (sqlPieces.Length - 1))
                    {
                        if (!enumer.MoveNext())
                        {
                            throw new ArgumentException("Command sql has " +
                                                        (sqlPieces.Length - 1) + " params, but you only passed " +
                                                        (x + 1) +
                                                        " parameters: " +
                                                        SqlUtilities.SqlParamsToString(cmd.CommandText, parameters) +
                                                        "  You may get this if your sql string has a ? in it.  In that case you" +
                                                        " can try parameterizing whatever string constant has the ? and passing" +
                                                        " the constant from your code.  Not super elegant, but a whole lot easier than" +
                                                        " making this code understand quoted or escaped characters in the sql string.");
                        }
                    }
                }
                // Check that we don't have leftover parameters.
                if (enumer.MoveNext())
                {
                    throw new ArgumentException("Command sql has " +
                                                (sqlPieces.Length - 1) + " params, but you passed more than that: " +
                                                SqlUtilities.SqlParamsToString(cmd.CommandText, parameters));
                }
                // Append the last sql fragment.
                sb.Append(sqlPieces[sqlPieces.Length - 1]);
                cmd.CommandText = sb.ToString();
                DbCaches.StringBuilders.Return(sb);
            }
        }

        /// <exclude/>
        public override DbDataAdapter CreateNewAdapter(IDbCommand cmd)
        {
            return new NpgsqlDataAdapter((NpgsqlCommand) cmd);
        }

        /// <exclude/>
        public override bool UsePooling()
        {
            return true;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeModulusClause(string columnName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            sb.Append("(");
            sb.Append(columnName);
            sb.Append(" % ");
            retVal.PartBeforeValue = sb.ToString();
            retVal.PartAfterValue = ")";
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override string MakeSequenceValueQuery(string sequenceName)
        {
            return "select nextval(" + sequenceName + ")";
        }
        /// <exclude/>
        public override bool NeedToAliasColumns()
        {
            return true;
        }

        /// <exclude/>
        public override bool NeedAsForColumnAliases()
        {
            return true;
        }

        /// <exclude/>
        public override string ColumnAliasPrefix()
        {
            return "\"";
        }

        /// <exclude/>
        public override string ColumnAliasSuffix()
        {
            return "\"";
        }

        /// <exclude/>
        public override string TableAliasPrefix()
        {
            return "";
        }

        /// <exclude/>
        public override string TableAliasSuffix()
        {
            return "";
        }
    }
}
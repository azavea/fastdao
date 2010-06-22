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
using System.Data.Common;
using System.Data.OleDb;
using System.Text;
using Azavea.Open.Common;
using Azavea.Open.DAO.Exceptions;
using Azavea.Open.DAO.SQL;
using Azavea.Open.DAO.Util;

namespace Azavea.Open.DAO.OleDb
{
    /// <summary>
    /// This class represents the info necessary to connect to an OleDb data source.
    /// </summary>
    public class OleDbDescriptor : AbstractSqlConnectionDescriptor, ITransactionalConnectionDescriptor
    {
        /// <summary>
        /// These are the database types for which support has been implemented via OleDB.
        /// </summary>
        public enum DatabaseType
        {
            /// <summary>
            /// Oracle (tested on 10g and 11g), uses the Oracle driver not the MS one.
            /// This requires you to have installed the Oracle client drivers.  You can
            /// use the MS drivers (though not recommended) by explicitly passing a provider
            /// string to the constructor / config.
            /// </summary>
            ORACLE,
            /// <summary>
            /// MS SQL Server (2000, 2005, 2008 tested).
            /// </summary>
            SQLSERVER,
            /// <summary>
            /// MS Access (most versions).
            /// </summary>
            ACCESS
        }

        /// <summary>
        /// The type of database (Oracle, SQL Server, etc).
        /// </summary>
        public readonly DatabaseType Type;

        /// <exclude/>
        protected readonly string _connectionStr;
        /// <exclude/>
        protected readonly string _cleanConnStr;

        /// <summary>
        /// The "Provider", meaning the value for the provider field in the OleDB connection string.
        /// </summary>
        public readonly string Provider;
        /// <summary>
        /// The server name, meaningful for some databases (Oracle, SQL Server) but not others (Access).
        /// May be null depending on the database.
        /// </summary>
        public readonly string Server;
        /// <summary>
        /// The database name, meaningful for some databases (Access (filename), SQL Server)
        /// but not others (Oracle).  May be null depending on the database.
        /// </summary>
        public readonly string Database;
        /// <summary>
        /// The user name, if necessary to log into the database.  May be null.
        /// </summary>
        public readonly string User;
        /// <summary>
        /// The password for the User.  May be null.
        /// </summary>
        public readonly string Password;
        /// <summary>
        /// The connection timeout, in seconds.  May be null, meaning use the default.
        /// </summary>
        public readonly int? ConnectTimeout;

        /// <summary>
        /// This constructor reads all the appropriate values from a config file.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <param name="decryptionDelegate">Delegate to call to decrypt password fields.
        ///                                  May be null if passwords are in plain text.</param>
        public OleDbDescriptor(Config config, string component,
            ConnectionInfoDecryptionDelegate decryptionDelegate)
            : this(GetTypeFromConfig(config, component),
                   config.GetParameter(component, "Provider", null),
                   config.GetParameter(component, "Server", null),
                   config.GetParameter(component, "Database", null),
                   config.GetParameter(component, "User", null),
                   GetDecryptedConfigParameter(config, component, "Password", decryptionDelegate),
                   config.GetParameterAsInt(component, "Connect_Timeout", null)) {}

        /// <summary>
        /// Constructor that lets you pass everything as parameters rather than requiring a config.
        /// </summary>
        /// <param name="provider">Database provider string.  May not be null.</param>
        /// <param name="server">Server (or in the case of access, .mdb file) May be null, but you
        ///                      probably won't be able to connect to anything.</param>
        /// <param name="database">database name on that server, if applicable.  May be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Password for the user. May be null.</param>
        /// <param name="timeout">Connection timeout, in seconds.  May be null.</param>
        public OleDbDescriptor(string provider, string server, string database,
                               string user, string password, int? timeout)
            : this(GuessTypeFromProvider(provider), provider, server, database, user, password, timeout) {}

        /// <summary>
        /// Constructor that lets you pass everything as parameters rather than requiring a config.
        /// </summary>
        /// <param name="type">Type of database to connect to.  May not be null.  This parameter
        ///                    will be used to determine what OleDb provider to use.</param>
        /// <param name="server">Server (or in the case of access, .mdb file) May be null, but you
        ///                      probably won't be able to connect to anything.</param>
        /// <param name="database">database name on that server, if applicable.  May be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Password for the user. May be null.</param>
        /// <param name="timeout">Connection timeout, in seconds.  May be null.</param>
        public OleDbDescriptor(DatabaseType type, string server, string database,
                               string user, string password, int? timeout)
            : this(type, null, server, database, user, password, timeout) {}

        /// <summary>
        /// Constructor that lets you pass everything as parameters rather than requiring a config.
        /// </summary>
        /// <param name="provider">Database provider string.  May be null.  If null, we will use a
        ///                        provider value based on the type, if not null, we will use this
        ///                        provider.</param>
        /// <param name="type">Type of database to connect to.  May not be null.  This parameter
        ///                    will be used to determine what OleDb provider to use.</param>
        /// <param name="server">Server (or in the case of access, .mdb file) May be null, but you
        ///                      probably won't be able to connect to anything.</param>
        /// <param name="database">database name on that server, if applicable.  May be null.</param>
        /// <param name="user">Database user name, may be null.</param>
        /// <param name="password">Password for the user. May be null.</param>
        /// <param name="timeout">Connection timeout, in seconds.  May be null.</param>
        public OleDbDescriptor(DatabaseType type, string provider, string server, string database,
                               string user, string password, int? timeout)
        {
            Type = type;
            _connectionStr = (provider == null
                                  ? MakeConnectionString(type, server, database, user, password, timeout)
                                  : MakeConnectionString(provider, server, database, user, password, timeout));
            _cleanConnStr = (provider == null
                                 ? MakeConnectionString(type, server, database, user, null, timeout)
                                 : MakeConnectionString(provider, server, database, user, null, timeout));
            Provider = provider;
            Server = server;
            Database = database;
            User = user;
            Password = password;
            ConnectTimeout = timeout;
        }

        /// <summary>
        /// Begins the transaction.  Returns a NEW ConnectionDescriptor that you should
        /// use for operations you wish to be part of the transaction.
        /// 
        /// NOTE: You MUST call Commit or Rollback on the returned ITransaction when you are done.
        /// </summary>
        /// <returns>The ConnectionDescriptor object to pass to calls that you wish to have
        ///          happen as part of this transaction.</returns>
        public ITransaction BeginTransaction()
        {
            return new SqlTransaction(this);
        }

        /// <summary>
        /// Gets the type based on a couple optional parameters in the DB config file.
        /// </summary>
        /// <param name="config">Config to get params from.</param>
        /// <param name="component">Section of the config XML to look in for db params.</param>
        /// <returns>The type as specified in the config file, or throws an exception if
        ///          there is no type correctly specified.</returns>
        private static DatabaseType GetTypeFromConfig(Config config, string component)
        {
            if (config.ParameterExists(component, "Type"))
            {
                return (DatabaseType)Enum.Parse(typeof(DatabaseType), config.GetParameter(component, "Type").Trim().ToUpper());
            }
            if (config.ParameterExists(component, "Provider"))
            {
                return GuessTypeFromProvider(config.GetParameter(component, "Provider").Trim());
            }
            throw new BadDaoConfigurationException(
                "Database connection config for '" + component + "' is missing both type and provider.");
        }

        /// <summary>
        /// Attempts to parse the provider string and determine what database type we're connecting to.
        /// </summary>
        /// <param name="provider">OleDB "provider" piece of the connection string.</param>
        /// <returns>The DatabaseVendorName identifying the database (an exception will be thrown
        ///          if we cannot determine the type).</returns>
        public static DatabaseType GuessTypeFromProvider(string provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider", "Provider cannot be null.");
            }
            DatabaseType retVal;
            switch (provider.ToUpper())
            {
                case "MICROSOFT.JET.OLEDB.4.0":
                    retVal = DatabaseType.ACCESS;
                    break;
                case "SQLOLEDB":
                    retVal = DatabaseType.SQLSERVER;
                    break;
                case "ORAOLEDB.ORACLE.1":
                case "MSDAORA.1":
                    retVal = DatabaseType.ORACLE;
                    break;
                default:
                    throw (new ArgumentException("This OleDbProvider (" + provider + ") is not supported.", "provider"));
            }
            return retVal;
        }

        /// <summary>
        /// Assembles a OLEDB connection string that can be used to get a database connection.
        /// All the parameters are optional for the purposes of this method, although obviously
        /// it would be possible to create a useless connection string if you leave out important
        /// parameters.
        /// </summary>
        /// <param name="providerType">Database type, will be used to determine provider string.</param>
        /// <param name="server">Server name that is hosting the database</param>
        /// <param name="database">Database name, if necessary to specify</param>
        /// <param name="user">User name to use when accessing the db</param>
        /// <param name="password">Password for above user.</param>
        /// <param name="connectionTimeout">How long to wait before giving up on a command, in seconds.</param>
        /// <returns>A connection string that can be used to create OleDbConnections.</returns>
        public static string MakeConnectionString(DatabaseType providerType, string server,
               string database, string user, string password, int? connectionTimeout)
        {
            string provider;
            if (DatabaseType.SQLSERVER == providerType)
            {
                provider = "SQLOLEDB";
            }
            else if (DatabaseType.ORACLE == providerType)
            {
                provider = "OraOLEDB.Oracle.1";
                // Oracle doesn't want this parameter.
                database = null;
            }
            else if (DatabaseType.ACCESS == providerType)
            {
                provider = "Microsoft.Jet.OLEDB.4.0";
            }
            else
            {
                throw new ArgumentException(providerType + " is an unsupported type of database.", providerType.ToString());
            }

            return MakeConnectionString(provider, server, database, user, password, connectionTimeout);
        }

        /// <summary>
        /// Assembles a OLEDB connection string that can be used to get a database connection.
        /// All the parameters are optional for the purposes of this method, although obviously
        /// it would be possible to create a useless connection string if you leave out important
        /// parameters.
        /// </summary>
        /// <param name="provider">Database driver/type/something, for example "SQLOLEDB"</param>
        /// <param name="server">Server name that is hosting the database</param>
        /// <param name="database">Database name, if necessary to specify</param>
        /// <param name="user">User name to use when accessing the db</param>
        /// <param name="password">Password for above user.</param>
        /// <param name="connectionTimeout">How long to wait before giving up on a command, in seconds.</param>
        /// <returns>A connection string that can be used to create OleDbConnections.</returns>
        public static string MakeConnectionString(string provider, string server,
                                                  string database, string user, string password,
                                                  int? connectionTimeout)
        {
            string connStr = "";

            // Assemble the string.  Only include parameters that have values.
            if (StringHelper.IsNonBlank(provider))
            {
                connStr += "Provider=" + provider + ";";
            }
            if (StringHelper.IsNonBlank(server))
            {
                connStr += "Data Source=" + server + ";";
            }
            if (StringHelper.IsNonBlank(database))
            {
                connStr += "Initial Catalog=" + database + ";";
            }
            if (connectionTimeout != null)
            {
                connStr += "Connect Timeout=" + connectionTimeout + ";";
            }
            if (StringHelper.IsNonBlank(user))
            {
                connStr += "User ID=" + user + ";";
            }
            if (StringHelper.IsNonBlank(password))
            {
                connStr += "Password=" + password;
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
            return new OleDbConnection(_connectionStr);
        }

        /// <exclude/>
        public override void SetParametersOnCommand(IDbCommand cmd, IEnumerable parameters)
        {
            int x = 0;
            foreach (object param in parameters)
            {
                object addMe = param ?? DBNull.Value;
                string paramName = DbCaches.ParamNames.Get(x);
                // This is a hack for MS Access.  For SQL Server and Oracle, OleDb
                // correctly identifies the type when you add the parameter, but with
                // Access it uses the wrong type and results in a type mismatch
                // exception at runtime.
                if ((Type == DatabaseType.ACCESS) && (addMe is DateTime))
                {
                    OleDbParameter oleParam = new OleDbParameter(paramName, OleDbType.Date);
                    oleParam.Value = addMe;
                    cmd.Parameters.Add(oleParam);
                }
                else
                {
                    cmd.Parameters.Add(new OleDbParameter(paramName, addMe));
                }
                x++;
            }
        }

        /// <exclude/>
        public override DbDataAdapter CreateNewAdapter(IDbCommand cmd)
        {
            return new OleDbDataAdapter((OleDbCommand)cmd);
        }

        /// <exclude/>
        public override bool UsePooling()
        {
            return Type != DatabaseType.ACCESS;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeModulusClause(string fieldName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    sb.Append("MOD(");
                    sb.Append(fieldName);
                    sb.Append(", ");
                    retVal.PartBeforeValue = sb.ToString();
                    retVal.PartAfterValue = ")";
                    break;
                case DatabaseType.SQLSERVER:
                    sb.Append("(");
                    sb.Append(fieldName);
                    sb.Append(" % ");
                    retVal.PartBeforeValue = sb.ToString();
                    retVal.PartAfterValue = ")";
                    break;
                case DatabaseType.ACCESS:
                    sb.Append("(");
                    sb.Append(fieldName);
                    sb.Append(" MOD ");
                    retVal.PartBeforeValue = sb.ToString();
                    retVal.PartAfterValue = ")";
                    break;
                default:
                    throw new NotImplementedException("Modulus clause not implemented for DB type " + Type);
            }
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override string MakeSequenceValueQuery(string sequenceName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            if (DatabaseType.ORACLE.Equals(Type))
            {
                sb.Append("SELECT ");
                sb.Append(sequenceName);
                sb.Append(".NEXTVAL FROM DUAL");
            }
            else
            {
                throw new NotImplementedException(
                    "Sequence ID generation is not supported for db type " + Type);
            }
            string retVal = sb.ToString();
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }

        /// <exclude/>
        public override SqlClauseWithValue MakeBitwiseAndClause(string columnName)
        {
            StringBuilder sb = DbCaches.StringBuilders.Get();
            SqlClauseWithValue retVal = DbCaches.Clauses.Get();
            switch (Type)
            {
                case DatabaseType.SQLSERVER:
                    sb.Append(" (");
                    sb.Append(columnName);
                    sb.Append(" & ");
                    retVal.PartBeforeValue = sb.ToString();
                    retVal.PartAfterValue = ")";
                    break;
                default:
                    throw new NotImplementedException("Bitwise and is not supported for this connection: " + this);
            }
            DbCaches.StringBuilders.Return(sb);
            return retVal;
        }
        /// <exclude/>
        public override string MakeCreateIndexCommand(string indexName,
                                                      bool isUnique, string tableName,
                                                      IEnumerable<string> columnNames)
        {
            string sql = base.MakeCreateIndexCommand(indexName, isUnique, tableName, columnNames);
            if (Type == DatabaseType.ORACLE)
            {
                sql += " COMPUTE STATISTICS";
            }
            return sql;
        }
        /// <exclude/>
        public override bool SupportsTruncate()
        {
            return (Type != DatabaseType.ACCESS);
        }

        /// <exclude/>
        public override string TableAliasPrefix()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "";
                case DatabaseType.SQLSERVER:
                    return "";
                case DatabaseType.ACCESS:
                    return "";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }
        /// <exclude/>
        public override string TableAliasSuffix()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "";
                case DatabaseType.SQLSERVER:
                    return "";
                case DatabaseType.ACCESS:
                    return "";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override bool NeedToAliasColumns()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return true;
                case DatabaseType.SQLSERVER:
                    return true;
                case DatabaseType.ACCESS:
                    return false;
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        public override bool NeedAsForColumnAliases()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return false;
                case DatabaseType.SQLSERVER:
                    return false;
                case DatabaseType.ACCESS:
                    return true;
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override string ColumnAliasPrefix()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "\"";
                case DatabaseType.SQLSERVER:
                    return "[";
                case DatabaseType.ACCESS:
                    return "\"";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override string ColumnAliasSuffix()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "\"";
                case DatabaseType.SQLSERVER:
                    return "]";
                case DatabaseType.ACCESS:
                    return "\"";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override bool ColumnAliasWrappersInResults()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return false;
                case DatabaseType.SQLSERVER:
                    return false;
                case DatabaseType.ACCESS:
                    return true;
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override string FullOuterJoinKeyword()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "FULL OUTER JOIN";
                case DatabaseType.SQLSERVER:
                    return "FULL OUTER JOIN";
                case DatabaseType.ACCESS:
                    return "OUTER JOIN";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }

        /// <exclude/>
        public override string LowerCaseFunction()
        {
            switch (Type)
            {
                case DatabaseType.ORACLE:
                    return "LOWER";
                case DatabaseType.SQLSERVER:
                    return "LOWER";
                case DatabaseType.ACCESS:
                    return "LCase";
                default:
                    throw new NotImplementedException("Support for type " + Type +
                                                      " is not yet fully implemented.");
            }
        }
    }
}
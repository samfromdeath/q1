using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace q3
{
    public sealed class Database : IDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly MySqlTransaction _transaction;
        private static string _address;
        private static uint _port;
        private static string _username;
        private static string _password;
        public static bool Configured;

        public Database()
        {
            //get connection
            MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder
            {
                Server = _address,
                Port = _port,
                UserID = _username,
                Password = _password
            };
            _connection = new MySqlConnection(connString.ConnectionString);

            //create new transaction
            OpenConnection(_connection);
            _transaction = _connection.BeginTransaction();
        }

        public Database(DatabaseNames database)
        {
            //get connection
            MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder
            {
                Server = _address,
                Database = database.Name(),
                Port = _port,
                UserID = _username,
                Password = _password
            };
            _connection = new MySqlConnection(connString.ConnectionString);
            //create new transaction
            OpenConnection(_connection);
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Sets the Database connection values
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static bool SetDatabaseConnection(string address, uint port, string username, string password)
        {
            MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder
            {
                Server = address,
                Port = port,
                UserID = username,
                Password = password
            };
            using (MySqlConnection conn = new MySqlConnection(connString.ConnectionString))
            {
                try
                {
                    OpenConnection(conn);
                }
                catch (MySqlException)
                {
                    return false;
                }
                _address = address;
                _port = port;
                _username = username;
                _password = password;
            }
            Configured = true;
            return true;
        }


        /// <summary>
        /// Executes a query inside a MySqlTransaction object and returns the value in position [0,0]
        /// </summary>
        /// <param name="query">The Query you wish to execute</param>
        /// <param name="parameters">The MySqlParameter you wish to attach to the query</param>
        /// <returns>The value in position [0,0]</returns>
        internal T GetDataSingleResult<T>(string query, MySqlParameter[] parameters)
        {
            MySqlConnection conn = CheckConnectionValid();
            using (MySqlCommand cmd = CreateCommand(conn, query, parameters))
            {
                try
                {
                    object value = cmd.ExecuteScalar();
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (MySqlException ex)
                {
                    TransactionRollback();
                    throw new MySqlCommandError("Encountered an error executing this query", ex);
                }
            }
        }

        /// <summary>
        /// Executes a query inside a MySqlTransaction object and returns a datatable of the results
        /// </summary>
        /// <param name="query">The Query you wish to execute</param>
        /// <param name="parameters">The MySqlParameter you wish to attach to the query</param>
        /// <returns>A DataTable containing the results view</returns>
        internal DataTable GetDataTable(string query, MySqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            MySqlConnection conn = CheckConnectionValid();
            using (MySqlCommand cmd = CreateCommand(conn, query, parameters))
            {
                try
                {
                    using (MySqlDataReader dr = cmd.ExecuteReader())
                    {
                        DataSet ds = new DataSet();
                        ds.Tables.Add(dt);
                        ds.EnforceConstraints = false;
                        dt.Load(dr);
                    }
                }
                catch (MySqlException ex)
                {
                    TransactionRollback();
                    throw new MySqlCommandError("Encountered an error executing this query", ex);
                }
            }
            return dt;
        }

        internal long SetDataReturnLastInsertId(string query, MySqlParameter[] parameters)
        {
            using (MySqlCommand cmd = SetData(query, parameters))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    return cmd.LastInsertedId;
                }
                catch (MySqlException ex)
                {
                    TransactionRollback();
                    throw new MySqlCommandError("Encountered an error executing this query", ex);
                }
            }
        }

        internal int SetDataReturnRowCount(string query, MySqlParameter[] parameters)
        {
            using (MySqlCommand cmd = SetData(query, parameters))
            {
                try
                {
                    int value = cmd.ExecuteNonQuery();
                    return value;
                }
                catch (MySqlException ex)
                {
                    TransactionRollback();
                    throw new MySqlCommandError("Encountered an error executing this query", ex);
                }
            }
        }

        internal void SetDataReturnNone(string query, MySqlParameter[] parameters)
        {
            using (MySqlCommand cmd = SetData(query, parameters))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    TransactionRollback();
                    throw new MySqlCommandError("Encountered an error executing this query", ex);
                }
            }
        }

        /// <summary>
        /// Executes a query inside a MySqlTransaction object and returns meta infomation about the query
        /// </summary>
        /// <param name="query">The Query you wish to execute</param>
        /// <param name="parameters">The MySqlParameter you wish to attach to the query</param>
        /// <returns>A long value containing info in relation to the returnInfo parameter</returns>
        private MySqlCommand SetData(string query, MySqlParameter[] parameters)
        {
            MySqlConnection conn = CheckConnectionValid();
            MySqlCommand cmd = CreateCommand(conn, query, parameters);
            return cmd;
        }

        /// <summary>
        /// Commits the contents of the MySqlTransaction Object to the database.
        /// </summary>
        internal void TransactionCommit()
        {
            try
            {
                _transaction.Commit();
            }
            catch (MySqlException)
            {
                _transaction.Rollback();
            }
        }

        private void TransactionRollback()
        {
            MySqlConnection conn = _transaction.Connection;
            _transaction.Rollback();
        }

        private MySqlConnection CheckConnectionValid()
        {
            MySqlConnection conn = _transaction.Connection;
            if (conn.State != ConnectionState.Closed)
            {
                return conn;
            }
            OpenConnection(conn);
            return conn;
        }

        private MySqlCommand CreateCommand(MySqlConnection conn, string query, MySqlParameter[] parameters)
        {
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = query,
                Transaction = _transaction,
                Connection = conn
            };
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        private static IDbConnection OpenConnection(IDbConnection conn)
        {
            try
            {
                conn.Open();
            }
            catch (MySqlException ex)
            {
                throw new MySqlConnectionError("Failed to open a connection to the database.", ex);
            }
            return conn;
        }

        public void Dispose()
        {
            // get rid of managed resources, call Dispose on member variables...
            _transaction.Dispose();
            _connection.Dispose();
        }
    }

    public class MySqlCommandError : Exception
    {
        public MySqlCommandError() : this("Encountered a error running this query")
        {
        }

        public MySqlCommandError(string message) : base(message)
        {

        }
        public MySqlCommandError(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public class MySqlConnectionError : Exception
    {

        public MySqlConnectionError() : this("Failed to open a connection to the database")
        {
        }

        public MySqlConnectionError(string message) : base(message)
        {

        }
        public MySqlConnectionError(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
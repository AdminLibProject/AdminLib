using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using AdminLib.Model;
using System.Configuration;
using AdminLib.Application;

namespace AdminLib.Database {

    using Timer = Debug.Timer;

    public class AdminConnection : AdminLib.Model.IConnection {

        /******************** Static Attributes ********************/
        private static int                  lastID = 0;
        public  static AdminConnection       serverConnection { get; private set; }
        public  static List<AdminConnection> connections = new List<AdminConnection>();


        /******************** Attributes ********************/
        public  bool                autoCommit { get; private set; }
        private bool                closeASAP    = false;
        private OracleConnection    connection;
        public  Debug.Connection    debug { get; private set; }
        public  string              id    { get; private set; }
        private List<BaseCursor>    openedCursors = new List<BaseCursor>();
        private Auth.Session        session;
        public  int                 uid {get; private set; }

        public string               sessionID {
            get {
                if (this.session == null)
                    return null;

                return this.session.sessionId;
            }
        }

        private OracleTransaction   transaction;

        /******************** Structures ********************/
        private struct FunctionResult {
            public string STRING_VALUE {get; set;}
            public int    INT_VALUE    {get; set;}
        }

        /******************** Constructors ********************/
        public AdminConnection ( Auth.Session session    = null
                              , bool         autoCommit = true
                              , string       id         = null) {

            this.autoCommit = autoCommit;
            this.session    = session;
            this.id         = id;
            this.uid        = AdminConnection.lastID++;

            this.debug = new Debug.Connection(this);

            AdminConnection.connections.Add(this);

            // Initialize connection
            this.connection = AdminConnection.GetNewOracleConnection();

            if (!this.autoCommit)
                this.transaction = this.connection.BeginTransaction();

        }

        /******************** Static Methods ********************/
        private static OracleConnection GetNewOracleConnection() {
            OracleConnection oracleConnection;

            oracleConnection = new OracleConnection(Config.defaultConnectionString);
            oracleConnection.Open();

            return oracleConnection;
        }

        /******************** Methods ********************/

        private string BuildQuery(string procedure, Dictionary<string, Object> parameters) {

            string query;

            query = procedure + "(";

            foreach (KeyValuePair<string, Object> entry in parameters) {
                query += entry.Key + "=> :" + entry.Key + " ,";
            }

            // removing the last comma
            query = query.Substring(0, query.Length - 1) + ')';


            return query;
        }

        /// <summary>
        ///     Close the connection.
        ///     If force is false, then the connection will remain if there is still at least one cursor opened.
        /// </summary>
        /// <param name="force">If true, then the connection will be closed, even if there is opened cursors</param>
        /// <param name="commitTransactions">If true, then all remaining transactions will be commited</param>
        public bool Close(bool force = false, bool? commitTransactions = null) {

            if (this.connection.State == ConnectionState.Closed)
                return true;

            if (!force) {
                foreach(BaseCursor cursor in this.openedCursors) {
                    if (cursor.IsOpen())
                        return false;
                }
            }
            else {
                // If the connection is force to close, we close all cursors

                this.closeASAP = false; // Avoiding loops
                foreach (BaseCursor cursor in this.openedCursors) {
                    cursor.Close();
                }
            }

            try {
                AdminConnection.connections.Remove(this);
            }
            catch (ArgumentOutOfRangeException) { }
            
            this.closeASAP = true;

            // Commiting transactions only if asked or if auto commit is enabled
            if (commitTransactions == true || (commitTransactions == null && this.autoCommit))
                this.Commit();
            else
                this.Rollback();

            this.connection.Close();
            return true;
        }

        /// <summary>
        ///     Comiting the transaction conditionnaly
        /// </summary>
        /// <param name="condition"></param>
        private void Commit(bool? condition) {
            if (condition == true || (condition == null && this.autoCommit))
                this.Commit();
        }

        /// <summary>
        ///     Commit all performed transactions.
        /// </summary>
        public void Commit() {

            if (this.transaction == null)
                return;

            this.transaction.Commit();

            // Creating a new transaction;
            this.transaction = this.connection.BeginTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        public void ExecuteDML ( string            query
                               , OracleParameter[] parameters = null
                               , bool              bindByName = true
                               , bool?             commit     = null) {

            OracleDataAdapter adapter;
            OracleCommand     command;
            Debug.SqlQuery    debugQuery;

            // Adding the query to the debug object
            debugQuery = this.debug.addQuery(query, parameters);;

            // Creating the command
            command = new OracleCommand(query, this.connection);

            // Adding parameters
            if (parameters != null)
                foreach(OracleParameter parameter in parameters) {
                    command.Parameters.Add(parameter);
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName;

            adapter = new OracleDataAdapter(command);

            // Executing the query
            try {
                debugQuery.start();
                command.ExecuteNonQuery();
                debugQuery.end();
            }
            catch (OracleException exception) {
                throw Error.QueryException.get ( exception  : exception
                                               , query      : query
                                               , parameters : parameters);
            }

            // Commit the transactions only if condition is true or if autoCommit is enabled
            this.Commit(commit);
        }

        /// <summary>
        ///     Execute the given function with the parameters.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="parameters"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        /// <returns></returns>
        public AdminLib.Model.Query.FunctionResult ExecuteFunction ( string                     function
                                                                , Dictionary<string, object> parameters
                                                                , bool?                      commit = null) {

            OracleParameter       oracleParameter;
            List<OracleParameter> oracleParameters;
            Object                value;

            oracleParameters = new List<OracleParameter>();

            foreach(KeyValuePair<string, object> entry in parameters) {

                if (entry.Value is bool)
                    value = (bool) entry.Value ? 1 : 0;
                else
                    value = entry.Value;

                oracleParameter = new OracleParameter ( parameterName : entry.Key
                                                      , obj           : value );

                oracleParameters.Add(oracleParameter);
            }

            return this.ExecuteFunction ( function   : BuildQuery ( procedure  : function
                                                                  , parameters : parameters)

                                        , parameters : oracleParameters.ToArray()
                                        , commit     : commit);
        }

        /// <summary>
        ///     Execute the given function.
        /// </summary>
        /// <typeparam name="T">The return type of the function result</typeparam>
        /// <param name="function">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public AdminLib.Model.Query.FunctionResult ExecuteFunction ( string            function
                                                                , OracleParameter[] parameters = null
                                                                , bool              bindByName = true
                                                                , bool?             commit     = null) {

            FunctionResult[] queryResults;
            string           sqlQuery;
            AdminLib.Model.Query.FunctionResult result;

            sqlQuery = "SELECT TO_CHAR(" + function + ") AS string_value FROM DUAL";

            queryResults = Query<FunctionResult> ( sqlQuery   : sqlQuery
                                                 , parameters : parameters
                                                 , bindByName : bindByName);

            result = new AdminLib.Model.Query.FunctionResult(queryResults[0].STRING_VALUE);

            if (commit == true || (commit == null && this.autoCommit))
                this.Commit();

            return result;
        }

        public void ExecuteProcedure ( string                     procedure
                                     , Dictionary<string, object> parameters) {

            OracleParameter       oracleParameter;
            List<OracleParameter> oracleParameters;

            oracleParameters = new List<OracleParameter>();

            foreach(KeyValuePair<string, object> entry in parameters) {

                oracleParameter = new OracleParameter ( parameterName : entry.Key
                                                      , obj           : entry.Value );

                oracleParameters.Add(oracleParameter);
            }

            this.ExecuteCode ( procedure  : BuildQuery ( procedure  : procedure
                                                       , parameters : parameters)
                             , parameters : oracleParameters.ToArray());
        }

        /// <summary>
        ///     Execute the given PL/SQL code.
        ///     The code will be placed into a BEGIN/END bloc.
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        public void ExecuteCode ( string            procedure
                                , OracleParameter[] parameters = null
                                , bool              bindByName = true) {

            OracleDataAdapter adapter;
            OracleCommand     command;
            Debug.SqlQuery    debugQuery;

            procedure = "BEGIN " + procedure + "; END;";

            // Adding the query to the debug object
            debugQuery = this.debug.addQuery(procedure, parameters);

            // Creating the command
            command = new OracleCommand(procedure, this.connection);

            // Adding parameters
            if (parameters != null)
                foreach(OracleParameter parameter in parameters) {
                    command.Parameters.Add(parameter);
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName;

            adapter = new OracleDataAdapter(command);

            // Executing the query
            try {
                debugQuery.start();
                command.ExecuteNonQuery();
                debugQuery.end();
            }
            catch (OracleException exception) {
                throw Error.QueryException.get ( exception  : exception
                                               , query      : procedure
                                               , parameters : parameters);
            }
        }

        /// <summary>
        ///     Execute the given function. The function is expected to return a int value
        /// </summary>
        /// <param name="sqlFunction">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int FunctionAsInt ( string            sqlFunction
                                 , OracleParameter[] parameters = null) {

            FunctionResult[] results;
            string           sqlQuery;

            sqlQuery = "select " + sqlFunction + " AS int_value FROM DUAL";

            results = Query<FunctionResult> ( sqlQuery   : sqlQuery
                                            , parameters : parameters);

            return results[0].INT_VALUE;
        }

        /// <summary>
        ///     Execute the given function. The function is expected to return a string value
        /// </summary>
        /// <param name="sqlFunction">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string FunctionAsString ( string            sqlFunction
                                       , OracleParameter[] parameters = null) {

            FunctionResult[] results;
            string           sqlQuery;

            sqlQuery = "select " + sqlFunction + " AS string_value FROM DUAL";

            results = Query<FunctionResult> ( sqlQuery   : sqlQuery
                                            , parameters : parameters);

            return results[0].STRING_VALUE;
        }

        /// <summary>
        ///     Initialize the connection
        /// </summary>
        public void Initialize() {
            OracleParameter[] parameters;
            string            sqlFunction;

            sqlFunction = "PAK_AUTH.DEFINE_SESSION(p_session_id => :session_id)";

            parameters = new OracleParameter[1];

            parameters[0] = new OracleParameter ( direction    : ParameterDirection.Input
                                                , obj          : this.session.sessionId
                                                , parameterName: ":session_id"
                                                , type         : OracleDbType.Varchar2);

            this.ExecuteFunction ( function  : sqlFunction
                                 , parameters   : parameters);
        }

        /// <summary>
        ///     Indicate if the connection do an autocommit at each DML command or not.
        /// </summary>
        /// <returns></returns>
        public bool IsAutoCommitEnabled() {
            return this.autoCommit;
        }

        public OracleCommand getCommand(string sqlQuery) {
            return new OracleCommand(sqlQuery, this.connection);
        }

        public void registerCursor(BaseCursor cursor) {
            this.openedCursors.Add(cursor);
        }

        /// <summary>
        /// Rollback the transaction.
        /// </summary>
        public void Rollback() {

            if (this.transaction == null)
                return;

            this.transaction.Rollback();

            // Creating a new transaction;
            this.transaction = this.connection.BeginTransaction();
        }

        public void unregisterCursor(BaseCursor cursor) {
            this.openedCursors.Remove(cursor);

            // If the connection has been previously asked to be close
            // The we try (again) to close it.
            if (this.closeASAP)
                this.Close();

        }

        /// <summary>
        ///     Execute the given query.
        ///     All rows are fetched in one single time. If the query return 1.000.000 rows, all of them will be retreived
        /// </summary>
        /// <typeparam name="Row">Return type of each row</typeparam>
        /// <param name="sqlQuery">Query to execute</param>
        /// <param name="connection">Connection to use for execution</param>
        /// <param name="parameters">Parameters of the query</param>
        /// <returns></returns>
        public Row[] Query<Row> ( string            sqlQuery
                                , OracleParameter[] parameters = null
                                , bool              bindByName = true)
            where Row: new() {

            object            instance;
            int               nbRows;
            PropertyInfo      property;
            DataTable         queryTable;
            Row[]             results;
            DataRow           row;
            Type              typeInt;
            Type              typeIntNullable;
            Type              typeT;

            queryTable = this.QueryDataTable ( sqlQuery   : sqlQuery
                                             , parameters : parameters
                                             , bindByName : bindByName);

            nbRows = queryTable.Rows.Count;

            // Building the array of result type
            results = new Row[nbRows];

            typeT           = typeof(Row);
            typeInt         = typeof(int);
            typeIntNullable = typeof(int?);

            // Populating the result array
            for(int q=0; q < nbRows; q++) {

                row = queryTable.Rows[q];

                instance = new Row();

                foreach(DataColumn column in queryTable.Columns) {
                    property = typeT.GetProperty(column.ColumnName);

                    if (row[column] is System.DBNull)
                        continue;

                    if (row[column] is System.Decimal && property.PropertyType == typeInt)
                        property.SetValue(instance, Convert.ToInt32(row[column]), null);

                    else if (row[column] is System.Decimal && property.PropertyType == typeIntNullable)
                        property.SetValue(instance, row[column] == null ? (int?) null : Convert.ToInt32(row[column]), null);

                    else
                        property.SetValue(instance, row[column], null);
                }

                results[q] = (Row) instance;
            }

            return results;
        }

        /// <summary>
        ///     Execute the given query.
        ///     All rows are fetched in one single time. If the query return 1.000.000 rows, all of them will be retreived
        /// </summary>
        /// <typeparam name="Row">Return type of each row</typeparam>
        /// <param name="sqlQuery">Query to execute</param>
        /// <param name="connection">Connection to use for execution</param>
        /// <param name="parameters">Parameters of the query</param>
        /// <returns></returns>
        public DataTable QueryDataTable ( string            sqlQuery
                                        , OracleParameter[] parameters = null
                                        , bool              bindByName = true) {

            OracleDataAdapter adapter;
            OracleCommand     command;
            DataSet           dataSet;
            Debug.SqlQuery    debugQuery;
            DataTable         queryTable;

            // Creating the command
            command = new OracleCommand(sqlQuery, this.connection);

            // Adding parameters
            if (parameters != null)
                foreach(OracleParameter parameter in parameters) {
                    command.Parameters.Add(parameter);
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName;

            adapter = new OracleDataAdapter(command);

            dataSet = new DataSet();

            // Adding the query into the debug
            debugQuery = debug.addQuery(sqlQuery, parameters);
            
            try {
                debugQuery.start();

                // Populating the dataset with the results of the query.
                adapter.Fill(dataSet, "query");

                // Retreiving the table of results
                queryTable = dataSet.Tables["query"];

                debugQuery.end();
            }
            catch (OracleException exception) {
                debugQuery.raiseError(exception);
                throw Error.QueryException.get ( exception  : exception
                                               , query      : sqlQuery
                                               , parameters : parameters);
            }

            adapter.Dispose();
            command.Dispose();

            return queryTable;
        }
    }
}
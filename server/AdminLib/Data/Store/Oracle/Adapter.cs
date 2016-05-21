using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using AdminLib.Data.Query;
using AdminLib.Data.Store.Oracle.Exception;

namespace AdminLib.Data.Store.Oracle {

    [AdapterDeclaration("AdminLib.Data.Store.Oracle")]
    public class Adapter : SQLAdapter {

        /******************** Attributes ********************/
        private bool             closeASAP    = false;
        public  Configuration    configuration { get; private set; }
        public  Debug.Connection debug { get; private set; }
        public  string           id    { get; private set; }
        private OracleConnection oracleConnection;
        private List<BaseCursor> openedCursors = new List<BaseCursor>();
        public  int              uid {get; private set; }

        public  override ConnectionState state {
            get {
                return this.oracleConnection.State;
            }
        }

        private OracleTransaction   transaction;

        /******************** Static Attributes ********************/
        private static Creator creator;

        /******************** Constructor ********************/
        public Adapter ( AdapterConfiguration configuration ) : base ( configuration) {}

        public Adapter ( string configuration) : base ( configuration) {}

        /******************** Structures ********************/
        private struct FunctionResult {
            public string STRING_VALUE {get; set;}
            public int    INT_VALUE    {get; set;}
        }

        /******************** Static Methods ********************/

        public static void Declare() {
            Creator creator;

            if (Adapter.creator == null)
                throw new System.Exception("Already declared");

            creator = new Creator("AdminLib.Data.Store.Oracle");

            Adapter.creator = creator;

            Adapter.DeclareAdapter(creator : creator);
        }

        /// <summary>
        ///     Create a new oracle connection using the default configuration
        /// </summary>
        /// <returns></returns>
        internal static OracleConnection GetNewOracleConnection() {
            OracleConnection oracleConnection;

            oracleConnection = new OracleConnection(""); // TODO
            oracleConnection.Open();

            return oracleConnection;
        }

        /// <summary>
        ///     Convert the query parameter to an oracle parameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        internal static OracleParameter ToOracleParameter(QueryParameter parameter) {

            OracleParameter oracleParameter;

            oracleParameter = new OracleParameter ( parameterName : parameter.name
                                                  , obj           : parameter.value);

            return oracleParameter;
        }

        internal static OracleParameter[] ToOracleParameters(QueryParameter[] parameters) {

            List<OracleParameter> oracleParameters;

            oracleParameters = new List<OracleParameter>();

            foreach(QueryParameter parameter in parameters) {
                oracleParameters.Add(Adapter.ToOracleParameter(parameter));
            }

            return oracleParameters.ToArray();
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
        public override bool Close(bool force = false, bool? commitTransactions = null) {

            if (this.oracleConnection.State == ConnectionState.Closed)
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
            
            this.closeASAP = true;

            // Commiting transactions only if asked or if auto commit is enabled
            if (commitTransactions == true || (commitTransactions == null && this.adapterConfiguration.autoCommit))
                this.Commit();
            else
                this.Rollback();

            this.oracleConnection.Close();
            return true;
        }

        /// <summary>
        ///     Comiting the transaction conditionnaly
        /// </summary>
        /// <param name="condition"></param>
        private void Commit(bool? condition) {
            if (condition == true || (condition == null && this.adapterConfiguration.autoCommit))
                this.Commit();
        }

        /// <summary>
        ///     Commit all performed transactions.
        /// </summary>
        public override void Commit() {

            if (this.transaction == null)
                return;

            this.transaction.Commit();

            // Creating a new transaction;
            this.transaction = this.oracleConnection.BeginTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        public override void ExecuteDML ( string           query
                                        , QueryParameter[] parameters = null
                                        , bool?            bindByName = null
                                        , bool?            commit     = null) {

            OracleDataAdapter adapter;
            OracleCommand     command;

            // Creating the command
            command = new OracleCommand(query, this.oracleConnection);

            // Adding parameters
            if (parameters != null)
                foreach(OracleParameter parameter in Adapter.ToOracleParameters(parameters)) {
                    command.Parameters.Add(parameter);
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName ?? true;

            adapter = new OracleDataAdapter(command);

            // Executing the query
            try {
                command.ExecuteNonQuery();
            }
            catch (OracleException exception) {
                throw AdapterException.Get ( exception  : exception
                                           , query      : query
                                           , parameters : parameters);
            }

            // Commit the transactions only if condition is true or if autoCommit is enabled
            this.Commit(commit);
        }

        /// <summary>
        ///     Execute the given PL/SQL code.
        ///     The code will be placed into a BEGIN/END bloc.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parameters"></param>
        public override void ExecuteCode ( string           code
                                         , QueryParameter[] parameters = null
                                         , bool?            bindByName = null
                                         , bool?            commit     = null) {

            OracleDataAdapter adapter;
            OracleCommand     command;

            code = "BEGIN " + code + "; END;";

            // Creating the command
            command = new OracleCommand(code, this.oracleConnection);

            // Adding parameters
            if (parameters != null)
                foreach(QueryParameter parameter in parameters) {
                    command.Parameters.Add(Adapter.ToOracleParameter(parameter));
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName ?? true;

            adapter = new OracleDataAdapter(command);

            // Executing the query
            try {
                command.ExecuteNonQuery();
            }
            catch (OracleException exception) {
                throw AdapterException.Get ( exception  : exception
                                           , query      : code
                                           , parameters : parameters);
            }
        }

        /// <summary>
        ///     Execute the given function.
        /// </summary>
        /// <typeparam name="T">The return type of the function result</typeparam>
        /// <param name="function">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override T ExecuteFunction<T> ( string           function
                                             , QueryParameter[] parameters = null
                                             , bool?            bindByName = null
                                             , bool?            commit     = null) {

            QueryParameter   outputParameter;
            QueryParameter   parameter;
            string           code;
            QueryParameter[] queryParameters;

            outputParameter = new QueryParameter ( name      : null
                                                 , direction : ParameterDirection.Output);

            // Building the code
            /*

            Builded code :
                BEGIN
                    :outputParameter = <function> ( <parameter1.name> => :<parameter1.name>
                                                  , <parameter2.name> => :<parameter2.name>
                                                  , <parameter3.name> => :<parameter3.name>
                                                  , ...
                                                  , <parameterN.name> => :<parameterN.name>);

                    <if commit>
                        COMMIT;

                END;
            */

            code = "BEGIN ";

            if (bindByName ?? true)
                code += ':' + outputParameter.name + " := " + function;
            else
                code += ":0 := " + function;

            for(int q=0; q < parameters.Length; q++) {

                parameter = parameters[q];

                if (bindByName ?? true)
                    code += parameter.name  + "=> :" + parameter.name + ',';
                else
                    code += ':' + (q+1) + ',';

            }

            code = code.Substring(0, code.Length - 1);
            code += ';';

            if (commit ?? false)
                code += "COMMIT;";

            code += "END;";

            // End building code

            queryParameters = new QueryParameter[parameters.Length + 1];

            parameters.CopyTo(queryParameters, 0);

            queryParameters[queryParameters.Length - 1] = outputParameter;

            this.ExecuteCode ( code       : code
                             , parameters : queryParameters
                             , bindByName : bindByName
                             , commit     : false);

            return (T) outputParameter.value;
        }

        /// <summary>
        ///     Execute the given function.
        /// </summary>
        /// <typeparam name="T">The return type of the function result</typeparam>
        /// <param name="function">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override void ExecuteProcedure ( string           procedure
                                              , QueryParameter[] parameters = null
                                              , bool?            bindByName = null
                                              , bool?            commit     = null) {

            QueryParameter   parameter;
            string           code;

            // Building the code
            /*

            Builded code :
                BEGIN
                    <procedure> ( <parameter1.name> => :<parameter1.name>
                                , <parameter2.name> => :<parameter2.name>
                                , <parameter3.name> => :<parameter3.name>
                                , ...
                                , <parameterN.name> => :<parameterN.name>);

                    <if commit>
                        COMMIT;
                END;
            */

            code = "BEGIN " + procedure + "(";

            for(int q=0; q < parameters.Length; q++) {

                parameter = parameters[q];

                if (bindByName ?? true)
                    code += parameter.name  + "=> :" + parameter.name + ',';
                else
                    code += ':' + (q+1) + ',';

            }

            code = code.Substring(0, code.Length - 1);
            code += ';';

            if (commit ?? false)
                code += "COMMIT;";

            code += "END;";

            // End building code

            this.ExecuteCode ( code       : code
                             , parameters : parameters
                             , bindByName : bindByName
                             , commit     : false);

            return;
        }


        internal OracleCommand getCommand(string sqlQuery) {
            return new OracleCommand(sqlQuery, this.oracleConnection);
        }

        public override void RegisterCursor(BaseCursor cursor) {
            this.openedCursors.Add(cursor);
        }

        /// <summary>
        /// Rollback the transaction.
        /// </summary>
        public override void Rollback() {

            if (this.transaction == null)
                return;

            this.transaction.Rollback();

            // Creating a new transaction;
            this.transaction = this.oracleConnection.BeginTransaction();
        }

        protected override void Initialize() {
            this.oracleConnection = Adapter.GetNewOracleConnection();

            if (!this.adapterConfiguration.autoCommit)
                this.transaction = this.oracleConnection.BeginTransaction();
        }

        public override void UnregisterCursor(BaseCursor cursor) {
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
        /// <param name="parameters">Parameters of the query</param>
        /// <returns></returns>
        public override DataTable QueryDataTable ( string           sqlQuery
                                                 , QueryParameter[] parameters = null
                                                 , bool?            bindByName = null) {

            OracleDataAdapter adapter;
            OracleCommand     command;
            DataSet           dataSet;
            DataTable         queryTable;

            // Creating the command
            command = new OracleCommand(sqlQuery, this.oracleConnection);

            // Adding parameters
            if (parameters != null)
                foreach(QueryParameter parameter in parameters) {
                    command.Parameters.Add(Adapter.ToOracleParameter(parameter));
                }

            command.CommandType = CommandType.Text;
            command.BindByName  = bindByName ?? true;

            adapter = new OracleDataAdapter(command);

            dataSet = new DataSet();
            
            try {

                // Populating the dataset with the results of the query.
                adapter.Fill(dataSet, "query");

                // Retreiving the table of results
                queryTable = dataSet.Tables["query"];
            }
            catch (OracleException exception) {
                throw AdapterException.Get ( exception  : exception
                                           , query      : sqlQuery
                                           , parameters : parameters);
            }

            adapter.Dispose();
            command.Dispose();

            return queryTable;
        }
    }
}
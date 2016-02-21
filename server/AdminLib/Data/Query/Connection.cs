using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using AdminLib.Data.Store;
using AdminLib.Model.Model;
using AdminLib.Model.Query;

namespace AdminLib.Data.Query {
    public class Connection {

        /******************** Static Attributes ********************/
        private static int              lastID = 0;
        public  static Connection       serverConnection { get; private set; }
        public  static List<Connection> connections = new List<Connection>();
         
        /******************** Attributes ********************/
        private Adapter             adapter;

        public  bool                autoCommit { get; private set; }
        public  Debug.Connection    debug      { get; private set; }
        public  int                 uid        { get; private set; }
        public ConnectionState      state      { get; private set; }

        /******************** Constructors ********************/
        public Connection ( string configuration = null
                          , string manager       = null
                          , bool   autoCommit    = true) {

            this.adapter = Adapter.GetAdapter ( configuration : configuration
                                              , autoCommit    : true);

            this.uid        = Connection.lastID++;
            this.state      = ConnectionState.Connecting;
            this.debug      = new Debug.Connection(this);

            Connection.connections.Add(this);
        }

        /******************** Static Methods ********************/
        /// <summary>
        ///     Convert a dictionnary parameter list ot an array of queryParameter objects.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static QueryParameter[] ToArray(Dictionary<string, object> parameters){

            QueryParameter       queryParameter;
            List<QueryParameter> queryParameters;
            Object               value;

            queryParameters = new List<QueryParameter>();

            foreach(KeyValuePair<string, object> entry in parameters) {

                if (entry.Value is bool)
                    value = (bool) entry.Value ? 1 : 0;
                else
                    value = entry.Value;

                queryParameter = new QueryParameter ( name  : entry.Key
                                                    , value : value );

                queryParameters.Add(queryParameter);
            }

            return queryParameters.ToArray();

        }

        /******************** Methods ********************/

        /// <summary>
        ///     Close the connection.
        ///     If force is false, then the connection will remain if there is still at least one cursor opened.
        /// </summary>
        /// <param name="force">If true, then the connection will be closed, even if there is opened cursors</param>
        /// <param name="commitTransactions">If true, then all remaining transactions will be commited</param>
        public bool Close(bool force = false, bool? commitTransactions = null) {

            bool closed;

            if (this.state == ConnectionState.Closed)
                return true;

            try {
                closed = this.adapter.Close ( force              : force
                                            , commitTransactions : commitTransactions);

                if (!closed)
                    return false;
            }
            catch(System.Exception) {
                return false;
            }

            this.state = ConnectionState.Closed;
            Connection.connections.Remove(this);

            return true;
        }

        /// <summary>
        ///     Commit all performed transactions.
        /// </summary>
        public void Commit() {
            this.adapter.Commit();
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
        ///     Reccord the instance into the store.
        ///     If the model is sequence based and the instance has no ID provided, then the function
        ///     will create a new ID using the sequence.
        ///     The newly created ID will be return.
        ///
        ///     If no ID has been created (e.g because the item is not sequence based), then null is returned.
        /// </summary>
        /// <returns>Newly created ID</returns>
        public int? Create (AStructure model, object instance, string[] fields=null) {

            return this.adapter.Create ( model    : model
                                       , instance : instance
                                       , fields   : fields);

        }

        /// <summary>
        ///     Delete in the database the reccord corresponding to the instance.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="instance"></param>
        public void Delete(AStructure model, object instance) {

            this.adapter.Delete ( model    : model
                                , instance : instance);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        public void ExecuteDML ( string           query
                               , QueryParameter[] parameters
                               , bool?            bindByName = null
                               , bool?            commit     = null) {

            this.adapter.ExecuteDML ( query      : query
                                    , parameters : parameters ?? new QueryParameter[0]
                                    , bindByName : bindByName
                                    , commit     : commit);
        }

        /// <summary>
        ///     Execute the given code into the data store
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        public void ExecuteDML ( string                     query
                               , Dictionary<string, object> parameters = null
                               , bool?                      commit     = null) {

            this.ExecuteDML ( query      : query
                            , parameters : Connection.ToArray(parameters)
                            , bindByName : true
                            , commit     : commit);
        }

        /// <summary>
        ///     Execute the given code into the data store
        ///     The code will be placed into a BEGIN/END bloc.
        /// </summary>
        /// <param name="code">Code to execute</param>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        public void ExecuteCode ( string           code
                                , QueryParameter[] parameters = null
                                , bool?            bindByName = null
                                , bool?            commit     = null) {

            this.adapter.ExecuteCode ( code       : code
                                     , parameters : parameters ?? new QueryParameter[0]
                                     , bindByName : bindByName
                                     , commit     : commit);

        }

        /// <summary>
        ///     Execute the given code into the data store
        ///     The code will be placed into a BEGIN/END bloc.
        /// </summary>
        /// <param name="code">Code to execute</param>
        /// <param name="parameters"></param>
        /// <param name="commit"></param>
        public void ExecuteCode ( string                     code 
                                , Dictionary<string, object> parameters
                                , bool?                      commit     = null) {

            this.ExecuteCode ( code       : code
                             , parameters : Connection.ToArray(parameters)
                             , bindByName : true
                             , commit     : commit);

        }

        /// <summary>
        ///     Execute the given function.
        /// </summary>
        /// <typeparam name="T">The return type of the function result</typeparam>
        /// <param name="function">Function to execute</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteFunction<T> ( string           function
                                    , QueryParameter[] parameters
                                    , bool?            bindByName = null
                                    , bool?            commit     = null) {

            return this.adapter.ExecuteFunction<T> ( function   : function
                                                   , parameters : parameters
                                                   , bindByName : bindByName
                                                   , commit     : commit);
        }

        /// <summary>
        ///     Execute the given function with the parameters.
        /// </summary>
        /// <typeparam name="T">The return type of the function result</typeparam>
        /// <param name="function"></param>
        /// <param name="parameters"></param>
        /// <param name="commit">If null, the commit will be done if the autocommit is enabled</param>
        /// <returns></returns>
        public T ExecuteFunction<T> ( string                     function
                                    , Dictionary<string, object> parameters = null
                                    , bool?                      commit     = null) {

            return this.ExecuteFunction<T> ( function   : function
                                           , parameters : Connection.ToArray(parameters)
                                           , bindByName : true
                                           , commit     : commit);
        }

        public string ExecuteFunction ( string           function
                                      , QueryParameter[] parameters
                                      , bool?            bindByName = null
                                      , bool?            commit     = null) {

            return this.ExecuteFunction<string> ( function   : function
                                                , parameters : parameters ?? new QueryParameter[0]
                                                , bindByName : bindByName
                                                , commit     : commit);

        }

        public string ExecuteFunction ( string                     function
                                      , Dictionary<string, object> parameters = null
                                      , bool?                      commit     = null) {

            return this.ExecuteFunction ( function   : function
                                        , parameters : Connection.ToArray(parameters)
                                        , bindByName : true
                                        , commit     : commit);

        }

        /// <summary>
        ///     Execute a procedure into the data store
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <param name="commit"></param>
        public void ExecuteProcedure ( string                     procedure
                                     , Dictionary<string, object> parameters = null
                                     , bool?                      commit     = null) {

            this.ExecuteProcedure ( procedure  : procedure
                                  , parameters : Connection.ToArray(parameters)
                                  , bindByName : true
                                  , commit     : commit);
        }

        /// <summary>
        ///     Execute a procedure into the data store
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <param name="bindByName"></param>
        /// <param name="commit"></param>
        public void ExecuteProcedure ( string           procedure
                                     , QueryParameter[] parameters
                                     , bool?            bindByName = null
                                     , bool?            commit     = null) {

            this.adapter.ExecuteProcedure ( procedure  : procedure
                                          , parameters : parameters ?? new QueryParameter[0]
                                          , bindByName : bindByName
                                          , commit     : commit);

        }

        /// <summary>
        ///     Indicate if the connection do an autocommit at each DML command or not.
        /// </summary>
        /// <returns></returns>
        public bool IsAutoCommitEnabled() {
            return this.autoCommit;
        }

        public object[] QueryItems ( AStructure model
                                   , string[]   fields
                                   , Filter     filter
                                   , OrderBy[]  orderBy = null) {

            return this.adapter.QueryItems ( model   : model
                                           , fields  : fields
                                           , filter  : filter
                                           , orderBy : orderBy);

        }

        public void RegisterCursor(BaseCursor cursor) {
            this.adapter.RegisterCursor(cursor);
        }

        /// <summary>
        /// Rollback the transaction.
        /// </summary>
        public void Rollback() {
            this.adapter.Rollback();
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
        public Row[] Query<Row> ( string           query
                                , QueryParameter[] parameters
                                , bool?            bindByName = null)
            where Row: new() {

            return this.adapter.Query<Row> ( query      : query
                                           , parameters : parameters
                                           , bindByName : bindByName);
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
        public Row[] Query<Row> ( string                     query
                                , Dictionary<string, object> parameters = null)
            where Row: new() {

            return this.Query<Row> ( query      : query
                                   , parameters : Connection.ToArray(parameters)
                                   , bindByName : true);

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
        public DataTable QueryDataTable ( string           query
                                        , QueryParameter[] parameters = null
                                        , bool?            bindByName = null) {

            return this.adapter.QueryDataTable ( query      : query
                                               , parameters : parameters ?? new QueryParameter[0]
                                               , bindByName : bindByName);
        }

        public void UnregisterCursor(BaseCursor cursor) {
            this.adapter.UnregisterCursor(cursor);
        }

        /// <summary>
        ///     Update the corresponding row in the database.
        /// </summary>
        /// <param name="connection">Connection to use</param>
        /// <param name="model">Model corresponding to the instance</param>
        /// <param name="instance">Instance that will be updated in the database</param>
        /// <param name="fields">Fields to update. If null, then all fields will be updated. NULL fields will not be updated</param>
        /// <param name="emptyFields">All given fields will be emptied in the database</param>
        public void Update ( AStructure model
                           , object     instance
                           , string[]   fields      = null
                           , string[]   emptyFields = null) {

            this.adapter.Update ( model       : model
                                , instance    : instance
                                , fields      : fields
                                , emptyFields : emptyFields);

        }

    }
}
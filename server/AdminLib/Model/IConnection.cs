using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminLib.Model.Query;

namespace AdminLib.Model {
    public interface IConnection {

        DataTable QueryDataTable ( string            sqlQuery
                                 , OracleParameter[] parameters = null
                                 , bool              bindByName = true);

        /// <summary>
        /// Close the connection.
        /// 
        /// The closing MUST fail if there is remaining curors opened.
        /// 
        /// </summary>
        /// <param name="force">
        /// If true, then the closing WILL NOT fail if there is remaining cursors.
        /// </param>
        /// <param name="commitTransactions">
        /// If true, then all transactions will be commited before closing.
        /// </param>
        /// <returns>
        /// Indicate if the closing was successful (true) or not (false).
        /// The closing will be unsuccessful if force=false and there is still remaining cursors.
        /// </returns>
        bool Close ( bool force = false
                   , bool? commitTransactions = null);

        void Commit();

        void ExecuteDML ( string            sqlQuery
                        , OracleParameter[] parameters=null
                        , bool              bindByName=true
                        , bool?             commit=null);

        FunctionResult ExecuteFunction ( string            function
                                       , OracleParameter[] parameters = null
                                       , bool              bindByName = true
                                       , bool?             commit     = null);

        FunctionResult ExecuteFunction ( string                     function
                                       , Dictionary<string, Object> parameters
                                       , bool?                      commit = null);

        void ExecuteProcedure ( string                     procedure
                              , Dictionary<string, Object> parameters);

        void ExecuteCode ( string            code
                         , OracleParameter[] parameters
                         , bool              bindByName=true);

        /// <summary>
        ///     Indicate if the connection do an autocommit at each DML command or not.
        /// </summary>
        /// <returns></returns>
        bool IsAutoCommitEnabled();

        void Rollback();
    }
}

using System.Data;
using AdminLib.Data.Query;

namespace AdminLib.Data.Store.Adapter
{
    public interface IAdapter {

        ConnectionState state {get; }

        DataTable QueryDataTable ( string           query
                                 , QueryParameter[] parameters = null
                                 , bool?            bindByName = null);

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

        void ExecuteDML ( string           query
                        , QueryParameter[] parameters = null
                        , bool?            bindByName = null
                        , bool?            commit     = null);

        T ExecuteFunction<T> ( string           function
                             , QueryParameter[] parameters = null
                             , bool?            bindByName = null
                             , bool?            commit     = null);

        void ExecuteProcedure ( string           procedure
                              , QueryParameter[] parameters = null
                              , bool?            bindByName = null
                              , bool?            commit     = null);

        void ExecuteCode ( string           code
                         , QueryParameter[] parameters = null
                         , bool?            bindByName = null
                         , bool?            commit     = null);

        void RegisterCursor (BaseCursor cursor);

        void UnregisterCursor(BaseCursor cursor);

        void Rollback();
    }
}

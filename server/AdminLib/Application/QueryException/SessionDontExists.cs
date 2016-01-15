using AdminLib.Database;
using System.Data.Common;

namespace AdminLib.App.QueryException {

    public class SessionDontExists : AdminLib.Database.Error.QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.App.QueryException.SessionDontExists";

        //******************** Constructors ********************/
        public SessionDontExists ( DbException exception
                                 , string query=null
                                 , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
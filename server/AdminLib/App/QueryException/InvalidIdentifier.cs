using AdminLib.Data.Query;
using System.Data.Common;

namespace AdminLib.App.QueryException {

    public class InvalidIdentifier : AdminLib.Data.Query.Exception.QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.App.QueryException.InvalidIdentifier";

        //******************** Constructors ********************/
        public InvalidIdentifier ( DbException      exception
                                 , string           query=null
                                 , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
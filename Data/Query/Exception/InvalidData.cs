using System.Data.Common;

namespace AdminLib.Data.Query.Exception {
    public class InvalidData : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Data.Query.Error.InvalidData";

        //******************** Constructors ********************/
        public InvalidData ( DbException exception
                           , string query=null
                           , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
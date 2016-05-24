using System.Data.Common;

namespace AdminLib.Data.Query.Exception {

    public class InvalidID : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Data.Query.Error.InvalidID";

        //******************** Constructors ********************/
        public InvalidID ( DbException exception
                         , string query=null
                         , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }

}
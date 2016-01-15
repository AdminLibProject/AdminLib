using System.Data.Common;

namespace AdminLib.Database.Error {

    public class InvalidID : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Database.Error.InvalidID";

        //******************** Constructors ********************/
        public InvalidID ( DbException exception
                         , string query=null
                         , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }

}
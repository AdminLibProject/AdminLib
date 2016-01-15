using System.Data.Common;

namespace AdminLib.Database.Error
{
    public class InvalidData : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Database.Error.InvalidData";

        //******************** Constructors ********************/
        public InvalidData ( DbException exception
                           , string query=null
                           , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
using AdminLib.Database;
using System.Data.Common;

namespace AdminLib.App.QueryException {

    public class InvalidPassword : AdminLib.Database.Error.QueryException  {

        //******************** Constants ********************/
        public string code = "AdminLib.App.QueryException.InvalidPassword";

        //******************** Constructors ********************/
        public InvalidPassword ( DbException      exception
                               , string           query=null
                               , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
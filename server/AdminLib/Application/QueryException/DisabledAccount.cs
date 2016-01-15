using AdminLib.Database;
using System.Data.Common;

namespace AdminLib.App.QueryException {

    public class DisabledAccount : AdminLib.Database.Error.QueryException {

        /******************** Constants ********************/
        public string code = "AdminLib.App.QueryException.DisabledAccount";

        /******************** Constructors ********************/
        public DisabledAccount ( DbException      exception
                               , string           query=null
                               , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
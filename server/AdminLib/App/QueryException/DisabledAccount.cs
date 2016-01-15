using AdminLib.Data.Query;
using System.Data.Common;

namespace AdminLib.App.QueryException {

    public class DisabledAccount : AdminLib.Data.Query.Exception.QueryException {

        /******************** Constants ********************/
        public string code = "AdminLib.App.QueryException.DisabledAccount";

        /******************** Constructors ********************/
        public DisabledAccount ( DbException      exception
                               , string           query=null
                               , QueryParameter[] parameters=null) :
            base(exception, query, parameters) { }

    }
}
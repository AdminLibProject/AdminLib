using System.Data.Common;

namespace AdminLib.Data.Query.Exception
{

    public class UniqueConstraintViolated : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Data.Query.Error.UniqueConstraintViolated";

        //******************** Constructors ********************/
        public UniqueConstraintViolated ( DbException      exception
                                        , string           query=null
                                        , QueryParameter[] parameters=null)
            : base(exception, query, parameters) { }

    }
}
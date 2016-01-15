using System.Data.Common;

namespace AdminLib.Database.Error
{

    public class UniqueConstraintViolated : QueryException {

        //******************** Constants ********************/
        public string code = "AdminLib.Database.Error.UniqueConstraintViolated";

        //******************** Constructors ********************/
        public UniqueConstraintViolated ( DbException      exception
                                        , string           query=null
                                        , QueryParameter[] parameters=null)
            : base(exception, query, parameters) { }

    }
}
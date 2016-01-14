using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminLib.Model;

namespace AdminLib {
    public abstract class Model<Self> : AdminLib.Model.DjangoModel<Self>
                                      , IAdminQueryResult
        where Self: AdminLib.Model.DjangoModel<Self> {

        public Debug.Debug  debug   { get; set;}
        public string       message { get; set; }

    }
}
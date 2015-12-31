using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DjangoSharp;
using AdminLib.Model;

namespace AdminLib {
    public abstract class Model<Self> : DjangoSharp.DjangoModel<Self>
                                      , IAdminQueryResult
        where Self: DjangoSharp.DjangoModel<Self> {

        public Debug.Debug  debug   { get; set;}
        public string       message { get; set; }

    }
}
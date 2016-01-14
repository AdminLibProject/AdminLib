using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model {

    [AttributeUsage ( AttributeTargets.Class
                    , AllowMultiple = false)
    ]
    public class Meta : System.Attribute {

        /******************** Attribute ********************/
        public Model.Meta meta { get; private set; }

        /******************** Constructors ********************/
        public Meta(string table = null, string apiName = null) {

            this.meta = new Model.Meta ( table         : table
                                       , apiName       : apiName
                                       , sequenceBased : null);

        }

        public Meta(bool sequenceBased, string table = null, string apiName = null) {

            this.meta = new Model.Meta ( table         : table
                                       , apiName       : apiName
                                       , sequenceBased : sequenceBased);

        }

    }
}
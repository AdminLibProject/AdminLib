using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Query {

    public class Filter {

        /******************** Attributes ********************/
        public  List<FieldFilter> filters = new List<FieldFilter>();

        /******************** Constructors ********************/
        public Filter() {
        }

        public Filter(FieldFilter[] filters) {
            foreach (FieldFilter filter in filters) {
                this.Add(filter);
            }
        }

        public Filter(FieldFilter filter) {
            this.Add(filter);
        }

        /******************** Methods ********************/
        public void Add(FieldFilter filter) {
            this.filters.Add(filter);
        }
    }

}
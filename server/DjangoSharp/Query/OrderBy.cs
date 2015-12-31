using DjangoSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp.Query {
    public class OrderBy {

        public OrderByDirection direction { get; private set; }
        public string           field     { get; private set; }

        public OrderBy(string field) {

            string direction;

            if (field.IndexOf(':') > -1) {
                direction = field.Substring(field.IndexOf(':') + 1).ToUpper();
                field     = field.Substring(0, field.IndexOf(':'));
            }
            else
                direction = "ASC";

            this.field = field;
            this.direction = direction == "DESC" ? OrderByDirection.desc : OrderByDirection.asc;
        }

        public OrderBy(string field, OrderByDirection direction) {
            this.field     = field;
            this.direction = direction;
        }

    }
}
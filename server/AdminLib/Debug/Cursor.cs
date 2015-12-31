using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminLib.Database;

namespace AdminLib.Debug {

    public class Cursor {

        /******************** Attributes ********************/
        public int id;

        /******************** Constructors ********************/
        public Cursor(BaseCursor cursor) {
            this.id = cursor.GetID();
        }

    }
}
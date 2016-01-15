using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Data.Query {
    public struct CursorResult<Item> {

        public int    id;
        public bool   IsOpen;
        public Item[] items;

        public CursorResult(BaseCursor cursor, Item[] items) {
            this.id     = cursor.GetID();
            this.IsOpen = cursor.IsOpen();
            this.items  = items;
        }

    }
}
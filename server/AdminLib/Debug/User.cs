using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {
    public class User {
        /******************** Attributes ********************/
        public int?   id;
        public string username;
        public string email;

        /******************** Constructors ********************/
        public User(Auth.User user) {
            this.id       = user.id;
            this.email    = user.email;
            this.username = user.username;
        }

    }
}
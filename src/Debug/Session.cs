using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Debug {
    public class Session {
        /******************** Attributes ********************/
        public  string           id;
        private App.Auth.Session session;
        public  User             user;

        /******************** Constructors ********************/
        public Session (App.Auth.Session session) {

            this.session = session;
            this.id      = session.sessionId;

            if (session.user != null)
                this.user = new User(user : session.user);
            else
                this.user = null;
        }
    }
}
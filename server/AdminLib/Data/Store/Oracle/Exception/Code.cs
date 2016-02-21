using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Data.Store.Oracle.Exception
{
    public enum Code {

        // Application errors
          INVALID_ID                     = 20001
        , DUPLICATE_KEY                  = 20002
        , INVALID_DATA                   = 20003
        , INVALID_PASSWORD               = 20004
        , DISABLED_ACCOUNT               = 20005
        , SESSION_DONT_EXISTS            = 20006
        , NO_USER_CONNECTED              = 20007
        , INVALID_FORMAT                 = 20008
        , DEPENDENCY_ERROR               = 20009

          // Oracle standard errors
        , UNIQUE_CONSTRAINT_VIOLATED     = 1
        , INVALID_IDENTIFIER             = 904
        , INSUFFICIENT_PRIVILEGES        = 1031
        , SUCCESS_WITH_COMPILATION_ERROR = 24344
    }
}
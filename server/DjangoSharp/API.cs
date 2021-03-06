﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DjangoSharp.Query;
using DjangoSharp.Model;

namespace DjangoSharp {
    public static class API {

        /******************** Methods ********************/

        public static FilterOperator? GetFilterOperator(string operation) {
            return FilterType_extension.StringToFilter(operation);
        }

        public static bool IsFilterOperator(string operation) {
            return GetFilterOperator(operation) != null;
        }

        public static GroupOperator? GetGroupOperator(string operation) {
            return GroupOperator_extension.Get(operation);
        }

        public static bool IsGroupOperator(string operation) {
            return GetGroupOperator(operation) != null;
        }

    }
}
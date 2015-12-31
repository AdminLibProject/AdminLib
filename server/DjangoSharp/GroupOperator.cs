using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp {
    /*
     * List of all filter's type
     * A filter type define the condition of a filter.
     * 
     */

    public enum GroupOperator {
          count
        , sum
        , avg
    }

    public enum OrderByDirection
    {
          desc
        , asc
    }

    public static class GroupOperator_extension {

        /// <summary>
        ///     Dictionnary of all types by their string representation
        /// </summary>
        private static Dictionary<string, GroupOperator> operatorFromString = new Dictionary<string, GroupOperator>() {
              {"count" , GroupOperator.count }
            , {"sum"   , GroupOperator.sum   }
            , {"avg"   , GroupOperator.avg   }
        };

        /// <summary>
        ///     Dictionnary of all types by their string representation
        /// </summary>
        private static Dictionary<GroupOperator, string> typeToString = new Dictionary<GroupOperator, string>() {
              {GroupOperator.count, "count" }
            , {GroupOperator.sum  , "sum"   }
            , {GroupOperator.avg  , "avg"   }
        };

        /// <summary>
        ///     Return the string representation of a type
        ///     If no type correspond, then return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToString(this GroupOperator type) {
            if (!typeToString.ContainsKey(type))
                return "";

            return typeToString[type];
        }

        /// <summary>
        ///     Return the operator corresponding to a string.
        ///     If no type correspond, then return null.
        /// </summary>
        /// <param name="operatorName"></param>
        /// <returns></returns>
        public static GroupOperator? Get(string operatorName) {
            if (!operatorFromString.ContainsKey(operatorName))
                return null;

            return operatorFromString[operatorName];
        }

        /// <summary>
        /// 
        ///     Convert the filter operator to a SQL operation.
        ///     The function will also add a new SQL parameter into the parameters list.
        ///     Example :
        ///         FilterOperator.equal.toSQL("id", ["1"], SQLType.NUMBER)
        ///             ==> "id = 1"
        ///
        ///         FilterOperator.in.toSQL("code", ["fr", "en", "us", "ge", "it"], [parameters], "@code")
        ///             ==> "code IN @code"
        ///             
        ///         FilterOperator.between.toSQL("value", ["1", "10"], [parameters], "@value")
        ///             ==> "value BETWEEN @value_min AND @value_max"
        ///             
        ///         FilterOperator.like.toSQL("label", "%france%", [parameters], @label)
        ///             ==> "label LIKE @label"
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="column"></param>
        /// <param name="dbType"></param>
        /// <param name="values"></param>
        /// <param name="parameters">List of parameters of the SQL query. Used to add the new OracleParameter objects</param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static string toSQL(this GroupOperator type, string column) {

            switch (type) {

                case GroupOperator.avg:
                    return "AVG(" + column + ")";

                case GroupOperator.count:
                    return "COUNT(" + column + ")";

                case GroupOperator.sum:
                    return "SUM(" + column + ")";               
            }

            return null;

        }

    }
}
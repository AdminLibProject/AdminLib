using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DjangoSharp {
    /*
     * List of all filter's type
     * A filter type define the condition of a filter.
     * 
     */

    public enum FilterOperator {
          between
        , different
        , equal
        , greaterOrEqualThan
        , greaterThan
        , inList
        , like
        , lesserThan
        , lesserOrEqualThan
        , notInList
        , notLike 
        , isNull
        , notNull
    }

    public static class FilterType_extension {
        /// <summary>
        ///     Dictionnary of all types by their string representation
        /// </summary>
        private static Dictionary<string, FilterOperator> operatorFromString = new Dictionary<string,FilterOperator>() {
              {"between"           , FilterOperator.between           }
            , {"different"         , FilterOperator.different         }
            , {"equal"             , FilterOperator.equal             }
            , {"greaterThan"       , FilterOperator.greaterThan       }
            , {"greaterOrEqualThan", FilterOperator.greaterOrEqualThan}
            , {"lesserThan"        , FilterOperator.lesserThan        }
            , {"lesserOrEqualThan" , FilterOperator.lesserOrEqualThan }
            , {"like"              , FilterOperator.like              }
            , {"in"                , FilterOperator.inList            }
            , {"notIn"             , FilterOperator.notInList         }
            , {"notLike"           , FilterOperator.notLike           }
            , {"null"              , FilterOperator.isNull            }
            , {"notNull"           , FilterOperator.notNull           }
        };

        /// <summary>
        ///     Dictionnary of string representation of each filter types
        /// </summary>
        private static Dictionary<FilterOperator, string> operatorToString = new Dictionary<FilterOperator, string>() {
              {FilterOperator.between           , "between"           }
            , {FilterOperator.different         , "different"         }
            , {FilterOperator.equal             , "equal"             }
            , {FilterOperator.greaterThan       , "greaterThan"       }
            , {FilterOperator.greaterOrEqualThan, "greaterOrEqualThan"}
            , {FilterOperator.lesserThan        , "lesserThan"        }
            , {FilterOperator.lesserOrEqualThan , "lesserOrEqualThan" }
            , {FilterOperator.like              , "like"              }
            , {FilterOperator.inList            , "in"                }
            , {FilterOperator.notInList         , "notIn"             }
            , {FilterOperator.notLike           , "notLike"           }
            , {FilterOperator.isNull            , "null"              }
            , {FilterOperator.notNull           , "notNull"           }
        };

        /// <summary>
        ///     Return the string representation of a type
        ///     If no type correspond, then return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToString(this FilterOperator type) {
            if (!operatorToString.ContainsKey(type))
                return "";

            return operatorToString[type];
        }

        /// <summary>
        ///     Return the operator corresponding to a string.
        ///     If no type correspond, then return null.
        /// </summary>
        /// <param name="operatorName"></param>
        /// <returns></returns>
        public static FilterOperator? ToFilter(this string operatorName) {

            return FilterType_extension.StringToFilter(operatorName);

        }

        public static FilterOperator? StringToFilter(string operatorName) {
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
        public static string toSQL(this FilterOperator type, string column, OracleDbType dbType, string[] values, List<OracleParameter> parameters, string parameterName=null) {

            bool   addParameter;
            bool   first;
            string paramName_max;
            string paramName_min;
            string sql;

            addParameter  = true;
            sql = column;
            parameterName = ":" + (parameterName == null ? 'p' + parameters.Count.ToString() : parameterName);

            switch (type) {

                case FilterOperator.between:

                    paramName_max = parameterName + "_max";
                    paramName_min = parameterName + "_min";

                    sql += " BETWEEN " + parameterName + " AND " + paramName_max;

                    parameters.Add(new OracleParameter ( direction     : ParameterDirection.Input
                                                       , obj           : values[0] 
                                                       , parameterName : paramName_min
                                                       , type          : dbType) );

                    parameters.Add(new OracleParameter ( direction     : ParameterDirection.Input
                                                       , obj           : values[1] 
                                                       , parameterName : paramName_max
                                                       , type          : dbType) );

                    addParameter = false;
                    break;

                case FilterOperator.different:
                    sql += " <> " + parameterName;
                    break;

                case FilterOperator.equal:
                    sql += " = " + parameterName;
                    break;

                case FilterOperator.greaterOrEqualThan:
                    sql += " >= " + parameterName;
                    break;

                case FilterOperator.greaterThan:
                    sql += " > " + parameterName;
                    break;

                case FilterOperator.inList:
                    sql += " IN (";
                    first = true;

                    for(int v=0; v < values.Length; v++) {

                        sql += (first ? "" : ",") + parameterName + "_" + v.ToString();

                        parameters.Add(new OracleParameter ( direction     : ParameterDirection.Input
                                                           , obj           : values[v] 
                                                           , parameterName : parameterName + "_" + v.ToString()
                                                           , type          : dbType) );

                        first = false;
                    }

                    sql += ")";

                    addParameter = false;

                    break;

                case FilterOperator.lesserOrEqualThan:
                    sql += " <= " + parameterName;
                    break;

                case FilterOperator.lesserThan:
                    sql += " < " + parameterName;
                    break;

                case FilterOperator.like:
                    sql += " LIKE " + parameterName;
                    break;

                case FilterOperator.notInList:
                    sql += " NOT IN ";
                    first = true;

                    for(int v=0; v < values.Length; v++) {

                        sql += (first ? "" : ",") + parameterName + "_" + v.ToString();

                        parameters.Add(new OracleParameter ( direction     : ParameterDirection.Input
                                                           , obj           : values[v] 
                                                           , parameterName : parameterName + "_" + v.ToString()
                                                           , type          : dbType) );

                        first = false;
                    }

                    sql += ")";

                    addParameter = false;

                    break;

                case FilterOperator.notLike:
                    sql += " NOT LIKE " + parameterName;
                    break;

                case FilterOperator.isNull:
                    sql += " IS NULL ";
                    addParameter = false;
                    break;

                case FilterOperator.notNull:
                    sql += " IS NOT NULL ";
                    addParameter = false;
                    break;
            }

            if (addParameter)
                parameters.Add(new OracleParameter ( direction     : ParameterDirection.Input
                                                   , obj           : values[0]
                                                   , parameterName : parameterName
                                                   , type          : dbType) );

            return sql;
        }

    }

}
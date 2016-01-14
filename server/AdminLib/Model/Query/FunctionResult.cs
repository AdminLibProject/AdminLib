using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model.Query {
    public class FunctionResult {

        private string value;

        /******************** Constructors ********************/
        public FunctionResult(string value) {
            this.value = value;
        }

        /******************** explicit operators ********************/
        public static implicit operator string(FunctionResult result) {
            return result.value;
        }

        public static implicit operator int(FunctionResult result) {
            return Convert.ToInt32(result.value);
        }

        public static implicit operator int?(FunctionResult result) {
            if (result.value == null)
                return null;

            return Convert.ToInt32(result.value);
        }

        public static implicit operator bool(FunctionResult result) {
            return result.value != null && result.value != "" && result.value != "0";
        }

        public static implicit operator bool?(FunctionResult result) {
            if (result.value == null)
                return null;

            return (bool) result;
        }

    }
}
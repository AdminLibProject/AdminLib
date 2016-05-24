using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Model
{
    public class InvalidStructure : Exception    {

        public InvalidStructure(string message) : base(message) { }

    }
}
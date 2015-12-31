using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DjangoSharp
{
    public class InvalidStructure : Exception    {

        public InvalidStructure(string message) : base(message) { }

    }
}
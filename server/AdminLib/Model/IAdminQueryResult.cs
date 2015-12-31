using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminLib.Model {
    public interface IAdminQueryResult {
        Debug.Debug  debug   { get; set; }
        string       message { get; set; }
    }
}

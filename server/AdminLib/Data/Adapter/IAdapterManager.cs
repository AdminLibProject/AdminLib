using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminLib.Data.Adapter {

    public interface IAdapterManager {

        IAdapter GetNewAdapter(bool autoCommit);

    }

}
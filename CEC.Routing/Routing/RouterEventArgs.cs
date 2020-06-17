using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Routing.Data
{
    public class RouterEventArgs : EventArgs
    {
        public bool ResubmitUrl { get; set; }

        public RouterEventArgs()
        {
        }

        public RouterEventArgs(bool reSubmitUrl)
        {
            this.ResubmitUrl = reSubmitUrl;
        }
    }
}

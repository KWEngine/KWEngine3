using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal struct LoadPackage
    { 
        public object Receiver;
        public Type ReceiverType;
        public Action Action;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class ProxyConnection
    {
        public static IWebProxy defaultProxy = WebRequest.GetSystemWebProxy();
    }
}

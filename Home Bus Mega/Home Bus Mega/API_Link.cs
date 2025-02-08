using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Bus_Mega
{
    class API_Link
    {
        public static string KMB_base_url = @"https://data.etabus.gov.hk/";
        public static string KMB_stop_eta_url = KMB_base_url + "/v1/transport/kmb/stop-eta/";
    }
}

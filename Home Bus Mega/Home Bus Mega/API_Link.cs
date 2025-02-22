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

        public static string CTB_base_url = @"https://rt.data.gov.hk";
        public static string CTB_eta_url = CTB_base_url + "/v2/transport/citybus/eta/CTB/"; //{stop_id}/{route_name}

        public static string LRT_eta_url = @"https://rt.data.gov.hk/v1/transport/mtr/lrt/getSchedule?station_id="; //{station id}

        public static string NLB_base_url = @"https://rt.data.gov.hk/v2/transport/nlb/",
            NLB_eta_url = NLB_base_url + @"stop.php?action=estimatedArrivals&routeId={0}&stopId={1}&language=en";

        public static string MTRB_eta_url = "https://rt.data.gov.hk/v1/transport/mtr/bus/getSchedule";

        public static string GMB_base_url = "https://data.etagmb.gov.hk";
        public static string GMB_eta_url = GMB_base_url + "/eta/stop/"; //{stop id}

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Bus_Mega
{
    class Bus_Route
    {
        public static string[] LWB_non_A_routes = 
            {
                // N routes
                "N30", "N31", "N42", "N42A", "N64",

                // R route
                "R8", "R33", "R42",

                // X route
                "X1", "X33", "X36", "X40", "X43", "X47"
            };

        public static bool is_LWB_route(string route_name)
        {
            if (!route_name.StartsWith("SP"))
            {
                if (route_name.StartsWith("A") ||
                    route_name.StartsWith("NA") ||
                    route_name.StartsWith("S") ||
                    LWB_non_A_routes.Contains(route_name))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool is_CTBA_route(string route_name)
        {
            if (route_name.StartsWith("A") || route_name.StartsWith("NA"))
            {
                return true;
            }
            return false;
        }
    }
}

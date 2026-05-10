using Common;
using System.Collections.Generic;

namespace Service
{
    public static class SensorDatabase
    {
        public static List<SensorSample> Measurements { get; set; } = new List<SensorSample>();
        public static List<SensorSample> Rejects { get; set; } = new List<SensorSample>();
        public static SessionMeta CurrentSession { get; set; } = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SensorSample
    {
        [DataMember]
        public double Volume { get; set; }

        [DataMember]
        public double CO { get; set; }

        [DataMember]
        public double NO2 { get; set; }

        [DataMember]
        public double Pressure { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            return $"[{DateTime}] Pressure={Pressure}, CO={CO}, NO2={NO2}, Volume={Volume}";
        }
    }
}

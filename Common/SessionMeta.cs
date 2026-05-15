using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public int ExpectedSampleCount { get; set; }

        [DataMember]
        public string VolumeHeader { get; set; } = "Volume [mV]";

        [DataMember]
        public string COHeader { get; set; } = "Carbon_Monoxide [Ohms]";

        [DataMember]
        public string NO2Header { get; set; } = "Nitrogen_Dioxide [Ohms]";

        [DataMember]
        public string PressureHeader { get; set; } = "Pressure [Hectopascal]";

        [DataMember]
        public string DateTimeHeader { get; set; } = "Date time";

        public override string ToString()
        {
            return $"Session [{SessionId}] started at {StartTime}, expecting {ExpectedSampleCount} samples.";
        }
    }
}

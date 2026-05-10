using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString()
        {
            return $"Session [{SessionId}] started at {StartTime}, expecting {ExpectedSampleCount} samples.";
        }
    }
}

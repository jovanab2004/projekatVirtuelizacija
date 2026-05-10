using Common;
using System;

namespace Service
{
    public delegate void TransferStartedHandler(SessionMeta meta);
    public delegate void SampleReceivedHandler(SensorSample sample);
    public delegate void TransferCompletedHandler(string sessionId);
    public delegate void WarningRaisedHandler(string warningMessage);

    public static class SensorEvents
    {
        public static event TransferStartedHandler OnTransferStarted;
        public static event SampleReceivedHandler OnSampleReceived;
        public static event TransferCompletedHandler OnTransferCompleted;
        public static event WarningRaisedHandler OnWarningRaised;

        public static void RaiseTransferStarted(SessionMeta meta)
        {
            OnTransferStarted?.Invoke(meta);
        }

        public static void RaiseSampleReceived(SensorSample sample)
        {
            OnSampleReceived?.Invoke(sample);
        }

        public static void RaiseTransferCompleted(string sessionId)
        {
            OnTransferCompleted?.Invoke(sessionId);
        }

        public static void RaiseWarning(string message)
        {
            OnWarningRaised?.Invoke(message);
        }
    }
}

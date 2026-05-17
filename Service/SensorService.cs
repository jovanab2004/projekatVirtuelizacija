using Common;
using System;
using System.ServiceModel;
using System.Configuration;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SensorService : ISensorService, IDisposable
    {
        private FileManager _fileManager;
        private bool _disposed = false;

        public SensorService()
        {
            SensorEvents.OnTransferStarted += meta => Console.WriteLine($"[EVENT] Sesija pokrenuta: {meta}");
            SensorEvents.OnSampleReceived += sample => Console.WriteLine($"[EVENT] Uzorak primljen: {sample}");
            SensorEvents.OnTransferCompleted += id => Console.WriteLine($"[EVENT] Prenos završen. SessionId={id}");
            SensorEvents.OnWarningRaised += msg => Console.WriteLine($"[UPOZORENJE] {msg}");
        }

        public SampleResponse StartSession(SessionMeta meta)
        {
            if (meta == null || string.IsNullOrWhiteSpace(meta.SessionId))
                throw new FaultException<ValidationFault>(
                    new ValidationFault("SessionMeta nije validan ili SessionId nedostaje."));

            SensorDatabase.CurrentSession = meta;
            SensorDatabase.Measurements.Clear();
            SensorDatabase.Rejects.Clear();
            SensorAnalytics.Reset();

            _fileManager?.Dispose();
            _fileManager = new FileManager(meta.SessionId);

            Console.WriteLine($"[SESIJA] Zaglavlje: {meta.VolumeHeader} | {meta.COHeader} | {meta.NO2Header} | {meta.PressureHeader} | {meta.DateTimeHeader}");

            SensorEvents.RaiseTransferStarted(meta);

            return new SampleResponse
            {
                Ack = AckStatus.ACK,
                Status = SessionStatus.IN_PROGRESS,
                Message = $"Sesija {meta.SessionId} pokrenuta."
            };
        }

        public SampleResponse PushSample(SensorSample sample)
        {
            if (SensorDatabase.CurrentSession == null)
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Sesija nije pokrenuta. Pozovite StartSession prvo."));

            if (sample == null)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("Uzorak je null."));

            if (sample.Pressure <= 0)
            {
                SensorDatabase.Rejects.Add(sample);
                _fileManager?.WriteReject(sample, $"Nevalidan pritisak: {sample.Pressure}");

                return new SampleResponse
                {
                    Ack = AckStatus.NACK,
                    Status = SessionStatus.IN_PROGRESS,
                    Message = $"Nevalidan pritisak: {sample.Pressure}"
                };
            }

            SensorSample previous = SensorDatabase.Measurements.Count > 0
                ? SensorDatabase.Measurements[SensorDatabase.Measurements.Count - 1]
                : null;

            SensorAnalytics.Analyze(previous, sample);

            SensorDatabase.Measurements.Add(sample);
            _fileManager?.WriteMeasurement(sample);

            Console.WriteLine();
            Console.WriteLine($"[PRENOS] prenos u toku... {sample}");
            SensorEvents.RaiseSampleReceived(sample);

            return new SampleResponse
            {
                Ack = AckStatus.ACK,
                Status = SessionStatus.IN_PROGRESS,
                Message = "Uzorak primljen."
            };
        }

        public SampleResponse EndSession()
        {
            if (SensorDatabase.CurrentSession == null)
                return new SampleResponse
                {
                    Ack = AckStatus.NACK,
                    Status = SessionStatus.COMPLETED,
                    Message = "Nije bilo aktivne sesije."
                };

            string sessionId = SensorDatabase.CurrentSession.SessionId;
            SensorDatabase.CurrentSession = null;

            _fileManager?.Dispose();
            _fileManager = null;

            Console.WriteLine("\n[PRENOS] završen prenos.");
            SensorEvents.RaiseTransferCompleted(sessionId);

            return new SampleResponse
            {
                Ack = AckStatus.ACK,
                Status = SessionStatus.COMPLETED,
                Message = $"Sesija {sessionId} završena. Uzoraka: {SensorDatabase.Measurements.Count}"
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _fileManager?.Dispose();
                }
                _disposed = true;
            }
        }

        ~SensorService() { Dispose(false); }
    }
}
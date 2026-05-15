using Common;
using System;
using System.IO;

namespace Service
{
    public class FileManager : IDisposable
    {
        private StreamWriter _measurementsWriter;
        private StreamWriter _rejectsWriter;
        private bool _disposed = false;
        private readonly string _measurementsPath;
        private readonly string _rejectsPath;

        public FileManager(string sessionId)
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;

            // Naziv fajla prema specifikaciji: measurements_session.csv
            _measurementsPath = Path.Combine(folder, "measurements_session.csv");
            _rejectsPath = Path.Combine(folder, "rejects.csv");

            _measurementsWriter = new StreamWriter(
                new FileStream(_measurementsPath, FileMode.Create, FileAccess.Write));
            _measurementsWriter.WriteLine("DateTime,Volume,CO,NO2,Pressure");

            _rejectsWriter = new StreamWriter(
                new FileStream(_rejectsPath, FileMode.Create, FileAccess.Write));
            _rejectsWriter.WriteLine("DateTime,Volume,CO,NO2,Pressure,Reason");
        }

        public void WriteMeasurement(SensorSample sample)
        {
            if (_disposed) return;
            _measurementsWriter.WriteLine(
                $"{sample.DateTime:yyyy-MM-dd HH:mm:ss},{sample.Volume},{sample.CO},{sample.NO2},{sample.Pressure}");
            _measurementsWriter.Flush();
        }

        public void WriteReject(SensorSample sample, string reason = "Nevalidan pritisak")
        {
            if (_disposed) return;
            _rejectsWriter.WriteLine(
                $"{sample.DateTime:yyyy-MM-dd HH:mm:ss},{sample.Volume},{sample.CO},{sample.NO2},{sample.Pressure},{reason}");
            _rejectsWriter.Flush();
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
                    try { _measurementsWriter?.Flush(); } catch { }
                    try { _measurementsWriter?.Close(); } catch { }
                    try { _measurementsWriter?.Dispose(); } catch { }

                    try { _rejectsWriter?.Flush(); } catch { }
                    try { _rejectsWriter?.Close(); } catch { }
                    try { _rejectsWriter?.Dispose(); } catch { }
                }
                _disposed = true;
            }
        }

        ~FileManager()
        {
            Dispose(false);
        }
    }
}
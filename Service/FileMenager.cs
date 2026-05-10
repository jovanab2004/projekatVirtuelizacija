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
            _measurementsPath = Path.Combine(folder, $"measurements_{sessionId}.csv");
            _rejectsPath = Path.Combine(folder, $"rejects_{sessionId}.csv");

            _measurementsWriter = new StreamWriter(new FileStream(_measurementsPath, FileMode.Create, FileAccess.Write));
            _measurementsWriter.WriteLine("DateTime,Volume,CO,NO2,Pressure");

            _rejectsWriter = new StreamWriter(new FileStream(_rejectsPath, FileMode.Create, FileAccess.Write));
            _rejectsWriter.WriteLine("DateTime,Volume,CO,NO2,Pressure");
        }

        public void WriteMeasurement(SensorSample sample)
        {
            _measurementsWriter.WriteLine($"{sample.DateTime},{sample.Volume},{sample.CO},{sample.NO2},{sample.Pressure}");
            _measurementsWriter.Flush();
        }

        public void WriteReject(SensorSample sample)
        {
            _rejectsWriter.WriteLine($"{sample.DateTime},{sample.Volume},{sample.CO},{sample.NO2},{sample.Pressure}");
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
                    _measurementsWriter?.Close();
                    _measurementsWriter?.Dispose();
                    _rejectsWriter?.Close();
                    _rejectsWriter?.Dispose();
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
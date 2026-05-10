using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Client
{
    public class CsvReader : IDisposable
    {
        private StreamReader _reader;
        private StreamWriter _logWriter;
        private bool _disposed = false;

        private readonly string _csvPath;
        private readonly string _logPath;
        private const int MaxRows = 110;

        public CsvReader(string csvPath, string logPath)
        {
            _csvPath = csvPath;
            _logPath = logPath;
        }

        public List<SensorSample> ReadSamples()
        {
            List<SensorSample> samples = new List<SensorSample>();

            _reader = new StreamReader(new FileStream(_csvPath, FileMode.Open, FileAccess.Read));
            _logWriter = new StreamWriter(new FileStream(_logPath, FileMode.Create, FileAccess.Write));

            string header = _reader.ReadLine(); // preskoči zaglavlje
            int rowIndex = 0;
            int validCount = 0;

            while (!_reader.EndOfStream && validCount < MaxRows)
            {
                rowIndex++;
                string line = _reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    _logWriter.WriteLine($"[Red {rowIndex}] Prazan red, preskočen.");
                    continue;
                }

                SensorSample sample = TryParse(line, rowIndex);

                if (sample != null)
                {
                    samples.Add(sample);
                    validCount++;
                }
            }

            Console.WriteLine($"Učitano {validCount} validnih uzoraka. Nevalidni redovi zapisani u log.");
            return samples;
        }

        private SensorSample TryParse(string line, int rowIndex)
        {
            try
            {
                string[] parts = line.Split(',');

                // Očekivani format: DateTime, Volume, CO, NO2, Pressure (+ ostale kolone)
                DateTime dateTime = DateTime.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                double volume = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                double co = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                double no2 = double.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
                double pressure = double.Parse(parts[4].Trim(), CultureInfo.InvariantCulture);

                if (pressure <= 0)
                {
                    _logWriter.WriteLine($"[Red {rowIndex}] Nevalidan pritisak: {pressure}. Red: {line}");
                    return null;
                }

                return new SensorSample
                {
                    DateTime = dateTime,
                    Volume = volume,
                    CO = co,
                    NO2 = no2,
                    Pressure = pressure
                };
            }
            catch (Exception ex)
            {
                _logWriter?.WriteLine($"[Red {rowIndex}] Greška pri parsiranju: {ex.Message}. Red: {line}");
                return null;
            }
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
                    _reader?.Close();
                    _reader?.Dispose();
                    _logWriter?.Close();
                    _logWriter?.Dispose();
                }
                _disposed = true;
            }
        }

        ~CsvReader()
        {
            Dispose(false);
        }
    }
}

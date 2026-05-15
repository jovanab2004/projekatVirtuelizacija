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

            // Čitaj zaglavlje i pronađi indekse kolona po imenu
            string headerLine = _reader.ReadLine();
            if (headerLine == null)
            {
                Console.WriteLine("CSV je prazan!");
                return samples;
            }

            string[] headers = headerLine.Split(',');
            int idxDate = FindIndex(headers, "date time");
            int idxVolume = FindIndex(headers, "volume [mv]");
            int idxPressure = FindIndex(headers, "pressure [hectopascal]");
            int idxCO = FindIndex(headers, "carbon_monoxide [ohms]");
            int idxNO2 = FindIndex(headers, "nitrogen_dioxide [ohms]");

            if (idxDate < 0 || idxVolume < 0 || idxPressure < 0 || idxCO < 0 || idxNO2 < 0)
            {
                Console.WriteLine("CSV nema očekivane kolone! Provjeri format fajla.");
                _logWriter.WriteLine("GREŠKA: nedostaju obavezne kolone u zaglavlju.");
                return samples;
            }

            int rowIndex = 0;
            int validCount = 0;

            while (!_reader.EndOfStream)
            {
                rowIndex++;
                string line = _reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    _logWriter.WriteLine($"[Red {rowIndex}] Prazan red, preskočen.");
                    continue;
                }

                if (validCount >= MaxRows)
                {
                    _logWriter.WriteLine($"[Red {rowIndex}] Višak reda (limit {MaxRows}): {line}");
                    continue;
                }

                SensorSample sample = TryParse(line, rowIndex, idxDate, idxVolume, idxPressure, idxCO, idxNO2);
                if (sample != null)
                {
                    samples.Add(sample);
                    validCount++;
                }
            }

            Console.WriteLine($"Učitano {validCount} validnih uzoraka. Nevalidni redovi zapisani u log.");
            return samples;
        }

        private int FindIndex(string[] headers, string name)
        {
            for (int i = 0; i < headers.Length; i++)
                if (headers[i].Trim().ToLowerInvariant() == name)
                    return i;
            return -1;
        }

        private SensorSample TryParse(string line, int rowIndex,
            int idxDate, int idxVolume, int idxPressure, int idxCO, int idxNO2)
        {
            try
            {
                string[] parts = line.Split(',');

                int maxIdx = Math.Max(Math.Max(idxDate, idxVolume),
                             Math.Max(Math.Max(idxPressure, idxCO), idxNO2));

                if (parts.Length <= maxIdx)
                {
                    _logWriter.WriteLine($"[Red {rowIndex}] Nedovoljan broj kolona ({parts.Length}). Red: {line}");
                    return null;
                }

                DateTime dateTime = DateTime.Parse(
                parts[idxDate].Trim(),
                CultureInfo.InvariantCulture);

                double volume = double.Parse(parts[idxVolume].Trim(), CultureInfo.InvariantCulture);
                double pressure = double.Parse(parts[idxPressure].Trim(), CultureInfo.InvariantCulture);
                double co = double.Parse(parts[idxCO].Trim(), CultureInfo.InvariantCulture);
                double no2 = double.Parse(parts[idxNO2].Trim(), CultureInfo.InvariantCulture);

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
                    try { _reader?.Close(); } catch { }
                    try { _reader?.Dispose(); } catch { }
                    try { _logWriter?.Flush(); } catch { }
                    try { _logWriter?.Close(); } catch { }
                    try { _logWriter?.Dispose(); } catch { }
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
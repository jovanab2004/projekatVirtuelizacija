using Common;
using System;
using System.Configuration;

namespace Service
{
    public static class SensorAnalytics
    {
        private static double _pressureSum = 0;
        private static int _pressureCount = 0;

        private static double _coSum = 0;
        private static int _coCount = 0;

        private static double _no2Sum = 0;
        private static int _no2Count = 0;

        public static void Reset()
        {
            _pressureSum = 0;
            _pressureCount = 0;
            _coSum = 0;
            _coCount = 0;
            _no2Sum = 0;
            _no2Count = 0;
        }

        public static void Analyze(SensorSample previous, SensorSample current)
        {
            double pThreshold = double.Parse(ConfigurationManager.AppSettings["P_threshold"]);
            double coThreshold = double.Parse(ConfigurationManager.AppSettings["CO_threshold"]);
            double no2Threshold = double.Parse(ConfigurationManager.AppSettings["NO2_threshold"]);

            // --- Pressure analitika ---
            _pressureSum += current.Pressure;
            _pressureCount++;
            double pMean = _pressureSum / _pressureCount;

            if (previous != null)
            {
                double deltaP = current.Pressure - previous.Pressure;
                if (Math.Abs(deltaP) > pThreshold)
                {
                    string direction = deltaP > 0 ? "iznad očekivanog" : "ispod očekivanog";
                    SensorEvents.RaiseWarning($"[PressureSpike] ΔP={deltaP:F2}, smjer: {direction}");
                }
            }

            if (current.Pressure < 0.75 * pMean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] Pritisak ispod očekivane vrijednosti (P={current.Pressure:F2}, mean={pMean:F2})");
            else if (current.Pressure > 1.25 * pMean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] Pritisak iznad očekivane vrijednosti (P={current.Pressure:F2}, mean={pMean:F2})");

            // --- CO analitika ---
            _coSum += current.CO;
            _coCount++;
            double coMean = _coSum / _coCount;

            if (previous != null)
            {
                double deltaCO = current.CO - previous.CO;
                if (Math.Abs(deltaCO) > coThreshold)
                {
                    string direction = deltaCO > 0 ? "iznad očekivanog" : "ispod očekivanog";
                    SensorEvents.RaiseWarning($"[COSpike] ΔCO={deltaCO:F2}, smjer: {direction}");
                }
            }

            if (current.CO < 0.75 * coMean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] CO ispod očekivane vrijednosti (CO={current.CO:F2}, mean={coMean:F2})");
            else if (current.CO > 1.25 * coMean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] CO iznad očekivane vrijednosti (CO={current.CO:F2}, mean={coMean:F2})");

            // --- NO2 analitika ---
            _no2Sum += current.NO2;
            _no2Count++;
            double no2Mean = _no2Sum / _no2Count;

            if (previous != null)
            {
                double deltaNO2 = current.NO2 - previous.NO2;
                if (Math.Abs(deltaNO2) > no2Threshold)
                {
                    string direction = deltaNO2 > 0 ? "iznad očekivanog" : "ispod očekivanog";
                    SensorEvents.RaiseWarning($"[NO2Spike] ΔNO2={deltaNO2:F2}, smjer: {direction}");
                }
            }

            if (current.NO2 < 0.75 * no2Mean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] NO2 ispod očekivane vrijednosti (NO2={current.NO2:F2}, mean={no2Mean:F2})");
            else if (current.NO2 > 1.25 * no2Mean)
                SensorEvents.RaiseWarning($"[OutOfBandWarning] NO2 iznad očekivane vrijednosti (NO2={current.NO2:F2}, mean={no2Mean:F2})");
        }
    }
}
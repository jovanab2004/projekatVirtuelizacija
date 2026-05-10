using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.ServiceModel;
using Common;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string csvPath = ConfigurationManager.AppSettings["CsvPath"];
            string logPath = ConfigurationManager.AppSettings["LogPath"];

            List<SensorSample> samples;
            using (CsvReader reader = new CsvReader(csvPath, logPath))
            {
                samples = reader.ReadSamples();
            }

            if (samples.Count == 0)
            {
                Console.WriteLine("Nema validnih uzoraka. Proverite CSV putanju i format.");
                return;
            }

            ChannelFactory<ISensorService> factory = null;
            ISensorService proxy = null;

            try
            {
                factory = new ChannelFactory<ISensorService>("SensorEndpoint");
                proxy = factory.CreateChannel();

                SessionMeta meta = new SessionMeta
                {
                    SessionId = Guid.NewGuid().ToString("N").Substring(0, 8),
                    StartTime = DateTime.Now,
                    ExpectedSampleCount = samples.Count
                };

                Console.WriteLine($"\n[KLIJENT] Pokretanje sesije: {meta.SessionId}");
                SampleResponse startResp = proxy.StartSession(meta);
                Console.WriteLine($"[SERVER] {startResp.Ack} | {startResp.Message}");

                if (startResp.Ack == AckStatus.NACK)
                {
                    Console.WriteLine("Sesija odbijena. Prekid.");
                    return;
                }

                int sent = 0, rejected = 0;

                for (int i = 0; i < samples.Count; i++)
                {
                    try
                    {
                        if (i == 50)
                            throw new Exception("Simulirani prekid veze usred prenosa!");

                        SampleResponse resp = proxy.PushSample(samples[i]);
                        if (resp.Ack == AckStatus.ACK)
                            sent++;
                        else
                        {
                            rejected++;
                            Console.WriteLine($"[KLIJENT] NACK za uzorak {i + 1}: {resp.Message}");
                        }

                        Thread.Sleep(10);
                    }
                    catch (FaultException ex)
                    {
                        Console.WriteLine($"[GREŠKA - Fault] uzorak {i + 1}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GREŠKA - Prekid] uzorak {i + 1}: {ex.Message}");
                        Console.WriteLine("[KLIJENT] Zatvaram resurse nakon prekida...");
                        break; 
                    }
                }

                Console.WriteLine("\n[KLIJENT] Završavanje sesije...");
                SampleResponse endResp = proxy.EndSession();
                Console.WriteLine($"[SERVER] {endResp.Ack} | {endResp.Status} | {endResp.Message}");
                Console.WriteLine($"\n[KLIJENT] Poslato: {sent}, Odbijeno: {rejected}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GREŠKA] {ex.Message}");
            }
            finally
            {
                (proxy as ICommunicationObject)?.Close();
                factory?.Close();
            }

            Console.WriteLine("\nPritisnite Enter za izlaz...");
            Console.ReadLine();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Threading;
using Common;

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
                Console.ReadLine();
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

                Console.ForegroundColor = startResp.Ack == AckStatus.ACK
                    ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"[SERVER] {startResp.Ack} | {startResp.Message}");
                Console.ResetColor();

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

                        SampleResponse resp = proxy.PushSample(samples[i]);

                        if (resp.Ack == AckStatus.ACK)
                        {
                            sent++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[{i + 1}] ACK: {resp.Message}");
                            Console.ResetColor();
                        }
                        else
                        {
                            rejected++;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[{i + 1}] NACK: {resp.Message}");
                            Console.ResetColor();
                        }

                        Thread.Sleep(10);
                    }
                    catch (FaultException<ValidationFault> ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[GREŠKA - Validacija] uzorak {i + 1}: {ex.Detail.Message}");
                        Console.ResetColor();
                    }
                    catch (FaultException<DataFormatFault> ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[GREŠKA - Format] uzorak {i + 1}: {ex.Detail.Message}");
                        Console.ResetColor();
                    }
                    catch (FaultException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[GREŠKA - WCF] uzorak {i + 1}: {ex.Message}");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[GREŠKA - Prekid] uzorak {i + 1}: {ex.Message}");
                        Console.WriteLine("[KLIJENT] Zatvaram resurse nakon prekida...");
                        Console.ResetColor();
                        break;
                    }
                }

                Console.WriteLine("\n[KLIJENT] Završavanje sesije...");
                SampleResponse endResp = proxy.EndSession();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[SERVER] {endResp.Ack} | {endResp.Status} | {endResp.Message}");
                Console.ResetColor();

                Console.WriteLine($"\n[KLIJENT] Poslato: {sent}, Odbijeno: {rejected}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[GREŠKA] {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                try { (proxy as ICommunicationObject)?.Close(); } catch { }
                try { factory?.Close(); } catch { }
            }

            Console.WriteLine("\nPritisnite Enter za izlaz...");
            Console.ReadLine();
        }
    }
}
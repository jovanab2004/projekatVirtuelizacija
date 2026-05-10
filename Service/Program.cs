using System;
using System.ServiceModel;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SensorService serviceInstance = new SensorService())
            using (ServiceHost host = new ServiceHost(serviceInstance))
            {
                host.Open();
                Console.WriteLine("=== Kancelarijski Senzor - WCF Servis ===");
                Console.WriteLine("Servis je pokrenut na: net.tcp://localhost:9000/SensorService");
                Console.WriteLine("Pritisnite Enter za gašenje...");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}

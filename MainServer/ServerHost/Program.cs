using MainServer;
using System;
using System.ServiceModel;

namespace ServerHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (var host = new ServiceHost(typeof(ClientService)))
                {
                    host.Open();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    foreach (var uri in host.BaseAddresses)
                        Console.WriteLine($"Listening on: {uri.AbsoluteUri}");
                    Console.WriteLine("Press <Enter> to stop the service.");
                    Console.ReadLine();

                    // Close the ServiceHost.
                    host.Close();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Error: {e.Message}");
                Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
        }
    }
}

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IOTDeviceControl
{
    public class Program
    {
        private static IConfiguration _config;
        private static string _connectionString;
        private static string _serviceConnectionString;
        private static ServiceClient _serviceClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub - Control device.");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json", optional: false);

            _config = builder.Build();

            _connectionString = _config.GetSection("IOTHubConnectionString").Value;
            _serviceConnectionString = _config.GetSection("IOTHubServiceConnectionString").Value;

            //This sample accepts the device connection string as a parameter, if present
            ValidateConnectionString(_connectionString);

            // Create a ServiceClient to communicate with service-facing endpoint on your hub.
            _serviceClient = ServiceClient.CreateFromConnectionString(_serviceConnectionString);

            await InvokeMethodAsync();

            _serviceClient.Dispose();

            Console.WriteLine("\nPress Enter to exit.");
            Console.ReadLine();
        }

        private static void ValidateConnectionString(string connectionString)
        {
            if (connectionString != null)
            {
                try
                {
                    var cs = Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(connectionString);
                    _connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error: Cannot recognize as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                }
                catch (Exception)
                {
                    Console.WriteLine("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the appsetting.json.");
                    Environment.Exit(1);
                }
            }
        }

        // Invoke the direct method on the device, passing the payload
        private static async Task InvokeMethodAsync()
        {
            var methodInvocation = new CloudToDeviceMethod("setExecutionStatus")
            {
                ResponseTimeout = TimeSpan.FromSeconds(30),
            };
            methodInvocation.SetPayloadJson("true");

            // Invoke the direct method asynchronously and get the response from the simulated device.
            var response = await _serviceClient.InvokeDeviceMethodAsync("device-one", methodInvocation);

            Console.WriteLine($"\nResponse status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        }
    }
}

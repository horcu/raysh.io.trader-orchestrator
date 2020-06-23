using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace raysh.io.trader_orchestrator
{
    public class Worker : BackgroundService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private  FirebaseClient FirebaseClient;

        Process process { get; set; }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
           FirebaseClient = new FirebaseClient("https://rayshio-ousqfg.firebaseio.com/");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
             SetupListener(stoppingToken);
        }

        private  void SetupListener(CancellationToken stoppingToken)
        {
            try
            {
                FirebaseClient
                 .Child("services")
                 .Child("fgsjggfgdsfsd")
                 .AsObservable<string>()
                 .Subscribe(async x => {
                     if(x.Key == "command")
                    await ProcessCommand(x.Object, stoppingToken).ConfigureAwait(false);
                 });
            }
            catch ( Exception ex)
            {
                LogError(ex.Message);
            }
        }

        private IObservable<FirebaseEvent<dynamic>> LogError(string message)
        {
            Console.WriteLine(message);
            return null;
        }

        private async Task ProcessCommand(string command, CancellationToken stoppingToken)
        {
            try
            {
                switch (command)
                {
                    case "start":
                        {
                            _logger.LogInformation("Trader starting at at: {time}", DateTimeOffset.Now);
                             await StartTrader(stoppingToken);

                            break;
                        }
                    case "restart":
                        {
                            _logger.LogInformation("Trader restarting at at: {time}", DateTimeOffset.Now);
                            await RestartTrader(stoppingToken);
                            break;
                        }
                    default:
                        {
                            _logger.LogInformation("Trader stopped at at: {time}", DateTimeOffset.Now);
                           await  StopTrader(stoppingToken);
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }

        protected async Task<bool> StartTrader(CancellationToken token)
        {

            SendHealth("good");
          
            process = new Process();
            
                process.StartInfo.FileName = @"C:\Users\ray\source\repos\raysh.io.security_active_receiver_wkr\bin\Debug\netcoreapp3.0\raysh.io.security_active_receiver_wkr.exe"; // relative path. absolute path works too.
                //process.StartInfo.Arguments = $"{id}";
                //process.StartInfo.FileName = @"cmd.exe";
                //process.StartInfo.Arguments = @"/c dir";      // print the current working directory information
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

               
                process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
                Console.WriteLine("starting");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            // var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
            // Console.WriteLine($"exit {exited}");
            //process.WaitForExit();

            SendHealth("good");
            return await Task.FromResult(true).ConfigureAwait(false);
            
        }

        protected async Task RestartTrader(CancellationToken token)
        {
            SendHealth("sick");
            if (process != null)
            {
                process.Refresh();
            }
            SendHealth("good");
            await StartTrader(token).ConfigureAwait(false);  
        }
        protected async Task StopTrader(CancellationToken token)
        {
            SendHealth("sick");

            _logger.LogInformation("Trader stopped at at: {time}", DateTimeOffset.Now);
            if(process != null)
            process.Close();
        }

        internal void SendHealth(String health)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

            // send directly to firebase
            var status = health;

                FirebaseClient
               .Child("services")
               .Child("fgsjggfgdsfsd")
               .Child("status")
               .PutAsync(status);

        }
    }
}

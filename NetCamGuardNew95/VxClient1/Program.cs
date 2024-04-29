using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Autofac;
using Encryption;
using System.Text;

namespace VxGuardClient
{
    public class Program
    {
        public static void Main(string[] args)
        { 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                }).ConfigureServices((hostContext, services) =>
                {
                    Console.WriteLine("CHECK ConfigureServices");
#if RELEASE
                    bool v = true; //EncryptionRSA.VerifyCurrentMachine(); //true;//
                    v = EncryptionRSA.VerifyCurrentMachine(); 
                    Console.WriteLine("CHECK ERSA = {0}",v); 
#endif
                });

        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //    .UseUrls("https://*:5001")
        //    .UseKestrel(option =>
        //    {
        //        option.ConfigureHttpsDefaults(i =>
        //        {
        //            i.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("./ssl.pfx", "123456");
        //        });
        //    }).UseStartup<Startup>();
        //}

    }
}

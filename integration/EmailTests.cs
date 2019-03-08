using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Json;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Registry;
using Xunit;

namespace integration
{
    public class EmailTests
    {
        public const string GeneratorApiRoot = "http://generator";
        public const string MailHogApiV2Root = "http://mail:8025/api/v2";

        //public const string GeneratorApiRoot = "http://localhost:8086";
        //public const string MailHogApiV2Root = "http://localhost:8025/api/v2";

        [Fact]
        public async Task SendEmailWithNames_IsFromGenerator()
        {
            //var httpClient = new HttpClient();



            //var builder = new HostBuilder()
            //    .ConfigureServices((hostContext, services) =>
            //    {
            //        //IPolicyRegistry<string> registry = services.AddPolicyRegistry();

            //        //IAsyncPolicy<HttpResponseMessage> httWaitAndpRetryPolicy =
            //        //    Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //        //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            //        //registry.Add("SimpleWaitAndRetryPolicy", httWaitAndpRetryPolicy);

            //        //IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync()
            //        //    .AsAsyncPolicy<HttpResponseMessage>();

            //        //registry.Add("NoOpPolicy", noOpPolicy);

            //        services.AddHttpClient("GitHub", x =>
            //        {
            //            x.BaseAddress = new Uri("https://api.github.com/");
            //            x.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //        }).AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(new[]
            //        {
            //            TimeSpan.FromSeconds(1),
            //            TimeSpan.FromSeconds(5),
            //            TimeSpan.FromSeconds(10)
            //        }));
            //    });

            //        //setup our DI
            //        var serviceProvider = new ServiceCollection()
            //            //.AddSingleton<IFooService, FooService>()                
            //            //.AddSingleton<IBarService, BarService>()
            //        .AddHttpClient("GitHub", client =>
            //{
            //    client.BaseAddress = new Uri("https://api.github.com/");
            //    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //})

            //.AddHttpClient("MailServer", x =>
            //{
            //    x.BaseAddress = new Uri("https://api.github.com/");
            //    //x.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            //})

            //.BuildServiceProvider();

            //Console.WriteLine("WAITING 10 SECS");
            //System.Threading.Thread.Sleep(10000);

            // send email
            var client = new HttpClient();
            var sendEmail = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneratorApiRoot}/EmailRandomNames")
            };



            Console.WriteLine($"Sending email: {sendEmail.RequestUri}");

            var response = await Policy
                .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                })
                .ExecuteAsync(() => client.SendAsync(sendEmail));

            //if (response.IsSuccessStatusCode)
            //    Console.WriteLine("Response was successful.");
            //else
            //    Console.WriteLine($"Response failed. Status code {response.StatusCode}");

            response.EnsureSuccessStatusCode();


            //using (var response = await client.SendAsync(sendEmail))
            //{
            //    response.EnsureSuccessStatusCode();
            //}

            // check if email
            var checkEmails = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{MailHogApiV2Root}/messages")
            };

            Console.WriteLine($"Checking emails: {checkEmails.RequestUri}");

            using (var response2 = await client.SendAsync(checkEmails))
            {
                //Console.WriteLine($"Checking emails: {checkEmails.RequestUri}");

                response2.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var messages = JObject.Parse(content);

                messages.Should().HaveElement("total").Which.Should().BeEquivalentTo(1);
                //messages.Should().HaveElement("total");

                var items = messages.Should().HaveElement("items")
                    .Which.Should().BeOfType<JArray>();

                var firstItem = items.Which.First;

                var raw = firstItem.Should().HaveElement("Raw")
                    .Which.Should().HaveElement("From")
                    .Which.Should().HaveValue("generator@generate.com");

                //var from = raw.Should().HaveElement("From");
                //.Which.First.Should().HaveElement("Raw")
                //.Which.Should().HaveElement("Raw");
                //.Which.Should().HaveElement("From");
                //.Which.Should().BeEquivalentTo("generator@generate.com");
            }
        }
    }
}
using System;
using System.Diagnostics;
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

        //public const string GeneratorApiRoot = "http://localhost:49551";
        //public const string MailHogApiV2Root = "http://localhost:8025/api/v2";

        public static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            //services.AddLogging(c => c.AddConsole());
            //services.AddSingleton<IFileSystem, FileSystem>();
            //services.AddSingleton<IMarkdownToHtml, MarkdownToHtml>();

            //services.AddHttpClient<IBasketService, BasketService>()
            //        .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
            //        .AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient("Generic", client =>
            {
                //client.BaseAddress = new Uri("http://localhost:49551");
                //client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            })
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            }, (result, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Could not connect. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                Debug.WriteLine($"Could not connect. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                //logger.Warning($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                //Console.WriteLine($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            }));

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task SendEmailWithNames_IsFromGenerator()
        {
            // send email
            var provider = ConfigureServices();

            var _httpClientFactory = provider.GetService<IHttpClientFactory>();

            var client = _httpClientFactory.CreateClient("Generic");

            //var names = client.GetStringAsync("/Names").Result;



            //var client = new HttpClient();
            var sendEmail = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneratorApiRoot}/EmailRandomNames")
                //RequestUri = new Uri("http://localhost:49551/EmailRandomNames")
            };

            Console.WriteLine($"Sending email: {sendEmail.RequestUri}");
            using (var response = await client.SendAsync(sendEmail))
            {
                response.EnsureSuccessStatusCode();
            }



            //Console.WriteLine($"Sending email: {sendEmail.RequestUri}");

            //var response = await Policy
            //    .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
            //    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
            //    {
            //        Console.WriteLine($"Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
            //    })
            //    .ExecuteAsync(() => client.SendAsync(sendEmail));

            //if (response.IsSuccessStatusCode)
            //    Console.WriteLine("Response was successful.");
            //else
            //    Console.WriteLine($"Response failed. Status code {response.StatusCode}");

            //response.EnsureSuccessStatusCode();


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

            using (var response = await client.SendAsync(checkEmails))
            {
                //Console.WriteLine($"Checking emails: {checkEmails.RequestUri}");

                response.EnsureSuccessStatusCode();
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
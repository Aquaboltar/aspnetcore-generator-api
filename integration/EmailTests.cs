using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            // send email
            var client = new HttpClient();
            var sendEmail = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneratorApiRoot}/EmailRandomNames")
            };
            Console.WriteLine($"Sending email: {sendEmail.RequestUri}");
            using (var response = await client.SendAsync(sendEmail))
            {
                response.EnsureSuccessStatusCode();
            }

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
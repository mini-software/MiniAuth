using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Xunit;

namespace MiniAuth.Tests.AspNetCore
{
    [Collection("BasicTest")]
    public class BasicTest
    {
        [Fact]
        public async Task StaticFileTest()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { })
                .Configure(app =>
                {
                    app.UseMiniAuth();
                    app.Run(_ => Task.CompletedTask);
                });
            using (var server = new TestServer(builder))
            {
                // Test CSS
                using (var response = await server.CreateClient().GetAsync("/miniauth/styles.css").ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Assert.Contains("body {", content);
                    Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
                }
                // Test JS
                using (var response = await server.CreateClient().GetAsync("/miniauth/script.js").ConfigureAwait(false))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Assert.Contains("document", content);
                    Assert.Equal("application/javascript", response.Content.Headers.ContentType?.MediaType);
                }
            }
        }
    }
}

using JWT.Algorithms;
using JWT.Serializers;
using JWT;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using MiniAuth.Managers;

namespace MiniAuth.Tests.AspNetCore
{
    [Collection("BasicTest")]
    public class BasicTest
    {
        public ITestOutputHelper Output { get; }

        public BasicTest(ITestOutputHelper output)
        {
            Output = output;
        }

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

        [Fact]
        public async Task LoginApiTest()
        {
            using (var server = GetTestServer())
            {
                var client = server.CreateClient();
                string token;

                // first access should redirect to login
                using (var response = await client.GetAsync("/"))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    // assert {StatusCode: 302, ReasonPhrase: 'Found', Version: 1.1, Content: System.Net.Http.StreamContent, Headers: { Location: /MiniAuth/login?returnUrl=/ }}
                    Assert.Equal("", content);
                    await OutWrite(response);
                    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                    Assert.Equal("/MiniAuth/login?returnUrl=/", response.Headers.Location?.OriginalString);
                }

                // client.PostAsync miniauth/login with username and password
                var loginData = new Dictionary<string, string>
                {
                    { "username", "miniauth" },
                    { "password", "miniauth" }
                };

                // client.PostAsync by useranme and password json body
                using (var response = await client.PostAsJsonAsync("/MiniAuth/login", loginData))
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    // get key from json "{\"X-MiniAuth-Token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJtaW5pYXV0aCIsIm5hbWUiOiJtaW5pYXV0aCIsImlzcyI6Im1pbmlhdXRoIiwicm9sZXMiOlsiYWRtaW4iXSwianRpIjoiMzk5OTU0ZGUtODI4Ni00ZTgzLWE5OWEtNDkwMmY0Yzk2YTgwIiwiZXhwIjoxNzA4MzE2MDI1LCJpYXQiOjE3MDgyMjk2MjV9.fVAw0rQOD3wmIZ4Jv2NDTNNn9_YR_oWw6mWopSEZjFN-kDaAim6M8-DoCgK1Bg2ITffPbIWeg06qmpusjpmQSJF0MLdrKdPEYlZwNR71xUjEBhMyvgnO0aSFr5BE17tJoujVbfvV-wZ4eSyH1AVG5vOkAGtuJChdY9uP858F1c_e6t1EFyKXY4YhD6lA9Y4OamEROGLf4rIZmRlqNGzKe2OrryOFr0G6TLyv90Xcfrmevb_rZ8NQWLHbCTWXBxkX8ywPK9FhW3PazFGc6aHa1tVlpQzkwez7-9VhRehcb9Y8I1nCDExyF5NlrsGjZkcvujkJjzytWPvohPQ-ptwh4g\"}"
                    Assert.Contains("X-MiniAuth-Token", content);
                    token = response.Headers.GetValues("X-MiniAuth-Token").First();
                    // get token from cookie
                    var cookieToken = response.Headers.GetValues("X-MiniAuth-Token").First();
                    Assert.Equal(cookieToken, token);
                    {
                        var manager = new JWTManager("miniauth", "miniauth", "miniauth.pfx");
                        var json = manager.DecodeToken(token);
                        var obj = JsonConvert.DeserializeObject<dynamic>(json);
                        // assert json property "{\"sub\":\"miniauth\",\"name\":\"miniauth\",\"iss\":\"miniauth\",\"roles\":[\"admin\"],\"jti\":\"385f93f0-f553-4eab-b349-f5a7a913272d\",\"exp\":1708316582,\"iat\":1708230182}"
                        Assert.Equal("miniauth", obj.sub.ToString());
                        Assert.Equal("miniauth", obj.name.ToString());
                        Assert.Equal("miniauth", obj.iss.ToString());
                        Assert.Equal("admin", obj.roles[0].ToString());
                    }
                    await OutWrite(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
            }
        }

        private static TestServer GetTestServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { })
                .Configure(app =>
                {
                    app.UseMiniAuth();
                    app.Run(_ => Task.CompletedTask);
                });
            var server = new TestServer(builder);
            return server;
        }

        private async Task OutWrite(System.Net.Http.HttpResponseMessage response)
        {
            Output.WriteLine("Uri: " + response.RequestMessage?.RequestUri);
            Output.WriteLine("Response: " + await response.Content.ReadAsStringAsync());
            Output.WriteLine("Code: " + response.StatusCode);
        }
    }
}

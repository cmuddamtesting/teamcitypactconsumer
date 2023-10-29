using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactConsumerTests.Models;
using PactNet;
using PactNet.Matchers;
using System.Net;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace PactConsumerTests
{
    public class ConsumerTests
    {
        private readonly IPactBuilderV3 _pact;
        public ConsumerTests(ITestOutputHelper output)
        {
            var config = new PactConfig
            {
                // PactDir = @"./pacts",
                PactDir = Path.Join("..", "..", "..", "pacts"),
                Outputters = new[]
    {
                    new XUnitOutput(output)
                },
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
            var pact = PactNet.Pact.V3(ConsumerPactTestsConstants.ConsumerName, ConsumerPactTestsConstants.ProviderName, config);
            _pact = pact.UsingNativeBackend();
        }
        [Fact]
        public async Task ValidateCreateUser()
        {
            var createUserRequestString = File.ReadAllText(@"./Data/CreateUser.json");
            var createUserRequest = JsonConvert.DeserializeObject<CreateUserRequest>(createUserRequestString);
            _pact
                .UponReceiving("A valid create user request")
                .Given("There is a new user")
                .WithRequest(HttpMethod.Post, "/users")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(createUserRequest)
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(
                    new
                    {
                        message = Match.Type("User created")
                    }
                );

            await _pact.VerifyAsync(async ctx =>
            {
                var result = await CreateUserTestAsync(createUserRequest, ctx.MockServerUri);

                // Assert
                Assert.Equal("User created", result.createProductResponse.Message);

            });
        }

        public async Task<(CreateUserResponse createProductResponse, string error)> CreateUserTestAsync(CreateUserRequest request, Uri uri)
        {
            var sendRequest = new SendRequest(uri);
            var response = await sendRequest.SendRequestAsync<CreateUserRequest, CreateUserResponse>(request, "/users");
            return response;
        } 
    } 
}
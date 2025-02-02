using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Dapper;
using Doppler.BillingUser.Authorization;
using Doppler.BillingUser.Encryption;
using Doppler.BillingUser.Enums;
using Doppler.BillingUser.ExternalServices.Sap;
using Doppler.BillingUser.Model;
using Doppler.BillingUser.Test.Utils;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Dapper;
using Xunit;

namespace Doppler.BillingUser.Test
{
    public class PutInvoiceRecipientsTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoyMDAwMDAwMDAwfQ.E3RHjKx9p0a-64RN2YPtlEMysGM45QBO9eATLBhtP4tUQNZnkraUr56hAWA-FuGmhiuMptnKNk_dU3VnbyL6SbHrMWUbquxWjyoqsd7stFs1K_nW6XIzsTjh8Bg6hB5hmsSV-M5_hPS24JwJaCdMQeWrh6cIEp2Sjft7I1V4HQrgzrkMh15sDFAw3i1_ZZasQsDYKyYbO9Jp7lx42ognPrz_KuvPzLjEXvBBNTFsVXUE-ur5adLNMvt-uXzcJ1rcwhjHWItUf5YvgRQbbBnd9f-LsJIhfkDgCJcvZmGDZrtlCKaU1UjHv5c3faZED-cjL59MbibofhPjv87MK8hhdg";

        public PutInvoiceRecipientsTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PUT_Invoice_email_recipients_should_save_emails_recipients_correctly()
        {
            // Arrange
            var user = new User
            {
                BillingEmails = "test@mail.com, test2@mail.com"
            };

            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<User>(null, null, null, null, null))
                .ReturnsAsync(user);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());
            var billingEmailRecipients = new
            {
                Recipients = new[]
                {
                    "test@mail.com",
                    "test2@mail.com"
                }
            };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518}");

            // Act
            var response = await client.PutAsJsonAsync("accounts/test1@test.com/billing-information/invoice-recipients", billingEmailRecipients);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PUT_Invoice_email_recipients_should_send_to_sap_with_email_recipients_when_user_has_billings_system_is_QBL()
        {
            // Arrange
            var user = new User
            {
                Email = "test@mail.com",
                BillingEmails = "test@mail.com, test2@mail.com",
                SapProperties = "{\"ContractCurrency\" : false,\"GovernmentAccount\" : false,\"Premium\" : false,\"Plus\" : false,\"ComercialPartner\" : false,\"MarketingPartner\" : false,\"OnBoarding\" : false,\"Layout\" : false,\"Datahub\" : false,\"PushNotification\" : false,\"ExclusiveIp\" : false,\"Advisory\" : false,\"Reports\" : false,\"SMS\" : false}",
                IdResponsabileBilling = (int)ResponsabileBillingEnum.QBL
            };
            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<User>(null, null, null, null, null))
                .ReturnsAsync(user);

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });

            });
            factory.Server.PreserveExecutionContext = true;
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions());
            var billingEmailRecipients = new
            {
                Recipients = new[]
                {
                    "test@mail.com",
                    "test2@mail.com"
                }
            };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518}");
            var httpTest = new HttpTest();

            // Act
            var response = await client.PutAsJsonAsync("accounts/test1@test.com/billing-information/invoice-recipients", billingEmailRecipients);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            httpTest.ShouldHaveMadeACall();
        }

        [Fact]
        public async Task PUT_Invoice_email_recipients_should_send_to_sap_with_email_recipients_when_user_has_billings_system_is_GBBISIDE()
        {
            // Arrange
            var user = new User
            {
                Email = "test@mail.com",
                BillingEmails = "test@mail.com, test2@mail.com",
                SapProperties =
                    "{\"ContractCurrency\" : false,\"GovernmentAccount\" : false,\"Premium\" : false,\"Plus\" : false,\"ComercialPartner\" : false,\"MarketingPartner\" : false,\"OnBoarding\" : false,\"Layout\" : false,\"Datahub\" : false,\"PushNotification\" : false,\"ExclusiveIp\" : false,\"Advisory\" : false,\"Reports\" : false,\"SMS\" : false}",
                IdResponsabileBilling = (int)ResponsabileBillingEnum.GBBISIDE
            };

            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<User>(null, null, null, null, null))
                .ReturnsAsync(user);
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                    services.AddSingleton(GetSapSettingsMock().Object);
                    services.AddSingleton(Mock.Of<ICurrentRequestApiTokenGetter>());
                });
            });
            factory.Server.PreserveExecutionContext = true;
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions());

            var billingEmailRecipients = new
            {
                Recipients = new[]
                {
                    "test@mail.com",
                    "test2@mail.com"
                }
            };
            client.DefaultRequestHeaders.Add("Authorization",
                $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518}");

            // Act
            using var httpTest = new HttpTest();
            var response = await client.PutAsJsonAsync("accounts/test1@test.com/billing-information/invoice-recipients", billingEmailRecipients);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            httpTest.ShouldHaveMadeACall();
        }

        [Fact]
        public async Task PUT_Invoice_email_recipients_should_no_send_to_sap_with_email_recipients_when_user_has_billings_system_is_GB()
        {
            // Arrange
            var user = new User
            {
                BillingEmails = "test@mail.com, test2@mail.com",
                SapProperties = "{\"ContractCurrency\" : false,\"GovernmentAccount\" : false,\"Premium\" : false,\"Plus\" : false,\"ComercialPartner\" : false,\"MarketingPartner\" : false,\"OnBoarding\" : false,\"Layout\" : false,\"Datahub\" : false,\"PushNotification\" : false,\"ExclusiveIp\" : false,\"Advisory\" : false,\"Reports\" : false,\"SMS\" : false}",
                IdResponsabileBilling = (int)ResponsabileBillingEnum.GB
            };

            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<User>(null, null, null, null, null))
                .ReturnsAsync(user);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            var billingEmailRecipients = new
            {
                Recipients = new[] { "test@mail.com", "test2@mail.com" }
            };

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518}");
            using var httpTest = new HttpTest();

            // Act
            var response = await client.PutAsJsonAsync("accounts/test1@test.com/billing-information/invoice-recipients", billingEmailRecipients);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            httpTest.ShouldNotHaveCalled("https://localhost:5000/businesspartner/createorupdatebusinesspartner");
        }

        private static Mock<IOptions<SapSettings>> GetSapSettingsMock()
        {
            var accountPlansSettingsMock = new Mock<IOptions<SapSettings>>();
            accountPlansSettingsMock.Setup(x => x.Value)
                .Returns(new SapSettings
                {
                    SapBaseUrl = "https://localhost:5000/",
                    SapCreateBusinessPartnerEndpoint = "businesspartner/createorupdatebusinesspartner"
                });

            return accountPlansSettingsMock;
        }
    }
}

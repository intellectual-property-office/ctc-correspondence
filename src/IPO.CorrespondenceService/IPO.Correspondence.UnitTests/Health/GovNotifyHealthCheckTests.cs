using IPO.Common.Infrastructure;
using IPO.Correspondence.Health;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Notify.Exceptions;

namespace IPO.Correspondence.UnitTests.APIWebJob
{
    [TestClass]
    public class GovNotifyHealthCheckTests
    {
        private GovNotifyHealthCheck? _uut;

        private Mock<IServiceProvider>? _serviceProvider;
        private Mock<INotifyGateway>? _notifyGateway;

        [TestInitialize]
        public void TestInitialize()
        {
            _serviceProvider = new Mock<IServiceProvider>();

            _notifyGateway = new Mock<INotifyGateway>();
        }

        [TestMethod]
        public async Task ReturnsUnhealthyForNoNotifyGatewayTest()
        {
            // Arrange

            _uut = new GovNotifyHealthCheck(_serviceProvider!.Object);

            // Act

            var actual = await _uut.CheckHealthAsync(null!, CancellationToken.None);

            // Assert

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(HealthCheckResult));
            Assert.AreEqual("Gateway is NULL", actual.Description);
            Assert.IsNull(actual.Exception);
            Assert.AreEqual(HealthStatus.Unhealthy, actual.Status);
        }

        [TestMethod]
        public async Task ReturnsHealthyForTemplatePreviewedTest()
        {
            // Arrange

            var previewResponse = new TemplatePreviewResponse("Letter", "Id001", 4, "Test Subject", "Test Body");

            _serviceProvider!.Setup(e => e.GetService(typeof(INotifyGateway))).Returns(_notifyGateway!.Object);

            _notifyGateway.Setup(e => e.GenerateTemplatePreviewAsync(It.IsAny<TemplatePreviewRequest>())).ReturnsAsync(previewResponse);

            _uut = new GovNotifyHealthCheck(_serviceProvider.Object);

            // Act

            var actual = await _uut.CheckHealthAsync(null!, CancellationToken.None);

            // Assert

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(HealthCheckResult));
            Assert.AreEqual(string.Empty, actual.Description);
            Assert.IsNull(actual.Exception);
            Assert.AreEqual(HealthStatus.Healthy, actual.Status);
        }

        [TestMethod]
        public async Task ReturnsHealthyForNotFoundTemplateTest()
        {
            // Arrange

            var notifyException = new NotifyClientException("\"error\": \"NoResultFound\" \"message\": \"No result found\"");
            var statusException = new StatusCodeException(Error.Create<string>("E08"), "", notifyException, 1);
            var exception = new Exception("", statusException);

            _serviceProvider!.Setup(e => e.GetService(typeof(INotifyGateway))).Returns(_notifyGateway!.Object);

            _notifyGateway.Setup(e => e.GenerateTemplatePreviewAsync(It.IsAny<TemplatePreviewRequest>())).ThrowsAsync(exception);

            _uut = new GovNotifyHealthCheck(_serviceProvider.Object);

            // Act

            var actual = await _uut.CheckHealthAsync(null!, CancellationToken.None);

            // Assert

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(HealthCheckResult));
            Assert.AreEqual(string.Empty, actual.Description);
            Assert.IsNull(actual.Exception);
            Assert.AreEqual(HealthStatus.Healthy, actual.Status);
        }

        [TestMethod]
        public async Task ReturnsUnHealthyForExceptionTest()
        {
            // Arrange

            var exception = new Exception("Error calling notify", null);

            _serviceProvider!.Setup(e => e.GetService(typeof(INotifyGateway))).Returns(_notifyGateway!.Object);

            _notifyGateway.Setup(e => e.GenerateTemplatePreviewAsync(It.IsAny<TemplatePreviewRequest>())).ThrowsAsync(exception);

            _uut = new GovNotifyHealthCheck(_serviceProvider.Object);

            // Act

            var actual = await _uut.CheckHealthAsync(null!, CancellationToken.None);

            // Assert

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(HealthCheckResult));
            Assert.AreEqual("Error calling notify", actual.Description);
            Assert.AreEqual(exception, actual.Exception);
            Assert.AreEqual(HealthStatus.Unhealthy, actual.Status);
        }
    }
}

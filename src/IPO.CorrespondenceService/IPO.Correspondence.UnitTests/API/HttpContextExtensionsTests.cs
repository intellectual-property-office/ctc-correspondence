using AwesomeAssertions;
using IPO.Correspondence.API;
using IPO.Correspondence.API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.FeeService.UnitTests.API
{
    [TestClass]
    public class HttpContextExtensionsTests
    {
        [TestMethod]
        public void SetOrganisationIdSetsRightOrganisationId()
        {
            // Arrange 
            var httpContext = new DefaultHttpContext();
            var organisationId = Guid.NewGuid();

            // Act 
            httpContext.SetOrganisationId(organisationId);

            // Assert  
            httpContext.Items.Should().NotBeNullOrEmpty();
            httpContext.Items.Should().ContainKey(Constants.OrganisationIdItemsKey);
            organisationId.Should().Be((Guid)httpContext.Items[Constants.OrganisationIdItemsKey]!);
        }

        [TestMethod]
        public void GetOrganisationIdWhenIdExistsReturnsRightId()
        {
            // Arrange 
            var httpContext = new DefaultHttpContext();
            var organisationId = Guid.NewGuid();
            httpContext.Items.Add(Constants.OrganisationIdItemsKey, organisationId);

            // Act 
            var result = httpContext.GetOrganisationId();

            // Assert  
            result.Should().NotBeNull();
            result.Should().Be(organisationId);
        }

        [TestMethod]
        public void GetOrganisationIdWhenIdNotExistsReturnsNull()
        {
            // Arrange 
            var httpContext = new DefaultHttpContext();
            var organisationId = Guid.NewGuid();

            // Act 
            var result = httpContext.GetOrganisationId();

            // Assert  
            result.Should().BeNull();
        }
    }
}
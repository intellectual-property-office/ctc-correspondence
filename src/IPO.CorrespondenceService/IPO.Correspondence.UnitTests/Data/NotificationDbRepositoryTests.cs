using AutoFixture;
using AwesomeAssertions;
using IPO.Correspondence.Data.Notifications;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPO.Correspondence.UnitTests.Data
{
    [TestClass]
    [TestCategory("NotificationDbRepositoryTests")]
    public class NotificationDbRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly NotificationDbContext _notificationDbContext;
        private readonly Mock<ILogger> _logger;


        public NotificationDbRepositoryTests()
        {
            _fixture = new Fixture();
            _logger = new Mock<ILogger>();
            _notificationDbContext = new NotificationDbContext(
                                    new DbContextOptionsBuilder<NotificationDbContext>()
                                        .UseInMemoryDatabase(databaseName: _fixture.Create<string>()
                                                            , options => options.EnableNullChecks(false))
                                        .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);

        }

        [TestMethod]
        public async Task GetPendingNotificationsAsyncWhenExternalReadFalseReturnsExpectedNotifications()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var messages = await _notificationDbContext.AddTestNotificationMessageSetForOwner(_fixture, owner, 5);
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            messages.ForEach(o =>
            {
                o.ExternalRead = false;
                o.OrganisationId = owner.Id;
            });
            _notificationDbContext.SaveChanges();
            var expectedNotificationViewModels = messages.Select(o => new NotificationViewModel(o, owner.Id));

            // Act  
            var result = await notificationDbRepositoryService.GetPendingNotificationsAsync(owner.Id, false);

            // Assert 
            result.Should().BeEquivalentTo(expectedNotificationViewModels);
        }


        [TestMethod]
        public async Task GetPendingNotificationsAsyncWhenInternalReadIsTrueReturnsExpectedNotifications()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var messages = await _notificationDbContext.AddTestNotificationMessageSetForOwner(_fixture, owner, 5, nm => nm.InternalRead = false);
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            messages.ForEach(o =>
            {
                o.InternalRead = false;
                o.OrganisationId = owner.Id;
            });
            _notificationDbContext.SaveChanges();

            var expectedNotificationViewModels = messages.Select(o => new NotificationViewModel(o, owner.Id));

            // Act  
            var result = await notificationDbRepositoryService.GetPendingNotificationsAsync(owner.Id, true);

            // Assert 
            result.Should().BeEquivalentTo(expectedNotificationViewModels);
        }

        [TestMethod]
        public async Task GetAllNotificationsFromTimeStampAsyncWhenInternalReadTrueReturnsExpectedNotifications()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var from = DateTime.Now.AddMinutes(-10);
            var to = DateTime.Now;
            var messages = await _notificationDbContext
                .AddTestNotificationMessageSetForOwnerWithTimeStamp(_fixture, owner, 5, DateTime.Now.AddMinutes(10));
            var messagesWithinTimeRange = await _notificationDbContext
                .AddTestNotificationMessageSetForOwnerWithTimeStamp(_fixture, owner, 5, DateTime.Now.AddMinutes(-1));

            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            messages.ForEach(o => o.InternalRead = true);
            _notificationDbContext.SaveChanges();
            var expectedNotificationViewModels = messagesWithinTimeRange.Select(o => new NotificationViewModel(o, owner.Id));

            // Act  
            var result = await notificationDbRepositoryService.GetAllNotificationsFromTimeStampAsync(owner.Id, from, to, true);

            // Assert 
            result.Should().BeEquivalentTo(expectedNotificationViewModels);
        }

        [TestMethod]
        public async Task GetAllNotificationsFromTimeStampAsyncWhenInternalReadFalseReturnsExpectedNotifications()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var from = DateTime.Now.AddMinutes(-10);
            var to = DateTime.Now;
            var messages = await _notificationDbContext
                .AddTestNotificationMessageSetForOwnerWithTimeStamp(_fixture, owner, 5, DateTime.Now.AddMinutes(10));

            var messagesWithinTimeRange = await _notificationDbContext
                .AddTestNotificationMessageSetForOwnerWithTimeStamp(_fixture, owner, 5, DateTime.Now.AddMinutes(-1));

            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            messages.ForEach(o => o.InternalRead = false);
            var expectedNotificationViewModels = messagesWithinTimeRange.Select(o => new NotificationViewModel(o, owner.Id));

            // Act  
            var result = await notificationDbRepositoryService.GetAllNotificationsFromTimeStampAsync(owner.Id, from, to, false);

            // Assert 
            result.Should().BeEquivalentTo(expectedNotificationViewModels);
        }

        [TestMethod]
        public async Task PeekNotificationAsyncReturnsExpectedNotifications()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var notificationId = Guid.NewGuid();
            var message = _fixture.Build<NotificationMessage>().Create();
            message.Id = notificationId;
            message.Owner = owner;
            await _notificationDbContext.Messages.AddAsync(message);
            await _notificationDbContext.SaveChangesAsync();
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            var expectedNotificationViewModel = new NotificationViewModel(message);

            // Act  
            var result = await notificationDbRepositoryService.PeekNotificationAsync(notificationId);

            // Assert 
            result.Should().BeEquivalentTo(expectedNotificationViewModel);
        }

        [TestMethod]
        public async Task StoreAsyncWhenExternalIdProvidedStoresNotificationMessageSuccessfully()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var notificationId = Guid.NewGuid();
            var message = _fixture.Build<NotificationMessage>().Create();
            message.Id = notificationId;
            message.Owner = owner;
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            var notifyResultId = Guid.NewGuid().ToString();
            message.ExternalId = notifyResultId;

            // Act  
            await notificationDbRepositoryService.StoreAsync(message, notifyResultId);
            var result = await _notificationDbContext.Messages.FirstOrDefaultAsync(o => o.Id == notificationId);

            // Assert 
            result.Should().BeEquivalentTo(message);
        }

        [TestMethod]
        public async Task StoreAsyncWhenExternalIdNullStoresNotificationMessageSuccessfully()
        {
            // Arrange    
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var notificationId = Guid.NewGuid();
            var message = _fixture.Build<NotificationMessage>().Create();
            message.Id = notificationId;
            message.Owner = owner;
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);
            message.ExternalId = null;

            // Act  
            await notificationDbRepositoryService.StoreAsync(message, null!);
            var result = await _notificationDbContext.Messages.FirstOrDefaultAsync(o => o.Id == notificationId);

            // Assert 
            result.Should().BeEquivalentTo(message);
        }

        [TestMethod]
        public async Task GetMessagesForStatusUpdateAsyncReturnsExpectedMessages()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var messages = _fixture.CreateMany<NotificationMessage>(5).ToList();
            messages.ForEach(o =>
            {
                o.Owner = owner;
                o.Status = NotificationStatus.SendingToProvider;
                o.Type = NotificationType.Letter;
            });
            await _notificationDbContext.Messages.AddRangeAsync(messages);
            await _notificationDbContext.SaveChangesAsync();
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);

            // Act  
            var result = await notificationDbRepositoryService.GetMessagesForStatusUpdateAsync();

            // Assert 
            result.Should().HaveCount(5);
            result.Should().BeEquivalentTo(messages);
        }

        [TestMethod]
        public async Task NotifyOwnersForPendingNotificationsAsyncNotifiesOwnersSuccessfully()
        {
            // Arrange   
            var owner = await _notificationDbContext.AddTestOwner(_fixture);
            var messages = _fixture.CreateMany<NotificationMessage>(5).ToList();
            messages.ForEach(o =>
            {
                o.Owner = owner;
                o.Type = NotificationType.API;
                o.ExternalRead = false;
            });
            await _notificationDbContext.Messages.AddRangeAsync(messages);
            await _notificationDbContext.SaveChangesAsync();
            var notificationDbRepositoryService = new NotificationDbRepository(_notificationDbContext, _logger.Object);

            var ownersCalled = new List<NotificationOwner>();
            var notifyChangesAsync = async (IEnumerable<NotificationOwner> owners) =>
            {
                ownersCalled.AddRange(owners);
                await Task.CompletedTask;
            };

            // Act  
            await notificationDbRepositoryService.NotifyOwnersForPendingNotificationsAsync(notifyChangesAsync);

            // Assert 
            ownersCalled.Should().ContainSingle();
            ownersCalled.First().Id.Should().Be(owner.Id);
        }

        [TestMethod]
        public async Task UpdateMessageOwnerAsyncWhenOwnerExistsUpdatesMessageOwnerSuccessfully()
        {
            // Arrange   
            var message = _fixture.Create<NotificationMessage>();
            var oldOwner = new NotificationOwner()
            {
                Id = Guid.NewGuid(),
                TemplateId = $"{Guid.NewGuid()}_{Guid.NewGuid()}",
                Email = $"{Guid.NewGuid()}_{Guid.NewGuid()}"
            };
            var newOwner = _fixture.Build<NotificationOwner>().Create();
            newOwner.Id = oldOwner.Id;
            await _notificationDbContext.Owners.AddAsync(oldOwner);
            await _notificationDbContext.SaveChangesAsync();
            message.Owner = newOwner;
            var notificationDbRepositoryService = new NotificationDbRepositoryTestsHelper(_notificationDbContext, _logger.Object);

            // Act  
            await notificationDbRepositoryService.TestUpdateMessageOwnerAsync(message);

            // Assert 
            message.Owner.Should().NotBeNull();
            message.Owner.Id.Should().Be(newOwner.Id);
            message.Owner.TemplateId.Should().Be(newOwner.TemplateId);
            message.Owner.Email.Should().Be(newOwner.Email);
        }

        [TestMethod]
        public async Task UpdateMessageOwnerAsyncWhenOwnerNotExistsReturnsNull()
        {
            // Arrange   
            var message = _fixture.Create<NotificationMessage>();
            var oldOwner = new NotificationOwner()
            {
                Id = Guid.NewGuid(),
                TemplateId = $"{Guid.NewGuid()}_{Guid.NewGuid()}",
                Email = $"{Guid.NewGuid()}_{Guid.NewGuid()}"
            };
            var newOwner = _fixture.Build<NotificationOwner>().Create();
            newOwner.Id = oldOwner.Id;
            message.Owner = null;
            var notificationDbRepositoryService = new NotificationDbRepositoryTestsHelper(_notificationDbContext, _logger.Object);

            // Act  
            await notificationDbRepositoryService.TestUpdateMessageOwnerAsync(message);

            // Assert 
            message.Owner.Should().BeNull();
        }


        [TestMethod]
        public async Task UpdateMessageOwnerAsyncWhenDbOwnerIsNullReturnsOriginalMessageIntact()
        {
            // Arrange   
            var message = _fixture.Create<NotificationMessage>();
            var oldOwner = new NotificationOwner()
            {
                Id = Guid.NewGuid(),
                TemplateId = $"{Guid.NewGuid()}_{Guid.NewGuid()}",
                Email = $"{Guid.NewGuid()}_{Guid.NewGuid()}"
            };
            var newOwner = _fixture.Build<NotificationOwner>().Create();
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(databaseName: "Notifications")
                .Options;

            newOwner.Id = Guid.Empty;
            message.Owner = newOwner;

            var ctx = new NotificationDbContext(options);
            ctx.Owners.Add(oldOwner);
            ctx.SaveChanges();

            var notificationDbRepositoryService = new NotificationDbRepositoryTestsHelper(ctx, _logger.Object);

            // Act  
            await notificationDbRepositoryService.TestUpdateMessageOwnerAsync(message);

            // Assert 
            message.Owner.Should().BeEquivalentTo(newOwner);
        }
    }
}
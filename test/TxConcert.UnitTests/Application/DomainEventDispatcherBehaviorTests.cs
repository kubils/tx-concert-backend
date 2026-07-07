using TxConcert.Application.Common.Behaviors;
using TxConcert.Application.Common.Interfaces;
using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Cqrs;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NodaTime;

namespace TxConcert.UnitTests.Application;

public class DomainEventDispatcherBehaviorTests
{
    private sealed record TestEvent(Instant OccurredAt) : IDomainEvent;

    private sealed class TestEntity : BaseEntity
    {
        public static TestEntity Create()
        {
            TestEntity entity = new();
            entity.SetId("test");
            return entity;
        }

        public void RaiseEvent(IDomainEvent @event) => AddDomainEvent(@event);
    }

    private sealed record TestRequest : IRequest<string>;

    [Fact]
    public async Task Handle_WithDomainEvents_PublishesAndClears()
    {
        // Arrange
        TestEntity entity = TestEntity.Create();
        TestEvent domainEvent = new(SystemClock.Instance.GetCurrentInstant());
        entity.RaiseEvent(domainEvent);

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using TestDbContext dbContext = new(options);
        dbContext.TestEntities.Add(entity);
        await dbContext.SaveChangesAsync();

        Mock<IPublisher> publisher = new();
        publisher.Setup(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DomainEventDispatcherBehavior<TestRequest, string> behavior = new(
            dbContext,
            publisher.Object,
            NullLoggerFactory.Instance.CreateLogger<DomainEventDispatcherBehavior<TestRequest, string>>());

        // Act
        string result = await behavior.Handle(
            new TestRequest(),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        publisher.Verify(p => p.Publish(It.Is<IDomainEvent>(e => e.Equals(domainEvent)), It.IsAny<CancellationToken>()), Times.Once);
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithoutDomainEvents_DoesNotPublish()
    {
        // Arrange
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using TestDbContext dbContext = new(options);

        Mock<IPublisher> publisher = new();

        DomainEventDispatcherBehavior<TestRequest, string> behavior = new(
            dbContext,
            publisher.Object,
            NullLoggerFactory.Instance.CreateLogger<DomainEventDispatcherBehavior<TestRequest, string>>());

        // Act
        string result = await behavior.Handle(
            new TestRequest(),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        publisher.Verify(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsNextDelegateFirst()
    {
        // Arrange
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using TestDbContext dbContext = new(options);

        bool nextCalled = false;

        DomainEventDispatcherBehavior<TestRequest, string> behavior = new(
            dbContext,
            Mock.Of<IPublisher>(),
            NullLoggerFactory.Instance.CreateLogger<DomainEventDispatcherBehavior<TestRequest, string>>());

        // Act
        await behavior.Handle(
            new TestRequest(),
            () =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    /// <summary>In-memory DbContext for testing domain event dispatch via ChangeTracker.</summary>
    private sealed class TestDbContext : DbContext, IDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.Ignore(x => x.DomainEvents);
            });
        }
    }
}

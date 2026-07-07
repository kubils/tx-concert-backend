using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Cqrs;
using FluentAssertions;
using NodaTime;

namespace TxConcert.UnitTests.Domain;

public class DomainEventTests
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

    [Fact]
    public void AddDomainEvent_AddsToCollection()
    {
        TestEntity entity = TestEntity.Create();
        TestEvent @event = new(SystemClock.Instance.GetCurrentInstant());

        entity.RaiseEvent(@event);

        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(@event);
    }

    [Fact]
    public void AddDomainEvent_Multiple_PreservesOrder()
    {
        TestEntity entity = TestEntity.Create();
        Instant now = SystemClock.Instance.GetCurrentInstant();
        TestEvent first = new(now);
        TestEvent second = new(now.Plus(Duration.FromSeconds(1)));

        entity.RaiseEvent(first);
        entity.RaiseEvent(second);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents[0].Should().Be(first);
        entity.DomainEvents[1].Should().Be(second);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAll()
    {
        TestEntity entity = TestEntity.Create();
        entity.RaiseEvent(new TestEvent(SystemClock.Instance.GetCurrentInstant()));
        entity.RaiseEvent(new TestEvent(SystemClock.Instance.GetCurrentInstant()));

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_IsReadOnly()
    {
        TestEntity entity = TestEntity.Create();

        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
    }

    [Fact]
    public void NewEntity_HasEmptyDomainEvents()
    {
        TestEntity entity = TestEntity.Create();

        entity.DomainEvents.Should().BeEmpty();
    }
}

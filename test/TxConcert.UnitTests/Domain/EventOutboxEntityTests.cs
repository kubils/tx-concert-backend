using TxConcert.Domain.Events;
using FluentAssertions;

namespace TxConcert.UnitTests.Domain;

public class EventOutboxEntityTests
{
    [Fact]
    public void Create_GeneratesIdWithCorrectPrefix()
    {
        EventOutboxEntity entity = EventOutboxEntity.Create("TestEvent", "{\"bar\":2}");

        entity.Id.Should().StartWith("evout_");
        entity.EventName.Should().Be("TestEvent");
        entity.Content.Should().Be("{\"bar\":2}");
        entity.ProcessedBy.Should().BeEmpty();
    }

    [Fact]
    public void MarkProcessedBy_AddsSubscriberId()
    {
        EventOutboxEntity entity = EventOutboxEntity.Create("TestEvent", "{}");

        entity.MarkProcessedBy("subscriber-1");
        entity.MarkProcessedBy("subscriber-2");

        entity.ProcessedBy.Should().Contain("subscriber-1");
        entity.ProcessedBy.Should().Contain("subscriber-2");
    }

    [Fact]
    public void MarkProcessedBy_DoesNotDuplicate()
    {
        EventOutboxEntity entity = EventOutboxEntity.Create("TestEvent", "{}");

        entity.MarkProcessedBy("subscriber-1");
        entity.MarkProcessedBy("subscriber-1");

        entity.ProcessedBy.Should().HaveCount(1);
    }
}

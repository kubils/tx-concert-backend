using TxConcert.Domain.Commands;
using FluentAssertions;

namespace TxConcert.UnitTests.Domain;

public class CommandOutboxEntityTests
{
    [Fact]
    public void Create_GeneratesIdWithCorrectPrefix()
    {
        CommandOutboxEntity entity = CommandOutboxEntity.Create("TestCommand", "{\"foo\":1}");

        entity.Id.Should().StartWith("cdout_");
        entity.Command.Should().Be("TestCommand");
        entity.Content.Should().Be("{\"foo\":1}");
        entity.IsRetry.Should().BeFalse();
        entity.PublishedOn.Should().BeNull();
        entity.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public void SetPublished_SetsPublishedOn()
    {
        CommandOutboxEntity entity = CommandOutboxEntity.Create("TestCommand", "{}");

        entity.SetPublished();

        entity.PublishedOn.Should().NotBeNull();
    }

    [Fact]
    public void SetSuccess_SetsIsSuccessfulAndProcessedOn()
    {
        CommandOutboxEntity entity = CommandOutboxEntity.Create("TestCommand", "{}");

        entity.SetSuccess();

        entity.IsSuccessful.Should().BeTrue();
        entity.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public void SetFailed_SetsErrorMessageAndIsSuccessfulFalse()
    {
        CommandOutboxEntity entity = CommandOutboxEntity.Create("TestCommand", "{}");

        entity.SetFailed("Something went wrong");

        entity.IsSuccessful.Should().BeFalse();
        entity.ErrorMessage.Should().Be("Something went wrong");
        entity.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public void Retry_CreatesNewEntityWithIsRetryTrue()
    {
        CommandOutboxEntity original = CommandOutboxEntity.Create("TestCommand", "{\"data\":1}");

        CommandOutboxEntity retried = original.Retry();

        retried.Id.Should().NotBe(original.Id);
        retried.Command.Should().Be("TestCommand");
        retried.Content.Should().Be("{\"data\":1}");
        retried.IsRetry.Should().BeTrue();
        retried.JobId.Should().Be(original.JobId);
    }
}

using TxConcert.Domain.Common.Base;
using TxConcert.Domain.Common.Errors;
using FluentAssertions;
using NodaTime;

namespace TxConcert.UnitTests.Domain;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
        public static TestEntity Create(string prefix = "test")
        {
            TestEntity entity = new();
            entity.SetId(prefix);
            return entity;
        }
    }

    [Fact]
    public void SetId_WithValidPrefix_GeneratesCorrectId()
    {
        TestEntity entity = TestEntity.Create("usr");

        entity.Id.Should().StartWith("usr_");
        entity.Id.Length.Should().BeGreaterThan(4);
    }

    [Fact]
    public void SetId_WithMaxLengthPrefix_Works()
    {
        TestEntity entity = TestEntity.Create("abcde");

        entity.Id.Should().StartWith("abcde_");
    }

    [Fact]
    public void SetId_WithTooLongPrefix_ThrowsEntityPrefixException()
    {
        Action act = () => TestEntity.Create("toolong");

        act.Should().Throw<EntityPrefixException>();
    }

    [Fact]
    public void SoftDelete_SetsDeletedAt()
    {
        TestEntity entity = TestEntity.Create();

        entity.SoftDelete();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetAuditFields_FirstCall_SetsCreatedById()
    {
        TestEntity entity = TestEntity.Create();

        entity.SetAuditFields("user-123", "John");

        entity.CreatedById.Should().Be("user-123");
        entity.CreatedByName.Should().Be("John");
        entity.ModifiedById.Should().Be("user-123");
    }

    [Fact]
    public void SetAuditFields_SecondCall_OnlyUpdatesModified()
    {
        TestEntity entity = TestEntity.Create();
        entity.SetAuditFields("user-1", "First");
        entity.SetAuditFields("user-2", "Second");

        entity.CreatedById.Should().Be("user-1");
        entity.CreatedByName.Should().Be("First");
        entity.ModifiedById.Should().Be("user-2");
        entity.ModifiedByName.Should().Be("Second");
    }

    [Fact]
    public void SetCreatedAt_SetsValue()
    {
        TestEntity entity = TestEntity.Create();
        Instant now = SystemClock.Instance.GetCurrentInstant();

        entity.SetCreatedAt(now);

        entity.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void SetUpdatedAt_SetsValue()
    {
        TestEntity entity = TestEntity.Create();
        Instant now = SystemClock.Instance.GetCurrentInstant();

        entity.SetUpdatedAt(now);

        entity.UpdatedAt.Should().Be(now);
    }
}

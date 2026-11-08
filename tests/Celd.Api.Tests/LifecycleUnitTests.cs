using Celd.Api.Domain;
using Xunit;

namespace Celd.Api.Tests;

public sealed class LifecycleUnitTests
{
    [Theory]
    [InlineData(Statuses.Reserved,Statuses.Cancelled)]
    [InlineData(Statuses.Reserved,Statuses.CheckedOut)]
    [InlineData(Statuses.CheckedOut,Statuses.Returned)]
    public void BR_RES_003_all_and_only_legal_transitions_are_permitted(string from,string to)
    {
        Assert.True(ReservationLifecycle.CanTransition(from,to));
    }

    [Theory]
    [InlineData(Statuses.Reserved,Statuses.Returned)]
    [InlineData(Statuses.CheckedOut,Statuses.Cancelled)]
    [InlineData(Statuses.Cancelled,Statuses.Reserved)]
    [InlineData(Statuses.Returned,Statuses.Reserved)]
    [InlineData(Statuses.Cancelled,Statuses.CheckedOut)]
    [InlineData(Statuses.Returned,Statuses.CheckedOut)]
    [InlineData(Statuses.Reserved,Statuses.Reserved)]
    [InlineData(Statuses.CheckedOut,Statuses.CheckedOut)]
    [InlineData(Statuses.Returned,Statuses.Returned)]
    [InlineData(Statuses.Cancelled,Statuses.Cancelled)]
    public void BR_RES_003_illegal_transitions_are_rejected(string from,string to)
    {
        Assert.False(ReservationLifecycle.CanTransition(from,to));
    }
}

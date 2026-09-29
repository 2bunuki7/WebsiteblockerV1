using Xunit;
using WebsiteBlocker.Core.Services;
namespace WebsiteBlocker.Tests;

public class DomainMatcherTests
{
    [Fact]
    public void NormalizeDomain_RemovesHttps()
    {
        string result =
            DomainMatcher.NormalizeDomain(
                "https://youtube.com");

        Assert.Equal(
            "youtube.com",
            result);
    }

    [Fact]
    public void NormalizeDomain_RemovesWww()
    {
        string result =
            DomainMatcher.NormalizeDomain(
                "www.youtube.com");

        Assert.Equal(
            "youtube.com",
            result);
    }

    [Fact]
    public void Matches_DetectsExactDomain()
    {
        bool result =
            DomainMatcher.Matches(
                "youtube.com",
                "youtube.com");

        Assert.True(result);
    }

    [Fact]
    public void Matches_DetectsSubdomain()
    {
        bool result =
            DomainMatcher.Matches(
                "music.youtube.com",
                "youtube.com");

        Assert.True(result);
    }

    [Fact]
    public void Matches_RejectsDifferentDomain()
    {
        bool result =
            DomainMatcher.Matches(
                "google.com",
                "youtube.com");

        Assert.False(result);
    }
}
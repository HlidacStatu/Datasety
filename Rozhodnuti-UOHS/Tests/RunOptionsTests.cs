using Xunit;

namespace Rozhodnuti_UOHS.Tests;

public sealed class RunOptionsTests
{
    [Fact]
    public void Parse_OnlyApiKey_SelectsDiscoveryWithoutStartId()
    {
        var options = RunOptions.Parse(["/apikey=secret"]);

        Assert.Equal("secret", options.ApiKey);
        Assert.Null(options.StartId);
        Assert.Null(options.Count);
        Assert.False(options.DryRun);
        Assert.False(options.RecreateDataset);
        Assert.False(options.Debug);
    }

    [Fact]
    public void Parse_StartAndNum_ReturnsRange()
    {
        var options = RunOptions.Parse(["/apiKey=secret", "/start=24000", "/num=600"]);

        Assert.Equal(24000, options.StartId);
        Assert.Equal(600, options.Count);
    }

    [Fact]
    public void Parse_Flags_AreRecognizedCaseInsensitively()
    {
        var options = RunOptions.Parse(["/APIKEY=secret", "/DryRun", "/Debug"]);

        Assert.True(options.DryRun);
        Assert.True(options.Debug);
    }

    [Fact]
    public void Parse_ApiKeyContainingEqualsSign_KeepsWholeValue()
    {
        var options = RunOptions.Parse(["/apikey=abc=def"]);

        Assert.Equal("abc=def", options.ApiKey);
    }

    [Theory]
    [InlineData]
    [InlineData("/apikey=")]
    [InlineData("/apikey")]
    public void Parse_MissingApiKey_Throws(params string[] args)
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(args));
    }

    [Theory]
    [InlineData("/start=24000x")]
    [InlineData("/start=-1")]
    [InlineData("/start=")]
    [InlineData("/start")]
    [InlineData("/start=1", "/num=0")]
    [InlineData("/start=1", "/num=1o00")]
    public void Parse_InvalidNumber_Throws(params string[] extraArgs)
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", .. extraArgs]));
    }

    [Fact]
    public void Parse_NumWithoutStart_Throws()
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", "/num=10"]));
    }

    [Fact]
    public void Parse_DryRunWithNew_Throws()
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", "/dryrun", "/new"]));
    }

    [Fact]
    public void Parse_UnknownArgument_Throws()
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", "/strat=10"]));
    }

    [Fact]
    public void Parse_DuplicateArgument_Throws()
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", "/start=1", "/START=2"]));
    }

    [Fact]
    public void Parse_FlagWithValue_Throws()
    {
        Assert.Throws<RunOptionsException>(() => RunOptions.Parse(["/apikey=secret", "/dryrun=false"]));
    }

    [Fact]
    public void ToLogString_ApiKey_IsMasked()
    {
        var text = RunOptions.ToLogString(["/ApiKey=secret", "/start=5"]);

        Assert.Equal("/apikey=*** /start=5", text);
    }
}

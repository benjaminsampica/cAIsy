namespace Caisy.Tests;

public static class FakeFactory
{
    public static readonly Fixture Fixture = new();

    public static string RandomString = Fixture.Create<string>();
}

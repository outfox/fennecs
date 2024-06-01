namespace fennecs.tests;

public class JoinsTests
{
    [Theory]
    [InlineData(new[] {1, 1, 1})]
    [InlineData(new[] {1, 1, 3})]
    [InlineData(new[] {1, 5, 1})]
    [InlineData(new[] {1, 1, 5})]
    [InlineData(new[] {5, 1, 1})]
    [InlineData(new[] {9, 5, 3})]
    [InlineData(new[] {42, 23, 69})]
    private void CrossJoin_Counts_All(int[] limiter)
    {
        int[] counter = [0, 0, 0];

        var count = 0;
        do
        {
            count++;
        } while (Joins.FullPermutation(counter, limiter));

        var product = limiter.Aggregate(1, (current, i) => current * i);
        Assert.Equal(product, count);
    }
}
namespace fennecs.tests;

public class MatchTests
{
    [Fact]
    private void CrossJoin_Counts_All()
    {
        int[] counter = [0, 0, 0];
        int[] limiter = [9, 5, 3];

        var count = 0;
        do
        {
            count++;
        } while (Match.CrossJoin(counter, limiter));

        Assert.Equal(9 * 5 * 3, count);
    }
}
namespace Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        //Arrange
        var myMath = new MyMath();
        int a = 5, b = 10;
        var expected = 15;

        //Act
        var actual = myMath.Add(a, b);


        //Assert
        Assert.Equal(expected, actual);
    }
}
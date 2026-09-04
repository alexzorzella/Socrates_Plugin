using NUnit.Framework;

public class PracticalityTests {
    [Test]
    public void TestIncrementWithOverflow_OneItemPlus() {
        Assert.AreEqual(0, IncrementWithOverflow.Run(0, 1, 1));
    }

    [Test]
    public void TestIncrementWithOverflow_OneItemPlusLoopManyTimesOver() {
        Assert.AreEqual(0, IncrementWithOverflow.Run(0, 1, 10));
    }
    
    [Test]
    public void TestIncrementWithOverflow_OneItemMinus() {
        Assert.AreEqual(0, IncrementWithOverflow.Run(0, 1, -1));
    }

    [Test]
    public void TestIncrementWithOverflow_TwoItemsPlus() {
        Assert.AreEqual(1, IncrementWithOverflow.Run(0, 2, 1));
    }

    [Test]
    public void TestIncrementWithOverflow_TwoItemsPlusLoopManyTimesOver() {
        Assert.AreEqual(0, IncrementWithOverflow.Run(0, 2, 10));
    }
    
    [Test]
    public void TestIncrementWithOverflow_TwoItemsMinus() {
        Assert.AreEqual(1, IncrementWithOverflow.Run(0, 2, -1));
    }

    [Test]
    public void TestIncrementWithOverflow_ThreeItemsPlus() {
        Assert.AreEqual(1, IncrementWithOverflow.Run(0, 3, 1));
    }

    [Test]
    public void TestIncrementWithOverflow_ThreeItemsMinus() {
        Assert.AreEqual(2, IncrementWithOverflow.Run(0, 3, -1));
    }

    [Test]
    public void TestIncrementWithOverflow_ThreeItemsPlus_Wrapping() {
        Assert.AreEqual(2, IncrementWithOverflow.Run(0, 3, 2));
    }

    [Test]
    public void TestIncrementWithOverflow_ThreeItemsMinus_Wrapping() {
        Assert.AreEqual(1, IncrementWithOverflow.Run(0, 3, -2));
    }

    [Test]
    public void TestIncrementWithOverflow_FourItemsPlus() {
        Assert.AreEqual(1, IncrementWithOverflow.Run(0, 4, 1));
    }

    [Test]
    public void TestIncrementWithOverflow_FourItemsMinus() {
        Assert.AreEqual(3, IncrementWithOverflow.Run(0, 4, -1));
    }
}
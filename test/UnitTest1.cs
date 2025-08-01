namespace CyanDevelopers.SimpleIntegratedDB.Tests;

public class TestDB(string path) : Database(path)
{
    public Table<long> Test { get; set; } = new Table<long>()
    {
        12,34
    };
    public new void Save() => base.Save();
}

public class UnitTest
{
    [Fact]
    public void Test1()
    {
        TestDB db = new TestDB("test.json");
        db.Test.Add(45);
        db.Save();
        TestDB db2 = new TestDB("test.json");
        Assert.True(db2.Test.ToList()[2] == 45);
    }
}
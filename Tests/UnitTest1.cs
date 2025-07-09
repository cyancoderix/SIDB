using SimpleIntegratedDB.Databases;

namespace SimpleIntegratedDB.Tests;

public class DatabaseTests
{
    [Fact]
    public async Task FileTest()
    {
        TestingClass cls1 = new TestingClass();
        cls1.FirstDatabase = new List<dynamic>()
        {
            "First", "Second", 3
        };
        cls1.SecondDatabase = new List<dynamic>()
        {
            4, "Fifth", "Sixth"
        };

        await cls1.Client.Save();

        TestingClass cls2 = new TestingClass();
        await cls2.Client.Load();
        Assert.True(CheckDatabases(cls1.FirstDatabase, cls2.FirstDatabase) &&
                    CheckDatabases(cls1.SecondDatabase, cls2.SecondDatabase));
    }

    bool CheckDatabases(ICollection<dynamic> a, ICollection<dynamic> b)
    {
        if (a.Count != b.Count) return false;
        bool result = true;
        for (int i = 0; i < a.Count; i++)
            result = a.ToArray()[i] != b.ToArray()[i] ? false : result;
        return result;
    }

    [Fact]
    public void CheckCheckingDatabases()
    {
        (ICollection<dynamic> a, ICollection<dynamic> b, ICollection<dynamic> c) = (["a", "b"], ["a", "b"], ["c", "d"]);
        Assert.True(CheckDatabases(a, b) && !CheckDatabases(b, c));
    }

    [Fact]
    public async Task WrongDatabaseTest()
    {
        TestingClass cls1 = new TestingClass();
        cls1.WrongDatabase = new List<string>()
        {
            "First", "Second"
        };
        await cls1.Client.Save();

        TestingClass cls2 = new TestingClass();
        await cls2.Client.Load();
        
        Assert.True(cls2.WrongDatabase.Count == 0);
    }
}

public class TestingClass
{
    [Database] public ICollection<dynamic> FirstDatabase { get; set; } = [];
    [Database] public ICollection<dynamic> SecondDatabase { get; set; } = [];
    [Database] public ICollection<string> WrongDatabase { get; set; } = [];
    public DatabaseClient Client { get; }

    public TestingClass()
    {
        Client = new DatabaseClient(this, "test.json");
    }
}
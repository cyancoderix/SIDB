namespace CyanDevelopers.SimpleIntegratedDB.Tests;

public class TestDBPrimitive() : Database(new DatabaseConfiguration()
{
    Path = "testPrim.json",
    LoadLate = true,
    SaveOnExit = false,
})
{
    public Table<long> Test { get; set; } = new Table<long>()
    {
        12,34
    };
    public new void Save() => base.Save();
}
public class TestDBReference() : Database(new DatabaseConfiguration()
{
    Path = "testRef.json",
    LoadLate = true,
    SaveOnExit = false
})
{
    public Table<MyClass> Test { get; set; } = new Table<MyClass>()
    {
        new MyClass("Test1","Desc1"),
        new MyClass("Test2","Desc2"),
    };
    public new void Save() => base.Save();
}
public class MyClass(string name, string? description)
{
    public string Name { get; set; } = name;
    public string? Description { get; set; } = description;
    public static bool operator ==(MyClass a, MyClass b) => a.Equals(b);
    public static bool operator !=(MyClass a, MyClass b) => !a.Equals(b);
    public override bool Equals(object? obj) => obj is MyClass c ? Name == c.Name && Description == c.Description : false;
    public override int GetHashCode() => base.GetHashCode();
}

public class UnitTest
{
    [Fact]
    public void Test1Primitive()
    {
        DeleteFile("testPrim.json");
        TestDBPrimitive db = new TestDBPrimitive();
        db.Test.Add(45);
        db.Save();
        TestDBPrimitive db2 = new TestDBPrimitive();
        db2.Load();
        Assert.True(db.Test.ToList().SequenceEqual(db2.Test.ToList()));
    }
    [Fact]
    public void Test1Reference()
    {
        DeleteFile("testRef.json");
        TestDBReference db = new TestDBReference();
        db.Test.Add(new MyClass("Test3", "Desc3"));
        db.Save();
        TestDBReference db2 = new TestDBReference();
        db2.Load();
        Assert.True(CheckRefCollection(db.Test.ToList(),db2.Test.ToList()));
    }
    private void DeleteFile(string path) => File.Delete(path);
    private bool CheckRefCollection<T>(ICollection<T> a, ICollection<T> b)
    {
        bool result = true;
        for (int i = 0; i < a.Count; i++)
            result = a.ToArray()[i]!.Equals(b.ToArray()[i]) ? result : false;
        return result;
    }
}
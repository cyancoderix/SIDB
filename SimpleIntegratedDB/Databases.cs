using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleIntegratedDB.Databases;

[AttributeUsage(AttributeTargets.Property)]
public class DatabaseAttribute : Attribute { }

public class DatabaseClient(object obj, string path)
{
    object Obj { get; } = obj;
    string Path { get; } = path;

    ICollection<PropertyInfo> GetDatabases() =>
        Obj.GetType().GetProperties().Where(x =>
                x.GetCustomAttribute(typeof(DatabaseAttribute)) != null && x.GetValue(Obj) is (ICollection<dynamic>))
            .ToArray();

    public async Task Save()
    {
        IEnumerable<PropertyInfo> dbs = Obj.GetType().GetProperties().Where(x =>
            x.GetCustomAttribute(typeof(DatabaseAttribute)) != null && x.GetValue(Obj) is (ICollection<dynamic>));
        await File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(dbs.Select(x => x.GetValue(Obj)).ToList()));
    }

    public async Task Load()
    {
        ICollection<ICollection<dynamic>> loadedData =
            JsonConvert.DeserializeObject<ICollection<ICollection<dynamic>>>(await File.ReadAllTextAsync(Path))!;
        PropertyInfo[] dbs = GetDatabases().ToArray();
        for (int i = 0; i < loadedData.Count; i++)
            dbs[i].SetValue(Obj, loadedData.ToArray()[i]);
    }
}

// TODO 
// - Validation
//   - Type
//   - Type structure
//   - IO
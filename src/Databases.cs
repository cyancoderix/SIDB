using System.Collections;
using System.Reflection;
using Newtonsoft.Json;

namespace CyanDevelopers.SimpleIntegratedDB;

public class Database : IDisposable
{
    private string Path { get; init; }
    ICollection<PropertyInfo> Tables { get; init; }

    public Database(string path, bool loadLate = false, bool saveExit = true)
    {
        Tables = GetType().GetProperties().Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(Table<>))
            .ToList();
        Path = path;
        if (!loadLate) LoadSync(path);
        if (saveExit)
            AppDomain.CurrentDomain.ProcessExit += (object? s, EventArgs args) => Save();
    }

    private void LoadSync(string path)
    {
        if (!File.Exists(Path)) return;
        Dictionary<string, ICollection<object>?> rawDatabase = JsonConvert.DeserializeObject<Dictionary<string, ICollection<object>?>>(File.ReadAllText(Path)) ?? new();
        LoadDictionary(rawDatabase);
    }

    public async Task Load()
    {
        if (!File.Exists(Path)) return;
        Dictionary<string, ICollection<object>?> rawDatabase = JsonConvert.DeserializeObject<Dictionary<string, ICollection<object>?>>(await File.ReadAllTextAsync(Path)) ?? new();
        LoadDictionary(rawDatabase);
    }

    private void LoadDictionary(Dictionary<string, ICollection<object>?> db)
    {
        foreach (KeyValuePair<string, ICollection<object>?> table in db)
        {
            PropertyInfo? property = Tables.FirstOrDefault(x => x.Name == table.Key);
            if (table.Value == null || property == null) continue;
            // TODO Null check
            Type target = property.PropertyType.GenericTypeArguments[0];
            object? castedIEnumerable =
                typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(target).Invoke(null, [table.Value]);
            MethodInfo toList = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod([target]);
            object? castedTable = typeof(Table<>).MakeGenericType(target).GetConstructor([typeof(ICollection<>).MakeGenericType(target)])?.Invoke([toList.Invoke(null, [castedIEnumerable])]);
            if (castedTable == null) return;
            property.SetValue(this, castedTable);
        }
    }

    protected void Save()
    {
        Dictionary<string, ICollection<object>?> tables = new();
        foreach (PropertyInfo table in Tables)
            tables.Add(table.Name, ((IEnumerable)table.GetValue(this)).Cast<object>().ToList());
        File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(tables));
    }
    public void Dispose() => Save();
}

public sealed class Table<T> : ICollection<T>
{
    public int Count => Values.Count;

    public bool IsReadOnly => Values.IsReadOnly;
    ICollection<T> Values { get; }

    public void Add(T item) =>
        Values.Add(item);

    public void Clear() =>
        Values.Clear();

    public bool Contains(T item) =>
        Values.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) =>
        Values.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() =>
        Values.GetEnumerator();

    public bool Remove(T item) =>
        Values.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)Values).GetEnumerator();

    public Table(params ICollection<T> values) => Values = values;
}

// TODO 
// Complete system change - DataSet from EF
// - Validation
//   - Type
//   - Type structure
//   - IO
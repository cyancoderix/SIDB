using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanDevelopers.SimpleIntegratedDB;

public class Database : IDisposable
{
    private string Path { get; init; }
    ICollection<PropertyInfo> Tables { get; init; }

    public Database(DatabaseConfiguration configuration)
    {
        Path = configuration.Path;
        Tables = GetType().GetProperties().Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(Table<>)).ToList();
        if (!configuration.LoadLate) Load();
        if (configuration.SaveOnExit) AppDomain.CurrentDomain.ProcessExit += (object? s, EventArgs args) => Save();
    }

    public void Load()
    {
        if (!File.Exists(Path)) return;
        LoadStringGeneric(File.ReadAllText(Path));
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(Path)) return;
        LoadStringGeneric(await File.ReadAllTextAsync(Path));
    }
    private void LoadStringGeneric(string s)
    {
        Dictionary<string, ICollection<JToken>?> db = JsonConvert.DeserializeObject<Dictionary<string, ICollection<JToken>?>>(s) ?? new();
        foreach (KeyValuePair<string, ICollection<JToken>?> table in db)
        {
            PropertyInfo? property = Tables.FirstOrDefault(x => x.Name == table.Key);
            if (table.Value == null || property == null) continue;
            Type target = property.PropertyType.GenericTypeArguments[0];
            StringBuilder debug = new StringBuilder();
            typeof(Database)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(x => x.Name == "LoadTableGeneric" && x.IsGenericMethodDefinition)
                .MakeGenericMethod(target)
                .Invoke(this, [property, table.Value]);
        }
    }
    private void LoadTableGeneric<T>(PropertyInfo property, ICollection<JToken> jvalue)
    {
        List<T?> nullableValue = jvalue.Select(x => x.ToObject<T>()).ToList();
        nullableValue.RemoveAll(x => x == null);
        ICollection<T>? value = nullableValue!;
        if (value == null) return;
        property.SetValue(this, new Table<T>(value));
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

public class DatabaseConfiguration()
{
    public bool LoadLate { get; set; } = false;
    public bool SaveOnExit { get; set; } = true;
    public required string Path { get; set; }
    public static DatabaseConfiguration Default(string path) => new DatabaseConfiguration() { Path = path };
}
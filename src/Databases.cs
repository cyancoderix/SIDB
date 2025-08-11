using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanDevelopers.SimpleIntegratedDB;

/// <summary>
/// The main database class, that should be inherited.
/// </summary>
public abstract class Database : IDisposable
{
    private string Path { get; init; }
    ICollection<PropertyInfo> Tables { get; init; }

    /// <summary>
    /// Creates new Database "session", when you can manage all the Tables and manipulate with current data (saving, loading).
    /// </summary>
    /// <param name="configuration">
    /// You have to create <see cref="DatabaseConfiguration"/> object to configurate behaviour of the database
    /// </param>
    public Database(DatabaseConfiguration configuration)
    {
        Path = configuration.Path;
        Tables = GetType().GetProperties().Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(Table<>)).ToList();
        if (!configuration.LoadLate) Load();
        if (configuration.SaveOnExit) AppDomain.CurrentDomain.ProcessExit += (object? s, EventArgs args) => Save();
    }

    /// <summary>
    /// Loads all the tables from the predefined path.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(Path)) return;
        LoadStringGeneric(File.ReadAllText(Path));
    }
    /// <summary>
    /// Loads all the tables from the predefined path asynchronously. 
    /// </summary>

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
    /// <summary>
    /// Saves all the tables to the predefined path asynchronously. 
    /// </summary>
    public async Task SaveAsync()
    {
        Dictionary<string, ICollection<object>?> tables = new();
        foreach (PropertyInfo table in Tables)
        {
            if (table.GetValue(this) is IEnumerable enumerable)
                tables.Add(table.Name, enumerable.Cast<object>().ToList());
        }
        await File.WriteAllTextAsync(Path, JsonConvert.SerializeObject(tables));
    }
    /// <summary>
    /// Saves all the tables to the predefined path. 
    /// </summary>
    public void Save()
    {
        Dictionary<string, ICollection<object>?> tables = new();
        foreach (PropertyInfo table in Tables)
        {
            if (table.GetValue(this) is IEnumerable enumerable)
                tables.Add(table.Name, enumerable.Cast<object>().ToList());
        }
        File.WriteAllText(Path, JsonConvert.SerializeObject(tables));
    }
    /// <inheritdoc/>
    public void Dispose() => Save();
}

/// <summary>
/// The <see cref="ICollection{T}"/> that is passed into the <see cref="Database"/>.
/// </summary>
/// <typeparam name="T">Type of the ICollection</typeparam>
public sealed class Table<T> : ICollection<T>
{
    /// <inheritdoc />
    public int Count => Values.Count;

    /// <inheritdoc />
    public bool IsReadOnly => Values.IsReadOnly;
    ICollection<T> Values { get; }
    /// <inheritdoc />
    public void Add(T item) =>
        Values.Add(item);

    /// <inheritdoc />
    public void Clear() =>
        Values.Clear();

    /// <inheritdoc />
    public bool Contains(T item) =>
        Values.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) =>
        Values.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() =>
        Values.GetEnumerator();

    /// <inheritdoc />
    public bool Remove(T item) =>
        Values.Remove(item);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)Values).GetEnumerator();

    /// <summary>
    /// Creates new object of Table, which is recommended to declare in <see cref="Database"/>.
    /// </summary>
    /// <param name="values">All the values that should Table contain from declaring.</param>
    public Table(params ICollection<T> values) => Values = values;
}
/// <summary>
/// Object that is passed into the <see cref="Database"/> constructor.
/// </summary>
public sealed class DatabaseConfiguration()
{
    /// <summary>
    /// Deactivates loading in the <see cref="Database"/> constructor.
    /// Default value is false. (It will load automaticaly)
    /// </summary>
    public bool LoadLate { get; set; } = false;
    /// <summary>
    /// Activates automatic saving on the <see cref="AppDomain.ProcessExit"/> event .
    /// Default value is true. (It will save automaticaly when the program ends)
    /// </summary>
    public bool SaveOnExit { get; set; } = true;
    /// <summary>
    /// Path where will be the database stored.
    /// All the content will be in JSON, so it is recommended to use <c>.json</c> extension in the string.
    /// </summary>
    public required string Path { get; set; }
    /// <summary>
    /// The default configuration that needs only the required parameters.
    /// </summary>
    /// <param name="path">
    /// Path where will be the database stored.
    /// All the content will be in JSON, so it is recommended to use <c>.json</c> extension in the string.
    /// </param>
    /// <returns>The default database configuration.</returns>
    public static DatabaseConfiguration Default(string path) => new DatabaseConfiguration() { Path = path };
}
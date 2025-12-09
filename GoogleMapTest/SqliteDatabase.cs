using SQLite;

namespace GoogleMapTest;

// SQLite Database Handler
public class LocationDatabase
{
    private SQLiteAsyncConnection _database;

    // Constructor
    public LocationDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
        _database.CreateTableAsync<LocationData>().Wait(); // Create table if it doesn't exist
    }

    // Get the database path
    public static string GetDatabasePath()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, "MapLocation.db3");
    }

    // Save a location
    public Task<int> SaveLocationAsync(LocationData location)
    {
        if (location.Id != 0)
        {
            return _database.UpdateAsync(location);
        }
        else
        {
            return _database.InsertAsync(location);
        }
    }

    // Get all saved locations
    public Task<List<LocationData>> GetLocationsAsync()
    {
        return _database.Table<LocationData>().ToListAsync();
    }

    // Clear all saved locations
    public async Task ClearTableAsync()
    {
        await _database.DeleteAllAsync<LocationData>();
    }
}
// Location Data Model
public class LocationData
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

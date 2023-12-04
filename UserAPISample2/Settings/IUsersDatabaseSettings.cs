namespace UserAPISample2.Settings
{
    public interface IUsersDatabaseSettings
    {
        string? ConnectionString { get; set; }
        string? DbHost { get; set; }
        string? DbPort { get; set; }
        string? DatabaseName { get; set; }
        string? UsersCollectionName { get; set; }
        string? CountersCollectionName { get; set; }
    }
}

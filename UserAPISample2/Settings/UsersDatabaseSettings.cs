namespace UserAPISample2.Settings
{
    public class UsersDatabaseSettings : IUsersDatabaseSettings
    {
        public string? ConnectionString { get; set; }
        public string? DbHost { get; set; }
        public string? DbPort { get; set; }
        public string? DatabaseName { get; set; }
        public string? UsersCollectionName { get; set; }
        public string? CountersCollectionName { get; set; }
    }
}

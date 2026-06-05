using Npgsql;

namespace EnglishForDevs.Api.Shared;

public static class DatabaseConnection
{
    public static string GetConfiguredConnectionString(IConfiguration configuration)
    {
        return Normalize(configuration.GetConnectionString(ConfigurationKeys.DefaultConnectionName));
    }

    public static string Normalize(string? connectionString)
    {
        connectionString = connectionString?.Trim().Trim('"', '\'');

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return "";
        }

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "",
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
            SslMode = SslMode.Require
        };

        if (!uri.IsDefaultPort)
        {
            builder.Port = uri.Port;
        }

        return builder.ConnectionString;
    }
}

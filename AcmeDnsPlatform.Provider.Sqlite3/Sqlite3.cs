using AcmeDnsPlatform.Api;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Text;

namespace AcmeDnsPlatform.Provider.Sqlite3;

public class Sqlite3 : IPlatformAccountManagement
{
    private readonly IHashFunction _hashFunction;
    private readonly IPlatformDnsManagement _platformDnsManagement;
    private readonly SqliteConnection _connection;

    private string _dbPath = "";
    
    public Sqlite3(IHashFunction hashFunction, IPlatformDnsManagement platformDnsManagement)
    {
        this.GetVariables();

        _hashFunction = hashFunction;
        _platformDnsManagement = platformDnsManagement;
        _connection = new SqliteConnection("Data Source=" + _dbPath);
        _connection.Open();

        this.CreateTable();
    }

    private void GetVariables()
    {
        var envDbPath = Environment.GetEnvironmentVariable("SQLITE3_DBPATH");
        if (envDbPath == null)
        {
            throw new PlatformEnvironmentVariableNotSet("Environment variable \"SQLITE3_DBPATH\" not set!");
        }
        _dbPath = envDbPath;
    }

    public Account RegisterAccount(List<string> allowFrom)
    {
        var passwordBytes = new byte[64];
        Random.Shared.NextBytes(passwordBytes);

        var credentials = new Credentials()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Convert.ToBase64String(passwordBytes),
            AllowFrom = allowFrom
        };

        var domain = Guid.NewGuid().ToString();

        var result = new Account()
        {
            Username = credentials.Username,
            Password = credentials.Password,
            AllowFrom = credentials.AllowFrom,
            FullDomain = "_acme-challenge." + domain + "." + _platformDnsManagement.GetDomain(),
            Subdomain = "_acme-challenge." + domain
        };

        var hashedPassword = Convert.ToHexString(_hashFunction.Hash(Encoding.Default.GetBytes(result.Password)));

        var command = _connection.CreateCommand();
        command.CommandText = @"
INSERT INTO accounts(accountUsername, accountPassword, accountFullDomain,accountSubdomain)
VALUES(@u, @p, @fd, @s);
";
        command.Parameters.AddWithValue("@u", result.Username);
        command.Parameters.AddWithValue("@p", hashedPassword);
        command.Parameters.AddWithValue("@fd", result.FullDomain);
        command.Parameters.AddWithValue("@s", result.Subdomain);
        command.ExecuteNonQuery();

        foreach (var allowFromEntry in result.AllowFrom)
        {
            var command2 = _connection.CreateCommand();
            command2.CommandText = @"
INSERT INTO allowFromCidr(allowFromCidrAccountUsername, allowFromCidrEntry)
VALUES(@au, @e);
";
            command2.Parameters.AddWithValue("@au", result.Username);
            command2.Parameters.AddWithValue("@e", allowFromEntry);
            command2.ExecuteNonQuery();
            command2.Dispose();
        }

        return result;
    }

    public Account GetAccount(string username)
    {
        var result = new Account();

        var command = _connection.CreateCommand();
        command.CommandText = @"
SELECT * FROM accounts WHERE accountUsername = @u;
";
        command.Parameters.AddWithValue("@u", username);
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result = new Account()
            {
                Username = reader.GetString(0),
                Password = reader.GetString(1),
                FullDomain = reader.GetString(2),
                Subdomain = reader.GetString(3),
            };
        }
        reader.Close();

        var command2 = _connection.CreateCommand();
        command2.CommandText = @"
SELECT * FROM allowFromCidr where allowFromCidrAccountUsername = @au;
";
        command2.Parameters.AddWithValue("@au", username);
        var allowFromCidrs = new List<string>();
        var reader2 = command.ExecuteReader();
        while (reader2.Read())
        {
            allowFromCidrs.Add(reader2.GetString(1));
        }
        reader2.Close();

        result.AllowFrom = allowFromCidrs;

        return result;
    }

    public bool CheckCredentials(string username, string password, string ip)
    {
        var hashedPassword = Convert.ToHexString(_hashFunction.Hash(Encoding.Default.GetBytes(password)));

        var command = _connection.CreateCommand();
        command.CommandText = @"
SELECT * FROM accounts WHERE accountUsername = @u;
";
        command.Parameters.AddWithValue("@u", username);
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var passworddb = reader.GetString(1);
            if (hashedPassword != passworddb)
            {
                reader.Close();
                return false;
            }
        }
        reader.Close();

        var command2 = _connection.CreateCommand();
        command2.CommandText = @"
SELECT * FROM allowFromCidr WHERE allowFromCidrAccountUsername = @au;
";
        command2.Parameters.AddWithValue("@au", username);
        var allowFromCidrs = new List<string>();
        var reader2 = command.ExecuteReader();
        while (reader2.Read())
        {
            var cidrEntry = reader2.GetString(1);
            if (string.IsNullOrWhiteSpace(cidrEntry))
            {
                allowFromCidrs.Add(cidrEntry);
            }
        }
        reader2.Close();

        if (allowFromCidrs.Count > 0)
        {
            var allowed = allowFromCidrs.Any(x => IPlatformAccountManagement.IsInSubnetMask(ip, x));
            return allowed;
        }

        return true;
    }

    private void CreateTable()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS allowFromCidr (
    allowFromCidrAccountUsername NOT NULL,
    allowFromCidrEntry VARCHAR(128) NOT NULL);
";
        command.ExecuteNonQuery();

        command.CommandText = @"
CREATE TABLE IF NOT EXISTS accounts (
    accountUsername VARCHAR(36) PRIMARY KEY,
    accountPassword VARCHAR(128) NOT NULL,
    accountFullDomain VARCHAR(512) NOT NULL,
    accountSubdomain VARCHAR(512) NOT NULL);
";
        command.ExecuteNonQuery();
    }
}
using System.Text;
using AcmeDnsPlatform.Api;
using Microsoft.Data.Sqlite;

namespace AcmeDnsPlatform.Provider.Sqlite3;

public class Sqlite3 : IPlatformAccountManagement
{
    private IPlatformDnsManagement _platformDnsManagement;
    private SqliteConnection _sqliteConnection;
    private IHashFunction _hashFunction;

    private string _dbPath = "";
    
    public Sqlite3(IHashFunction hashFunction, IPlatformDnsManagement platformDnsManagement)
    {
        this.GetVariables();

        _platformDnsManagement = platformDnsManagement;
        _sqliteConnection = new SqliteConnection("Data Source=" + _dbPath);
        _sqliteConnection.Open();
        _hashFunction = hashFunction;

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
        var passwordBytes = new byte[32];
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
        
        var hashedPassword = Encoding.Default.GetString(_hashFunction.Hash(Encoding.Default.GetBytes(result.Password)));
        
        var command = _sqliteConnection.CreateCommand();
        command.CommandText = @"
INSERT INTO accounts(accountUsername, accountPassword, accountFullDomain,accountSubdomain)
VALUES($u, $p, $fd, $s);
";
        command.Parameters.Add(new SqliteParameter("$u", result.Username));
        command.Parameters.Add(new SqliteParameter("$p", hashedPassword));
        command.Parameters.Add(new SqliteParameter("$fd", result.FullDomain));
        command.Parameters.Add(new SqliteParameter("$s", result.Subdomain));
        command.ExecuteNonQuery();

        foreach (var allowFromEntry in result.AllowFrom)
        {
            var command2 = _sqliteConnection.CreateCommand();
            command2.CommandText = @"
INSERT INTO allowFromCidr(allowFromCidrAccountUsername, allowFromCidrEntry)
VALUES($au, $e);
";
            command2.Parameters.Add(new SqliteParameter("$au", result.Username));
            command2.Parameters.Add(new SqliteParameter("$e", allowFromEntry));
            command2.ExecuteNonQuery();
        }

        return result;
    }

    public Account GetAccount(string username)
    {
        var result = new Account();
        
        var command = _sqliteConnection.CreateCommand();
        command.CommandText = @"
SELECT * FROM accounts where accountUsername = $u;
";
        command.Parameters.Add(new SqliteParameter("$u", username));
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
        
        var command2 = _sqliteConnection.CreateCommand();
        command2.CommandText = @"
SELECT * FROM allowFromCidr where allowFromCidrAccountUsername = $au;
";
        command2.Parameters.Add(new SqliteParameter("$au", username));
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
        var hashedPassword = Encoding.Default.GetString(_hashFunction.Hash(Encoding.Default.GetBytes(password)));
        
        var command = _sqliteConnection.CreateCommand();
        command.CommandText = @"
SELECT * FROM accounts where accountUsername = $u;
";
        command.Parameters.Add(new SqliteParameter("$u", username));
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
        
        var command2 = _sqliteConnection.CreateCommand();
        command2.CommandText = @"
SELECT * FROM allowFromCidr where allowFromCidrAccountUsername = $au;
";
        command2.Parameters.Add(new SqliteParameter("$au", username));
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
        var command = _sqliteConnection.CreateCommand();
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
    accountSubdomain VARCAHR(512) NOT NULL);
";
        command.ExecuteNonQuery();
    }
}
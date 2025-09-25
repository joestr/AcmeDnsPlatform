using AcmeDnsPlatform.Api;
using Nelknet.LibSQL.Data;
using System.Text;

namespace AcmeDnsPlatform.Provider.LibSql
{
    public class LibSql : IPlatformAccountManagement
    {
        private IHashFunction _hashFunction;
        private IPlatformDnsManagement _platformDnsManagement;
        private LibSQLConnection _connection;

        private string _connectionString = "";

        public LibSql(IHashFunction hashFunction, IPlatformDnsManagement platformDnsManagement)
        {
            this.GetVariables();

            this._hashFunction = hashFunction;
            this._platformDnsManagement = platformDnsManagement;
            this._connection = new LibSQLConnection(_connectionString);
            this._connection.Open();
            
            this.CreateTable();
        }

        private void GetVariables()
        {
            var connectionString = Environment.GetEnvironmentVariable("LIBSQL_CONNECTIONSTRING");
            if (connectionString == null)
            {
                throw new PlatformEnvironmentVariableNotSet("Environment variable \"LIBSQL_CONNECTIONSTRING\" not set!");
            }
            _connectionString = connectionString;
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

            var command = _connection.CreateCommand();
            command.CommandText = @"
INSERT INTO accounts(accountUsername, accountPassword, accountFullDomain,accountSubdomain)
VALUES($u, $p, $fd, $s);
";
            command.Parameters.Add(new LibSQLParameter("$u", result.Username));
            command.Parameters.Add(new LibSQLParameter("$p", hashedPassword));
            command.Parameters.Add(new LibSQLParameter("$fd", result.FullDomain));
            command.Parameters.Add(new LibSQLParameter("$s", result.Subdomain));
            command.ExecuteNonQuery();

            foreach (var allowFromEntry in result.AllowFrom)
            {
                var command2 = _connection.CreateCommand();
                command2.CommandText = @"
INSERT INTO allowFromCidr(allowFromCidrAccountUsername, allowFromCidrEntry)
VALUES($au, $e);
";
                command2.Parameters.Add(new LibSQLParameter("$au", result.Username));
                command2.Parameters.Add(new LibSQLParameter("$e", allowFromEntry));
                command2.ExecuteNonQuery();
            }

            return result;
        }

        public Account GetAccount(string username)
        {
            var result = new Account();

            var command = _connection.CreateCommand();
            command.CommandText = @"
SELECT * FROM accounts where accountUsername = $u;
";
            command.Parameters.Add(new LibSQLParameter("$u", username));
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
SELECT * FROM allowFromCidr where allowFromCidrAccountUsername = $au;
";
            command2.Parameters.Add(new LibSQLParameter("$au", username));
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

            var command = _connection.CreateCommand();
            command.CommandText = @"
SELECT * FROM accounts where accountUsername = $u;
";
            command.Parameters.Add(new LibSQLParameter("$u", username));
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
SELECT * FROM allowFromCidr where allowFromCidrAccountUsername = $au;
";
            command2.Parameters.Add(new LibSQLParameter("$au", username));
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
    accountSubdomain VARCAHR(512) NOT NULL);
";
            command.ExecuteNonQuery();
        }
    }
}

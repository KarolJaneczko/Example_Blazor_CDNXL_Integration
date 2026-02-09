using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EBCI_BackEnd.Services {
    public class DatabaseService {
        private readonly string _server, _database, _login, _password;

        public DatabaseService(string server, string database, string login, string password) {
            _server = server;
            _database = database;
            _login = login;
            _password = password;
        }

        public IEnumerable<T> Query<T>(string query, object parameters) {
            using (var connection = CreateConnection(_server, _database, _login, _password)) {
                return connection.Query<T>(query, parameters);
            }
        }

        private IDbConnection CreateConnection(string dataSource, string initialCatalog, string userId, string password) {
            var sqlBuilder = new SqlConnectionStringBuilder {
                DataSource = dataSource,
                InitialCatalog = initialCatalog,
                UserID = userId,
                Password = password,
                IntegratedSecurity = false
            };
            return new SqlConnection(sqlBuilder.ToString());
        }
    }
}
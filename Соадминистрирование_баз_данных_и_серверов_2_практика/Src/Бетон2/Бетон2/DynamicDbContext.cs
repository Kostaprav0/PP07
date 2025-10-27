using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Бетон2
{
    public class DynamicDbContext : IDisposable
    {
        private SqlConnection _connection;

        public string ConnectionString { get; private set; }
        public bool IsConnected => _connection?.State == ConnectionState.Open;

        public DynamicDbContext(string connectionString)
        {
            ConnectionString = connectionString;
            _connection = new SqlConnection(connectionString);
        }

        public void Connect()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        // Существующие методы для работы с таблицами...
        public List<string> GetTableNames()
        {
            var tables = new List<string>();
            try
            {
                Connect();
                var command = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME",
                    _connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения списка таблиц: {ex.Message}");
            }
            return tables;
        }

        public DataTable GetTableData(string tableName)
        {
            var dataTable = new DataTable();
            try
            {
                Connect();
                var command = new SqlCommand($"SELECT * FROM [{tableName}]", _connection);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки данных таблицы {tableName}: {ex.Message}");
            }
            return dataTable;
        }

        // НОВЫЕ МЕТОДЫ ДЛЯ РАБОТЫ С ПОЛЬЗОВАТЕЛЯМИ И РОЛЯМИ БД

        public DataTable GetDatabaseUsers()
        {
            var dataTable = new DataTable();
            try
            {
                Connect();
                var command = new SqlCommand(@"
                    SELECT 
                        name,
                        principal_id,
                        type_desc as type,
                        create_date,
                        modify_date,
                        default_schema_name,
                        authentication_type_desc as authentication_type
                    FROM sys.database_principals 
                    WHERE type IN ('S', 'U', 'G') 
                    AND name NOT LIKE '##%'
                    AND name NOT IN ('guest', 'INFORMATION_SCHEMA', 'sys')
                    ORDER BY name", _connection);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения списка пользователей: {ex.Message}");
            }
            return dataTable;
        }

        public DataTable GetDatabaseRoles()
        {
            var dataTable = new DataTable();
            try
            {
                Connect();
                var command = new SqlCommand(@"
                    SELECT 
                        name,
                        principal_id,
                        type_desc as type,
                        create_date
                    FROM sys.database_principals 
                    WHERE type = 'R'
                    AND name NOT IN ('public')
                    ORDER BY name", _connection);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения списка ролей: {ex.Message}");
            }
            return dataTable;
        }

        public void CreateDatabaseUser(string userName, string password)
        {
            try
            {
                Connect();

                // ИСПРАВЛЕННЫЙ СИНТАКСИС - создаем сначала логин, затем пользователя
                var command = new SqlCommand(
                    $"CREATE LOGIN [{userName}] WITH PASSWORD = '{password}'; " +
                    $"CREATE USER [{userName}] FOR LOGIN [{userName}];", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания пользователя: {ex.Message}");
            }
        }

        public void DeleteDatabaseUser(string userName)
        {
            try
            {
                Connect();

                // Сначала удаляем пользователя из базы данных
                var dropUserCommand = new SqlCommand($"DROP USER [{userName}];", _connection);
                dropUserCommand.ExecuteNonQuery();

                // Затем удаляем логин
                var dropLoginCommand = new SqlCommand($"DROP LOGIN [{userName}];", _connection);
                dropLoginCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка удаления пользователя: {ex.Message}");
            }
        }

        public void ChangeUserPassword(string userName, string newPassword)
        {
            try
            {
                Connect();
                // ИСПРАВЛЕННЫЙ СИНТАКСИС - изменяем пароль логина
                var command = new SqlCommand($"ALTER LOGIN [{userName}] WITH PASSWORD = '{newPassword}';", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка смены пароля: {ex.Message}");
            }
        }

        public void CreateDatabaseRole(string roleName)
        {
            try
            {
                Connect();
                var command = new SqlCommand($"CREATE ROLE [{roleName}];", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания роли: {ex.Message}");
            }
        }

        public void DeleteDatabaseRole(string roleName)
        {
            try
            {
                Connect();
                var command = new SqlCommand($"DROP ROLE [{roleName}];", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка удаления роли: {ex.Message}");
            }
        }

        public void AssignRoleToUser(string userName, string roleName)
        {
            try
            {
                Connect();
                var command = new SqlCommand($"ALTER ROLE [{roleName}] ADD MEMBER [{userName}];", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка назначения роли: {ex.Message}");
            }
        }

        public void RevokeRoleFromUser(string userName, string roleName)
        {
            try
            {
                Connect();
                var command = new SqlCommand($"ALTER ROLE [{roleName}] DROP MEMBER [{userName}];", _connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка отзыва роли: {ex.Message}");
            }
        }

        // Остальные существующие методы...
        public int InsertRecord(string tableName, Dictionary<string, object> values)
        {
            try
            {
                Connect();
                var columns = string.Join(", ", values.Keys.Select(k => $"[{k}]"));
                var parameters = string.Join(", ", values.Keys.Select(k => $"@{k}"));
                var command = new SqlCommand($"INSERT INTO [{tableName}] ({columns}) VALUES ({parameters}); SELECT SCOPE_IDENTITY();", _connection);
                foreach (var kvp in values)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }
                var result = command.ExecuteScalar();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления записи: {ex.Message}");
            }
        }

        public int UpdateRecord(string tableName, Dictionary<string, object> values, string primaryKey, object id)
        {
            try
            {
                Connect();
                var setClause = string.Join(", ", values.Keys.Select(k => $"[{k}] = @{k}"));
                var command = new SqlCommand($"UPDATE [{tableName}] SET {setClause} WHERE [{primaryKey}] = @id", _connection);
                foreach (var kvp in values)
                {
                    command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                }
                command.Parameters.AddWithValue("@id", id);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления записи: {ex.Message}");
            }
        }

        public int DeleteRecord(string tableName, string primaryKey, object id)
        {
            try
            {
                Connect();
                var command = new SqlCommand($"DELETE FROM [{tableName}] WHERE [{primaryKey}] = @id", _connection);
                command.Parameters.AddWithValue("@id", id);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка удаления записи: {ex.Message}");
            }
        }

        public List<ColumnInfo> GetColumnsInfo(string tableName)
        {
            var columns = new List<ColumnInfo>();
            try
            {
                Connect();
                var command = new SqlCommand(@"
                    SELECT 
                        c.COLUMN_NAME,
                        c.DATA_TYPE,
                        c.IS_NULLABLE,
                        c.CHARACTER_MAXIMUM_LENGTH,
                        COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    WHERE c.TABLE_NAME = @tableName
                    ORDER BY c.ORDINAL_POSITION", _connection);
                command.Parameters.AddWithValue("@tableName", tableName);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new ColumnInfo
                        {
                            Name = reader.GetString(0),
                            DataType = reader.GetString(1),
                            IsNullable = reader.GetString(2) == "YES",
                            MaxLength = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                            IsIdentity = reader.GetInt32(4) == 1
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения информации о колонках: {ex.Message}");
            }
            return columns;
        }

        public string GetPrimaryKey(string tableName)
        {
            try
            {
                Connect();
                var command = new SqlCommand(@"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                    AND TABLE_NAME = @tableName", _connection);
                command.Parameters.AddWithValue("@tableName", tableName);
                var result = command.ExecuteScalar();
                return result?.ToString();
            }
            catch
            {
                var columns = GetColumnsInfo(tableName);
                return columns.FirstOrDefault()?.Name;
            }
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    public class ColumnInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public bool IsIdentity { get; set; }

        public Type GetNetType()
        {
            var dataTypeLower = DataType.ToLower();
            if (dataTypeLower == "int") return typeof(int);
            if (dataTypeLower == "bigint") return typeof(long);
            if (dataTypeLower == "smallint") return typeof(short);
            if (dataTypeLower == "tinyint") return typeof(byte);
            if (dataTypeLower == "bit") return typeof(bool);
            if (dataTypeLower == "decimal" || dataTypeLower == "numeric" || dataTypeLower == "money") return typeof(decimal);
            if (dataTypeLower == "float") return typeof(double);
            if (dataTypeLower == "real") return typeof(float);
            if (dataTypeLower == "datetime" || dataTypeLower == "datetime2" || dataTypeLower == "smalldatetime") return typeof(DateTime);
            if (dataTypeLower == "date") return typeof(DateTime);
            if (dataTypeLower == "time") return typeof(TimeSpan);
            if (dataTypeLower == "char" || dataTypeLower == "varchar" || dataTypeLower == "text" || dataTypeLower == "nchar" || dataTypeLower == "nvarchar" || dataTypeLower == "ntext") return typeof(string);
            if (dataTypeLower == "uniqueidentifier") return typeof(Guid);
            if (dataTypeLower == "binary" || dataTypeLower == "varbinary" || dataTypeLower == "image") return typeof(byte[]);
            return typeof(string);
        }
    }
}
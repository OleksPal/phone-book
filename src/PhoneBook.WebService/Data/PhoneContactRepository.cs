using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PhoneBookWebService.Models;

namespace PhoneBookWebService.Data
{
    public class PhoneContactRepository
    {
        private const string ContactFilterSql = @"
            (@FirstName IS NULL OR FirstName LIKE '%' + @FirstName + '%')
            AND (@LastName IS NULL OR LastName LIKE '%' + @LastName + '%')
            AND (@PhoneNumber IS NULL OR PhoneNumber LIKE '%' + @PhoneNumber + '%')
            AND (@Email IS NULL OR Email LIKE '%' + @Email + '%')";

        private readonly string _connectionString;

        public PhoneContactRepository()
        {
            _connectionString =
                ConfigurationManager
                    .ConnectionStrings["PhoneBookDb"]
                    .ConnectionString;
        }

        public PagedItems<PhoneContact> GetContacts(PhoneContactFilter filter, 
            int pageNumber = 1, int pageSize = 10, 
            string sortColumn =  "FirstName", string sortOrder = "asc")
        {
            filter = filter ?? new PhoneContactFilter();

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var orderBy = GetOrderBy(sortColumn, sortOrder);

            var result = new PagedItems<PhoneContact>
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var countSql = $@"
                SELECT COUNT(*)
                FROM PhoneContacts
                WHERE {ContactFilterSql};";

            var dataSql = $@"
                SELECT
                    Id,
                    FirstName,
                    LastName,
                    PhoneNumber,
                    Email
                FROM PhoneContacts
                WHERE {ContactFilterSql}
                ORDER BY {orderBy}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var countCommand = new SqlCommand(countSql, connection))
                {
                    AddFilterParameters(countCommand, filter);

                    result.TotalCount = (int)countCommand.ExecuteScalar();
                }

                using (var command = new SqlCommand(dataSql, connection))
                {
                    AddFilterParameters(command, filter);

                    var offset = (pageNumber - 1) * pageSize;

                    command.Parameters.Add("@Offset", SqlDbType.Int)
                        .Value = offset;

                    command.Parameters.Add("@PageSize", SqlDbType.Int)
                        .Value = pageSize;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Items.Add(MapContact(reader));
                        }
                    }
                }
            }

            return result;
        }

        public PhoneContact GetContactById(int id)
        {
            const string sql = @"
                SELECT
                    Id,
                    FirstName,
                    LastName,
                    PhoneNumber,
                    Email
                FROM PhoneContacts
                WHERE Id = @Id;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapContact(reader);
                    }
                }
            }

            return null;
        }

        public int AddContact(PhoneContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            const string sql = @"
                INSERT INTO PhoneContacts
                (
                    FirstName,
                    LastName,
                    PhoneNumber,
                    Email
                )
                VALUES
                (
                    @FirstName,
                    @LastName,
                    @PhoneNumber,
                    @Email
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                AddContactParameters(command, contact);

                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool UpdateContact(PhoneContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            const string sql = @"
                UPDATE PhoneContacts
                SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email
                WHERE Id = @Id;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int)
                    .Value = contact.Id;

                AddContactParameters(command, contact);

                connection.Open();

                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteContact(int id)
        {
            const string sql = @"
                DELETE FROM PhoneContacts
                WHERE Id = @Id;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                connection.Open();

                return command.ExecuteNonQuery() > 0;
            }
        }

        private void AddFilterParameters(
            SqlCommand command,
            PhoneContactFilter filter)
        {
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100)
                .Value = (object)filter.FirstName ?? DBNull.Value;

            command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100)
                .Value = (object)filter.LastName ?? DBNull.Value;

            command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50)
                .Value = (object)filter.PhoneNumber ?? DBNull.Value;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
                .Value = (object)filter.Email ?? DBNull.Value;
        }

        private static string GetOrderBy(string sortColumn, string sortOrder)
        {
            string column;

            switch (sortColumn)
            {
                case "FirstName":
                    column = "FirstName";
                    break;
                case "LastName":
                    column = "LastName";
                    break;
                case "PhoneNumber":
                    column = "PhoneNumber";
                    break;
                case "Email":
                    column = "Email";
                    break;
                default:
                    column = "LastName";
                    break;
            }

            return column + (
                string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
                    ? " DESC"
                    : " ASC")
                    + ", Id ASC";
        }

        private static void AddContactParameters(
            SqlCommand command,
            PhoneContact contact)
        {
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100)
                .Value = (object)contact.FirstName ?? DBNull.Value;

            command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100)
                .Value = (object)contact.LastName ?? DBNull.Value;

            command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50)
                .Value = (object)contact.PhoneNumber ?? DBNull.Value;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
                .Value = (object)contact.Email ?? DBNull.Value;
        }

        private PhoneContact MapContact(SqlDataReader reader)
        {
            return new PhoneContact
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                FirstName = GetNullableString(reader, "FirstName"),
                LastName = GetNullableString(reader, "LastName"),
                PhoneNumber = GetNullableString(reader, "PhoneNumber"),
                Email = GetNullableString(reader, "Email")
            };
        }

        private static string GetNullableString(
            SqlDataReader reader,
            string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }
    }
}

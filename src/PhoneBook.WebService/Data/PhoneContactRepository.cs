using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PhoneBookWebService.Models;

namespace PhoneBookWebService.Data
{
    public class PhoneContactRepository
    {
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
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            string orderBy = GetOrderBy(sortColumn, sortOrder);

            var result = new PagedItems<PhoneContact>
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            const string countSql = @"
                SELECT COUNT(*)
                FROM PhoneContacts
                WHERE
                    (@FirstName IS NULL OR FirstName LIKE '%' + @FirstName + '%')
                    AND (@LastName IS NULL OR LastName LIKE '%' + @LastName + '%')
                    AND (@PhoneNumber IS NULL OR PhoneNumber LIKE '%' + @PhoneNumber + '%')
                    AND (@Email IS NULL OR Email LIKE '%' + @Email + '%');";

            var dataSql = string.Format(@"
                SELECT
                    Id,
                    FirstName,
                    LastName,
                    PhoneNumber,
                    Email
                FROM PhoneContacts
                WHERE
                    (@FirstName IS NULL OR FirstName LIKE '%' + @FirstName + '%')
                    AND (@LastName IS NULL OR LastName LIKE '%' + @LastName + '%')
                    AND (@PhoneNumber IS NULL OR PhoneNumber LIKE '%' + @PhoneNumber + '%')
                    AND (@Email IS NULL OR Email LIKE '%' + @Email + '%')
                ORDER BY {0}
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;",
                orderBy);


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
                command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100)
                    .Value = (object)contact.FirstName ?? DBNull.Value;

                command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100)
                    .Value = (object)contact.LastName ?? DBNull.Value;

                command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50)
                    .Value = (object)contact.PhoneNumber ?? DBNull.Value;

                command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
                    .Value = (object)contact.Email ?? DBNull.Value;

                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool UpdateContact(PhoneContact contact)
        {
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

                command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100)
                    .Value = (object)contact.FirstName ?? DBNull.Value;

                command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100)
                    .Value = (object)contact.LastName ?? DBNull.Value;

                command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50)
                    .Value = (object)contact.PhoneNumber ?? DBNull.Value;

                command.Parameters.Add("@Email", SqlDbType.NVarChar, 255)
                    .Value = (object)contact.Email ?? DBNull.Value;

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

        private PhoneContact MapContact(SqlDataReader reader)
        {
            return new PhoneContact
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("Id")),

                FirstName = reader.IsDBNull(
                    reader.GetOrdinal("FirstName"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("FirstName")),

                LastName = reader.IsDBNull(
                    reader.GetOrdinal("LastName"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("LastName")),

                PhoneNumber = reader.IsDBNull(
                    reader.GetOrdinal("PhoneNumber"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("PhoneNumber")),

                Email = reader.IsDBNull(
                    reader.GetOrdinal("Email"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Email"))
            };
        }        
    }
}

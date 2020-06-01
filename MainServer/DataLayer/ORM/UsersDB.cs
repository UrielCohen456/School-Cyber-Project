using DataLayer;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public class UsersDB : Db<User>
    {
        public override string TableName => "USERS";

        protected override User CreateEntity(SqlDataReader reader)
        {
            var user = new User()
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                Username = reader["Username"].ToString(),
                Password = (byte[])reader["Password"],
                Salt = (byte[])reader["Salt"],
                GamesPlayed = (int)reader["GamesPlayed"],
                GamesWon = (int)reader["GamesWon"],
                GamesLost = (int)reader["GamesLost"],
                HighestScore = (int)reader["HighestScore"],
                TotalScore = (int)reader["TotalScore"],
            };
            
            return user;
        }

        protected override string GetSQLDeleteString(User entity)
        {
            return $"DELETE FROM {TableName} WHERE ID = @{entity.Id}";
        }

        protected override string GetSQLInsertString()
        {
            return $"INSERT INTO {TableName} " +
                    $"(Name, Username, Password, Salt, GamesPlayed, GamesWon, GamesLost, HighestScore, TotalScore) " +
                    $"VALUES " +
                    $"(@Name, @Username, @Password, @Salt, @GamesPlayed, @GamesWon, @GamesLost, @HighestScore, @TotalScore)";
        }

        protected override string GetSQLUpdateString(User entity)
        {
            return $"UPDATE {TableName} " +
                    $"SET Name = @Name, Username = @Username, Password = @Password, Salt = @Salt, " +
                    $"GamesPlayed = @GamesPlayed, GamesWon = @GamesWon, GamesLost = @GamesLost, HighestScore = @HighestScore, TotalScore = @TotalScore " +
                    $"WHERE Id = {entity.Id}";
        }

        protected override void SetSQLParameters(User entity)
        {
            sqlCommand.Parameters.Add("@Username", SqlDbType.NVarChar, 20).Value = entity.Username;
            sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 20).Value = entity.Name;
            sqlCommand.Parameters.Add("@Password", SqlDbType.Binary, 20).Value = entity.Password;
            sqlCommand.Parameters.Add("@Salt", SqlDbType.Binary, 20).Value = entity.Salt;
            sqlCommand.Parameters.Add("@GamesPlayed", SqlDbType.Int).Value = entity.GamesPlayed;
            sqlCommand.Parameters.Add("@GamesWon", SqlDbType.Int).Value = entity.GamesWon;
            sqlCommand.Parameters.Add("@GamesLost", SqlDbType.Int).Value = entity.GamesLost;
            sqlCommand.Parameters.Add("@HighestScore", SqlDbType.Int).Value = entity.HighestScore;
            sqlCommand.Parameters.Add("@TotalScore", SqlDbType.Int).Value = entity.TotalScore;
        }
    }
}

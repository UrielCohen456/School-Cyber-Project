using System;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public class WordsDB : Db<Word>
    {
        public override string TableName => "WORDS";

        protected override Word CreateEntity(SqlDataReader reader)
        {
            var word = new Word()
            {
                Id = Convert.ToInt32(reader["Id"]),
                Text = reader["Text"].ToString(),
            };

            return word;
        }

        protected override string GetSQLDeleteString(Word entity)
        {
            return $"DELETE FROM {TableName} WHERE ID = @{entity.Id}";
        }

        protected override string GetSQLInsertString()
        {
            return $"INSERT INTO {TableName} " +
                    $"(Text) " +
                    $"VALUES " +
                    $"(@Text)";
        }

        protected override string GetSQLUpdateString(Word entity)
        {
            return $"UPDATE {TableName} " +
                    $"SET Text = @Text " +
                    $"WHERE Id = {entity.Id}";
        }

        protected override void SetSQLParameters(Word entity)
        {
            sqlCommand.Parameters.Add("@Text", SqlDbType.NVarChar, 20).Value = entity.Text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public interface IDb<T> where T : BaseEntity, new()
    {
        /// <summary>
        /// Selects the the specified amount of entities from the database, default is 20 entities
        /// </summary>
        /// <param name="count">Amount of entities to select</param>
        /// <returns>Enumerable of entities</returns>
        IEnumerable<T> Select(int count = 20, string condition = null, SqlParameter[] sqlParameters = null);

        /// <summary>
        /// Inserts a new record into the database and assigns its id
        /// </summary>
        /// <param name="entity"></param>
        void Insert(T entity);

        /// <summary>
        /// Updates an entitiy based on its values
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);

        /// <summary>
        /// Deletes an entity based on its values
        /// </summary>
        /// <param name="entity"></param>
        void Delete(T entity);
    }

    public abstract class Db<T> : IDb<T> where T : BaseEntity, new()
    {
        protected readonly SqlConnection sqlConnection;
        protected readonly SqlCommand sqlCommand;
        
        public string ConnectionString { get; private set; } = @"Data Source=.\SQLEXPRESS;Initial Catalog=ProjectDB;Integrated Security=True";
        public abstract string TableName { get; }

        protected abstract void SetSQLParameters(T entity);
        protected abstract T CreateEntity(SqlDataReader reader);

        protected virtual string GetSQLSelectString(int count)
        {
            return $"SELECT TOP {count} * FROM {TableName} ";
        }
        protected abstract string GetSQLInsertString();
        protected abstract string GetSQLUpdateString(T entity);
        protected abstract string GetSQLDeleteString(T entity);

        public Db()
        {
            sqlConnection = new SqlConnection(ConnectionString);
            sqlCommand = new SqlCommand() { Connection = sqlConnection };
        }

        public virtual IEnumerable<T> Select(int count = 20, string addedSql = null, SqlParameter[] sqlParameters = null)
        {
            sqlCommand.Parameters.Clear();
            IEnumerable<T> entities = null;
            try
            {
                sqlConnection.Open();
                sqlCommand.CommandText = GetSQLSelectString(count) + addedSql;
                if (sqlParameters != null)
                    sqlCommand.Parameters.AddRange(sqlParameters);

                sqlCommand.CommandType = CommandType.Text;
                using var reader = sqlCommand.ExecuteReader();
                entities = CreateModel(reader); 
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }

            return entities;
        }

        public virtual void Insert(T entity)
        {
            sqlCommand.Parameters.Clear();
            sqlCommand.CommandText = GetSQLInsertString();
            sqlCommand.CommandType = CommandType.Text;
            SetSQLParameters(entity);
            Execute();
        }

        public virtual void Delete(T entity)
        {
            sqlCommand.Parameters.Clear();
            sqlCommand.CommandText = GetSQLDeleteString(entity);
            sqlCommand.CommandType = CommandType.Text;
            Execute();
        }

        public virtual void Update(T entity)
        {
            sqlCommand.Parameters.Clear();
            sqlCommand.CommandText = GetSQLUpdateString(entity);
            sqlCommand.CommandType = CommandType.Text;
            SetSQLParameters(entity);
            Execute();
        }

        private void Execute()
        {
            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private IEnumerable<T> CreateModel(SqlDataReader reader)
        {
            var entities = new List<T>();

            while(reader.Read())
                entities.Add(CreateEntity(reader));

            return entities;
        }
    }

}

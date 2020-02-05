using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLayer
{
    public abstract class BaseDB
    {
        protected string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=Airport;Integrated Security=True";

        protected SqlConnection connection;
        protected SqlCommand command;
        protected SqlDataReader reader;
        protected SqlTransaction trans = null;

        public BaseDB()
        {
            if (trans != null)
            {
                connection = trans.Connection;
            }
            else
            {
                connection = new SqlConnection(connectionString);
            }
            command = new SqlCommand
            {
                Connection = connection,
                Transaction = trans
            };
        }
    }

    public abstract class BaseDB<T> : BaseDB where T : BaseEntity, new()
    {
        abstract protected void SetSQLParameters(T model);
        abstract protected String GetSQLInsertString();
        abstract protected String GetSQLUpdateString(T model);
        abstract protected String GetSQLDeleteString(T model);
        abstract protected void GetModelColumns(T model);

        public void Insert(T model)
        {
            command.CommandText = GetSQLInsertString();
            SetSQLParameters(model);
            Execute(command);
        }
        public void Update(T model)
        {
            command.CommandText = GetSQLUpdateString(model);
            SetSQLParameters(model);
            Execute(command);
        }
        public void Delete(T model)
        {
            command.CommandText = GetSQLDeleteString(model);
            Execute(command);
        }


        protected void Execute(SqlCommand command)
        {
            try
            {
                if (trans == null)
                    connection.Open();
                else
                {
                    command.Transaction = trans;
                    command.Connection = trans.Connection;
                }
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open && trans == null)
                    connection.Close();
            }
        }

        public void IdentityInsert(T model)
        {
            command.CommandText = GetSQLInsertString();
            SetSQLParameters(model);
            try
            {
                if (trans == null)
                    connection.Open();
                else
                {
                    command.Transaction = trans;
                    command.Connection = trans.Connection;
                }
                command.ExecuteNonQuery();
                command.CommandText = "Select @@Identity";
                var id1 = command.ExecuteScalar();
                model.Id = int.Parse(id1.ToString());
            }
            catch (Exception e) { throw e; }
            finally {
                if (connection.State == ConnectionState.Open && trans == null)
                    connection.Close();
            }
        }


        protected List<T> Select()
        {
            List<T> list = null;
            try
            {
                if (trans == null)
                    connection.Open();
                else
                {
                    command.Transaction = trans;
                    command.Connection = trans.Connection;
                }
                reader = command.ExecuteReader();
                list = CreateModel();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (connection.State == ConnectionState.Open && trans == null)
                    connection.Close();
            }
            return list;
        }

        private List<T> CreateModel()
        {
            List<T> models = new List<T>();
            while (reader.Read())
            {
                T model = new T();
                GetModelColumns(model);
                models.Add(model);
            }
            return models;
        }

        public SqlTransaction BeginTransaction()
        {
            if (trans != null)
            {
                trans.Rollback();
                trans = null;
                throw new Exception
               ("Last transaction not committed/rolledbacked");
            }
            try
            {
                if (trans == null)
                    connection.Open();
                else
                {
                    command.Transaction = trans;
                    command.Connection = trans.Connection;
                }
                trans = connection.BeginTransaction();
                command.Transaction = trans;
            }
            catch
            {
                if (trans != null)
                    trans.Rollback();
                trans = null;
                throw new Exception("Open transaction failed");
            }
            return trans;
        }

        public void CloseTransaction(bool commitInd)
        {
            if (trans == null && commitInd) { throw new Exception("No open transaction to commit"); }
            try
            {
                if (commitInd)
                    trans.Commit();
                else
                    trans.Rollback();
            }
            catch (Exception e)
            {
                if (trans != null)
                {
                    try { trans.Rollback(); }
                    catch (Exception ex)
                    {
                        throw new Exception("Close transaction failed1:" + ex.Message);
                    }
                }
                throw new Exception("Close transaction failed2:" + e.Message);
            }
            finally
            {
                try
                {
                    if (trans.Connection != null && trans.Connection.State == ConnectionState.Open)
                        trans.Connection.Close();
                }
                catch { }
                command.Transaction = trans = null;
            }
        }

    }
}

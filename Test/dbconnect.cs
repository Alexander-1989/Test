using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

class dbconnect
{
    private SQLiteConnection Connect;
    private SQLiteCommand Command;
    private SQLiteDataAdapter DataAdapter;
    public delegate void GetErrorMessage(string message);
    public static event GetErrorMessage Message;

    // Создаем соединение с базой данных dbname
    public dbconnect(string DataBaseName)
    {
        try
        {
            Connect = new SQLiteConnection("Data Source = " + DataBaseName);
            OpenConnect();
        }
        catch (Exception exc) { Message?.Invoke(exc.Message); }
    }

    // Открываем соединение с базой данных
    public void OpenConnect()
    {
        if (Connect.State != ConnectionState.Open) // Если соединение еще не было открыто
        {
            try
            {
                Connect.Open();
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    // Зткрываем соединение с базой данных
    public void CloseConnect()
    {
        if (Connect.State != ConnectionState.Closed) // Если соединение открыто
        {
            try
            {
                Connect.Close();
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    public int ExecuteQuery(string query)
    {
        int res = -1;
        try
        {
            using (Command = new SQLiteCommand(query, Connect)) res = Command.ExecuteNonQuery();
        }
        catch (Exception exc) { Message?.Invoke(exc.Message); }

        return res;
    }

    public string[] GetTablesName()
    {
        if (Connect.State == ConnectionState.Open)
        {
            try
            {
                string query = "SELECT name FROM sqlite_master WHERE type = 'table'";
                List<string> names = new List<string>();
                using (Command = new SQLiteCommand(query, Connect))
                {
                    using (SQLiteDataReader dr = Command.ExecuteReader())
                    {
                        while (dr.Read()) names.Add(dr["name"].ToString());
                    }
                }
                return names.ToArray();
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
        return null;
    }

    public void GetTable(DataTable Table)
    {
        if (Connect.State == ConnectionState.Open)
        {
            try
            {
                string query = "SELECT * FROM " + Table.TableName;
                using (DataAdapter = new SQLiteDataAdapter(query, Connect))
                {
                    Table.Clear();
                    DataAdapter.Fill(Table);
                }
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    public void GetDataSet(DataSet data_set, string query)
    {
        if (Connect.State == ConnectionState.Open)
        {
            try
            {
                using (DataAdapter = new SQLiteDataAdapter(query, Connect))
                {
                    DataAdapter.Fill(data_set);
                }
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    // Внесение измененых или редактированных таблицы в базу данных
    public void UpdateTable(DataTable Table)
    {
        if (Connect.State == ConnectionState.Open)
        {
            try
            {
                string query = "SELECT * FROM " + Table.TableName;
                using (DataAdapter = new SQLiteDataAdapter(query, Connect))
                {
                    using (SQLiteCommandBuilder cb = new SQLiteCommandBuilder(DataAdapter))
                    {
                        DataAdapter.Update(Table);
                    }
                }
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    public void UpdateTable(DataSet data_set, string table_name)
    {
        if (Connect.State == ConnectionState.Open)
        {
            try
            {
                string query = "SELECT * FROM " + table_name;
                using (DataAdapter = new SQLiteDataAdapter(query, Connect))
                {
                    using (SQLiteCommandBuilder cb = new SQLiteCommandBuilder(DataAdapter))
                    {
                        DataAdapter.Update(data_set, table_name);
                    }
                }
            }
            catch (Exception exc) { Message?.Invoke(exc.Message); }
        }
    }

    // Создать новую базу данных
    static public void CreateDataBase(string DataBaseName)
    {
        if (!File.Exists(DataBaseName)) SQLiteConnection.CreateFile(DataBaseName);
        int result = -1;
        try
        {
            using (SQLiteConnection connect = new SQLiteConnection("Data Source = " + DataBaseName))
            {
                connect.Open();
                string query = @"CREATE TABLE documents (Number INTEGER, Date DATETIME, Amount DECIMAL, Note VARCHAR(50), PRIMARY KEY(Number));
                           CREATE TABLE positions (id INTEGER NOT NULL, Number INTEGER, Name VARCHAR(50), Amount DECIMAL, PRIMARY KEY(ID),
                           FOREIGN KEY(Number) REFERENCES documents(Number));";

                using (SQLiteCommand command = new SQLiteCommand(query, connect))
                {
                    result = command.ExecuteNonQuery();
                }
                connect.Close();
            }
        }
        catch (Exception exc) { Message?.Invoke(exc.Message); }
        if (result < 0) Message(string.Format("База Данных {0} не может быть создана", Path.GetFileName(DataBaseName)));
    }
}
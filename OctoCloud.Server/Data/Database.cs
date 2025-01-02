using Npgsql;

namespace OctoCloud.Server.Data
{
    public sealed class Database
    {
        #region Singleton
        private static Database instance = null;
        private static readonly object padlock = new object();

        public static Database Instance(string host = null, int port = 5432, string db = null, string user = null, string pass = null)
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        if (host == null || db == null || user == null || pass == null)
                        {
                            throw new ArgumentException("Parameters cannot be null when creating the instance for the first time.");
                        }
                        instance = new Database(host, port, db, user, pass);
                    }
                }
            }
            return instance;
        }
        
        public static Database Instance(){
            if(instance == null) throw new Exception("Database was not initialized");

            return instance;
        }
        #endregion

        private string Host;
        private int Port;
        private string User;
        private string Pass;
        private string Db;
        private string ConnectionString;


        private Database(string host, int port, string db, string user, string pass)
        {
            this.Host = host;
            this.Port = port;
            this.Db = db;
            this.User = user;
            this.Pass = pass;

            this.ConnectionString = $"Server={this.Host};Port={this.Port};User Id={this.User};Password={this.Pass};Database={this.Db};";
            // Test connection
            this.OpenConnection().Close();
        }

        public NpgsqlConnection OpenConnection(){
            return new NpgsqlConnection(this.ConnectionString);
        }
        public void CloseConnection(NpgsqlConnection connection){
            if (connection != null) { 
                connection.Close();
            }
        }

        public string DatabaseInfo() {
            return this.Host + ":" + this.Port;
        }

        public string ConnectionInfo(NpgsqlConnection connection){
            string result = "";
            if(connection.State == System.Data.ConnectionState.Open)
                result += "Connected to: ";
            else
                result += "Disconnected from: ";
            result += connection.Host + ":" + connection.Port;
            return result;
        }

        public Dictionary<string, dynamic>[] Get(string query, Dictionary<string, object> parameters, Dictionary<string, Type> columns)
        {
            LinkedList<Dictionary<string, dynamic>> result = new LinkedList<Dictionary<string, dynamic>>();

            using(NpgsqlConnection connection = this.OpenConnection())
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(query, connection)) { 
                    // Add parameters
                    foreach(KeyValuePair<string, object> param in parameters){
                        if (param.Value is string[])
                        {
                            cmd.Parameters.AddWithValue(param.Key, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, param.Value);
                        }else if(param.Value is string){
                            cmd.Parameters.AddWithValue(param.Key, NpgsqlTypes.NpgsqlDbType.Text, param.Value);
                        }
                    }
                    // Add query
                    cmd.CommandText = query;

                    // Execute query and return the data reader
                    var reader =  cmd.ExecuteReader();
                    while(reader.Read()) {
                        Dictionary<string, dynamic> item = new Dictionary<string, dynamic>();
                        foreach(KeyValuePair<string, Type> column in columns){
                            item.Add(column.Key, reader[column.Key]);
                        }
                        result.AddLast(item);
                    }
                    
                }
            }

            return result.ToArray<Dictionary<string, dynamic>>();
        }

        private bool QueryNoReturn(string query, Dictionary<string, object> parameters) {
            try{
                using(NpgsqlConnection connection = this.OpenConnection())
                {
                    connection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    {
                        // Add parameters
                        foreach(KeyValuePair<string, object> param in parameters){
                            if (param.Value is string[])
                            {
                                cmd.Parameters.AddWithValue(param.Key, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text, param.Value);
                            }else if(param.Value is string){
                                cmd.Parameters.AddWithValue(param.Key, NpgsqlTypes.NpgsqlDbType.Text, param.Value);
                            }
                        }
                        // Add query
                        cmd.CommandText = query;

                        // Execute query
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            } catch(Exception ex){
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Insert(string query, Dictionary<string, object> parameters) {
            return this.QueryNoReturn(query, parameters);
        }

        public bool Update(string query, Dictionary<string, object> parameters) {
            return this.QueryNoReturn(query, parameters);
        }

        public bool CreateTable(string query){
            if (string.IsNullOrEmpty(query)) { 
                throw new Exception("The query cannot be null or empty."); 
            }
            try{
                using(NpgsqlConnection connection = this.OpenConnection())
                {
                    connection.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            } catch(Exception ex){
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }
    }
}

using Npgsql;

namespace OctoCloud.Server.Database
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

        private NpgsqlConnection Connection;


        private Database(string host, int port, string db, string user, string pass)
        {
            this.Host = host;
            this.Port = port;
            this.Db = db;
            this.User = user;
            this.Pass = pass;

            string connectionString = $"Server={this.Host};Port={this.Port};User Id={this.User};Password={this.Pass};Database={this.Db};";
            this.Connection = new NpgsqlConnection(connectionString);
            this.OpenConnection();
        }

        public void OpenConnection(){
            this.Connection.Open();
        }
        public void CloseConnection(){
            if (this.Connection != null) { 
                this.Connection.Close();
            }
        }

        public string ConnectionInfo(){
            string result = "";
            if(this.Connection.State == System.Data.ConnectionState.Open)
                result += "Connected to: ";
            else
                result += "Disconnected from: ";
            result += this.Connection.Host + ":" + this.Connection.Port;
            return result;
        }

        public NpgsqlDataReader Get(string query, Dictionary<string, string> parameters) { 
            try { 
                using (var cmd = new NpgsqlCommand(query, this.Connection)) { 
                    // Add parameters
                    foreach (var param in parameters) { 
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    // Add query
                    cmd.CommandText = query;

                    // Execute query and return the data reader
                    return cmd.ExecuteReader(); 
                }
            } catch (Exception ex) { 
                Console.Error.WriteLine(ex.Message); 
                throw; // Re-throw the exception for the caller to handle
            }
        }

        private bool QueryNoReturn(string query, Dictionary<string, string> parameters) {
            try{
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Connection))
                {
                    // Add parameters
                    foreach(KeyValuePair<string, string> param in parameters){
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    // Add query
                    cmd.CommandText = query;

                    // Execute query
                    cmd.ExecuteNonQuery();
                }
                return true;
            } catch(Exception ex){
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Insert(string query, Dictionary<string, string> parameters) {
            return this.QueryNoReturn(query, parameters);
        }

        public bool Update(string query, Dictionary<string, string> parameters) {
            return this.QueryNoReturn(query, parameters);
        }

        public bool CreateTable(string query){
            if (string.IsNullOrEmpty(query)) { 
                throw new Exception("The query cannot be null or empty."); 
            }
            try{
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, this.Connection))
                {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                return true;
            } catch(Exception ex){
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }
    }
}

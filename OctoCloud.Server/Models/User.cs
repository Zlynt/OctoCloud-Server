using OctoCloud.Server.Security;
using DatabaseClass = OctoCloud.Server.Data.Database;

namespace OctoCloud.Server.Models
{
    public class User: TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static new readonly string DbSchema = """
        CREATE TABLE IF NOT EXISTS "User" (
            "username" VARCHAR(64) NOT NULL,
            "password" varchar(255) NOT NULL,
            "created_at" timestamp NOT NULL DEFAULT NOW(),
            "updated_at" timestamp NOT NULL DEFAULT NOW(),
            CONSTRAINT "users_pkey" PRIMARY KEY ("username")
        );
        """;
        
        public string Username{ get; }
        // Password
        private string password;
        public string Password{
            private get { return password; }
            set {
                password = Hash.CreatePasswordHash(value);
                this.Database.Update(
                """
                    UPDATE "User"
                    SET "password" = @Password, "updated_at" = NOW()
                    WHERE "username" = @Username;
                """,
                new Dictionary<string, object>{
                    { "Password", password.ToString() },
                    { "Username", this.Username.ToString() }
                }
                );
                this.RefreshInfo();
            }
        }

        private DateTime createdAt;
        public DateTime CreatedAt { 
            get { return createdAt; }
            private set {
                this.createdAt = value;
            }
        }

        private DateTime updatedAt;
        public DateTime UpdatedAt { 
            get { return updatedAt; }
            private set {
                this.updatedAt = value;
            }
        }

        private void RefreshInfo() {
            string query = """
            SELECT * FROM "User" WHERE "User"."username" = @Username ;
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { 
                { "@Username", this.Username }
            };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "password", typeof(string) },
                { "created_at", typeof(string) },
                { "updated_at", typeof(string) }
            };
            foreach(var row in DatabaseClass.Instance().Get(query, parameters, columns)){
                    password        = row["password"]; // Don't use the set from Password
                    this.CreatedAt  = Convert.ToDateTime(row["created_at"]);
                    this.UpdatedAt  = Convert.ToDateTime(row["updated_at"]);
            }
        }

        public User(string username){
            this.Username = username;
            this.Database = DatabaseClass.Instance();

            this.RefreshInfo();

        }

        public static User Create(string username, string password) {
            if(username != username.ToLower()) throw new Exception("Username can only have lower caracters");

            string query = """
            INSERT INTO "User" (
                "username",
                "password",
                "created_at", "updated_at"
            ) VALUES (
                @Username, 
                @Password, 
                NOW(), NOW()
            );
            """;

            Dictionary<string, object> parameters = new Dictionary<string, object> {
                { "Username", username.ToString() },
                { "Password", Hash.CreatePasswordHash(password).ToString() }
            };

            DatabaseClass.Instance().Insert(query, parameters);
            return new User(username);
        }

        public static void CreateTable() {
            TableLayout.CreateTable(DbSchema);
        }

        public bool VerifyPassword(string password) {
            return Hash.VerifyPasswordHash(password, this.Password);
        }
        
    }
}
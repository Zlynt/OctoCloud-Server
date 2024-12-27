using OctoCloud.Server.Security;
using DatabaseClass = OctoCloud.Server.Database.Database;

namespace OctoCloud.Server.Database
{
    public class User: TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static readonly string DbSchema = """
        CREATE TABLE IF NOT EXISTS "users" (
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
                    UPDATE "users"
                    SET "password" = @Password, "updated_at" = NOW()
                    WHERE "username" = @Username;
                """,
                new Dictionary<string, string>{
                    { "Password", password },
                    { "Username", this.Username }
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
            SELECT * FROM "users" WHERE "users"."username" = @Username ;
            """;
            Dictionary<string, string> parameters = new Dictionary<string, string> { 
                { "@Username", this.Username }
            };

            using (var reader = this.Database.Get(query, parameters))
            {
                if(reader.Read()){
                    password        = reader["password"].ToString(); // Don't use the set from Password
                    this.CreatedAt  = Convert.ToDateTime(reader["created_at"]);
                    this.UpdatedAt  = Convert.ToDateTime(reader["updated_at"]);
                }else throw new Exception("User not found");
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
            INSERT INTO "users" (
                "username",
                "password",
                "created_at", "updated_at"
            ) VALUES (
                @Username, 
                @Password, 
                NOW(), NOW()
            );
            """;

            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { "Username", username },
                { "Password", Hash.CreatePasswordHash(password) }
            };

            Database.Instance().Insert(query, parameters);
            return new User(username);
        }

        public static new void CreateTable() {
            Database.Instance().CreateTable(DbSchema);
        }

        public bool VerifyPassword(string password) {
            return Hash.VerifyPasswordHash(password, this.Password);
        }
        
    }
}
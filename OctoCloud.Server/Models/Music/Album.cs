using System.Text.Json;
using System.Text.Json.Serialization;
using DatabaseClass = OctoCloud.Server.Data.Database;

namespace OctoCloud.Server.Models.Music
{
    public class Album: TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static new readonly string DbSchema = """
        CREATE TABLE IF NOT EXISTS "Album" (
            "Id" TEXT not null,
            "Name" varchar(255) not null,
            "ImageUrl" TEXT null,
            constraint "Album_pkey" primary key ("Id")
        )
        """;

        public static void CreateTable() {
            TableLayout.CreateTable(DbSchema);
        }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("ImageUrl")]
        public string ImageUrl { get; set; }

        private Album(): base() {
            Database = DatabaseClass.Instance();
        }
        public Album(string id): this() {
            this.Id = id;
            // Get all the data
            this.RefreshInfo();
        }

        private void RefreshInfo() {
            string query = """
            SELECT * FROM "Album" WHERE "Album"."Id" = @Id ;
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { 
                { "@Id", this.Id }
            };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "Id", typeof(string) },
                { "Name", typeof(string) },
                { "ImageUrl", typeof(string) }
            };
            var rows = DatabaseClass.Instance().Get(query, parameters, columns);
            if(rows.Length == 0) throw new Exception("Album not found");
            foreach(var row in rows){
                this.Id = row["Id"];
                this.Name = row["Name"];
                this.ImageUrl = row["ImageUrl"];
            }
        }

        public static Album Create(
            string id, string name
        ) {
            string query = """
            INSERT INTO "Album" (
                "Id", "Name", "ImageUrl"
            ) VALUES (
                @Id, @Name, @ImageUrl
            );
            """;

            Dictionary<string, object> parameters = new Dictionary<string, object> {
                { "Id", id },
                { "Name", name },
                { "ImageUrl", $"https://coverartarchive.org/release-group/{id}/front" }
            };

            DatabaseClass.Instance().Insert(query, parameters);

            return new Album(id);
        }

    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using DatabaseClass = OctoCloud.Server.Data.Database;

namespace OctoCloud.Server.Models.Music
{
    public class Artist: TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static new readonly string DbSchema = """
        CREATE TABLE IF NOT EXISTS "Artist" (
            "Id"                TEXT not null,
            "Name"              varchar(255) null,
            constraint "Artist_pkey" primary key ("Id")
        )
        """;

        public static void CreateTable() {
            TableLayout.CreateTable(DbSchema);
        }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        private Artist(): base() {
            Database = DatabaseClass.Instance();
        }
        public Artist(string id): this() {
            this.Id = id;
            // Get all the data
            this.RefreshInfo();
        }

        private void RefreshInfo() {
            string query = """
            SELECT * FROM "Artist" WHERE "Artist"."Id" = @Id ;
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { 
                { "@Id", this.Id }
            };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "Id", typeof(string) },
                { "Name", typeof(string) }
            };

            var rows = DatabaseClass.Instance().Get(query, parameters, columns);
            if(rows.Length == 0) throw new Exception("Artist not found");
            foreach(var row in rows){
                this.Id = row["Id"];
                this.Name = row["Name"];
            }
        }

        public static Artist Create(
            string id, string name
        ) {
            string query = """
            INSERT INTO "Artist" (
                "Id", "Name"
            ) VALUES (
                @Id, @Name 
            );
            """;

            Dictionary<string, object> parameters = new Dictionary<string, object> {
                { "Id", id },
                { "Name", name }
            };

            DatabaseClass.Instance().Insert(query, parameters);

            return new Artist(id);
        }

    }

}
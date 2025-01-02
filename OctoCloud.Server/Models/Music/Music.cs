using System.Text.Json.Serialization;
using DatabaseClass = OctoCloud.Server.Data.Database;

namespace OctoCloud.Server.Models.Music
{
    public class Music: TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static new readonly string DbSchema = """
        CREATE TABLE IF NOT EXISTS "Music" (
            "Id"                TEXT not null,
            "Title"             varchar(255) not null,
            "AlbumId"           varchar(255) null,
            "ArtistsId"         TEXT[] null,
            "StreamUrl"         TEXT not null default NOW(),
            "LocalFilePath"     TEXT not null default NOW(),
            constraint "Music_StreamUrl_key" unique ("StreamUrl"), 
            constraint "Music_LocalFilePath_key" unique ("LocalFilePath"),
            constraint "Music_pkey" primary key ("Id")
        )
        """;

        public static void CreateTable() {
            TableLayout.CreateTable(DbSchema);
        }
        

        [JsonPropertyName("Id")]
        public string Id { get; set; }
        [JsonPropertyName("Title")]
        public string Title { get; set; }
        [JsonPropertyName("Album")]
        public Album? Album { get; set; }
        [JsonPropertyName("Artists")]
        public Artist[]? Artists { get; set; }
        [JsonPropertyName("StreamUrl")]
        public string StreamUrl { get; set; }
        [JsonIgnore]
        public string LocalPath { get; set; }

        private Music(): base() {
            Database = DatabaseClass.Instance();
        }

        public Music(string id): this() {
            this.Id = id;
            this.Artists = [];
            // Get all the data related to this Music<
            this.RefreshInfo();
        }

        private void RefreshInfo() {
            string query = """
            SELECT * FROM "Music" WHERE "Music"."Id" = @Id ;
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { 
                { "@Id", this.Id }
            };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "Id", typeof(string) },
                { "Title", typeof(string) },
                { "AlbumId", typeof(string) },
                { "StreamUrl", typeof(string) },
                { "LocalFilePath", typeof(string) },
                { "ArtistsId", typeof(string[]) }
            };
            var results = DatabaseClass.Instance().Get(query, parameters, columns);
            if(results.Length == 0) throw new Exception("Music not found");

            foreach(var row in results){
                this.Id         = row["Id"];
                this.Title      = row["Title"] ?? "Unkown Title";
                this.Album      = new Album(row["AlbumId"]);
                this.StreamUrl  = row["StreamUrl"];
                this.LocalPath  = row["LocalFilePath"];
                // Artists
                //try{
                    LinkedList<Artist> artists = new LinkedList<Artist>();
                    foreach(string artistId in row["ArtistsId"]) {
                        artists.AddLast(new Artist(artistId));
                    }
                    this.Artists = artists.ToArray<Artist>();
                //} catch(Exception ex) {}
            }
        }

        public static Music FindMusicByLocalFilePath(string localFilePath) {
            string query = """
            SELECT * FROM "Music" WHERE "Music"."LocalFilePath" = @LocalFilePath ;
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { 
                { "@LocalFilePath", localFilePath }
            };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "Id", typeof(string) },
                { "Title", typeof(string) },
                { "AlbumId", typeof(string) },
                { "StreamUrl", typeof(string) },
                { "LocalFilePath", typeof(string) },
                { "ArtistsId", typeof(string[]) }
            };
            var results = DatabaseClass.Instance().Get(query, parameters, columns);
            if(results.Length == 0) throw new Exception("Music not found");

            foreach(var row in results){
                return new Music(row["Id"]);
            }
            throw new Exception("Music not found");
        }

        public static Music[] GetAllMusic() {
            string query = """
            SELECT * FROM "Music";
            """;
            Dictionary<string, object> parameters = new Dictionary<string, object> { };
            Dictionary<string, Type> columns = new Dictionary<string, Type>{
                { "Id", typeof(string) }
            };
            var results = DatabaseClass.Instance().Get(query, parameters, columns);
            
            LinkedList<Music> toReturn = new LinkedList<Music>();
            foreach(var row in results){
                toReturn.AddLast(new Music(row["Id"]));
            }

            return toReturn.ToArray<Music>();
        }

        public static Music Create(
            string id, string title,
            Album album, Artist[] artists,
            string streamUrl, string localFilePath
        ) {
            string query = """
            INSERT INTO "Music" (
                "Id", "Title",
                "AlbumId", "ArtistsId",
                "StreamUrl", "LocalFilePath"
            ) VALUES (
                @Id, @Title,
                @AlbumId, @ArtistsId,
                @StreamUrl, @LocalFilePath
            );
            """;

            Dictionary<string, object> parameters = new Dictionary<string, object> {
                { "Id", id },
                { "Title", title },
                { "AlbumId", album.Id },
                { "ArtistsId", artists.Select(artist => artist.Id).ToArray<string>() },
                { "StreamUrl", streamUrl },
                { "LocalFilePath", localFilePath }
            };

            DatabaseClass.Instance().Insert(query, parameters);

            return new Music(id);
        }
    }
}
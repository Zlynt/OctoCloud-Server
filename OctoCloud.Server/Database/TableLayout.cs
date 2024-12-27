using DatabaseClass = OctoCloud.Server.Database.Database;

namespace OctoCloud.Server.Database
{

    public abstract class TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        private static readonly string DbSchema = "";

        public TableLayout(){
            this.Database = Database.Instance();
        }

        public static void CreateTable() {
            Database.Instance().CreateTable(DbSchema);
        }

    }
}
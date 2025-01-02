using DatabaseClass = OctoCloud.Server.Data.Database;

namespace OctoCloud.Server.Models
{

    public abstract class TableLayout
    {
        // Database connection
        private DatabaseClass Database;
        // Db Schema
        protected static string DbSchema = "";

        public TableLayout(){
            this.Database = DatabaseClass.Instance();
        }

        protected static void CreateTable(string dbSchema) {
            DatabaseClass.Instance().CreateTable(dbSchema);
        }

    }
}
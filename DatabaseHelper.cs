using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NoteyDictionary
{
    public enum Database
    {
        Lite = 0,
        FullAlpha = 1,
        Full = 2
    }

    class DatabaseHelper
    {
        public static string DetermineBestDatabase(bool noUI = false)
        {
            if (noUI)
                throw new NotImplementedException();
            DatabaseManager databaseManager = new DatabaseManager();
            databaseManager.ShowDialog();
            if (databaseManager.selectedFilePath == "")
                throw new Exception();
            return databaseManager.selectedFilePath;
        }

        public static Database GetDatabaseFromTag(string tag)
        {
            return tag switch
            {
                "lite" => Database.Lite,
                "full_alpha" => Database.FullAlpha,
                "full" => Database.Full,
                _ => throw new ArgumentException()
            };  
        }
    }
}

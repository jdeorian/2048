using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace _2048_c_sharp
{
    public class DB
    {
        private SQLiteConnection conn;

        public DB(string filename)
        {
            bool firstTime = !File.Exists(filename);
            if (firstTime) SQLiteConnection.CreateFile(filename);

            conn = new SQLiteConnection($"Data Source={filename};Version=3;");
            conn.Open();



        }

        private void InitializeTables()
        {

        }

        public void Close()
        {
            conn.Close();
        }
    }
}

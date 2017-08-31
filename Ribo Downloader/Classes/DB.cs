using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Ribo_Downloader.Classes
{

    class DB
    {
        private string file = VAR.LocalData+ "data.rbw";
        SQLiteConnection conn;
        public DB()
        {
            if (!File.Exists(file))
            {
                SQLiteConnection.CreateFile(file);
                conn = new SQLiteConnection("Data Source=" + file + ";Version=3;");
                conn.SetPassword("587fasdf878wer4sdf5s7we");
                conn.Open();
                SQLiteCommand SQLcommand = new SQLiteCommand();
                SQLcommand = conn.CreateCommand();
                SQLcommand.CommandText = "CREATE TABLE IF NOT EXISTS history(Name VARCHAR(300), dtime VARCHAR(30), fsize VARCHAR(30), status VARCHAR(15), link VARCHAR(255), localpath VARCHAR(255));";
                SQLcommand.ExecuteNonQuery();
                SQLcommand.Dispose();
            }
            else
            {
                conn = new SQLiteConnection("Data Source=" + file + ";Version=3; Password=587fasdf878wer4sdf5s7we;");
                conn.Open();
                //conn.ChangePassword("587fasdf878wer4sdf5s7we");

            }
        }
        public void insert(string name, string dtime, string fsize, string status, string link, string local)
        {
            string sql = "insert into history (Name, dtime, fsize, status, link, localpath) values ('" + name + "', '" + dtime + "', '" + fsize + "', '" + status + "', '" + link + "', '" + local + "')";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
            command.Dispose();
        }
        public bool Clear()
        {
            string sql = "delete from history";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
            return true;
        }
        public DataTable get_all()
        {
            SQLiteCommand cmd = new SQLiteCommand("select * from history", conn);
            SQLiteDataAdapter da = new SQLiteDataAdapter();
            DataTable dt = new DataTable();
            //cmd.ExecuteNonQuery();
            da.SelectCommand = cmd;
            da.Fill(dt);
            da.Dispose();
            return dt;
        }
        public DataTable search(string s)
        {
            SQLiteCommand cmd = new SQLiteCommand("select rowid, name from history WHERE utf LIKE '%" + s + "%' ORDER BY utf = '" + s + "' DESC, utf LIKE '" + s + "%' DESC", conn);
            SQLiteDataAdapter da = new SQLiteDataAdapter();
            DataTable dt = new DataTable();
            //cmd.ExecuteNonQuery();
            da.SelectCommand = cmd;
            da.Fill(dt);
            da.Dispose();
            return dt;
        }
        public void delete_by_link(string link)
        {
            string sql = "delete from history where link='" + link+"'";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            command.ExecuteNonQuery();
        }
        public void Close()
        {
            conn.Close();
        }
    }
}

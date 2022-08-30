using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace mailingbot
{
   internal class ConnectDB
   {
      public static SQLiteDataReader Query(string str)
      {
         SQLiteConnection SQLiteConnection = new SQLiteConnection("Data Source=|DataDirectory|mailing.db");
         SQLiteCommand SQLiteCommand = new SQLiteCommand(str, SQLiteConnection);
         try {
            SQLiteConnection.Open();
            SQLiteDataReader reader = SQLiteCommand.ExecuteReader();
            return reader;
         } catch { return null; }
      }

      public static void LoadUser(List<User> data)
      {
         data.Clear();
         SQLiteDataReader query = Query("select * from `User`;");
         if (query != null) {
            while (query.Read()) {
               data.Add(new User(
                  query.GetValue(0).ToString(),
                  query.GetValue(1).ToString(),
                  query.GetValue(2).ToString()
               ));
            }
         }
      }

      public static void LoadAdmin(List<Admin> data)
      {
         data.Clear();
         SQLiteDataReader query = Query("select * from `Admin`;");
         if (query != null) {
            while (query.Read()) {
               data.Add(new Admin(
                  query.GetValue(0).ToString(),
                  query.GetValue(1).ToString()
               ));
            }
         }
      }

      public static void LoadChanel(List<Chanel> data)
      {
         data.Clear();
         SQLiteDataReader query = Query("select * from `Chanel`;");
         if (query != null) {
            while (query.Read()) {
               data.Add(new Chanel(
                  Convert.ToInt32(query.GetValue(0)),
                  query.GetValue(1).ToString(),
                  query.GetValue(2).ToString(),
                  query.GetValue(3).ToString()
               ));
            }
         }
      }

      public static void LoadHello(List<Hello> data)
      {
         data.Clear();
         SQLiteDataReader query = Query("select * from `Hello`;");
         if (query != null) {
            while (query.Read()) {
               data.Add(new Hello(
                  Convert.ToInt32(query.GetValue(0)),
                  query.GetValue(1).ToString()
               ));
            }
         }
      }

      public static void LoadMailing(List<Mailing> data)
      {
         data.Clear();
         SQLiteDataReader query = Query("select * from `Mailing`;");
         if (query != null) {
            while (query.Read()) {
               data.Add(new Mailing(
                  Convert.ToInt32(query.GetValue(0)),
                  query.GetValue(1).ToString(),
                  query.GetValue(2).ToString(),
                  query.GetValue(3).ToString(),
                  query.GetValue(4).ToString()
               ));
            }
         }
      }
   }
}

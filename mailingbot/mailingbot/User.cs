namespace mailingbot
{
   internal class User
   {
      public string id { get; set; }
      public string first_name { get; set; }
      public string id_chanel { get; set; }
      public User(string id, string first_name, string id_chanel)
      {
         this.id = id;
         this.first_name = first_name;
         this.id_chanel = id_chanel;
      }
   }
}

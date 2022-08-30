using System;
using System.Collections.Generic;
using System.Text;

namespace mailingbot
{
   internal class Chanel
   {
      public int id { get; set; }
      public string id_chanel { get; set; }
      public string name { get; set; }
      public string id_user { get; set; }
      public string chanels { get; set; }
      public Chanel(int id, string id_chanel, string name, string id_user)
      {
         this.id = id;
         this.id_chanel = id_chanel;
         this.name = name;
         this.id_user = id_user;
      }
   }
}

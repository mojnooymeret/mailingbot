using System;
using System.Collections.Generic;
using System.Text;

namespace mailingbot
{
   internal class Admin
   {
      public string id { get; set; }
      public string first_name { get; set; }
      public Admin(string id, string first_name)
      {
         this.id = id;
         this.first_name = first_name;
      }
   }
}

namespace mailingbot
{
   internal class Mailing
   {
      public int id { get; set; }
      public string text { get; set; }
      public string media { get; set; }
      public string status { get; set; }
      public string chanels { get; set; }
      public Mailing(int id, string text, string media, string status, string chanels)
      {
         this.id = id;
         this.text = text;
         this.media = media;
         this.status = status;
         this.chanels = chanels;
      }
   }
}

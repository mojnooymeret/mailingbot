namespace mailingbot
{
   internal class Hello
   {
      public int id { get; set; }
      public string text { get; set; }
      public Hello(int id, string text)
      {
         this.id = id;
         this.text = text;
      }
   }
}

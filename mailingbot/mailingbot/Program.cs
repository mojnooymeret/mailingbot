using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;

namespace mailingbot
{
   internal class Program
   {
      private static string token { get; set; } = "914409546:AAHmjXdn8Ksp_IznWDeBFeSS05wXBybm8PU";
      private static TelegramBotClient client;
      static void Main(string[] args)
      {
         client = new TelegramBotClient(token);
         client.StartReceiving();
         client.OnMessage += ClientMessage;
         client.OnUpdate += UpdateData;
         client.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) => {
            InlineButtonOperation(sc, ev);
         };
         Console.ReadLine();
      }

      public static List<User> users = new List<User>();
      public static List<Hello> hello = new List<Hello>();
      public static List<Mailing> mailing = new List<Mailing>();
      public static List<Chanel> chanels = new List<Chanel>();
      public static List<Admin> admins = new List<Admin>();

      private static async void ClientMessage(object sender, MessageEventArgs e)
      {
         var message = e.Message;
         ConnectDB.LoadAdmin(admins);
         try {
            try {
               if (message.Text == "/mychanels") {
                  int count = 1;
                  ConnectDB.LoadChanel(chanels);
                  string chanel = string.Empty;
                  for (int i = 0; i < chanels.Count; i++) if (chanels[i].id_user == message.Chat.Id.ToString()) { chanel += count + ". " + chanels[i].name + " (" + chanels[i].id_chanel + ");\n"; count++; }
                  chanel = chanel.Trim('\n').Trim(';') + ".";
                  await client.SendTextMessageAsync(message.Chat.Id, "*Ваши каналы*\n\n" + chanel, Telegram.Bot.Types.Enums.ParseMode.Markdown);
               }
               else if (message.Text == "/mailing") {
                  await client.SendTextMessageAsync(message.Chat.Id, "Чтобы начать рассылку введите команду /mailing id-канала(ов). ID своих каналов можно узнать командой /mychanels. (Пример команды рассылки: /mailing 123456 123543 1231245 453985)");
               }
               else {
                  var admin = admins.Find(x => x.id == message.Chat.Id.ToString());
                  if (admin != null) {
                     if (message.Text == "/hello") {
                        ConnectDB.LoadHello(hello);
                        if (hello[0].text != "none") {
                           ConnectDB.LoadHello(hello);
                           var keyborad = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Удалить приветствие", "DelHello") } });
                           await client.SendTextMessageAsync(message.Chat.Id, "*Текущее приветствие:*\n\n" + hello[0].text, replyMarkup: keyborad, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        }
                        else {
                           try {
                              var keyborad = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Установить приветствие", "SetHello") } });
                              await client.SendTextMessageAsync(message.Chat.Id, "Приветствие отсутствует", replyMarkup: keyborad);
                           } catch { }
                        }
                     }
                     else if (message.Text == "/stats") {
                        try {
                           ConnectDB.LoadUser(users);
                           await client.SendTextMessageAsync(message.Chat.Id, "Количество пользователей в БД: " + users.Count);
                        } catch { }
                     }
                     else if (message.Text == "/admins") {
                        try {
                           ConnectDB.LoadAdmin(admins);
                           string adminList = string.Empty;
                           for (int i = 0; i < admins.Count; i++) adminList += Convert.ToInt32(i + 1) + ". " + admins[i].first_name + " (" + admins[i].id + ");\n";
                           adminList = adminList.Trim('\n').Trim(';') + ".";
                           await client.SendTextMessageAsync(message.Chat.Id, "*Список администраторов*\n\n" + adminList, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        } catch { }
                     }
                     else if (message.Text.Contains("/deladmin")) {
                        try {
                           string id = message.Text;
                           if (id.Contains(' ')) {
                              ConnectDB.LoadAdmin(admins);
                              try {
                                 var delId = admins.Find(x => x.id == id.Split(' ')[1]);
                                 if (delId.id != message.Chat.Id.ToString()) {
                                    if (delId.id != "885185553" && delId.id != "1573181469") {
                                       ConnectDB.Query("delete from `Admin` where id = " + delId.id + ";");
                                       await client.SendTextMessageAsync(message.Chat.Id, "Пользователь " + delId.first_name + " (" + delId.id + ") успешно удален из списка администраторов");
                                    }
                                    else await client.SendTextMessageAsync(message.Chat.Id, "Данного администратора невозможно удалить");
                                 }
                                 else await client.SendTextMessageAsync(message.Chat.Id, "Невозможно удалить самого себя");
                              } catch { await client.SendTextMessageAsync(message.Chat.Id, "Данный пользователь не администратор"); }
                           }
                        } catch { }
                     }
                     else if (message.Text.Contains("/addadmin")) {
                        try {
                           string id = message.Text;
                           if (id.Contains(' ')) {
                              ConnectDB.LoadAdmin(admins);
                              try {
                                 var checkAdmin = admins.Find(x => x.id == id.Split(' ')[1]);
                                 if (checkAdmin != null) await client.SendTextMessageAsync(message.Chat.Id, "Данный пользователь уже явялется администратором");
                                 else {
                                    ConnectDB.LoadUser(users);
                                    try {
                                       var addId = users.Find(x => x.id == id.Split(' ')[1]);
                                       ConnectDB.Query("insert into `Admin` (id, first_name) values ('" + id.Split(' ')[1] + "', '" + addId.first_name + "');");
                                       await client.SendTextMessageAsync(message.Chat.Id, "Пользователь " + addId.first_name + " (" + id.Split(' ')[1] + ") успешно добавлен в список администраторов");
                                    } catch {
                                       await client.SendTextMessageAsync(message.Chat.Id, "Пользователь отсутствует в базе бота");
                                    }
                                 }
                              } catch { }
                           }
                        } catch { }
                     }
                     else if (message.Text.Contains("/mailing")) {
                        try {
                           if (message.Text.Contains(" ")) {
                              string[] chanel = message.Text.Split(' ');
                              string chanelStr = string.Empty, notify = string.Empty;
                              if (chanel.Length > 1) {
                                 if (chanel[1] == "all") chanelStr = "all";
                                 else {
                                    int count = 1;
                                    ConnectDB.LoadChanel(chanels);
                                    for (int i = 0; i < chanel.Length; i++) {
                                       try {
                                          for (int j = 0; j < chanels.Count; j++) {
                                             if (chanels[j].id_chanel == chanel[i]) {
                                                long id = Convert.ToInt64(chanel[i]);
                                                chanelStr += chanel[i] + "‼";
                                                notify += count + ". " + chanel[i] + ";\n";
                                                count++;
                                                break;
                                             }
                                          }
                                       } catch { }
                                    }
                                    chanelStr = chanelStr.Trim('‼');
                                    notify = notify.Trim('\n').Trim(';') + ".";
                                 }
                                 if (chanelStr.Length > 0 && chanelStr != "" && chanelStr != string.Empty) {
                                    ConnectDB.Query("insert into `Mailing` (status, chanels) values ('" + message.Chat.Id + "‼progress', '" + chanelStr + "');");
                                    ConnectDB.LoadMailing(mailing);
                                    var mailId = mailing.Find(x => x.status.Split('‼')[0] == message.Chat.Id.ToString());
                                    if (chanelStr != "all") await client.SendTextMessageAsync(message.Chat.Id, "Рассылка будет выполнятся по базам следующих каналов (id):\n" + notify);
                                    else await client.SendTextMessageAsync(message.Chat.Id, "Рассылка будет выполнятся по всей базе бота");
                                    Mailing(message, mailId.id.ToString());
                                 }
                                 else await client.SendTextMessageAsync(message.Chat.Id, "Неверно введены id каналов (Пример: /mailing 123456 123456 123556 123546)");
                              }
                           }
                        } catch { }
                     }
                     else if (message.Text == "/chanels") {
                        ConnectDB.LoadChanel(chanels);
                        string chanel = string.Empty;
                        for (int i = 0; i < chanels.Count; i++) chanel += Convert.ToInt32(i + 1) + ". " + chanels[i].name + " (" + chanels[i].id_chanel + ");\n";
                        chanel = chanel.Trim('\n').Trim(';') + ".";
                        await client.SendTextMessageAsync(message.Chat.Id, "*Все каналы бота*\n\n" + chanel, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                     }
                  }
                  else {
                     if (message.Text.Contains("/mailing")) {
                        try {
                           if (message.Text.Contains(" ")) {
                              string[] chanel = message.Text.Split(' ');
                              string chanelStr = string.Empty, notify = string.Empty;
                              if (chanel.Length > 1) {
                                 int count = 1;
                                 ConnectDB.LoadChanel(chanels);
                                 for (int i = 0; i < chanel.Length; i++) {
                                    try {
                                       for (int j = 0; j < chanels.Count; j++) {
                                          if (chanels[j].id_user == message.Chat.Id.ToString() && chanels[j].id_chanel == chanel[i]) {
                                             long id = Convert.ToInt64(chanel[i]);
                                             chanelStr += chanel[i] + "‼";
                                             notify += count + ". " + chanel[i] + ";\n";
                                             count++;
                                             break;
                                          }
                                       }
                                    } catch { }
                                 }
                                 chanelStr = chanelStr.Trim('‼');
                                 notify = notify.Trim('\n').Trim(';') + ".";
                                 if (chanelStr.Length > 0 && chanelStr != "" && chanelStr != string.Empty) {
                                    ConnectDB.Query("insert into `Mailing` (status, chanels) values ('" + message.Chat.Id + "‼progress', '" + chanelStr + "');");
                                    ConnectDB.LoadMailing(mailing);
                                    var mailId = mailing.Find(x => x.status.Split('‼')[0] == message.Chat.Id.ToString());
                                    if (chanelStr != "all") await client.SendTextMessageAsync(message.Chat.Id, "Рассылка будет выполнятся по базам следующих каналов (id):\n" + notify);
                                    else await client.SendTextMessageAsync(message.Chat.Id, "Рассылка будет выполнятся по всей базе бота");
                                    Mailing(message, mailId.id.ToString());
                                 }
                                 else await client.SendTextMessageAsync(message.Chat.Id, "Неверно введены id каналов, либо не свои каналы (Пример: /mailing 123456 123456 123556 123546)");
                              }
                           }
                        } catch { }
                     }
                  }
               }
            } catch { }
         } catch { }
      }

      private static async void Mailing(Message message, string id)
      {
         try {
            var msg = await client.SendTextMessageAsync(message.Chat.Id, "*Текст*\n\nВведите текстовое содержимое сообщения рассылки", Telegram.Bot.Types.Enums.ParseMode.Markdown);
            int s = 0;
            while (true) {
               string text = string.Empty, media = string.Empty;
               var updates = await client.GetUpdatesAsync(s);
               for (int i = 0; i < updates.Length; i++) {
                  if (updates[i].Message.From.Id == message.Chat.Id) {
                     if (updates[i].Message.Type == Telegram.Bot.Types.Enums.MessageType.Text || updates[i].Message.Type == Telegram.Bot.Types.Enums.MessageType.Sticker) {
                        if (updates[i].Message.Text.Contains("/mailing") || updates[i].Message.Text == "/hello" || updates[i].Message.Text == "/stats" || updates[i].Message.Text == "/admins" || updates[i].Message.Text.Contains("/addadmin") || updates[i].Message.Text.Contains("/deladmin") || updates[i].Message.Text == "/chanels" || updates[i].Message.Text == "/mychanels") { ConnectDB.Query("delete from `Mailing` where id = " + id + ";"); await client.DeleteMessageAsync(message.Chat.Id, msg.MessageId); return; }
                        text = updates[i].Message.Text;
                        var keyborad = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Пропустить", "SkipMedia‼" + id) } });
                        try {
                           msg = await client.SendTextMessageAsync(message.Chat.Id, "*Медиа*\n\nВставьте медиа содержимое рассылки одним сообщением", Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: keyborad);
                        } catch { await client.SendTextMessageAsync(message.Chat.Id, "*Ошибка*\n\nДанный тип текста не поддерживается, возможно вы ввели недопустимые emoji.\nВведите текст еще раз.", Telegram.Bot.Types.Enums.ParseMode.Markdown); break; }
                        s++;
                        while (true) {
                           updates = await client.GetUpdatesAsync(s);
                           for (int j = 0; j < updates.Length; j++) {
                              try {

                                 if (updates[j].Message.Type == Telegram.Bot.Types.Enums.MessageType.Document || updates[j].Message.Type == Telegram.Bot.Types.Enums.MessageType.Audio || updates[j].Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo || updates[j].Message.Type == Telegram.Bot.Types.Enums.MessageType.Video) {
                                    ConnectDB.Query("update `Mailing` set status = '" + message.Chat.Id + "‼next' where id = " + id + ";");
                                    if (updates[j].Message.Photo != null) media += updates[j].Message.Type + "|" + updates[j].Message.Photo[updates[j].Message.Photo.Length - 1].FileId + "‼";
                                    else if (updates[j].Message.Video != null) media += updates[j].Message.Type + "|" + updates[j].Message.Video.FileId + "‼";
                                    else if (updates[j].Message.Document != null) media += updates[j].Message.Type + "|" + updates[j].Message.Document.FileId + "‼";
                                    else if (updates[j].Message.Audio != null) media += updates[j].Message.Type + "|" + updates[j].Message.Audio.FileId + "‼";
                                 }
                                 else if (updates[i].Message.Text.Contains("/mailing") || updates[i].Message.Text == "/hello" || updates[i].Message.Text == "/stats") return; if (updates[i].Message.Text.Contains("/mailing") || updates[i].Message.Text == "/hello" || updates[i].Message.Text == "/stats" || updates[i].Message.Text == "/admins" || updates[i].Message.Text.Contains("/addadmin") || updates[i].Message.Text.Contains("/deladmin") || updates[i].Message.Text == "/chanels" || updates[i].Message.Text == "/mychanels") { ConnectDB.Query("delete from `Mailing` where id = " + id + ";"); await client.DeleteMessageAsync(message.Chat.Id, msg.MessageId); return; }
                              } catch { }
                           }
                           media = media.Trim('‼');
                           ConnectDB.LoadMailing(mailing);
                           var checkMail = mailing.Find(x => x.status.Split('‼')[0] == message.Chat.Id.ToString());
                           if (checkMail.status.Split('‼')[1] == "next") {
                              await client.DeleteMessageAsync(message.Chat.Id, msg.MessageId);
                              var keyboradMailing = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Начать рассылку", "StartMailing‼" + id) }, new[] { InlineKeyboardButton.WithCallbackData("Отмена", "CancelMailing‼" + id) } });
                              if (media.Contains('‼')) {
                                 string[] medias = media.Split('‼');
                                 IAlbumInputMedia[] mediaGroup = GetMedia(medias, text);
                                 ConnectDB.Query("update `Mailing` set text = '" + text + "', media = '" + media + "', status = 'complete' where id = " + id + ";");
                                 await client.SendMediaGroupAsync(message.Chat.Id, mediaGroup);
                                 await client.SendTextMessageAsync(message.Chat.Id, "Сообщение рассылки сформировано", replyMarkup: keyboradMailing);
                              }
                              else if (media != string.Empty && media != "") {
                                 if (media.Split('|')[0] == "Photo") await client.SendPhotoAsync(message.Chat.Id, media.Split('|')[1], caption: text);
                                 if (media.Split('|')[0] == "Video") await client.SendVideoAsync(message.Chat.Id, media.Split('|')[1], caption: text);
                                 if (media.Split('|')[0] == "Document") await client.SendDocumentAsync(message.Chat.Id, media.Split('|')[1], caption: text);
                                 if (media.Split('|')[0] == "Audio") await client.SendAudioAsync(message.Chat.Id, media.Split('|')[1], caption: text);
                                 ConnectDB.Query("update `Mailing` set text = '" + text + "', media = '" + media + "', status = 'complete' where id = " + id + ";");
                                 await client.SendTextMessageAsync(message.Chat.Id, "Сообщение рассылки сформировано", replyMarkup: keyboradMailing);
                              }
                              else {
                                 await client.SendTextMessageAsync(message.Chat.Id, text, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                 ConnectDB.Query("update `Mailing` set text = '" + text + "', media = 'none', status = '" + message.Chat.Id + "‼complete' where id = " + id + ";");
                                 await client.SendTextMessageAsync(message.Chat.Id, "Сообщение рассылки сформировано", replyMarkup: keyboradMailing);
                              }
                              return;
                           }
                           s++;
                        }
                     }
                  }
               }
               s++;
            }
         } catch { }
      }

      private static async void InlineButtonOperation(object sc, CallbackQueryEventArgs ev)
      {
         var message = ev.CallbackQuery.Message;
         var data = ev.CallbackQuery.Data;
         if (data == "SetHello") {
            try {
               await client.SendTextMessageAsync(message.Chat.Id, "*Текст*\n\nВведите текстовое содержимое приветствия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
               int s = 0;
               while (true) {
                  string text = string.Empty, media = string.Empty;
                  var updates = await client.GetUpdatesAsync(s);
                  for (int i = 0; i < updates.Length; i++) {
                     if (updates[i].Message.From.Id == message.Chat.Id) {
                        if (updates[i].Message.Type == Telegram.Bot.Types.Enums.MessageType.Text || updates[i].Message.Type == Telegram.Bot.Types.Enums.MessageType.Sticker) {
                           if (updates[i].Message.Text.Contains("/mailing") || updates[i].Message.Text == "/hello" || updates[i].Message.Text == "/stats" || updates[i].Message.Text == "/admins" || updates[i].Message.Text.Contains("/addadmin") || updates[i].Message.Text.Contains("/deladmin") || updates[i].Message.Text == "/chanels" || updates[i].Message.Text == "/mychanels") return;
                           text = updates[i].Message.Text;
                           try {
                              await client.SendTextMessageAsync(message.Chat.Id, "*Приветствие установленно, приведено ниже*\n\n" + text, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                           } catch { await client.SendTextMessageAsync(message.Chat.Id, "*Ошибка*\n\nДанный тип текста не поддерживается, возможно вы ввели недопустимые emoji.\nВведите текст еще раз.", Telegram.Bot.Types.Enums.ParseMode.Markdown); break; }
                           ConnectDB.Query("update `Hello` set text = '" + text + "' where id = 1;");
                           return;
                        }
                     }
                  }
                  s++;
               }
            } catch { }
         }
         else if (data == "DelHello") {
            try {
               ConnectDB.Query("update `Hello` set text = 'none' where id = 1;");
               await client.SendTextMessageAsync(message.Chat.Id, "Приветствие удалено");
            } catch { }
         }
         else if (data.Contains("SkipMedia‼")) {
            ConnectDB.Query("update `Mailing` set status = '" + message.Chat.Id + "‼next' where id = " + data.Split('‼')[1] + ";");
         }
         else if (data.Contains("StartMailing‼")) {
            await client.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Процесс рассылки...");
            ConnectDB.LoadMailing(mailing);
            var mail = mailing.Find(x => x.id.ToString() == data.Split('‼')[1]);
            string[] chanel = new string[0];
            string[] user = new string[0];
            if (mail.chanels != "all") {
               chanel = mail.chanels.Split('‼');
               ConnectDB.LoadUser(users);
               for (int i = 0; i < chanel.Length; i++) {
                  for (int j = 0; j < users.Count; j++) {
                     if (users[j].id_chanel.Contains(chanel[i])) {
                        Array.Resize(ref user, user.Length + 1);
                        user[user.Length - 1] = users[j].id;
                     }
                  }
               }
            }
            int count = 0;
            int id = 0;
            for (int i = 0; i < mailing.Count; i++) if (mailing[i].id.ToString() == data.Split('‼')[1]) { id = i; break; }
            if (mailing[id].media.Contains('‼')) {
               string[] medias = mailing[id].media.Split('‼');
               IAlbumInputMedia[] mediaGroup = GetMedia(medias, mailing[id].text);
               if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendMediaGroupAsync(users[i].id, mediaGroup); Thread.Sleep(200); } catch { } }
               else for (int i = 0; i < user.Length; i++) { try { await client.SendMediaGroupAsync(user[i], mediaGroup); Thread.Sleep(200); } catch { } }
               await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
            }
            else if (mailing[id].media != string.Empty && mailing[id].media != "" && mailing[id].media != "none") {
               if (mailing[id].media.Split('|')[0] == "Photo") {
                  if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendPhotoAsync(users[i].id, mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  else for (int i = 0; i < user.Length; i++) { try { await client.SendPhotoAsync(user[i], mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
               }
               else if (mailing[id].media.Split('|')[0] == "Video") {
                  if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendVoiceAsync(users[i].id, mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  else for (int i = 0; i < user.Length; i++) { try { await client.SendVoiceAsync(user[i], mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
               }
               else if (mailing[id].media.Split('|')[0] == "Document") {
                  if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendDocumentAsync(users[i].id, mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  else for (int i = 0; i < user.Length; i++) { try { await client.SendDocumentAsync(user[i], mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
               }
               else if (mailing[id].media.Split('|')[0] == "Audio") {
                  if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendAudioAsync(users[i].id, mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  else for (int i = 0; i < user.Length; i++) { try { await client.SendAudioAsync(user[i], mailing[id].media.Split('|')[1], caption: mailing[id].text); Thread.Sleep(200); } catch { } }
                  await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
               }
            }
            else {
               ConnectDB.LoadUser(users);
               if (mail.chanels == "all") for (int i = 0; i < users.Count; i++) { try { await client.SendTextMessageAsync(users[i].id, mailing[id].text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown); count++; } catch { } }
               else for (int i = 0; i < user.Length; i++) { try { await client.SendTextMessageAsync(user[i], mailing[id].text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown); count++; } catch { } }
               await client.SendTextMessageAsync(message.Chat.Id, "Рассылка прошла успешно");
            }
            ConnectDB.Query("delete from `Mailing` where id = " + mail.id + ";");
            await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
         }
         else if (data.Contains("CancelMailing‼")) {
            ConnectDB.Query("delete from `Mailing` where id = " + data.Split('‼')[1]);
            await client.SendTextMessageAsync(message.Chat.Id, "Рассылка отменена");
            await client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
         }
      }

      private static IAlbumInputMedia[] GetMedia(string[] stringArray, string text)
      {
         var media = new IAlbumInputMedia[stringArray.Length];
         for (int i = 0; i < stringArray.Length; i++) {
            var mediaItem = new IAlbumInputMedia[1];
            if (i == 0) {
               if (stringArray[i].Split('|')[0] == "Photo") mediaItem[0] = new InputMediaPhoto(stringArray[i].Split('|')[1]) { Caption = text };
               else if (stringArray[i].Split('|')[0] == "Video") mediaItem[0] = new InputMediaVideo(stringArray[i].Split('|')[1]) { Caption = text };
               else if (stringArray[i].Split('|')[0] == "Document") mediaItem[0] = new InputMediaAudio(stringArray[i].Split('|')[1]) { Caption = text };
               else if (stringArray[i].Split('|')[0] == "Audio") mediaItem[0] = new InputMediaDocument(stringArray[i].Split('|')[1]) { Caption = text };
            }
            else {
               if (stringArray[i].Split('|')[0] == "Photo") mediaItem[0] = new InputMediaPhoto(stringArray[i].Split('|')[1]);
               else if (stringArray[i].Split('|')[0] == "Video") mediaItem[0] = new InputMediaVideo(stringArray[i].Split('|')[1]);
               else if (stringArray[i].Split('|')[0] == "Document") mediaItem[0] = new InputMediaAudio(stringArray[i].Split('|')[1]);
               else if (stringArray[i].Split('|')[0] == "Audio") mediaItem[0] = new InputMediaDocument(stringArray[i].Split('|')[1]);
            }
            media[i] = mediaItem[0];
         }
         return media;
      }

      public class Chat
      {
         public long id { get; set; }
      }
      public class From
      {
         public long id { get; set; }
         public string first_name { get; set; }
      }
      public class ChatJoinRequest
      {
         public Chat chat { get; set; }
         public From from { get; set; }
      }
      class Result
      {
         public ChatJoinRequest chat_join_request { get; set; }
      }
      class Update
      {
         public List<Result> result { get; set; }
      }

      private static async void UpdateData(object sender, UpdateEventArgs e)
      {
         try {
            var update = e.Update;
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Unknown) {
               string json = string.Empty;
               var bot = client.GetMeAsync();
               using (WebClient wc = new WebClient()) { json = wc.DownloadString("https://api.telegram.org/bot" + token + "/getUpdates"); }
               var up = JsonConvert.DeserializeObject<Update>(json);
               for (int i = 0; i < up.result.Count; i++) {
                  if (up.result[i].chat_join_request != null) {
                     ConnectDB.LoadUser(users);
                     try {
                        var find = users.Find(x => x.id == up.result[i].chat_join_request.from.id.ToString()).id;
                        WebRequest post = WebRequest.Create(@"https://api.telegram.org/bot" + token + "/approveChatJoinRequest?chat_id=" + up.result[i].chat_join_request.chat.id + "&user_id=" + up.result[i].chat_join_request.from.id);
                        post.Method = "POST";
                        post.GetResponse();
                        try {
                           var user = users.Find(x => x.id == up.result[i].chat_join_request.from.id.ToString());
                           string chanel = user.id_chanel;
                           if (!chanel.Contains(up.result[i].chat_join_request.chat.id.ToString())) {
                              if (chanel.Length > 0) {
                                 if (chanel != "none") chanel += "‼" + up.result[i].chat_join_request.chat.id.ToString();
                                 else chanel += up.result[i].chat_join_request.chat.id.ToString();
                              }
                              else chanel = up.result[i].chat_join_request.chat.id.ToString();
                              ConnectDB.Query("update `User` set id_chanel = '" + chanel + "' where id = " + user.id + ";");
                           }
                        } catch {
                           ConnectDB.Query("insert into `User` (id, first_name, id_chanel) values ('" + up.result[i].chat_join_request.from.id + "', '" + up.result[i].chat_join_request.from.first_name + "', '" + up.result[i].chat_join_request.chat.id + "');");
                           ConnectDB.LoadHello(hello);
                           if (hello[0].text != "none") {
                              await client.SendTextMessageAsync(up.result[i].chat_join_request.from.id, hello[0].text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                           }
                        }
                     } catch {
                        WebRequest post = WebRequest.Create(@"https://api.telegram.org/bot" + token + "/approveChatJoinRequest?chat_id=" + up.result[i].chat_join_request.chat.id + "&user_id=" + up.result[i].chat_join_request.from.id);
                        post.Method = "POST";
                        post.GetResponse();
                        ConnectDB.Query("insert into `User` (id, first_name, id_chanel) values ('" + up.result[i].chat_join_request.from.id + "', '" + up.result[i].chat_join_request.from.first_name + "', '" + up.result[i].chat_join_request.chat.id + "');");
                        ConnectDB.LoadHello(hello);
                        if (hello[0].text != "none") await client.SendTextMessageAsync(up.result[i].chat_join_request.from.id, hello[0].text, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        else await client.SendTextMessageAsync(up.result[i].chat_join_request.from.id, "Вы успешно добавлены в канал", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                     }
                  }
               }
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.MyChatMember && update.MyChatMember.NewChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Administrator) {
               try {
                  var me = await client.GetMeAsync();
                  if (update.MyChatMember.NewChatMember.User.Id == me.Id) {
                     ConnectDB.LoadUser(users);
                     try {
                        var user = users.Find(x => x.id == update.MyChatMember.From.Id.ToString());
                        if (user != null) {
                           WebRequest post = WebRequest.Create(@"https://api.telegram.org/bot" + token + "/createChatInviteLink?chat_id=" + update.MyChatMember.Chat.Id + "&creates_join_request=true&name=Бот");
                           post.Method = "POST";
                           post.GetResponse();
                           post = WebRequest.Create(@"https://api.telegram.org/bot" + token + "/exportChatInviteLink?chat_id=" + update.MyChatMember.Chat.Id + "&creates_join_request=true&name=Бот");
                           post.GetResponse();

                           ConnectDB.Query("insert into `Chanel` (id_chanel, name, id_user) values ('" + update.MyChatMember.Chat.Id + "', '" + update.MyChatMember.Chat.Title + "', '" + update.MyChatMember.From.Id.ToString() + "');");
                        }
                     } catch { }
                  }
               } catch { }
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.MyChatMember && update.MyChatMember.NewChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Kicked || update.Type == Telegram.Bot.Types.Enums.UpdateType.MyChatMember && update.MyChatMember.NewChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Left) {
               try {
                  if (update.MyChatMember.NewChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Left) ConnectDB.Query("delete from `Chanel` where id_chanel = '" + update.MyChatMember.Chat.Id + "';");
                  else if (update.MyChatMember.NewChatMember.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Kicked) ConnectDB.Query("delete from `Chanel` where id_chanel = '" + update.MyChatMember.Chat.Id + "' and id_user = '" + update.MyChatMember.From.Id + "';");
                  ConnectDB.LoadUser(users);
                  string request = string.Empty;
                  for (int i = 0; i < users.Count; i++) {
                     if (users[i].id_chanel.Contains(update.MyChatMember.Chat.Id.ToString())) {
                        if (users[i].id_chanel.Contains("‼")) {
                           string[] chanel = users[i].id_chanel.Split('‼');
                           string newChanel = string.Empty;
                           for (int j = 0; j < chanel.Length; j++) if (chanel[j] != update.MyChatMember.Chat.Id.ToString()) newChanel += chanel[j] + "‼";
                           newChanel = newChanel.Trim('‼');
                           request = "update `User` set id_chanel = '" + newChanel + "' where id = " + users[i].id + ";\n";
                        }
                        else {
                           request = "update `User` set id_chanel = 'none' where id = " + users[i].id + ";\n";
                        }
                     }
                  }
                  ConnectDB.Query(request);
               } catch { }
            }
         } catch { }
      }
   }
}
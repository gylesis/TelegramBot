using System;
using System.Collections.Generic;
using FireSharp;
using FireSharp.Config;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        private const string SetPurpose = "set_purpose";
        private const string AskCar = "ask_car";
        private const string SetCarId = "set_carId";
        private const string SetCarMark = "set_carMark";
        private const string SetFullName = "set_name";
        private const string SetListOfNames = "set_names";
        private const string SetOrg = "set_org";
        private const string SetConfirmation = "confirmation";
        private const string SetPassport = "set_passport";
        
        private const string FirebasePath = "https://stadion-hack-default-rtdb.firebaseio.com/";
        private const string FirebaseSecret = "EzO76LVyT4oGKWX38t6De7f6qO3ztTNUDXxXAHOW";
        
        private static string _botToken = "1939408307:AAHblmXPJzjcqpY5Xp0wWK1ZcVjME6ViS8c";

        private static TelegramBotClient _bot;

        private static FirebaseClient _firebaseClient;


        private enum RegState
        {
            Start,
            Name,
            Org,
            Names,
            AskCar,
            SetCarId,
            SetCarMark,
            Purpose,
            Passport,
            Confirmation,
        }

        private static RegState _state;
        private static int _step;
        private static int _lastMessageId;
        private static UserData _userData;


        static void Main(string[] args)
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = FirebaseSecret,
                BasePath = FirebasePath
            };

            _lastMessageId = -1;
            
            _firebaseClient = new FirebaseClient(config);
            _bot = new TelegramBotClient(_botToken);

            _bot.OnMessage += BotMessage;
            _bot.OnCallbackQuery += CallbackQuery;

            _bot.StartReceiving();

            Console.WriteLine("_____________Started!___________");
            Console.ReadKey();

            _bot.StopReceiving();
        }

        static async void BotMessage(object sender, MessageEventArgs data)
        {
            var message = data.Message;
            
            if (message.Text == null) return;

            var chatId = message.Chat.Id;

            switch (message.Text)
            {
                case "/start":
                    _state = RegState.Start;
                    await _bot.SendTextMessageAsync(chatId, "Здравствуйте!");
                    CheckStatus(data);
                    break;
                default:
                    CheckStatus(data);
                    break;
            }
        }

        private static async void CheckStatus(MessageEventArgs data)
        {
            var message = data.Message;

            var chatId = message.Chat.Id;

            var txtInput = message.Text;
            
            switch (_state)
            {
                case RegState.Start:
                    await _bot.SendTextMessageAsync(chatId,"Хотите оставить заявку чтобы попасть на стадион?", replyMarkup: GetName());
                    break;

                case RegState.Name:
                    _userData = new UserData();
                    _userData.Names = new List<string>();
                    
                    _userData.forwho = txtInput;

                    ChangeState(RegState.Org);
                    await _bot.SendTextMessageAsync(chatId, $"Введите организацию, в которую пребываете");
                    break;

                case RegState.Org:
                    _userData.organisation = txtInput;
                            
                    ChangeState(RegState.AskCar);
                    await _bot.SendTextMessageAsync(chatId,"Записать еще людей которые будут с вами?", replyMarkup:GetNames());
                    break;

                case RegState.Names:
                    _userData.Names.Add(txtInput);

                    await _bot.SendTextMessageAsync(chatId,"Добавить еще?", replyMarkup: SetNames());
                    break;
                        
                case RegState.AskCar:
                            
                    break;

                case RegState.SetCarMark:
                    _userData.CarMark = txtInput;
                    
                    ChangeState(RegState.SetCarId);
                    await _bot.SendTextMessageAsync(chatId,"Введите номер машины");
                    
                    break;
                case RegState.SetCarId:
                    _userData.CarID = txtInput;
                    
                    ChangeState(RegState.Purpose);
                    await _bot.SendTextMessageAsync(chatId,"Цель визита");
                    break;

                case RegState.Purpose:
                    _userData.purpose = txtInput;  
                    
                    ChangeState(RegState.Passport);
                    await _bot.SendTextMessageAsync(chatId,"Введите серию и номер паспорта");
                    
                    break;

                case RegState.Passport:

                    _userData.passport = txtInput;
                    
                    ChangeState(RegState.Confirmation);

                    await _bot.SendTextMessageAsync(chatId, $"*Проверьте правильность данных* \n {_userData}",
                        replyMarkup: Confirmation(),parseMode: ParseMode.Markdown);
                    break;
            }
        }

        private static void CallbackQuery(object? sender, CallbackQueryEventArgs data)
        {
            var message = data.CallbackQuery.Message;

            if (message.Text == null) return;

            if (_lastMessageId != -1)
            {
                if (message.MessageId == _lastMessageId) return;
            }

            var chatId = message.Chat.Id;
            var queryData = data.CallbackQuery.Data;

            switch (queryData)
            {
                case SetFullName:
                    ChangeState(RegState.Name);
                    _bot.SendTextMessageAsync(chatId, $"Введите ФИО");
                    break;

                case SetOrg:
                    ChangeState(RegState.Org);
                    _bot.SendTextMessageAsync(chatId, $"Введите организацию для посещения");
                    break;
                
                case SetListOfNames:
                    
                    ChangeState(RegState.Names);
                    _bot.SendTextMessageAsync(chatId, $"Введите еще ФИО");
                    break;

                case AskCar:
                    _bot.SendTextMessageAsync(chatId, $"Есть ли у вас машина?", replyMarkup:GetCar());
                    break;

                case SetCarMark:
                    ChangeState(RegState.SetCarMark);
                    _bot.SendTextMessageAsync(chatId, $"Введите марку машины");
                    break;
                
                case SetCarId:
                    ChangeState(RegState.SetCarId);
                    _bot.SendTextMessageAsync(chatId, $"Введите номер машины");
                    break;

                case SetPurpose:
                    ChangeState(RegState.Purpose);
                     _bot.SendTextMessageAsync(chatId,"Цель визита");
                    break;
                
                case SetPassport:
                    ChangeState(RegState.Passport);
                  //  _bot.SendTextMessageAsync(chatId, $"Введите причину посещения стадиона");
                    break;
                
                case SetConfirmation:
                    ChangeState(RegState.Confirmation);
                    
                    //_firebaseClient.PushTaskAsync("request_person", _userData);
                    
                    _bot.SendTextMessageAsync(chatId, "Вы успешно отправили заявку!");
                    break;
            }

            _lastMessageId = message.MessageId;
        }

        private static IReplyMarkup GetNames()
        {
            var agreeButton = new InlineKeyboardButton {Text = "Ввести", CallbackData = SetListOfNames};
            var disagreeButton = new InlineKeyboardButton {Text = "Дальше", CallbackData = AskCar};

            return new InlineKeyboardMarkup(new[] {agreeButton, disagreeButton});
        }

        private static IReplyMarkup GetCar()
        {
            var agreeButton = new InlineKeyboardButton {Text = "Да", CallbackData = SetCarMark};
            var disagreeButton = new InlineKeyboardButton {Text = "Нет", CallbackData = SetPurpose};

            return new InlineKeyboardMarkup(new[] {agreeButton, disagreeButton});
        }

        private static IReplyMarkup GetName()
        {
            var button = new InlineKeyboardButton {Text = "Оставить заявку", CallbackData = SetFullName};
            return new InlineKeyboardMarkup(button);
        }

        private static IReplyMarkup SetNames()
        {
            var agreeButton = new InlineKeyboardButton {Text = "Продолжить дальше", CallbackData = AskCar};
            var disagreeButton = new InlineKeyboardButton {Text = "Добавить", CallbackData = SetListOfNames};

            return new InlineKeyboardMarkup(new[] {disagreeButton, agreeButton});
        }

        private static IReplyMarkup Confirmation()
        {
            var agreeButton = new InlineKeyboardButton {Text = "Завершить регистрацию", CallbackData = SetConfirmation};
            var disagreeButton = new InlineKeyboardButton {Text = "Изменить", CallbackData = SetFullName};

            return new InlineKeyboardMarkup(new[] {agreeButton, disagreeButton});
        }

        private static void ChangeState(RegState status) =>
            _state = status;
    }
}
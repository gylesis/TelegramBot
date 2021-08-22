using System;
using System.Collections.Generic;
using System.Numerics;

namespace TelegramBot
{
    public class UserData
    {
        public string forwho;
        public string purpose;
        public string organisation;

        public bool confirmed_director;
        public bool confirmed_head;
        public bool dangerous;

        public string email;
        public string role = "default-user";
        public string password;
        
        public string passport;
        
        public string CarID = "";
        public string CarMark = "";
        
        public List<string> Names;

        public void AddName(string str)
        {
            Names.Add(str);
        }

        public override string ToString()
        {
            var name = $"ФИО : {forwho}";
            var organisation = $"Организация : {this.organisation}";

            var names = "Список лиц с вами : ";

            if (Names.Count == 0)
            {
                names += "-";
            }
            else
            {
                names += $"({Names.Count})";
                
                foreach (var str in Names)
                {
                    names += $" \n - {str}  ";
                }
            }

            var carInfo = "Машина : Нет";
            
            if (CarMark != String.Empty && CarID != String.Empty)
            {
                carInfo = $"Машина : Да \n  Марка : {CarMark} \n  Номер : {CarID}";
            }

            var purpose = $"Цель визита : {this.purpose}";

            var passport = $"Паспорт : {this.passport} ";

            var value = $"{name} \n {organisation} \n {names} \n {carInfo} \n {purpose} \n {passport} ";
            
            return value;
        }
    }

    public class CarInfo
    {
        public string ID;
        public string Mark;
    }
    
}
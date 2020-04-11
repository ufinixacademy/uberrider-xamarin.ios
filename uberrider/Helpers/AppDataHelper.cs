using System;
using Foundation;
using uberrider.DataModels;

namespace uberrider.Helpers
{
    public static class AppDataHelper
    {
        public static PlaceAttribute thisPlace;

        public static string GetFullName()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            string fullname = userDefaults.StringForKey("fullname");
            return fullname;
        }

        public static string GetEmail()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            string email = userDefaults.StringForKey("email");
            return email;
        }

        public static string GetUserID()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            string userid = userDefaults.StringForKey("user_id");
            return userid;

        }
        public static string GetPhone()
        {
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            string phone = userDefaults.StringForKey("phone");
            return phone;
        }

    }
}

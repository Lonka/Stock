using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Stock
{
    public class IniHelper
    {

        public static bool GetProfileBool(string iniFile, string section, string key, bool defaultValue)
        {
            string value = GetProfileString(iniFile, section, key, "").Trim();
            bool result = false;
            if (value == "1" || string.Compare(value, "true", true) == 0 || string.Compare(value, "on", true) == 0)
            {
                result = true;
            }
            else if (value == "0" || string.Compare(value, "false", true) == 0 || string.Compare(value, "off", true) == 0)
            {
                result = false;
            }
            else
            {
                result = defaultValue;
            }
            return result;
        }

        /// <summary>
        /// 取得Setting.ini的Enum資料
        /// </summary>
        /// <param name="enumType">要取回Enum的Type</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static object GetProfileEnum(Type enumType, string iniFile, string section, string key, string defaultValue)
        {
            object result = null;
            string profileValue = GetProfileString(iniFile, section, key, defaultValue);
            try
            {
                result = Enum.Parse(enumType, profileValue);
            }
            catch
            {
                result = Enum.Parse(enumType, defaultValue);
            }
            return result;
        }

        /// <summary>
        /// 取得INI的Int資料
        /// </summary>
        /// <param name="iniFile">INI檔的Path</param>
        /// <param name="section">INI檔的Section</param>
        /// <param name="key">INI檔中Section的Key</param>
        /// <param name="defaultValue">取不到值的預設值</param>
        /// <returns></returns>
        public static int GetProfileInt(string iniFile, string section, string key, int defaultValue)
        {
            string value = GetProfileString(iniFile, section, key, "");
            int result = int.MinValue;
            if (!int.TryParse(value, out result))
            {
                result = defaultValue;
            }
            return result;
        }

        /// <summary>
        /// 取得INI的String資料
        /// </summary>
        /// <param name="iniFile">INI檔的Path</param>
        /// <param name="section">INI檔的Section</param>
        /// <param name="key">INI檔中Section的Key</param>
        /// <param name="defaultValue">取不到值的預設值</param>
        /// <returns></returns>
        public static string GetProfileString(string iniFile, string section, string key, string defaultValue)
        {
            StringBuilder result = new StringBuilder(512);
            GetPrivateProfileString(section, key, defaultValue, result, 512, iniFile);
            return (result.ToString());
        }

        public static bool WriteProfileSection(string iniFile, string section, string value)
        {
            bool result = false;
            if (WritePrivateProfileSection(section, value, iniFile) > 0)
                result = true;
            return (result);
        }

        public static bool WriteProfileString(string iniFile, string section, string key, string value)
        {
            bool result = false;
            if (WritePrivateProfileString(section, key, value, iniFile) > 0)
                result = true;
            return (result);
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateProfileString(string appName,
                                                                     string keyName, string defaultString,
                                                                     StringBuilder returnedString, int size,
                                                                     string fileName);

        [DllImport("KERNEL32.DLL")]
        private static extern long WritePrivateProfileSection(string section,
                                                              string value,
                                                              string fileName);

        [DllImport("KERNEL32.DLL")]
        private static extern long WritePrivateProfileString(string section,
                                                             string key,
                                                             string value,
                                                             string fileName);
    }
}
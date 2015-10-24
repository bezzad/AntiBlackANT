using System;
using System.Collections.Generic;
using System.Text;
using Registry = SolRegistry.RegistryHelper;
using System.Resources;

namespace Security
{
    public static class AppDataManager
    {
        public static event EventHandler<EventArguments.ReportEventArgs> Reporter = delegate { };

        public static readonly string PasswordResourceName = "SH512_ProtectedKey";
        public static readonly string KeyName = "HashKeyCode";
        public static readonly string AppName = AppDomain.CurrentDomain.FriendlyName
            .Substring(0, AppDomain.CurrentDomain.FriendlyName.IndexOf("."));
        //
        // HKEY_LOCAL_MACHINE\Software as Behzad.Kh\AppName
        public static readonly string RegistryPath = @"Software\Behzad.Kh\Black ANT";

        /// <summary>
        /// Get Last changed password from registry and local resources...
        /// </summary>
        /// <returns>SHA512 Hash Password</returns>
        public static string GetHashPassword
        {
            get
            {
                string HASHPASS = "";

                try
                {
                    // Read a Registry
                    // Retrieve Data from at given path and Key
                    HASHPASS = Registry.Read<String>(RegistryPath, KeyName);
                }
                catch { }
                finally
                {
                    if (string.IsNullOrEmpty(HASHPASS))
                    {
                        // read --> global::AppName.Properties.Resources.SH512_ProtectedKey
                        global::System.Resources.ResourceManager rm =
                            new ResourceManager(AppName.Replace(' ', '_') + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

                        HASHPASS = rm.GetString(PasswordResourceName);
                    }
                }

                return HASHPASS;
            }
        }

        public static string GetMainHashPassword
        {
            get
            {
                string HASHPASS = "";

                try
                {
                    // read --> global::AppName.Properties.Resources.SH512_ProtectedKey
                    global::System.Resources.ResourceManager rm =
                        new ResourceManager(AppName.Replace(' ', '_') + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

                    HASHPASS = rm.GetString(PasswordResourceName);
                }
                catch { }

                return HASHPASS;
            }
        }

        /// <summary>
        /// Set New Hash Password for this app in Registry for next fetch...
        /// </summary>
        /// <param name="NewSHA512HashPass">New SHA512 Hash Password</param>
        /// <returns>Can changing to new hash pass in registry?</returns>
        public static bool SetHashPassword(string NewSHA512HashPass)
        {
            try
            {
                // Write a Registry
                // Create a new Key Under HKEY_LOCAL_MACHINE\Software as AppName
                Registry.Write(RegistryPath, KeyName, NewSHA512HashPass);
            }
            catch(Exception ex)
            {
                Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SetHashPassword", ex));
                return false;
            }

            Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SetHashPassword", "Hash Password Changed Successfully"));
            return true;
        }

        /// <summary>
        /// Delete any registry hash password for this app, 
        /// So just we have one pass that too in Resources...
        /// </summary>
        /// <returns>Can delete all Registry hash password from this app ?</returns>
        public static bool SetPasswordToDefault()
        {
            Boolean Flag = false;

            try
            {
                //// Delete Value of given Key
                Flag = Registry.Delete(RegistryPath, KeyName);
            }
            catch (Exception ex)
            {
                Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SetPasswordToDefault", ex));
                return false;
            }

            Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SetPasswordToDefault", "Hash Password Changed Successfully"));
            return Flag;
        }

        /// <summary>
        /// Save your data object in Registry by create a new Key Under HKEY_LOCAL_MACHINE\Software as AppName
        /// </summary>
        /// <param name="DataValue">Your Data Object Value</param>
        /// <param name="Key">Save Key in Registry</param>
        /// <returns>Get able to save your data or not?</returns>
        public static bool SaveData(object DataValue, string Key)
        {
            try
            {
                // Write a Registry
                // Create a new Key Under HKEY_LOCAL_MACHINE\Software as AppName
                Registry.Write(RegistryPath, Key, DataValue);
            }
            catch (Exception ex)
            {
                Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SaveData", ex));
                return false;
            }

            Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SaveData", "Data Saved Successfully"));
            return true;
        }

        public static bool SaveData(string DataValue, string Key, string Password)
        {
            try
            {
                // Write a Registry
                // Create a new Key Under HKEY_LOCAL_MACHINE\Software as AppName
                Registry.Write(RegistryPath, Key, DataValue.Encrypt(Password, true));
            }
            catch (Exception ex)
            {
                Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SaveData", ex));
                return false;
            }

            Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.SaveData", "Data Saved Successfully"));
            return true;
        }
        
        /// <summary>
        /// Get saved data object value from registry by your data key under HKEY_LOCAL_MACHINE\Software as AppName
        /// </summary>
        /// <param name="Key">Registry key, saved by that names.</param>
        /// <returns>your saved data object value</returns>
        public static object ReadData(string Key)
        {
            string DataValue = "";
            try
            {
                // Read a Registry
                // Retrieve Data from at given path and Key
                DataValue = Registry.Read<String>(RegistryPath, Key);
            }
            catch { }
            finally
            {
                if (string.IsNullOrEmpty(DataValue) && !string.IsNullOrEmpty(Key))
                {
                    // read --> global::AppName.Properties.Resources.SH512_ProtectedKey
                    global::System.Resources.ResourceManager rm =
                        new ResourceManager(AppName.Replace(' ', '_') + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

                    DataValue = rm.GetString(Key);
                }
            }

            return DataValue;
        }

        /// <summary>
        /// Get saved data object value from registry by your data key under HKEY_LOCAL_MACHINE\Software as AppName
        /// </summary>
        /// <param name="Key">Registry key, saved by that names.</param>
        /// <returns>your saved data object value</returns>
        public static string ReadData(string Key, string Password)
        {
            string DataValue = "";
            try
            {
                // Read a Registry
                // Retrieve Data from at given path and Key
                DataValue = Registry.Read<String>(RegistryPath, Key);
                DataValue = DataValue.Decrypt(Password, true);
            }
            catch { }
            finally
            {
                if (string.IsNullOrEmpty(DataValue) && !string.IsNullOrEmpty(Key))
                {
                    // read --> global::AppName.Properties.Resources.SH512_ProtectedKey
                    global::System.Resources.ResourceManager rm =
                        new ResourceManager(AppName.Replace(' ', '_') + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

                    DataValue = rm.GetString(Key);
                    DataValue = DataValue.Decrypt(Password, true);
                }
            }

            return DataValue;
        }

        /// <summary>
        /// Delete any data by your key name in registry path: 
        /// HKEY_LOCAL_MACHINE\Software as AppName
        /// </summary>
        /// <param name="Key">Target key name for delete</param>
        /// <returns>Can to delete registry data</returns>
        public static bool DeleteData(string Key)
        {
            Boolean Flag = false;

            try
            {
                //// Delete Value of given Key
                Flag = Registry.Delete(RegistryPath, Key);
            }
            catch (Exception ex)
            {
                Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.DeleteData", ex));
                return false;
            }

            Reporter("AppDataManager", new EventArguments.ReportEventArgs("AppDataManager.DeleteData", "Data Deleted Successfully"));
            return Flag;
        }
    }
}

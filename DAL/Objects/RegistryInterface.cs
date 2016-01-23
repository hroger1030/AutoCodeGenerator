using Microsoft.Win32;

namespace DAL
{
    /// <summary>
    /// The RegistryInterface object allows reading and writing of windows registry objects.
    /// </summary>
    public class RegistryInterface
    {
        #region Fields

            private static RegistryKey _RegistryKey;
            private static string _SubKey;

        #endregion

        #region Methods

            public RegistryInterface(string subkey_name)
            {
                _RegistryKey = Registry.CurrentUser;
                _SubKey = subkey_name;
            }

            public string Read(string key_name)
            {
                RegistryKey sub_key = _RegistryKey.OpenSubKey(_SubKey);

                if (sub_key == null)
                    return null;
                else
                    return (string)sub_key.GetValue(key_name.ToLower());
            }

            public void Write(string key_name, object key_value)
            {
                RegistryKey sub_key = _RegistryKey.CreateSubKey(_SubKey);

                sub_key.SetValue(key_name.ToLower(), key_value);
            }

            public void DeleteKey(string key_name)
            {
                RegistryKey sub_key = _RegistryKey.CreateSubKey(_SubKey);

                if (sub_key != null)
                    sub_key.DeleteValue(key_name);
            }

            public void DeleteSubKeyTree()
            {
                RegistryKey sub_key = _RegistryKey.OpenSubKey(_SubKey);

                if (sub_key != null)
                    _RegistryKey.DeleteSubKeyTree(_SubKey);
            }

            public int SubKeyCount()
            {
                RegistryKey sub_key = _RegistryKey.OpenSubKey(_SubKey);

                if (sub_key == null)
                    return 0;
                else
                    return sub_key.SubKeyCount;
            }

            public int ValueCount()
            {
                RegistryKey sub_key = _RegistryKey.OpenSubKey(_SubKey);

                if (sub_key == null)
                    return 0;
                else
                     return sub_key.ValueCount;
            }

        #endregion
    }
}


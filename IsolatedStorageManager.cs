using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Reflection;

namespace CommonLibSL
{
    public abstract class IsolatedStorageManager<T> : INotifyPropertyChanged where T : IsolatedStorageManager<T>, new()
    {
        public readonly IsolatedStorageSettings LocalStorageSettings = IsolatedStorageSettings.SiteSettings;
        private readonly string _saveKey;
        private static readonly object lockobj = new object();
        private static int level = 0;
        private static bool nosave = false;
        private static volatile T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.Refresh();
                }

                return _instance;
            }
        }

        protected IsolatedStorageManager(string prefix)
        {
            _saveKey = prefix + "_" + typeof(T).Name + "_saveKey";
        }

        public void Refresh()
        {
            if (LocalStorageSettings.Contains(_saveKey))
            {
                try
                {
                    string jsonStr = LocalStorageSettings[_saveKey]?.ToString();
                    if (jsonStr != null)
                    {
                        nosave = true;
                        T tmp = JsonConvert.DeserializeObject<T>(jsonStr);
                        var props = typeof(T).GetProperties();
                        foreach (PropertyInfo info in props)
                        {
                            if (info.Name != "Instance")
                            {
                                info.SetValue(this, info.GetValue(tmp, null), null);
                            }
                        }
                    }
                }
                catch { }
                nosave = false;
            }
            level = 0;
        }

        public bool Save()
        {
            lock (LocalStorageSettings)
            {
                try
                {
                    string jsonStr = JsonConvert.SerializeObject(Instance);
                    LocalStorageSettings[_saveKey] = jsonStr;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool Clear()
        {
            lock (LocalStorageSettings)
            {
                if (LocalStorageSettings.Contains(_saveKey))
                {
                    LocalStorageSettings.Remove(_saveKey);
                }
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

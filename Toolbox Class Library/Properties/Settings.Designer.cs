using System.ComponentModel;

namespace Toolbox_Class_Library.Properties
{
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase, INotifyPropertyChanged
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int TypingSpeed
        {
            get { return ((int)(this["TypingSpeed"])); }
            set { this["TypingSpeed"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int BlitzImportSpeed
        {
            get { return ((int)(this["BlitzImportSpeed"])); }
            set { this["BlitzImportSpeed"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Josh")]
        public string Username
        {
            get { return ((string)(this["Username"])); }
            set { this["Username"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ReverseImport
        {
            get { return ((bool)(this["ReverseImport"])); }
            set
            {
                if ((bool)this["ReverseImport"] != value)
                {
                    this["ReverseImport"] = value;
                    OnPropertyChanged(nameof(ReverseImport)); // Notify the UI when property changes
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                // Log the exception or show a message box for debugging
                Console.WriteLine($"Error raising PropertyChanged: {ex.Message}");
            }
        }
    }
}

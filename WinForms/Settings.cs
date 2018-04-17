  public static class SettingsM
    {
        //pobieranie ustawien widoku przy otwarciu formularza
        public static void loadSettings(Form f)
        {
            if (f.WindowState == FormWindowState.Minimized)
            {
                f.WindowState = FormWindowState.Normal;
            }
            if ((Settings.Default.FormSize.Width + Settings.Default.FormSize.Height) > 0)
            {
                f.ClientSize = Settings.Default.FormSize;
            }
            f.Location = Settings.Default.FormPoint;
        }
        //zapis aktualnych ustawień
        public static void closeSettings(Form f)
        {
            if (f.WindowState == FormWindowState.Normal)
            {
                Settings.Default.FormSize = f.ClientSize;
                Settings.Default.FormPoint = f.Location;
            }
            else
            {
                Settings.Default.FormSize = f.RestoreBounds.Size;
                Settings.Default.FormPoint = f.RestoreBounds.Location;
            }
            Settings.Default.Save();
        }
    }

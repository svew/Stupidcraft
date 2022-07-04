using System;
using System.IO;
using System.Net;
using System.Reflection;
using Gtk;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views
{
    public class LoginView : VBox
    {
        private LauncherWindow _window;

        public Entry UsernameText { get; set; }
        public Entry PasswordText { get; set; }
        public Button LogInButton { get; set; }
        public Button RegisterButton { get; set; }
        public Button OfflineButton { get; set; }
        public Image TrueCraftLogoImage { get; set; }
        public Label ErrorLabel { get; set; }
        public CheckButton RememberCheckBox { get; set; }

        public LoginView(LauncherWindow window)
        {
            _window = window;
            this.SetSizeRequest(250, -1);

            ErrorLabel = new Label("Username or password incorrect")
            {
               // TODO TextColor = Color.FromBytes(255, 0, 0),
               Justify = Justification.Center,
               Visible = false
            };
            UsernameText = new Entry();
            PasswordText = new Entry();
            PasswordText.Visibility = false;
            PasswordText.InputPurpose = InputPurpose.Password;
            LogInButton = new Button("Log In");
            RegisterButton = new Button("Register");
            OfflineButton = new Button("Play Offline");
            RememberCheckBox = new CheckButton("Remember Me");
            UsernameText.Text = UserSettings.Local.Username;
            if (UserSettings.Local.AutoLogin)
            {
                PasswordText.Text = UserSettings.Local.Password;
                RememberCheckBox.Active = true;
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrueCraft.Launcher.Content.truecraft_logo.png"))
                TrueCraftLogoImage = new Image(new Gdk.Pixbuf(stream, 350, 75));

            UsernameText.PlaceholderText = "Username";
            PasswordText.PlaceholderText = "Password";
            // TODO: restore this functionality.
            //PasswordText.KeyReleaseEvent += (sender, e) =>
            //{
            //    if (e.Key == Key.Return || e.Key == Key.NumPadEnter)
            //        LogInButton_Clicked(sender, e);
            //};
            //UsernameText.KeyReleaseEvent += (sender, e) =>
            //{
            //    if (e.Key == Key.Return || e.Key == Key.NumPadEnter)
            //        LogInButton_Clicked(sender, e);
            //};
            RegisterButton.Clicked += (sender, e) =>
            {
                _window.WebView.Text = "https://truecraft.io/register";
            };
            OfflineButton.Clicked += (sender, e) =>
            {
                _window.User.Username = UsernameText.Text;
                _window.User.SessionId = "-";
                _window.ShowMainMenuView();
            };
            var regoffbox = new HBox();
            regoffbox.PackStart(RegisterButton, true, false, 0);
            regoffbox.PackStart(OfflineButton, true, false, 0);
            LogInButton.Clicked += LogInButton_Clicked;

            this.PackEnd(regoffbox, true, false, 0);
            this.PackEnd(LogInButton, true, false, 0);
            this.PackEnd(RememberCheckBox, true, false, 0);
            this.PackEnd(PasswordText, true, false, 0);
            this.PackEnd(UsernameText, true, false, 0);
            this.PackEnd(ErrorLabel, true, false, 0);
        }

        private void DisableForm()
        {
            UsernameText.Sensitive = PasswordText.Sensitive = LogInButton.Sensitive =
                RegisterButton.Sensitive = OfflineButton.Sensitive = false;
        }

        private void EnableForm()
        {
            UsernameText.Sensitive = PasswordText.Sensitive = LogInButton.Sensitive =
                RegisterButton.Sensitive = OfflineButton.Sensitive = true;
        }

        private class LogInAsyncState
        {
            public LogInAsyncState(HttpWebRequest request, string username, string password)
            {
                Request = request;
                Username = username;
                Password = password;
            }

            public HttpWebRequest Request { get; }
            public string Username { get; }
            public string Password { get; }
        }

        private void LogInButton_Clicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameText.Text) || string.IsNullOrEmpty(PasswordText.Text))
            {
                ErrorLabel.Text = "Username and password are required";
                ErrorLabel.Visible = true;
                return;
            }
            ErrorLabel.Visible = false;
            DisableForm();

            _window.User.Username = UsernameText.Text;
            var request = WebRequest.CreateHttp(TrueCraftUser.AuthServer + "/api/login");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = false;
            request.BeginGetRequestStream(HandleLoginRequestReady, new LogInAsyncState(
                request, _window.User.Username, PasswordText.Text));
        }

        private void HandleLoginRequestReady(IAsyncResult asyncResult)
        {
            try
            {
                LogInAsyncState state = (LogInAsyncState)asyncResult.AsyncState!;
                HttpWebRequest request = state.Request;
                Stream requestStream = request.EndGetRequestStream(asyncResult);
                using (var writer = new StreamWriter(requestStream))
                    writer.Write(string.Format("user={0}&password={1}&version=12", state.Username, state.Password));
                request.BeginGetResponse(HandleLoginResponse, request);
            }
            catch
            {
                Application.Invoke((sender, e) =>
                {
                    EnableForm();
                    ErrorLabel.Text = "Unable to log in";
                    ErrorLabel.Visible = true;
                    RegisterButton.Label = "Offline Mode";
                });
            }
        }

        private void HandleLoginResponse(IAsyncResult asyncResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asyncResult.AsyncState!;
                WebResponse response = request.EndGetResponse(asyncResult);
                string session;
                using (var reader = new StreamReader(response.GetResponseStream()))
                    session = reader.ReadToEnd();
                if (session.Contains(":"))
                {
                    var parts = session.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    Application.Invoke((sender, e) =>
                    {
                        _window.User.Username = parts[2];
                        _window.User.SessionId = parts[3];
                        EnableForm();
                        _window.ShowMainMenuView();
                        UserSettings.Local.AutoLogin = RememberCheckBox.Active;
                        UserSettings.Local.Username = _window.User.Username;
                        if (UserSettings.Local.AutoLogin)
                            UserSettings.Local.Password = PasswordText.Text;
                        else
                            UserSettings.Local.Password = string.Empty;
                        UserSettings.Local.Save();
                    });
                }
                else
                {
                    Application.Invoke((sender, e) =>
                    {
                        EnableForm();
                        ErrorLabel.Text = session;
                        ErrorLabel.Visible = true;
                        RegisterButton.Label = "Offline Mode";
                    });
                }
            }
            catch
            {
                Application.Invoke((sender, e) =>
                {
                    EnableForm();
                    ErrorLabel.Text = "Unable to log in.";
                    ErrorLabel.Visible = true;
                    RegisterButton.Label = "Offline Mode";
                });
            }
        }
    }
}
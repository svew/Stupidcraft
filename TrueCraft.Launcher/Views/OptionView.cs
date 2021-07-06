using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gtk;
using TrueCraft.Core;
using Ionic.Zip;

namespace TrueCraft.Launcher.Views
{
    public class OptionView : VBox
    {
        private enum TexturePackColumns
        {
           Image,
           Name
        }

        public LauncherWindow Window { get; set; }

        public Label OptionLabel { get; set; }

        public Label ResolutionLabel { get; set; }
        private ComboBox _resolutionComboBox;
        private ListStore _resolutionList;

        public CheckButton FullscreenCheckBox { get; set; }
        public CheckButton InvertMouseCheckBox { get; set; }
        public Label TexturePackLabel { get; set; }

        private ListStore _texturePackStore;
        private TreeView _texturePackListView;

        public Button OfficialAssetsButton { get; set; }
        public ProgressBar OfficialAssetsProgress { get; set; }
        public Button OpenFolderButton { get; set; }
        public Button BackButton { get; set; }

        private List<TexturePack> _texturePacks;
        private TexturePack _lastTexturePack;

        public OptionView(LauncherWindow window)
        {
            _texturePacks = new List<TexturePack>();
            _lastTexturePack = null;

            Window = window;
            this.SetSizeRequest(250, -1);

            OptionLabel = new Label("Options")
            {
                Justify = Justification.Center
            };

            ResolutionLabel = new Label("Select a resolution...");
            _resolutionList = new ListStore(typeof(string));
            _resolutionComboBox = new ComboBox(_resolutionList);

            int resolutionIndex = -1;
            for (int i = 0; i < WindowResolution.Defaults.Length; i++)
            {
                _resolutionList.AppendValues(WindowResolution.Defaults[i].ToString());

                if (resolutionIndex == -1)
                {
                    resolutionIndex =
                        ((WindowResolution.Defaults[i].Width == UserSettings.Local.WindowResolution.Width) &&
                        (WindowResolution.Defaults[i].Height == UserSettings.Local.WindowResolution.Height)) ? i : -1;
                }
            }

            if (resolutionIndex == -1)
            {
                _resolutionList.AppendValues(UserSettings.Local.WindowResolution.ToString());
                resolutionIndex = WindowResolution.Defaults.Length;
            }

            _resolutionComboBox.Active = resolutionIndex;
            FullscreenCheckBox = new CheckButton
            {
                Label = "Fullscreen mode",
                State = (UserSettings.Local.IsFullscreen) ? CheckBoxState.On : CheckBoxState.Off
            };
            InvertMouseCheckBox = new CheckButton
            {
                Label = "Inverted mouse",
                State = (UserSettings.Local.InvertedMouse) ? CheckBoxState.On : CheckBoxState.Off
            };

            TexturePackLabel = new Label("Select a texture pack...");
            _texturePackStore = new ListStore(typeof(Image), typeof(string));
            _texturePackListView = new TreeView(_texturePackStore);
            _texturePackListView.SetSizeRequest(-1, 200);
            _texturePackListView.HeadersVisible = false;
            TreeSelection texturePackSelection = _texturePackListView.Selection;
            texturePackSelection.Mode = SelectionMode.Single;
            AddTexturePackColumns(_texturePackListView);

            OpenFolderButton = new Button("Open texture pack folder");
            BackButton = new Button("Back");

            _resolutionComboBox.Changed += (sender, e) =>
            {
                TreeIter iter;
                if (!_resolutionComboBox.GetActiveIter(out iter))
                    return;
                string resolution = (string)_resolutionList.GetValue(iter, 0);
                UserSettings.Local.WindowResolution = WindowResolution.FromString(resolution);
                UserSettings.Local.Save();
            };

            FullscreenCheckBox.Clicked += (sender, e) =>
            {
                UserSettings.Local.IsFullscreen = !UserSettings.Local.IsFullscreen;
                UserSettings.Local.Save();
            };

            InvertMouseCheckBox.Clicked += (sender, e) => 
            {
                UserSettings.Local.InvertedMouse = !UserSettings.Local.InvertedMouse;
                UserSettings.Local.Save();
            };

            _texturePackListView.Selection.Changed += (sender, e) =>
            {
                TreeSelection selection = (TreeSelection)sender;
                TreeIter iter;
                ITreeModel model;
                selection.GetSelected(out model, out iter);
                string name = (string)model.GetValue(iter, (int)TexturePackColumns.Name);

                // TODO: Are Texture Pack names sufficiently unique?
                TexturePack texturePack = _texturePacks.Where<TexturePack>(tp => tp.Name == name).First<TexturePack>();
                if (_lastTexturePack != texturePack)
                {
                    // TODO: show busy cursor; add try/catch/finally
                    UserSettings.Local.SelectedTexturePack = texturePack.Name;
                    UserSettings.Local.Save();
                }
            };

            OpenFolderButton.Clicked += (sender, e) =>
            {
                var dir = new DirectoryInfo(Paths.TexturePacks);
                Process.Start(dir.FullName);
            };

            BackButton.Clicked += (sender, e) =>
            {
                Window.InteractionBox.Remove(this);
                Window.InteractionBox.PackEnd(Window.MainMenuView, true, false, 0);
            };

            OfficialAssetsButton = new Button("Download Minecraft assets") { Visible = false };
            OfficialAssetsButton.Clicked += OfficialAssetsButton_Clicked;
            OfficialAssetsProgress = new ProgressBar() { Visible = false, Indeterminate = true };

            LoadTexturePacks();

            this.PackStart(OptionLabel, true, false, 0);
            this.PackStart(ResolutionLabel, true, false, 0);
            this.PackStart(_resolutionComboBox, true, false, 0);
            this.PackStart(FullscreenCheckBox, true, false, 0);
            this.PackStart(InvertMouseCheckBox, true, false, 0);
            this.PackStart(TexturePackLabel, true, false, 0);
            this.PackStart(_texturePackListView, true, false, 0);
            this.PackStart(OfficialAssetsProgress, true, false, 0);
            this.PackStart(OfficialAssetsButton, true, false, 0);
            this.PackStart(OpenFolderButton, true, false, 0);
            this.PackEnd(BackButton, true, false, 0);
        }

        private static void AddTexturePackColumns(TreeView tv)
        {
           // Texture Pack Image Column
           CellRendererPixbuf imageRenderer = new CellRendererPixbuf();
           TreeViewColumn column = new TreeViewColumn(String.Empty, imageRenderer,
                    "image", TexturePackColumns.Image);
           column.SortColumnId = (int)TexturePackColumns.Image;
           tv.AppendColumn(column);

            // Texture Pack Name column
            CellRendererText rendererText = new CellRendererText();
            column = new TreeViewColumn("Name", rendererText, "text", TexturePackColumns.Name);
            column.SortColumnId = (int)TexturePackColumns.Name;
            tv.AppendColumn(column);
        }

        void OfficialAssetsButton_Clicked(object sender, EventArgs e)
        {
            var result = MessageDialog.AskQuestion("Download Mojang assets",
                "This will download the official Minecraft assets from Mojang.\n\n" +
                "By proceeding you agree to the Mojang asset guidelines:\n\n" +
                "https://account.mojang.com/terms#brand\n\n" +
                "Proceed?",
                Command.Yes, Command.No);
            if (result == Command.Yes)
            {
                OfficialAssetsButton.Visible = false;
                OfficialAssetsProgress.Visible = true;
                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var stream = new WebClient().OpenRead("http://s3.amazonaws.com/Minecraft.Download/versions/b1.7.3/b1.7.3.jar");
                            var ms = new MemoryStream();
                            CopyStream(stream, ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            stream.Dispose();
                            var jar = ZipFile.Read(ms);
                            var zip = new ZipFile();
                            zip.AddEntry("pack.txt", "Minecraft textures");

                            string[] dirs = {
                                "terrain", "gui", "armor", "art",
                                "environment", "item", "misc", "mob"
                            };

                            foreach (var entry in jar.Entries)
                            {
                                foreach (var c in dirs)
                                {
                                    if (entry.FileName.StartsWith(c + "/"))
                                        CopyBetweenZips(entry.FileName, jar, zip);
                                }
                            }
                            CopyBetweenZips("pack.png", jar, zip);
                            CopyBetweenZips("terrain.png", jar, zip);
                            CopyBetweenZips("particles.png", jar, zip);

                            zip.Save(System.IO.Path.Combine(Paths.TexturePacks, "Minecraft.zip"));
                            Application.Invoke((sender, e) =>
                                {
                                    OfficialAssetsProgress.Visible = false;
                                    var texturePack = TexturePack.FromArchive(
                                        System.IO.Path.Combine(Paths.TexturePacks, "Minecraft.zip"));
                                    _texturePacks.Add(texturePack);
                                    AddTexturePackRow(texturePack);
                                });
                            ms.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Application.Invoke((sender, e) =>
                                {
                                    MessageDialog.ShowError("Error retrieving assets", ex.ToString());
                                    OfficialAssetsProgress.Visible = false;
                                    OfficialAssetsButton.Visible = true;
                                });
                        }
                    });
            }
        }

        public static void CopyBetweenZips(string name, ZipFile source, ZipFile destination)
        {
            using (var stream = source.Entries.SingleOrDefault(f => f.FileName == name).OpenReader())
            {
                var ms = new MemoryStream();
                CopyStream(stream, ms);
                ms.Seek(0, SeekOrigin.Begin);
                destination.AddEntry(name, ms);
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16*1024];
            int read;
            while((read = input.Read (buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        private void LoadTexturePacks()
        {
            // We load the default texture pack specially.
            _texturePacks.Add(TexturePack.Default);
            AddTexturePackRow(TexturePack.Default);

            // Make sure to create the texture pack directory if there is none.
            if (!Directory.Exists(Paths.TexturePacks))
                Directory.CreateDirectory(Paths.TexturePacks);

            var zips = Directory.EnumerateFiles(Paths.TexturePacks);
            bool officialPresent = false;
            foreach (var zip in zips)
            {
                if (!zip.EndsWith(".zip"))
                    continue;
                if (System.IO.Path.GetFileName(zip) == "Minecraft.zip")
                    officialPresent = true;

                var texturePack = TexturePack.FromArchive(zip);
                if (texturePack != null)
                {
                    _texturePacks.Add(texturePack);
                    AddTexturePackRow(texturePack);
                }
            }
            if (!officialPresent)
                OfficialAssetsButton.Visible = true;
        }

        private void AddTexturePackRow(TexturePack pack)
        {
           TreeIter row = _texturePackStore.Append();

           _texturePackStore.SetValue(row, (int)TexturePackColumns.Image, new Image(new Gdk.Pixbuf(pack.Image, 24, 24)));
           _texturePackStore.SetValue(row, (int)TexturePackColumns.Name, pack.Name + "\r\n" + pack.Description);
        }
    }
}

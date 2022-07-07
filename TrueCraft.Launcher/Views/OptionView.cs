using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gtk;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views
{
    public class OptionView : VBox
    {
        private enum TexturePackColumns
        {
           Image,
           Name,
           Description
        }

        private readonly LauncherWindow _window;

        private readonly Label _optionLabel;

        private readonly Label _resolutionLabel;
        private readonly ComboBox _resolutionComboBox;
        private readonly ListStore _resolutionList;

        private readonly CheckButton _fullscreenCheckBox;
        private readonly CheckButton _invertMouseCheckBox;

        private readonly Label _texturePackLabel;

        private readonly ListStore _texturePackStore;
        private readonly TreeView _texturePackListView;

        private readonly Button _officialAssetsButton;
        private readonly ProgressBar _officialAssetsProgress;
        private readonly Button _openFolderButton;
        private readonly Button _backButton;

        private readonly List<TexturePack> _texturePacks;
        private readonly TexturePack? _lastTexturePack;

        public OptionView(LauncherWindow window)
        {
            _texturePacks = new List<TexturePack>();
            _lastTexturePack = null;

            _window = window;
            this.SetSizeRequest(250, -1);

            _optionLabel = new Label("Options")
            {
                Justify = Justification.Center
            };

            _resolutionLabel = new Label("Select a resolution...");
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
            _fullscreenCheckBox = new CheckButton("Fullscreen mode");
            _fullscreenCheckBox.Active = UserSettings.Local.IsFullscreen;
            _invertMouseCheckBox = new CheckButton("Inverted mouse");
            _invertMouseCheckBox.Active = UserSettings.Local.InvertedMouse;

            _texturePackLabel = new Label("Select a texture pack...");
            _texturePackStore = new ListStore(typeof(Image), typeof(string), typeof(string));
            _texturePackListView = new TreeView(_texturePackStore);
            _texturePackListView.SetSizeRequest(-1, 200);
            _texturePackListView.HeadersVisible = false;
            TreeSelection texturePackSelection = _texturePackListView.Selection;
            texturePackSelection.Mode = SelectionMode.Single;
            AddTexturePackColumns(_texturePackListView);

            _openFolderButton = new Button("Open texture pack folder");
            _backButton = new Button("Back");

            _resolutionComboBox.Changed += (sender, e) =>
            {
                TreeIter iter;
                if (!_resolutionComboBox.GetActiveIter(out iter))
                    return;
                string resolution = (string)_resolutionList.GetValue(iter, 0);
                UserSettings.Local.WindowResolution = WindowResolution.FromString(resolution);
                UserSettings.Local.Save();
            };

            _fullscreenCheckBox.Clicked += (sender, e) =>
            {
                UserSettings.Local.IsFullscreen = _fullscreenCheckBox.Active;
                // TODO: show busy cursor; add try/catch/finally
                UserSettings.Local.Save();
            };

            _invertMouseCheckBox.Clicked += (sender, e) => 
            {
                UserSettings.Local.InvertedMouse = _invertMouseCheckBox.Active;
                // TODO: show busy cursor; add try/catch/finally
                UserSettings.Local.Save();
            };

            _texturePackListView.Selection.Changed += (sender, e) =>
            {
                TreeSelection selection = (TreeSelection)sender!;
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

            _openFolderButton.Clicked += (sender, e) =>
            {
                var dir = new DirectoryInfo(Paths.TexturePacks);
                Process.Start(dir.FullName);
            };

            _backButton.Clicked += (sender, e) => _window.ShowMainMenuView();

            _officialAssetsButton = new Button("Download Minecraft assets") { Visible = false };
            _officialAssetsButton.Clicked += OfficialAssetsButton_Clicked;
            // TODO: we have to call Pulse on the Progress Bar once in a while.
            _officialAssetsProgress = new ProgressBar() { Visible = false };

            LoadTexturePacks();

            this.PackStart(_optionLabel, true, false, 0);
            this.PackStart(_resolutionLabel, true, false, 0);
            this.PackStart(_resolutionComboBox, true, false, 0);
            this.PackStart(_fullscreenCheckBox, true, false, 0);
            this.PackStart(_invertMouseCheckBox, true, false, 0);
            this.PackStart(_texturePackLabel, true, false, 0);
            this.PackStart(_texturePackListView, true, false, 0);
            this.PackStart(_officialAssetsProgress, true, false, 0);
            this.PackStart(_officialAssetsButton, true, false, 0);
            this.PackStart(_openFolderButton, true, false, 0);
            this.PackEnd(_backButton, true, false, 0);
        }

        private static void AddTexturePackColumns(TreeView tv)
        {
           // Texture Pack Image Column
           CellRendererPixbuf imageRenderer = new CellRendererPixbuf();
           TreeViewColumn column = new TreeViewColumn(String.Empty, imageRenderer,
                    Array.Empty<object>());
           column.SortColumnId = (int)TexturePackColumns.Image;
           tv.AppendColumn(column);

            // Texture Pack Name column
            CellRendererText rendererText = new CellRendererText();
            column = new TreeViewColumn("Name", rendererText, "text", TexturePackColumns.Name);
            column.SortColumnId = (int)TexturePackColumns.Name;
            tv.AppendColumn(column);

            // Texture Pack Description column
            rendererText = new CellRendererText();
            column = new TreeViewColumn("Description", rendererText, "text", TexturePackColumns.Description);
            column.SortColumnId = (int)TexturePackColumns.Description;
            tv.AppendColumn(column);
        }

        void OfficialAssetsButton_Clicked(object? sender, EventArgs e)
        {
           using (MessageDialog msg = new MessageDialog(_window,
                     DialogFlags.Modal | DialogFlags.DestroyWithParent,
                     MessageType.Question, ButtonsType.YesNo,
                     false,
                     "Download Mojang assets",
                     Array.Empty<object>()))
           {
              msg.SecondaryText = "This will download the official Minecraft assets from Mojang.\n\n" +
                  "By proceeding you agree to the Mojang asset guidelines:\n\n" +
                  "https://account.mojang.com/terms#brand\n\n" +
                  "Proceed?";
              msg.Response += HandleOfficialAssetsResponse;
              msg.Run();
           }
        }

        private void HandleOfficialAssetsResponse(object sender, ResponseArgs e)
        {
           if (e.ResponseId != ResponseType.Yes)
              return;

            _officialAssetsButton.Visible = false;
            _officialAssetsProgress.Visible = true;
            Task.Factory.StartNew(() =>
            {
                Stream ms = new MemoryStream();

                try
                {
                    using (Stream stream = new WebClient().OpenRead("http://s3.amazonaws.com/Minecraft.Download/versions/b1.7.3/b1.7.3.jar"))
                    {
                        CopyStream(stream, ms);
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                }
                catch (Exception ex)
                {
                    Application.Invoke((sender, e) =>
                    {
                        using (MessageDialog msg = new MessageDialog(_window,
                                 DialogFlags.DestroyWithParent | DialogFlags.Modal,
                                 MessageType.Error,
                                 ButtonsType.Close,
                                 $"Error retrieving assets:\n{ex}",
                                 Array.Empty<object>()))
                        {
                            msg.Run();
                        }
                        _officialAssetsProgress.Visible = false;
                        _officialAssetsButton.Visible = true;
                    });
                    return;
                }

                ZipArchive jar = new ZipArchive(ms, ZipArchiveMode.Read);
                using (Stream outputPack = new FileStream(System.IO.Path.Combine(Paths.TexturePacks, "Minecraft.zip"), FileMode.Create))
                using (ZipArchive outputZip = new ZipArchive(outputPack, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry pack = outputZip.CreateEntry("pack.txt");
                    using (Stream outStrm = pack.Open())
                    using (StreamWriter sw = new StreamWriter(outStrm))
                        sw.WriteLine("Minecraft textures");

                    // TODO: achievement? font?
                    string[] dirs = {
                            "terrain", "gui", "armor", "art",
                            "environment", "item", "misc", "mob"
                        };

                    foreach (ZipArchiveEntry entry in jar.Entries)
                        foreach (string c in dirs)
                            if (entry.FullName.StartsWith(c + "/"))
                            {
                                ZipArchiveEntry outputEntry = outputZip.CreateEntry(entry.FullName);
                                using (Stream inStrm = entry.Open())
                                using (Stream outStrm = outputEntry.Open())
                                    CopyStream(inStrm, outStrm);
                            }

                    CopyBetweenZips("pack.png", jar, outputZip);
                    CopyBetweenZips("terrain.png", jar, outputZip);
                    CopyBetweenZips("particles.png", jar, outputZip);

                    Application.Invoke((sender, e) =>
                    {
                        _officialAssetsProgress.Visible = false;
                        TexturePack texturePack = TexturePack.FromArchive(
                            System.IO.Path.Combine(Paths.TexturePacks, "Minecraft.zip"))!;  // file was just created, so this won't return null.
                        _texturePacks.Add(texturePack);
                        AddTexturePackRow(texturePack);
                    });
                }
                ms.Dispose();
            }).ContinueWith((x) =>
            {
                _officialAssetsProgress.Visible = false;
                _officialAssetsButton.Visible = true;
            });
        }

        public static void CopyBetweenZips(string name, ZipArchive source, ZipArchive destination)
        {
            ZipArchiveEntry? sourceEntry = source.Entries.SingleOrDefault(f => f.FullName == name);
            if (sourceEntry is null)
                return;

            using (Stream src = sourceEntry.Open())
            using (Stream dest = destination.CreateEntry(name).Open())
                CopyStream(src, dest);
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

                TexturePack? texturePack = TexturePack.FromArchive(zip);
                if (texturePack is not null)
                {
                    _texturePacks.Add(texturePack);
                    AddTexturePackRow(texturePack);
                }
            }
            if (!officialPresent)
                _officialAssetsButton.Visible = true;
        }

        private void AddTexturePackRow(TexturePack pack)
        {
            TreeIter row = _texturePackStore.Append();

            _texturePackStore.SetValue(row, (int)TexturePackColumns.Image, new Image(new Gdk.Pixbuf(pack.Image, 24, 24)));
            _texturePackStore.SetValue(row, (int)TexturePackColumns.Name, pack.Name);
            _texturePackStore.SetValue(row, (int)TexturePackColumns.Description, pack.Description);
        }
    }
}

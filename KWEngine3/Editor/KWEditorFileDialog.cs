using ImGuiNET;
using System.Numerics;
using System.Reflection;

namespace KWEngine3.Editor
{
    // File dialog of the editor, drawn with ImGui as a modal window.
    // OpenImageFile() requests the dialog, Draw() (called once per frame by the editor, outside of other ImGui windows)
    // shows it and hands the chosen file to the callback. Only one dialog can be open at a time.
    internal static class KWEditorFileDialog
    {
        private const string POPUP_ID = "###KWEditorFileDialog"; // the visible title changes, the id stays the same
        private static readonly string[] _filterNames = { "Images (*.png, *.jpg, *.jpeg, *.dds)", "All files (*.*)" };
        private static readonly string[] _imageExtensions = { ".png", ".jpg", ".jpeg", ".dds" };
        private static readonly Vector4 COLOR_DIRECTORY = new Vector4(1f, 0.85f, 0.4f, 1f);
        private static readonly Vector4 COLOR_ERROR = new Vector4(1f, 0.4f, 0.4f, 1f);

        private static bool _openRequested = false;
        private static bool _isOpen = false;
        private static string _title = "";
        private static Action<string> _onSelected = null;
        private static World _world = null; // the dialog is closed if the world was changed meanwhile

        private static string _currentDirectory = "";
        private static string _directoryInput = "";
        private static string _fileNameInput = "";
        private static string _error = null;
        private static int _filterIndex = 0;
        private static string[] _drives = Array.Empty<string>();
        private static readonly List<string> _directories = new();
        private static readonly List<string> _allFiles = new();
        private static readonly List<string> _files = new(); // _allFiles reduced by the filter

        internal static bool IsOpen
        {
            get { return _openRequested || _isOpen; }
        }

        // Opens the dialog for image files (png, jpg, jpeg, dds) in the folder of the entry assembly.
        // onSelected receives the file name relative to the working directory ("./Textures/a.png") if the file
        // lies inside of it (otherwise the absolute path), or an empty string for "No file".
        internal static void OpenImageFile(string title, Action<string> onSelected)
        {
            if (IsOpen)
                return;
            _title = title;
            _onSelected = onSelected;
            _world = KWEngine.CurrentWorld;
            _fileNameInput = "";
            _filterIndex = 0;
            _drives = OperatingSystem.IsWindows() ? GetDrives() : Array.Empty<string>();
            ChangeDirectory(GetStartDirectory());
            _openRequested = true;
        }

        internal static void Draw()
        {
            if (!_openRequested && !_isOpen)
                return;

            string popupName = _title + POPUP_ID;
            if (_openRequested)
            {
                ImGui.OpenPopup(popupName);
                _openRequested = false;
                _isOpen = true;
            }

            Vector2 displaySize = ImGui.GetIO().DisplaySize;
            ImGui.SetNextWindowSize(new Vector2(Math.Min(760f, displaySize.X * 0.9f), Math.Min(520f, displaySize.Y * 0.85f)), ImGuiCond.Appearing);
            ImGui.SetNextWindowPos(displaySize * 0.5f, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            bool keepOpen = true;
            if (!ImGui.BeginPopupModal(popupName, ref keepOpen, ImGuiWindowFlags.NoCollapse))
            {
                Close(); // closed with the x button of the window
                return;
            }

            bool close = _world != KWEngine.CurrentWorld || ImGui.IsKeyPressed(ImGuiKey.Escape);
            string result = null; // file for the callback ("" = no file)
            string navigateTo = null;
            string confirmInput = null;

            // folder row
            DirectoryInfo parent = _currentDirectory.Length > 0 ? Directory.GetParent(_currentDirectory) : null;
            if (parent == null)
                ImGui.BeginDisabled(true);
            if (ImGui.Button("Up"))
                navigateTo = parent.FullName;
            if (parent == null)
                ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Program folder"))
                navigateTo = GetStartDirectory();
            if (_drives.Length > 1)
            {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.CalcTextSize("WW:\\").X + ImGui.GetFrameHeight() + ImGui.GetStyle().FramePadding.X * 2f);
                if (ImGui.BeginCombo("##drive", Path.GetPathRoot(_currentDirectory) ?? ""))
                {
                    foreach (string drive in _drives)
                    {
                        if (ImGui.Selectable(drive))
                            navigateTo = drive;
                    }
                    ImGui.EndCombo();
                }
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputText("##folder", ref _directoryInput, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                navigateTo = _directoryInput;

            // folders and files
            float footerHeight = ImGui.GetFrameHeightWithSpacing() * 2f + ImGui.GetTextLineHeightWithSpacing();
            if (ImGui.BeginChild("##entries", new Vector2(0f, -footerHeight), true))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, COLOR_DIRECTORY);
                if (parent != null && ImGui.Selectable("../"))
                    navigateTo = parent.FullName;
                for (int i = 0; i < _directories.Count; i++)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable(_directories[i] + "/"))
                        navigateTo = Path.Combine(_currentDirectory, _directories[i]);
                    ImGui.PopID();
                }
                ImGui.PopStyleColor();
                for (int i = 0; i < _files.Count; i++)
                {
                    ImGui.PushID(_directories.Count + i);
                    if (ImGui.Selectable(_files[i], _files[i] == _fileNameInput, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        _fileNameInput = _files[i];
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                            confirmInput = _files[i];
                    }
                    ImGui.PopID();
                }
            }
            ImGui.EndChild();

            // file name and filter
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("File");
            ImGui.SameLine();
            float filterWidth = ImGui.CalcTextSize(_filterNames[0]).X + ImGui.GetFrameHeight() + ImGui.GetStyle().FramePadding.X * 2f;
            ImGui.SetNextItemWidth(Math.Max(100f, ImGui.GetContentRegionAvail().X - filterWidth - ImGui.GetStyle().ItemSpacing.X));
            if (ImGui.InputText("##file", ref _fileNameInput, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                confirmInput = _fileNameInput;
            ImGui.SameLine();
            ImGui.SetNextItemWidth(filterWidth);
            if (ImGui.Combo("##filter", ref _filterIndex, _filterNames, _filterNames.Length))
                ApplyFilter();

            if (_error != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, COLOR_ERROR);
                ImGui.TextUnformatted(_error);
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.NewLine();
            }

            // buttons
            bool hasInput = _fileNameInput.Trim().Length > 0;
            if (!hasInput)
                ImGui.BeginDisabled(true);
            if (ImGui.Button("Open", new Vector2(100f, 0f)))
                confirmInput = _fileNameInput;
            if (!hasInput)
                ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("No file", new Vector2(100f, 0f)))
            {
                result = "";
                close = true;
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Removes the current file (e.g. the texture).");
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(100f, 0f)))
                close = true;

            // apply after drawing: the lists must not change while they are drawn
            if (navigateTo != null)
                ChangeDirectory(navigateTo);
            if (confirmInput != null && !close)
            {
                string file = ResolveInput(confirmInput);
                if (file != null)
                {
                    result = file;
                    close = true;
                }
            }

            if (close)
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();

            if (close)
            {
                Action<string> callback = _onSelected;
                World world = _world;
                Close();
                if (result != null && callback != null && world == KWEngine.CurrentWorld)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception ex)
                    {
                        KWEngine.LogWriteLine("[Editor] Could not use file " + result + ": " + ex.Message);
                    }
                }
            }
        }

        private static void Close()
        {
            _isOpen = false;
            _openRequested = false;
            _onSelected = null;
            _world = null;
        }

        // typed or selected input: a folder is opened, an existing file is returned (engine path), otherwise null
        private static string ResolveInput(string input)
        {
            input = input == null ? "" : input.Trim();
            if (input.Length == 0)
                return null;
            try
            {
                string path = Path.GetFullPath(Path.IsPathRooted(input) ? input : Path.Combine(_currentDirectory, input));
                if (Directory.Exists(path))
                {
                    ChangeDirectory(path);
                    _fileNameInput = "";
                    return null;
                }
                if (File.Exists(path))
                    return ToEnginePath(path);
            }
            catch (Exception)
            {
                // invalid path: handled below
            }
            _error = "File not found: " + input;
            return null;
        }

        private static void ChangeDirectory(string directory)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(Path.GetFullPath(directory));
                List<string> directories = new();
                List<string> files = new();
                foreach (DirectoryInfo d in info.EnumerateDirectories())
                {
                    if ((d.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                        directories.Add(d.Name);
                }
                foreach (FileInfo f in info.EnumerateFiles())
                {
                    if ((f.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                        files.Add(f.Name);
                }
                directories.Sort(StringComparer.OrdinalIgnoreCase);
                files.Sort(StringComparer.OrdinalIgnoreCase);

                _directories.Clear();
                _directories.AddRange(directories);
                _allFiles.Clear();
                _allFiles.AddRange(files);
                _currentDirectory = info.FullName;
                _directoryInput = info.FullName;
                _error = null;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _error = "Cannot open folder: " + ex.Message;
                _directoryInput = _currentDirectory;
            }
        }

        private static void ApplyFilter()
        {
            _files.Clear();
            foreach (string file in _allFiles)
            {
                if (_filterIndex != 0 || IsImageFile(file))
                    _files.Add(file);
            }
        }

        private static bool IsImageFile(string file)
        {
            string extension = Path.GetExtension(file);
            foreach (string imageExtension in _imageExtensions)
            {
                if (string.Equals(extension, imageExtension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string[] GetDrives()
        {
            try
            {
                List<string> drives = new();
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                        drives.Add(drive.Name);
                }
                return drives.ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }

        private static string GetStartDirectory()
        {
            string directory = null;
            try
            {
                string location = Assembly.GetEntryAssembly()?.Location;
                if (!string.IsNullOrEmpty(location))
                    directory = Path.GetDirectoryName(location);
            }
            catch (Exception)
            {
                directory = null;
            }
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                directory = AppContext.BaseDirectory; // single file apps have no assembly location
            return Path.GetFullPath(directory);
        }

        // relative to the working directory (the engine opens files relative to it), otherwise absolute
        private static string ToEnginePath(string file)
        {
            string fullPath = Path.GetFullPath(file);
            string workingDirectory = Path.GetFullPath(Environment.CurrentDirectory);
            string relativePath = Path.GetRelativePath(workingDirectory, fullPath);
            if (Path.IsPathRooted(relativePath) || relativePath == ".." || relativePath.StartsWith(".." + Path.DirectorySeparatorChar) || relativePath.StartsWith("../"))
                return fullPath.Replace('\\', '/');
            return "./" + relativePath.Replace('\\', '/');
        }
    }
}

using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections;

class ProjectTree : TreeView
{
    public string pathRoot;
    public string pathProject;
    public string pathImages;
    public string pathTemp;
    public string pathClipboard;

    public ArrayList customNodes = new ArrayList();

    public ContextMenuStrip contextMenu = new ContextMenuStrip();
    public FileSystemWatcher watcher = new FileSystemWatcher();

    public void Setup()
    {
        pathRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _Setup();
    }

    public void Setup(string root)
    {
        pathRoot = root;
        _Setup();
    }

    private void _Setup()
    {
        pathProject = pathRoot + "\\projects";
        if (Directory.Exists(pathProject) != true)
        {
            MessageBox.Show("Failed to find the projects folder.");
            Directory.CreateDirectory(pathProject);
        }

        pathImages = pathRoot + "\\images";
        if (Directory.Exists(pathImages) != true)
        {
            MessageBox.Show("Failed to find the images folder.");
            Directory.CreateDirectory(pathImages);
        }

        pathTemp = pathRoot + "\\tmp";
        if (Directory.Exists(pathImages) != true)
        {
            MessageBox.Show("Failed to find the tmp folder.");
            Directory.CreateDirectory(pathTemp);
        }

        LabelEdit = true;

        this.AfterExpand += new TreeViewEventHandler(OnProjectTree_NodeExpand);
        this.AfterCollapse += new TreeViewEventHandler(OnProjectTree_NodeCollapse);
        this.AfterLabelEdit += new NodeLabelEditEventHandler(OnProjectTree_AfterLabelEdit);

        watcher.Path = pathProject;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = this;
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.CreationTime;
        watcher.Deleted += new FileSystemEventHandler(OnWatcher_Deleted);
        watcher.Created += new FileSystemEventHandler(OnWatcher_Created);
        watcher.Renamed += new RenamedEventHandler(OnWatcher_Renamed);

        ImagesFromFolder();
        CreateContextMenu();
    }

    public void InitializeComponent()
    {
        if (DrawMode == TreeViewDrawMode.OwnerDrawText)
        {
            DoubleBuffered = true;
            this.DrawNode += new DrawTreeNodeEventHandler(OnProjectTree_DrawNode);

            if (customNodes.Count == 0)
            {
                CustomNode customNode = new CustomNode();
                customNode.pattern = @"\b([a-z0-9 _]{1,})\b";
                customNode.color = new Color[] { Color.DarkSlateGray };
                customNodes.Add(customNode);

                customNode = new CustomNode();
                customNode.pattern = @"([a-z0-9]{1,})([.txt]{4})";
                customNode.color = new Color[] { Color.Black, Color.Crimson };
                customNodes.Add(customNode);
            }
        }

        watcher.EnableRaisingEvents = true;
        Nodes.Clear();
        CreateProjectNodes();
    }

    public void ImagesFromFolder()
    {
        ImageList images = new ImageList();

        DirectoryInfo directory = new DirectoryInfo(pathImages);
        FileInfo[] fileinfo = directory.GetFiles("*.png");

        foreach (FileInfo info in fileinfo)
        {
            images.Images.Add(Path.GetFileNameWithoutExtension(info.FullName), Image.FromFile(info.FullName));
        }

        this.ImageList = images;
    }

    public string ImageToggle(string filename)
    {
        if (filename.Contains("Open"))
        {
            return filename.Replace("Open", "Close");
        }
        else if (filename.Contains("Close"))
        {
            return filename.Replace("Close", "Open");
        }
        else
        {
            return filename;
        }
    }

    public void CreateFolderNodes(TreeNode node)
    {
        foreach (string item in Directory.GetDirectories(node.Name))
        {
            node.Nodes.Add(item, Path.GetFileName(item), "folderClose", "folderClose");
            node.Nodes[item].ContextMenuStrip = contextMenu;
            CreateFolderNodes(node.Nodes[item]);
        }

        CreateFileNodes(node);
    }

    public void CreateFileNodes(TreeNode node)
    {
        foreach (string item in Directory.GetFiles(node.Name))
        {
            string ext = Path.GetExtension(item).Replace(".", "");
            node.Nodes.Add(item, Path.GetFileName(item), ext, ext);
            node.Nodes[item].ContextMenuStrip = contextMenu;
        }
    }

    public void CreateProjectNodes()
    {
        foreach (string item in Directory.GetDirectories(pathProject))
        {
            Nodes.Add(item, Path.GetFileName(item), "briefcaseClose", "briefcaseClose");
            Nodes[item].ContextMenuStrip = contextMenu;
            TreeNode node = Nodes[item];
            CreateFolderNodes(node);
        }
    }

    private void UpdateFolderPaths(TreeNode node, string newPath)
    {
        foreach (TreeNode item in node.Nodes)
        {
            item.Name = item.Name.Replace(item.Name, newPath);
            if (Directory.Exists(item.Name))
            {
                UpdateFolderPaths(item, newPath);
            }
        }
    }

    public struct CustomNode
    {
        public string pattern;
        public Color[] color;
    }

    private void OnProjectTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
    {
        if (e.Node == SelectedNode)
        {
            e.DrawDefault = true;
        }
        else
        {
            foreach (CustomNode item in customNodes)
            {
                if (Regex.IsMatch(e.Node.Text, item.pattern, RegexOptions.IgnoreCase))
                {
                    Match match = Regex.Match(e.Node.Text, item.pattern, RegexOptions.IgnoreCase);

                    int left = 0;
                    for (int i = 1; i < match.Groups.Count; i++)
                    {

                        int width = TextRenderer.MeasureText(match.Groups[i].Value, Font).Width;
                        Rectangle bounds = new Rectangle();
                        bounds = e.Bounds;
                        bounds.X += left;
                        bounds.Width = width;
                        left += width - 5;
                        TextRenderer.DrawText(e.Graphics, match.Groups[i].Value, Font, bounds, item.color[i - 1]);
                    }
                }
            }

            Brush brush = new SolidBrush(Color.Crimson);
        }
    }

    private void OnProjectTree_NodeExpand(object sender, TreeViewEventArgs e)
    {
        e.Node.ImageKey = ImageToggle(e.Node.ImageKey);
        e.Node.SelectedImageKey = ImageToggle(e.Node.SelectedImageKey);
    }

    private void OnProjectTree_NodeCollapse(object sender, TreeViewEventArgs e)
    {
        e.Node.ImageKey = ImageToggle(e.Node.ImageKey);
        e.Node.SelectedImageKey = ImageToggle(e.Node.SelectedImageKey);
    }

    private void OnProjectTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
        if (e.Label != null)
        {
            Rename(e.Node.Name, e.Label);
        }

        string newPath = e.Node.Name.Replace(e.Node.Text, e.Label);

        if (Directory.Exists(newPath))
        {
            UpdateFolderPaths(e.Node, newPath);
        }
    }

    private void OnWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.Equals(pathProject))
        {
            Nodes[e.FullPath].Remove();
        }
        else
        {
            TreeNode[] nodes = Nodes.Find(e.FullPath, true);
            nodes[0].Remove();
        }
    }

    private void OnWatcher_Created(object sender, FileSystemEventArgs e)
    {
        string ext = Path.GetExtension(e.FullPath).Replace(".", "");
        string directory = e.FullPath;

        if (ext != "")
        {
            directory = e.FullPath.Replace("\\" + Path.GetFileName(e.FullPath), "");
            TreeNode[] nodes = Nodes.Find(directory, true);
            nodes[0].Nodes.Add(e.FullPath, Path.GetFileName(e.FullPath), ext, ext);
            nodes[0].Nodes[e.FullPath].ContextMenuStrip = contextMenu;
        }

        else if (Path.GetDirectoryName(directory).Equals(pathProject))
        {
            Nodes.Add(e.FullPath, GetDirectory(e.FullPath), "briefcaseClose", "briefcaseClose");
            Nodes[e.FullPath].ContextMenuStrip = contextMenu;
        }

        else
        {
            directory = Path.GetDirectoryName(directory);
            TreeNode[] nodes = Nodes.Find(directory, true);
            nodes[0].Nodes.Add(e.FullPath, GetDirectory(e.FullPath), "folderClose", "folderClose");
            nodes[0].Nodes[e.FullPath].ContextMenuStrip = contextMenu;
        }
    }

    private void OnWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        TreeNode[] nodes;
        nodes = Nodes.Find("\\projects\\" + e.OldName, true);
        nodes[0].Name = e.FullPath;
        nodes[0].Text = Path.GetFileName(e.FullPath);
    }

    private string GetDirectory(string path)
    {
        string[] pieces = path.Split(new string[] { "\\" }, StringSplitOptions.None);
        return pieces[pieces.Length - 1];
    }

    public void Delete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private void Rename(string path, string newName)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Move(path, Path.GetDirectoryName(path) + "\\" + newName);
            }

            if (Directory.Exists(path))
            {
                Directory.Move(path, Path.GetDirectoryName(path) + "\\" + newName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to rename item.");
        }
    }

    //This function was on MSDN
    public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, false);
        }

    }

    public virtual ContextMenuStrip CreateContextMenu()
    {
        contextMenu.Items.Add(CreateMenuItem("Open", "open.png"));
        contextMenu.Items.Add(CreateMenuItem("Cut", "cut.png"));
        contextMenu.Items.Add(CreateMenuItem("Copy", "copy.png"));
        contextMenu.Items.Add(CreateMenuItem("Paste", "paste.png"));
        contextMenu.Items.Add(CreateMenuItem("Delete", "delete.png"));
        return contextMenu;
    }

    public ToolStripMenuItem CreateMenuItem(string label, string imageName)
    {
        ToolStripMenuItem item = new ToolStripMenuItem();
        item.Image = Image.FromFile(pathImages + "\\" + imageName);
        item.Name = imageName;
        item.Text = label;
        item.Click += OnMenuItemClick;
        return item;
    }

    public virtual void OnMenuItemClick(object sender, EventArgs e)
    {
        ToolStripMenuItem item = (ToolStripMenuItem)sender;
        switch (item.Text)
        {
            case "Open":
                OnOpen();
                break;
            case "Cut":
                OnCut();
                break;
            case "Copy":
                OnCopy();
                break;
            case "Paste":
                OnPaste();
                break;
            case "Delete":
                OnDelete();
                break;
        }
    }

    public virtual void OnOpen() { }

    public virtual void OnCut()
    {

        string path = SelectedNode.Name;
        string filename = SelectedNode.Text;

        try
        {
            if (File.Exists(path))
            {
                pathClipboard = pathTemp + "\\" + filename;
                File.Move(SelectedNode.Name, pathTemp + "\\" + filename);
            }
            else
            {
                pathClipboard = pathTemp + "\\" + filename;
                Directory.Move(SelectedNode.Name, pathTemp + "\\" + filename);
            }
        }
        catch (IOException ex)
        {
            Delete(pathClipboard);
            OnCut();
        }

    }

    public virtual void OnCopy()
    {
        string path = SelectedNode.Name;
        string filename = SelectedNode.Text;

        try
        {
            if (File.Exists(path))
            {
                pathClipboard = pathTemp + "\\" + filename;
                File.Copy(SelectedNode.Name, pathTemp + "\\" + filename);
            }
            else
            {
                pathClipboard = pathTemp + "\\" + filename;
                DirectoryCopy(SelectedNode.Name, pathTemp + "\\" + filename, true);

            }
        }
        catch (IOException ex)
        {
            Delete(pathClipboard);
            OnCopy();
        }
    }

    public virtual void OnPaste()
    {
        try
        {
            string pathPaste = SelectedNode.Name;
            if (File.Exists(pathPaste))
            {
                pathPaste = pathPaste.Replace(Path.GetFileName(pathPaste), "");
            }

            if (File.Exists(pathClipboard))
            {
                File.Copy(pathClipboard, pathPaste + "\\" + Path.GetFileName(pathClipboard));
            }

            if (Directory.Exists(pathClipboard))
            {
                DirectoryCopy(pathClipboard, pathPaste + "\\" + GetDirectory(pathClipboard), true);
            }
            Delete(pathClipboard);
        }
        catch (IOException ex)
        {
            MessageBox.Show("File already exists please delete/rename it before pasting.");
        }
    }

    public virtual void OnDelete()
    {
        Delete(SelectedNode.Name);
    }

}

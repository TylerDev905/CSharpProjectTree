# CSharpProjectTree
A file manager component that monitors the specified folder. Used for managing files in the file system.

<a href="http://imgur.com/CgTkhXu"><img src="http://i.imgur.com/CgTkhXu.png" title="source: imgur.com" /></a>

Features
```
1. Icons are loaded from a folder. Each image can be replaced for easy styling.
2. Extension icons are loaded from a folder, adding an icon for a file extension is as easy as adding the icon with the ext as the file name.
3. icons can be toggled when the nodes are expanded/collapsed. This is achieved by using the keyword Open or Close in the icon filename. All icons should be .png format.
4. The filesystem is monitored for any updates from the program itself or other occurances.
5. A context menu with several file options has been implemented. All items can be overriden. Items can also be added.
6. Renaming a file is possible by editing the label.
7. Color labels by setting the DrawMode property to OwnerDrawMode and supplying an arraylist of CustomNode structures, labels can be colorized using regex for parsing.
8. Default project Root is in My Documents.
```

Example
```cs
ProjectTree projectTree1 = new ProjectTree();
projectTree1.Setup(); //defaulted root = my documents
projectTree1.DrawMode = TreeViewDrawMode.OwnerDrawText;

//Add custom node structure array here if you want colored labels.
//Contextmenu modifications or replacements should be set here as well.

projectTree1.InitializeComponent();
```

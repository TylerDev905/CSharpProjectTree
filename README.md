# CSharpProjectTree
A file manager component that monitors the specified folder. Used for managing files in the file system.

<a href="http://imgur.com/CgTkhXu"><img src="http://i.imgur.com/CgTkhXu.png" title="source: imgur.com" /></a>

Features
```
1. Icons are loaded from a folder. Each image can be replaced for easy styling.
2. Extension icons are loaded from a folder, adding an icon for a file extension is as easy as adding the icon with the ext as the file name.
3. The filesystem is monitored for any updates from the program itself or other occurances.
4. A context menu with several file options has been implemented. All items can be overriden. Items can also be added.
5. Renaming a file is possible by editing the label.
6. Colored labels, by setting the DrawMode property to OwnerDrawMode and supplying an arraylist of CustomNode structures labels can be colorized using regex for parsing.
7. Default project Root is in My Documents.
```

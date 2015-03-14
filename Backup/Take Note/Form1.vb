Imports System.Xml
Public Class Form1

    Function getNode(ByVal txt As String, ByVal childs As XmlNodeList) As String
        For i = 0 To childs.Count - 1
            Dim itm As XmlNode = childs(i)
            Dim p As String = itm.Attributes("Title").InnerXml
            If p.ToLower = txt Then Return i
        Next
        Return -1
    End Function

    Function getReminder() As String
        Dim time As Long = DatePicker1.Value.ToFileTime
        Dim valid As Long = ValidHours.Value
        If Not DatePicker1.Checked Then valid = 0
        Dim lastTime As Long = time + 36000000000 * valid
        Return time & ";" & lastTime
    End Function

    Property storage() As String
        Get
            Dim p As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Take Note", "Storage", "")
            Return p
        End Get
        Set(ByVal value As String)
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Take Note", "Storage", value)
        End Set
    End Property

    Property startWithWindows() As Boolean
        Get
            Dim p As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Take Note", "StartWithWindows", False)
            Return p
        End Get
        Set(ByVal value As Boolean)
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Take Note", "StartWithWindows", value)
        End Set
    End Property

    Property showinBar() As Boolean
        Get
            Dim p As String = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Take Note", "ShowInTaskbar", True)
            Return p
        End Get
        Set(ByVal value As Boolean)
            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Take Note", "ShowInTaskbar", value)
        End Set
    End Property

    Sub loadItems(Optional ByVal src As String = "")
        Dim file As String = storage & "\Document.xml"
        Dim doc As New XmlDocument
        doc.Load(file)
        ListView1.Items.Clear()
        For i = 0 To doc.DocumentElement.ChildNodes.Count - 1
            Dim itm As XmlNode = doc.DocumentElement.ChildNodes(i)
            Dim title As String = itm.Attributes("Title").InnerXml
            Dim time As String = itm.Attributes("Time").InnerXml
            Dim remind As String = itm.Attributes("Remind").InnerXml
            Dim rtf As String = itm.InnerXml
            Dim tip As String = "Title: " & title & vbCrLf & "Last Edit: " & time
            If src.Length > 0 Then
                If Not title.ToLower.Contains(src) Then Continue For
            End If
            Dim list As New ListViewItem(New String() {title, time, remind, False, rtf})
            list.Tag = i
            ListView1.Items.Add(list).ToolTipText = tip
        Next
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'storage
        If storage = "" Then
            Dim p = MsgBox("Select a Storage folder." & vbCrLf & "Storage Folder used to keep notes and other settings", MsgBoxStyle.YesNo)
            If p = MsgBoxResult.Yes Then
                Button2_Click(New Object, New EventArgs)
            Else
                storage = CurDir()
            End If
        End If
        'other settings
        StorageFolder.Text = storage
        CheckBox1.Checked = startWithWindows
        CheckBox2.Checked = showinBar
        'doc
        Dim file As String = storage & "\Document.xml"
        If Not My.Computer.FileSystem.FileExists(file) Then
            Dim xml As String = "<?xml version='1.0'?><doc></doc>"
            My.Computer.FileSystem.WriteAllText(file, xml, False)
        End If
        loadItems("")
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim folder1 As New FolderBrowserDialog
        folder1.SelectedPath = StorageFolder.Text
        If folder1.ShowDialog = 1 Then
            StorageFolder.Text = folder1.SelectedPath
        End If
    End Sub

    Private Sub StorageFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StorageFolder.TextChanged
        storage = StorageFolder.Text
    End Sub

    Private Sub TextBox2_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SearchText.GotFocus
        If SearchText.Text = "Search" Then
            SearchText.Text = ""
            SearchText.ForeColor = Color.Black
        End If
        AcceptButton = SearchButton
    End Sub

    Private Sub TextBox2_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles SearchText.LostFocus
        If SearchText.Text.Trim = "" Then
            SearchText.Text = "Search"
            SearchText.ForeColor = Color.Gray
        End If
        AcceptButton = Nothing
    End Sub

    Private Sub Hide_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Hide_Button.Click
        Hide()
    End Sub

    Private Sub Close_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Close_Button.Click
        If MsgBox("Are your sure to close this completely?", 4) = 6 Then Me.Close()
    End Sub

    Private Sub SearchButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SearchButton.Click
        Dim src As String = TitleText.Text.Trim.ToLower
        If src = "search" Then src = ""
        loadItems(src)
    End Sub

    Private Sub ListView1_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListView1.ItemActivate
        Dim fmr As New Form2(ListView1.FocusedItem.Tag)
        fmr.Show()
    End Sub

    Private Sub AddnoteButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddnoteButton.Click
        'promt to set title if no title
        If TitleText.Text.Trim = "" Then
            MsgBox("Add a title to your note.")
            TitleText.Focus()
            Return
        End If
        'get data
        Dim file As String = storage & "\Document.xml"
        Dim doc As New XmlDocument
        doc.Load(file)
        Dim i As Integer = getNode(TitleText.Text.Trim.ToLower, doc.DocumentElement.ChildNodes)
        If Not i = -1 Then
            Dim p = MsgBox("Another note with same title exist already. Do you want to replace it?" & vbCrLf & _
                   "Yes: To replace old." & vbCrLf & "No: To do nothing", MsgBoxStyle.YesNo)
            If p = MsgBoxResult.Yes Then
                'replacing item
                Dim node As XmlNode = doc.DocumentElement.ChildNodes(i)
                node.InnerXml = Rich1.Rtf.ToString()
                node.Attributes("Title").InnerXml = TitleText.Text.Trim
                node.Attributes("Time").InnerXml = Date.Now
                node.Attributes("Remind").InnerXml = getReminder()
                doc.Save(storage & "\Document.xml")
            End If
        Else
            Dim nod As XmlNode = doc.CreateElement("Note")
            nod.Attributes.Append(doc.CreateAttribute("Title"))
            nod.Attributes.Append(doc.CreateAttribute("Time"))
            nod.Attributes.Append(doc.CreateAttribute("Remind"))
            nod.InnerXml = Rich1.Rtf.ToString()
            nod.Attributes("Title").InnerXml = TitleText.Text.Trim
            nod.Attributes("Time").InnerXml = Date.Now
            nod.Attributes("Remind").InnerXml = getReminder()
            doc.DocumentElement.AppendChild(nod)
            doc.Save(file)
        End If
        loadItems("")
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        startWithWindows = CheckBox1.Checked
        Try
            Dim des As String = My.Computer.FileSystem.SpecialDirectories.Programs & "\Startup\TakeNote.lnk"
            Dim source As String = CurDir() & "\Shortcut.lnk"
            If CheckBox1.Checked Then
                If Not My.Computer.FileSystem.FileExists(des) Then
                    My.Computer.FileSystem.CopyFile(source, des)
                End If
            Else
                My.Computer.FileSystem.DeleteFile(des)
            End If
        Catch ex As Exception
            CheckBox1.Checked = False
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        showinBar = CheckBox2.Checked
        Me.ShowInTaskbar = CheckBox2.Checked
    End Sub

    Private Sub TabPage1_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles TabPage1.Enter
        AcceptButton = AddnoteButton
    End Sub

    Private Sub TabPage1_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles TabPage1.Leave
        AcceptButton = Nothing
    End Sub

    Private Sub ViewOrEditToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ViewOrEditToolStripMenuItem.Click
        Dim fmr As New Form2(ListView1.FocusedItem.Tag)
        fmr.Show()
    End Sub

    Private Sub DeleteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeleteToolStripMenuItem.Click
        Try
            Dim p As Integer = ListView1.FocusedItem.Tag
            Dim file As String = storage & "\Document.xml"
            Dim doc As New XmlDocument
            doc.Load(storage & "\Document.xml")
            Dim node As XmlNode = doc.DocumentElement.ChildNodes(p)
            doc.DocumentElement.RemoveChild(node)
            doc.Save(file)
            loadItems()
        Catch ex As Exception
            MsgBox("Can not delete this note." & vbCrLf & ex.Message)
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        For Each itm As ListViewItem In ListView1.Items
            If itm.SubItems(3).Text = False Then
                Dim now As Long = Date.Now.ToFileTime
                Dim remind() As String = Split(itm.SubItems(2).Text, ";")
                If remind(1) > now And now > remind(0) Then
                    System.Media.SystemSounds.Hand.Play()
                    Dim frm As New Form3(itm.Text, itm.SubItems(4).Text, itm.Tag)
                    frm.ShowDialog()
                    itm.SubItems(3).Text = True
                    itm.ToolTipText &= vbCrLf & "Showed: Yes"
                End If
            End If
        Next
    End Sub
End Class

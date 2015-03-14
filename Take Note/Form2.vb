Imports System.Xml

Public Class Form2

    Private Sub CloseButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseButton.Click
        Me.Close()
    End Sub

    Dim doc As XmlDocument
    Dim index As Integer
    Public Sub New(ByVal i As Integer)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Dim file As String = Form1.storage & "\Document.xml"
        doc = New XmlDocument
        doc.Load(file)
        'add data
        Dim node As XmlNode = doc.DocumentElement.ChildNodes(index)
        TextBox1.Text = node.Attributes("Title").InnerXml
        RichTextBox1.Rtf = node.InnerXml
        showReminder(node.Attributes("Remind").InnerXml)
        Label6.Text = "Last Edit: " & node.Attributes("Time").InnerXml
    End Sub

    Private Sub SaveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveButton.Click
        Dim file As String = Form1.storage & "\Document.xml"
        Dim node As XmlNode = doc.DocumentElement.ChildNodes(index)
        node.InnerXml = RichTextBox1.Rtf
        node.Attributes("Title").InnerXml = TextBox1.Text.Trim
        node.Attributes("Time").InnerXml = Date.Now
        node.Attributes("Remind").InnerXml = getReminder()
        doc.Save(file)
        Form1.loadItems()
        Me.Close()
    End Sub

    Function getReminder() As String
        Dim time As Long = DatePicker1.Value.ToFileTime
        Dim valid As Long = ValidHours.Value
        If Not DatePicker1.Checked Then valid = 0
        Dim lastTime As Long = time + 36000000000 * valid
        Return time & ";" & lastTime
    End Function

    Sub showReminder(ByVal txt As String)
        Dim times() As String = Split(txt, ";")
        DatePicker1.Value = Date.FromFileTime(times(0))
        ValidHours.Value = (CLng(times(1)) - CLng(times(0))) / 36000000000
        If ValidHours.Value = 0 Then
            ValidHours.Value = 1
            DatePicker1.Checked = True
        End If
    End Sub
End Class
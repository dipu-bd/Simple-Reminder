Public Class Form3

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim frm As New Form2(_tag)
        frm.Show()
        Me.Close()
    End Sub

    Dim _tag As Integer
    Public Sub New(ByVal title As String, ByVal rtf As String, ByVal i As Integer)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Text = title
        RichTextBox1.Rtf = rtf
        tag = i
    End Sub
     
     
End Class
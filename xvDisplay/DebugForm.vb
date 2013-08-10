Public Class DebugForm

    Public IfClose As Boolean = False

    Private Sub DebugForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Top = -2
        TextBox1.Left = -2
        TextBox1.Height = Me.ClientSize.Height + 4
        TextBox1.Width = Me.ClientSize.Width + 4
    End Sub

    Public Sub Print(ByVal str As String)
        TextBox1.AppendText(String.Format("[{0}] ", Date.Now.ToString("HH:mm:ss.fff")))
        If Not str.EndsWith(vbCrLf) Then
            str += vbCrLf
        End If
        TextBox1.AppendText(str)
    End Sub

    Private Sub DebugForm_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        TextBox1.Height = Me.ClientSize.Height + 4
        TextBox1.Width = Me.ClientSize.Width + 4
    End Sub

    Private Sub DebugForm_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Me.Hide()
        If Not IfClose Then
            e.Cancel = True
        End If
    End Sub
End Class
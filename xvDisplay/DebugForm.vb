Public Class DebugForm

    Private Sub DebugForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TextBox1.Top = -2
        TextBox1.Left = -2
        TextBox1.Height = Me.ClientSize.Height + 4
        TextBox1.Width = Me.ClientSize.Width + 4
    End Sub

    Public Sub Print(ByVal str As String)
        TextBox1.AppendText(String.Format("[{0}] ", Date.Now.ToString("HH:mm:ss.fff")))
        TextBox1.AppendText(str)
    End Sub

    Private Sub DebugForm_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        TextBox1.Height = Me.ClientSize.Height + 4
        TextBox1.Width = Me.ClientSize.Width + 4
    End Sub
End Class
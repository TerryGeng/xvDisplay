Public Class Form1
    Dim ctrl As New Controller(Me)

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ctrl.Start()
    End Sub

    Private Sub Form1_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles MyBase.KeyPress
        If e.KeyChar = "c" Then
            ctrl.Dispose()
            ctrl = New Controller(Me)
            ctrl.Start()
        ElseIf e.KeyChar = "l" Then
            For i As Integer = 0 To 100
                ctrl.Dispose()
                ctrl = New Controller(Me)
                ctrl.Start()
            Next
            GC.Collect()
        End If
    End Sub
End Class

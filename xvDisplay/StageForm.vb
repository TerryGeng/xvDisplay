Public Class StageForm
    Dim ctrl As Controller

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SBSLibrary.StandardIO.Output = AddressOf DebugForm.Print
        ctrl = New Controller(Me)
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
        ElseIf e.KeyChar = "d" Then
            DebugForm.Visible = Not DebugForm.Visible
        End If
    End Sub
End Class

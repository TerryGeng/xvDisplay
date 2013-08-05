Public Class Controller
    Implements IDisposable

    Dim StageForm As Form
    Dim Drawer As Draw
    Dim ItemTable As Item.ItemTable
    Dim ResTable As Resources.ResTable
    Dim Configuration As Configuration

    Sub New(ByRef _form As Form)
        StageForm = _form
        ItemTable = New Item.ItemTable()
        ResTable = New Resources.ResTable()
        Drawer = New Draw(StageForm, ItemTable, ResTable)

        Configuration = New Configuration(StageForm, ItemTable, ResTable, Drawer)
    End Sub

    Sub Start()
        Configuration.LoadConfFile("E:\test.xdc", False)
        Configuration.LoadConfFile("E:\test2.xdc")
        DrawItemSet("newSecond")
    End Sub

    Sub DrawItemSet(ByVal setName As String)
        Dim childs() As System.UInt16 = ItemTable.GetItemByName(setName).Childs
        If childs Is Nothing Then
            Throw New ApplicationException("Error: Unknown ItemSet '" + setName + "'.")
        End If

        Drawer.LoadItemsToDraw(childs)
        Drawer.Draw()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Drawer.Dispose()
                ItemTable.Dispose()
                ResTable.Dispose()
            End If
        End If
        Me.disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

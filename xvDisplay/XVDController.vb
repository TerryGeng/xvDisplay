Public Class Controller
    Implements IDisposable

    Public Const Version As String = "v0.1a1"

    Friend StageForm As Form
    Friend Drawer As Draw
    Friend ItemTable As Item.ItemTable
    Friend ResTable As Resources.ResTable
    Friend Configuration As Configuration
    Friend ScriptEngine As SBSLibrary.SBSEngine
    Friend ScriptFunctions As ScriptFuncLib

    Sub New(ByRef _form As Form)
        SBSLibrary.StandardIO.PrintLine("xvDisplay Controller " + Version + " Initializing... ")
        StageForm = _form
        ItemTable = New Item.ItemTable()
        ResTable = New Resources.ResTable()

        SBSLibrary.StandardIO.PrintLine("Script Engine Initializing...")
        ScriptEngine = New SBSLibrary.SBSEngine()
        ScriptFunctions = New ScriptFuncLib(Me)

        Drawer = New Draw(StageForm, ItemTable, ResTable, ScriptEngine)
        Configuration = New Configuration(StageForm, ItemTable, ResTable, Drawer, ScriptEngine)

        SBSLibrary.StandardIO.PrintLine("Initializing done.")
    End Sub

    Sub Start()
        Try
            Configuration.LoadConfFile("startup.xdc", False)
        Catch ex As Exception
            SBSLibrary.StandardIO.PrintLine("Error: " + ex.Message)
            DebugForm.Show()
        End Try
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

Imports SBSLibrary

Public Class ScriptFuncLib
    Dim Controller As Controller

    Sub New(ByRef _controller As Controller)
        Controller = _controller

        Controller.ScriptEngine.AddFunction(New LibFunction("xvdreadconf", AddressOf XVDReadConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdreadglobalconf", AddressOf XVDReadGlobalConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdremovetempconf", AddressOf XVDRemoveTempConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvddrawitemset", AddressOf XVDDrawItemSet, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvddraw", AddressOf XVDDraw))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdchangeitemres", AddressOf XVDChangeItemRes, 4))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdsetdrawingupdateflag", AddressOf XVDSetDrawingUpdateFlag))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdexit", AddressOf XVDExit))

    End Sub

    Public Function XVDReadConf(ByRef argsList As ArrayList) As SBSValue
        Dim confPath As SBSValue = argsList(0)

        Controller.Configuration.LoadConfFile(CStr(confPath.Value), True)
        Return Nothing
    End Function

    Public Function XVDReadGlobalConf(ByRef argsList As ArrayList) As SBSValue
        Dim confPath As SBSValue = argsList(0)

        Controller.Configuration.LoadConfFile(CStr(confPath.Value), False)
        Return Nothing
    End Function

    Public Function XVDRemoveTempConf(ByRef argsList As ArrayList) As SBSValue
        Controller.ResTable.DisposeTempRes()
        Controller.ItemTable.DisposeTempTags()
        Return Nothing
    End Function

    Public Function XVDDrawItemSet(ByRef argslist As ArrayList) As SBSValue
        Dim setName As SBSValue = argslist(0)
        Controller.Drawer.DrawItemSet(CStr(setName.Value))
        Return Nothing
    End Function

    Public Function XVDDraw(ByRef argslist As ArrayList) As SBSValue
        Controller.Drawer.Draw()
        Return Nothing
    End Function

    Public Function XVDChangeItemRes(ByRef argslist As ArrayList) As SBSValue
        Dim itemName As String = CStr(argslist(0).Value)
        Dim resType As String = CStr(argslist(1).Value)
        Dim resPtr As UInt16 = Controller.ResTable.GetResPtr(CStr(argslist(2).Value))
        Dim status As UInt16 = GetStatusByName(CStr(argslist(3).Value))

        Dim item As Item.ItemTag = Controller.ItemTable.GetItemByName(itemName)
        Dim resptrs?() As UInt16 = GetResListFromItem(item, resType)

        If resptrs IsNot Nothing Then
            resptrs(status) = resPtr
            Controller.Drawer.Update = Draw.UpdateFlag.Reflow
        Else
            Throw New ApplicationException("Undefined item '" + itemName + "'.")
        End If


        Return Nothing
    End Function

    Public Function XVDSetDrawingUpdateFlag(ByRef argslist As ArrayList) As SBSValue
        Dim flag As String = CStr(argslist(0).Value).ToLower()

        If flag = "repaint" Then
            Controller.Drawer.Update = Draw.UpdateFlag.Repaint
        ElseIf flag = "reflow" Then
            Controller.Drawer.Update = Draw.UpdateFlag.Reflow
        End If

        Return Nothing
    End Function

    Public Function XVDExit(ByRef argslist As ArrayList) As SBSValue
        DebugForm.IfClose = True
        Application.Exit()
        Return Nothing
    End Function

    Private Function GetStatusByName(ByVal name As String) As Item.EventType
        name = name.ToLower()
        If name = "normal" OrElse name = "" Then
            Return Item.EventType.Normal
        ElseIf name = "hover" Then
            Return Item.EventType.Hover
        ElseIf name = "press" Then
            Return Item.EventType.Press
        End If
        Return Nothing
    End Function

    Private Function GetResListFromItem(ByRef item As Item.ItemTag, ByVal resType As String) As UInt16?()
        resType = resType.ToLower()
        If resType = "image" Then
            Return item.Content.Image
        ElseIf resType = "text" Then
            Return item.Content.Text
        ElseIf resType = "style" Then
            Return item.Content.Style
        ElseIf resType = "script" Then
            Return item.Content.Script
        End If
        Return Nothing
    End Function
End Class

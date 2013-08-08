Imports SBSLibrary

Public Class ScriptFuncLib
    Dim Controller As Controller

    Sub New(ByRef _controller As Controller)
        Controller = _controller

        Controller.ScriptEngine.AddFunction(New LibFunction("xvdreadconf", AddressOf XVDReadConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdreadglobalconf", AddressOf XVDReadGlobalConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvdremovetempconf", AddressOf XVDRemoveTempConf, 1))
        Controller.ScriptEngine.AddFunction(New LibFunction("xvddrawitemset", AddressOf XVDDrawItemSet, 1))
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

    Public Function XVDExit(ByRef argslist As ArrayList) As SBSValue
        Application.Exit()
        Return Nothing
    End Function
End Class

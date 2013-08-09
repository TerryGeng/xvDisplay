Imports System.Xml
Imports StandardIO = SBSLibrary.StandardIO

Public Class Configuration
    Declare Function GetTickCount Lib "kernel32" () As Long

    Const FILE_VERSION As String = "v0.1"

    Dim StageForm As Form
    Dim ItemTable As Item.ItemTable
    Dim ResTable As Resources.ResTable
    Dim Draw As Draw
    Dim ScriptEngine As SBSLibrary.SBSEngine

    Dim TempFlag As Boolean = False
    Dim itemPrefix As String = "--"
    Dim defaultStylePtr As UInt16 = 0

    Sub New(ByRef _stageForm As Form, ByRef _itemTable As Item.ItemTable, ByRef _resTable As Resources.ResTable, ByRef _draw As Draw, ByRef _scriptEngine As SBSLibrary.SBSEngine)
        StageForm = _stageForm
        ItemTable = _itemTable
        ResTable = _resTable
        Draw = _draw
        ScriptEngine = _scriptEngine

    End Sub

    Public Sub LoadConfFile(ByVal fileName As String, Optional ByVal temp As Boolean = True)
        Dim startTime As Long = GetTickCount()
        StandardIO.PrintLine("Conf: Loading configuration file: " + fileName)

        If temp Then
            If Not ResTable.HasTempFlag AndAlso Not ItemTable.HasTempFlag Then
                TempFlag = True
                ResTable.SetTempFlag()
                ItemTable.SetTempFlag()
            End If
        Else
            If ResTable.HasTempFlag OrElse ItemTable.HasTempFlag Then
                ThrowError("LoadConfFile", "--", "Global configurations can only be loaded before any temporary configurations.")
            End If
        End If

        Using ConfReader As XmlReader = XmlReader.Create(fileName)
            ConfReader.ReadStartElement("xdConfiguration")

            If Not CheckFileVersion(ConfReader) Then
                ThrowError("LoadConfFile", "--", "Unavailable configuration file version.")
            End If

            While ConfReader.Read()
                If ConfReader.IsStartElement("stage") Then
                    LoadStageConf(ConfReader)
                ElseIf ConfReader.IsStartElement("resources") Then
                    LoadRes(ConfReader)
                ElseIf ConfReader.IsStartElement("itemSet") Then
                    LoadItemSet(ConfReader)
                ElseIf ConfReader.IsStartElement("script") Then
                    Dim ptr As UInt16 = LoadScript(ConfReader)
                    Dim start As Integer = GetTickCount()

                    StandardIO.PrintLine("Conf: == Executing script... ==")
                    ScriptEngine.Perform(ResTable.GetScriptRange(ptr))
                    StandardIO.PrintLine("Conf: == Executing done. Cost " + CStr(GetTickCount() - start) + "ms. ==")
                End If
            End While
        End Using

        StandardIO.PrintLine("Conf: Done on loading " + fileName + ". Cost " + CStr(GetTickCount() - startTime) + "ms.")
    End Sub

    Private Sub LoadStageConf(ByRef ConfReader As XmlReader)
        ConfReader.ReadStartElement("stage")
        While ConfReader.Read()
            If ConfReader.IsStartElement() Then
                If ConfReader.IsStartElement("title") Then
                    StageForm.Text = ConfReader.ReadElementString()
                ElseIf ConfReader.IsStartElement("size") Then
                    Dim width As String = ConfReader.GetAttribute("width")
                    Dim height As String = ConfReader.GetAttribute("height")
                    If width IsNot Nothing Then
                        Draw.ChangeStageSize(width, height)
                        ConfReader.Skip()
                    End If
                End If
            ElseIf ConfReader.NodeType = XmlNodeType.EndElement Then
                Return
            End If
        End While
    End Sub

    Sub LoadRes(ByRef Confreader As XmlReader)
        Confreader.ReadStartElement("resources")

        While Confreader.Read()
            Dim res As New Resources.ResTag()
            If Confreader.IsStartElement("textRes") Then ' <textRes name="[Name]">[Text]</testRes>
                res.Name = Confreader.GetAttribute("name")
                res.Type = Resources.ResType.Text
                res.Text = Confreader.ReadElementContentAsString().Replace("\n", vbCrLf)

                ResTable.AddRes(res)
            ElseIf Confreader.IsStartElement("styleRes") Then
                LoadStyle(Confreader)
            ElseIf Confreader.IsStartElement("imageRes") Then
                Dim ImageSrc As String = Confreader.GetAttribute("src")
                res.Name = Confreader.GetAttribute("name")
                res.Type = Resources.ResType.Image

                StandardIO.PrintLine("Conf: Loading image '" + res.Name + "': " + ImageSrc)

                If res.Name Is Nothing Then
                    Warning("LoadRes", "resources." + res.Name, "Empty resource name (This means no item can cite this resource).")
                End If

                If ImageSrc Is Nothing Then
                    ThrowError("LoadRes", "resources." + res.Name, "Expect attribute 'src' for image resource.")
                End If

                Try
                    res.Image = Image.FromFile(ImageSrc)
                Catch ex As Exception
                    ThrowError("LoadRes", "resources." + res.Name, "Unable to load image from file: " + ex.Message)
                End Try

                ResTable.AddRes(res)
            ElseIf Confreader.NodeType = XmlNodeType.EndElement Then
                Return
            Else
                Warning("LoadRes", "resources", "Unexpected element '" + Confreader.Name + "'.")
            End If
        End While
    End Sub

    Function LoadStyle(ByRef ConfReader As XmlReader, Optional ByRef baseStylePtr? As Integer = Nothing) As UInt16
        If ConfReader.MoveToAttribute("res") Then
            Dim styleName As String = ConfReader.ReadContentAsString()
            Dim stylePtr As UInt16 = ResTable.GetResPtr(styleName)
            ConfReader.MoveToElement()
            Return stylePtr
        End If

        Dim baseName As String = ConfReader.GetAttribute("base")
        Dim baseStyleContent As Resources.Style = Nothing
        Dim tag As New Resources.ResTag

        If baseName IsNot Nothing Then
            baseStyleContent = ResTable.GetStyleRes(ResTable.GetResPtr(baseName))
        ElseIf baseStylePtr.HasValue Then
            baseStyleContent = ResTable.GetStyleRes(baseStylePtr)
        Else
            baseStyleContent = ResTable.GetStyleRes(defaultStylePtr)
        End If

        Dim styleContent As New Resources.Style
        Dim fontFamily As FontFamily = Nothing
        Dim fontSize As String = Nothing
        Dim color As String = Nothing
        Dim transparency As String = Nothing

        tag.Name = ConfReader.GetAttribute("name")

        ConfReader.ReadStartElement()

        tag.Type = Resources.ResType.Style

        Dim fontDef As Boolean = False
        Dim colorDef As Boolean = False

        While ConfReader.Read()
            If ConfReader.IsStartElement("fontFamily") Then
                fontDef = True
                fontFamily = New FontFamily(ConfReader.ReadElementContentAsString())
            ElseIf ConfReader.IsStartElement("fontSize") Then
                fontDef = True
                fontSize = ConfReader.ReadElementContentAsString()
            ElseIf ConfReader.IsStartElement("color") Then
                colorDef = True
                color = ConfReader.ReadElementContentAsString()
            ElseIf ConfReader.IsStartElement("transparency") Then
                colorDef = True
                transparency = ConfReader.ReadElementContentAsString()
            ElseIf ConfReader.NodeType = XmlNodeType.EndElement Then
                Exit While
            Else
                Warning("LoadStyle", "resources", "Unexpected element '" + ConfReader.Name + "'.")
            End If
        End While

        If fontDef Then
            Dim intSize As Integer = 0
            If fontFamily Is Nothing Then
                fontFamily = baseStyleContent.Font.FontFamily
            End If

            If fontSize Is Nothing Then
                intSize = baseStyleContent.Font.Size
            Else
                intSize = CInt(fontSize)
            End If

            styleContent.Font = New Font(fontFamily, intSize)
        Else
            styleContent.Font = baseStyleContent.Font
        End If

        If colorDef Then
            Dim intTrans As Integer = 0
            Dim mColor As Color = Nothing

            If transparency Is Nothing Then
                intTrans = baseStyleContent.Brush.Color.A
            Else
                intTrans = CInt(transparency)
                If intTrans > 255 Then
                    Warning("LoadStyle", itemPrefix, "Illegal transparency (0~255).")
                    intTrans = 255
                End If
            End If

            If color Is Nothing Then
                mColor = baseStyleContent.Brush.Color
            Else
                mColor = GetColorFromCode(color, intTrans)
            End If

            styleContent.Brush = New SolidBrush(mColor)
            styleContent.TransParency = intTrans
        Else
            styleContent.Brush = baseStyleContent.Brush
            styleContent.TransParency = baseStyleContent.TransParency
        End If

        tag.Style = styleContent

        Return ResTable.AddRes(tag)
    End Function

    Private Function LoadScript(ByRef ConfReader As XmlReader) As UInt16
        Dim tag As New Resources.ResTag
        Dim name As String = ConfReader.GetAttribute("name")
        Dim code As String = ConfReader.ReadElementContentAsString()

        tag.Name = name
        tag.Type = Resources.ResType.Script
        tag.ScriptRange = ScriptEngine.LoadCode(code)

        Return ResTable.AddRes(tag)
    End Function

    Sub LoadItemSet(ByRef ConfReader As XmlReader)
        Dim setItem As Item.ItemTag = New Item.ItemTag()
        Dim originPrefix As String = itemPrefix
        setItem.Name = ConfReader.GetAttribute("name")

        StandardIO.PrintLine("Conf: Loading item set: " + setItem.Name)

        setItem.Type = Item.ItemType.ItemSet
        itemPrefix = setItem.Name


        setItem.Childs = LoadItems(ConfReader, {0, Nothing, Nothing, Nothing})
        ItemTable.AddItem(setItem)
        itemPrefix = originPrefix
    End Sub

    Private Function LoadItems(ByRef ConfReader As XmlReader, Optional ByRef baseStylePtr?() As UInt16 = Nothing) As UInt16()
        Dim Childs As New ArrayList()

        ConfReader.ReadStartElement()

        While ConfReader.Read()
            If ConfReader.IsStartElement("block") Then
                Childs.Add(LoadBlock(ConfReader, baseStylePtr))
            ElseIf ConfReader.NodeType = XmlNodeType.EndElement Then
                Dim ChildArray() As UShort = Childs.ToArray(GetType(UShort))
                Childs.Clear()
                Return ChildArray
            Else
                Childs.Add(LoadNormalItem(ConfReader, baseStylePtr))
            End If
        End While

        Return Nothing
    End Function

    Private Function LoadBlock(ByRef ConfReader As XmlReader, Optional ByRef baseStylePtr?() As UInt16 = Nothing) As UInt16
        Dim tag As Item.ItemTag
        Dim baseTagName As String = ConfReader.GetAttribute("base")
        Dim enable As String = ConfReader.GetAttribute("enable")

        If baseTagName IsNot Nothing Then
            tag = ItemTable.CloneItem(ItemTable.GetItemByName(baseTagName))
        Else
            tag = New Item.ItemTag
            If baseStylePtr IsNot Nothing Then
                tag.Content.Style = baseStylePtr
            End If
        End If

        Dim originPrefix As String = itemPrefix

        If enable IsNot Nothing Then
            enable.ToLower()
            If enable = "true" Then
                tag.Enable = True
            ElseIf enable = "false" Then
                tag.Enable = False
            Else
                Warning("LoadBlock", itemPrefix, "Unexpected attribute value for 'enable'.")
            End If
        Else
            tag.Enable = True
        End If

        tag.Name = itemPrefix + "." + ConfReader.GetAttribute("name")
        itemPrefix = tag.Name
        tag.Type = Item.ItemType.Block

        If Not ConfReader.IsEmptyElement() Then
            ConfReader.ReadStartElement()

            While ConfReader.Read()
                If ConfReader.IsStartElement("range") Then
                    tag.Content.Range = LoadRange(ConfReader)
                ElseIf ConfReader.IsStartElement("style") Then
                    Dim status As Item.EventType = GetEventType(ConfReader.GetAttribute("status"))
                    tag.Content.Style(status) = LoadStyle(ConfReader)
                ElseIf ConfReader.IsStartElement("items") Then
                    tag.Childs = LoadItems(ConfReader, tag.Content.Style)
                ElseIf ConfReader.NodeType = XmlNodeType.EndElement Then
                    itemPrefix = originPrefix
                    Exit While
                Else
                    Warning("LoadBlock", "resources", "Unexpected element '" + ConfReader.Name + "'.")
                End If
            End While
        End If

        Return ItemTable.AddItem(tag)
    End Function

    Private Function LoadNormalItem(ByRef ConfReader As XmlReader, Optional ByRef baseStylePtr?() As UInt16 = Nothing) As UInt16
        Dim tag As New Item.ItemTag
        Dim name As String = ConfReader.GetAttribute("name")
        Dim enable As String = ConfReader.GetAttribute("enable")

        If ConfReader.IsStartElement("text") Then
            tag.Type = Item.ItemType.Text
        ElseIf ConfReader.IsStartElement("image") Then
            tag.Type = Item.ItemType.Image
        End If

        If baseStylePtr IsNot Nothing Then
            tag.Content.Style = baseStylePtr.Clone()
        End If
        If name IsNot Nothing Then
            tag.Name = itemPrefix + "." + name
        End If

        If enable IsNot Nothing Then
            enable.ToLower()
            If enable = "true" Then
                tag.Enable = True
            ElseIf enable = "false" Then
                tag.Enable = False
            Else
                Warning("LoadNormalItem", itemPrefix, "Unexpected attribute value for 'enable'.")
            End If
        Else
            tag.Enable = True
        End If

        If Not ConfReader.IsEmptyElement() Then
            ConfReader.ReadStartElement()

            While ConfReader.Read()
                If ConfReader.IsStartElement("value") Then
                    Dim resType As Resources.ResType = Resources.ResType.Undefined
                    Dim resPtr As UInt16 = 0
                    Dim resName As String = ConfReader.GetAttribute("res")
                    Dim status As Item.EventType = GetEventType(ConfReader.GetAttribute("status"))

                    If resName IsNot Nothing Then
                        resPtr = ResTable.GetResPtr(resName)
                    ElseIf tag.Type = Item.ItemType.Text Then
                        resPtr = ResTable.AddRes(New Resources.ResTag("", ConfReader.ReadElementContentAsString().Replace("\n", vbCrLf)))
                    Else
                        Warning("LoadItems", itemPrefix + "." + "name", "Unexpected resource name for '" + name + "' in element 'value'.")
                    End If

                    If tag.Type = Item.ItemType.Text Then
                        resType = Resources.ResType.Text
                        tag.Content.Text(status) = resPtr
                    ElseIf tag.Type = Item.ItemType.Image Then
                        resType = Resources.ResType.Image
                        tag.Content.Image(status) = resPtr
                    Else
                        Warning("LoadNormalItem", itemPrefix + "." + "name", "Unexpected element 'value' for '" + name + "'.")
                    End If
                ElseIf ConfReader.IsStartElement("range") Then
                    tag.Content.Range = LoadRange(ConfReader)
                ElseIf ConfReader.IsStartElement("style") Then
                    Dim status As Item.EventType = GetEventType(ConfReader.GetAttribute("status"))
                    tag.Content.Style(status) = LoadStyle(ConfReader)
                ElseIf ConfReader.NodeType = XmlNodeType.EndElement Then
                    Exit While
                Else
                    Warning("LoadNormalItem", itemPrefix, "Unexpected element '" + ConfReader.Name + "'.")
                End If
            End While
        End If

        Return ItemTable.AddItem(tag)
    End Function

    Private Function CheckFileVersion(ByRef ConfReader As XmlReader) As Boolean
        ConfReader.MoveToContent()

        If ConfReader.IsStartElement("fileVersion") AndAlso _
            ConfReader.ReadElementString() = FILE_VERSION Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function GetColorFromCode(ByVal code As String, ByVal transparent As Integer) As Color ' Get color from color code("#xxxxxx" or "red" in the "color:xxx")
        If code.StartsWith("#") Then ' #000000
            Return Color.FromArgb(transparent, Convert.ToInt32(code.Substring(1, 2), 16), Convert.ToInt32(code.Substring(3, 2), 16), Convert.ToInt32(code.Substring(5, 2), 16))
        Else ' Red
            Dim mColor As Color = Color.FromName(code)
            Return Color.FromArgb(transparent, mColor)
        End If
    End Function

    Private Function LoadRange(ByRef Confreader As XmlReader) As Rectangle
        Dim rec As New Rectangle
        Dim range As String = Confreader.GetAttribute("xywh")
        If range IsNot Nothing Then
            Dim num() As String = range.Split(",")
            If UBound(num) = 3 Then
                rec.X = CInt(num(0))
                rec.Y = CInt(num(1))
                rec.Width = CInt(num(2))
                rec.Height = CInt(num(3))
            ElseIf UBound(num) = 1 Then
                rec.X = CInt(num(0))
                rec.Y = CInt(num(1))
            Else
                Throw New ApplicationException()
            End If
        Else
            rec.X = CInt(Confreader.GetAttribute("x"))
            rec.Y = CInt(Confreader.GetAttribute("y"))
            rec.Width = CInt(Confreader.GetAttribute("width"))
            rec.Height = CInt(Confreader.GetAttribute("height"))
        End If
        Return rec
    End Function

    Private Function GetEventType(ByVal statusStr As String) As Item.EventType
        Dim status As Item.EventType = Item.EventType.Normal

        If statusStr IsNot Nothing Then
            statusStr = statusStr.ToLower()
            If statusStr = "normal" Then
                status = Item.EventType.Normal
            ElseIf statusStr = "hover" Then
                status = Item.EventType.Hover
            ElseIf statusStr = "press" Then
                status = Item.EventType.Press
            End If
        End If

        Return status
    End Function

    Private Sub Warning(ByVal funcName As String, ByVal Position As String, ByVal Message As String)
        MsgBox(String.Format("Configuration Warning ({1})[{0}]: {2}", funcName, Position, Message))
    End Sub

    Private Sub ThrowError(ByVal funcName As String, ByVal Position As String, ByVal Message As String)
        Throw New ApplicationException(String.Format("Configuration Error ({1})[{0}]: {2}", funcName, Position, Message))

    End Sub
End Class
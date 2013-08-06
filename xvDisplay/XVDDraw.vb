Imports ItemPtr = System.UInt16
Imports ResPtr = System.UInt16
Imports ImageTransparency = System.Byte
Imports xvDisplay.Item


Public Class Draw
    Implements IDisposable

    Public Enum UpdateFlag
        None
        Reflow
        Repaint
    End Enum

    Public Class DrawingItem
        Public Ptr As ItemPtr
        Public Tag As ItemTag
        Public ActualRange As Rectangle
        Public Status As Item.EventType

        Sub New(ByVal _ptr As ItemPtr, ByRef _tag As ItemTag, ByRef _actualRange As Rectangle)
            Ptr = _ptr ' TODO
            Tag = _tag
            ActualRange = _actualRange
            Status = Item.EventType.Normal
        End Sub
    End Class

    Public Class ItemEvent
        Public Delegate Sub EventHandler(ByVal itemPtr As ItemPtr, ByVal type As Item.EventType)

        Public Ptr As ItemPtr
        Public Type As Item.EventType
        Public Handler As EventHandler

        Sub New(ByVal _ptr As ItemPtr, ByVal _type As Item.EventType, ByRef _handler As EventHandler)
            Ptr = _ptr
            Type = _type
            Handler = _handler
        End Sub
    End Class

    Dim Stage As Form
    Dim StageGrap As Graphics

    Dim bufferBmp As Bitmap
    Dim bufferGrap As Graphics

    Dim ResTable As Resources.ResTable

    Dim ItemTable As ItemTable
    Dim ItemsToDraw As Hashtable
    Dim OriginalItems() As ItemPtr

    Dim ItemEvents As ArrayList

    Public UpdateF As UpdateFlag

    Sub New(ByRef _stageForm As Form, ByRef _itemTable As ItemTable, ByRef _resTable As Resources.ResTable)
        Stage = _stageForm
        ItemTable = _itemTable
        StageGrap = Graphics.FromHwnd(Stage.Handle)
        ResTable = _resTable
        ItemsToDraw = New Hashtable()
        ItemEvents = New ArrayList()
        OriginalItems = {}
        UpdateF = UpdateFlag.Reflow
        bufferBmp = New Bitmap(Stage.Width, Stage.Height)
        bufferGrap = Graphics.FromImage(bufferBmp)

        AddHandler Stage.Paint, AddressOf Draw
        AddHandler Stage.MouseMove, AddressOf OnMouseMove
    End Sub

    Private Property Update As UpdateFlag
        Get
            Return UpdateF
        End Get
        Set(ByVal update As UpdateFlag)
            UpdateF = update
            Draw()
        End Set
    End Property

    Public Sub ChangeStageSize(ByVal width As Integer, ByVal height As Integer)
        Stage.ClientSize = New Size(width, height)

        DisposeGraphicsAndBuffer()

        StageGrap = Graphics.FromHwnd(Stage.Handle)
        bufferBmp = New Bitmap(Stage.Width, Stage.Height)
        bufferGrap = Graphics.FromImage(bufferBmp)
    End Sub

    Private Sub ReloadGrap()
        StageGrap = Graphics.FromHwnd(Stage.Handle)
        bufferBmp = New Bitmap(Stage.Width, Stage.Height)
        bufferGrap = Graphics.FromImage(bufferBmp)
    End Sub

    Public Sub DrawItemSet(ByRef setName As String)
        OriginalItems = ItemTable.GetItemByName(setName).Childs
        If OriginalItems Is Nothing Then
            Throw New ApplicationException("Error: Unknown ItemSet '" + setName + "'.")
        End If

        Update = UpdateFlag.Reflow
        Draw()
    End Sub

    Public Sub LoadItemsToDraw(ByRef ptr() As ItemPtr, Optional ByRef originRange As Rectangle = Nothing)
        If ptr Is Nothing Then
            Return
        End If

        If originRange = Nothing Then
            originRange = New Rectangle(0, 0, Stage.Width, Stage.Height)
        End If
        For i As Integer = 0 To UBound(ptr)
            Dim mItem As ItemTag = ItemTable.GetItemByPtr(ptr(i))

            If mItem.Enable = False Then
                Continue For
            End If

            If mItem IsNot Nothing Then
                Dim addedNormalEvent As Boolean = False
                If mItem.Content.Image(Item.EventType.Hover) IsNot Nothing OrElse _
                    mItem.Content.Style(Item.EventType.Hover) IsNot Nothing OrElse _
                    mItem.Content.Text(Item.EventType.Hover) IsNot Nothing Then
                    ItemEvents.Add(New ItemEvent(ptr(i), Item.EventType.Hover, AddressOf ChangeItemStatus))
                    ItemEvents.Add(New ItemEvent(ptr(i), Item.EventType.Normal, AddressOf ChangeItemStatus))
                    addedNormalEvent = True
                End If

                If mItem.Content.Image(Item.EventType.Press) IsNot Nothing OrElse _
                    mItem.Content.Style(Item.EventType.Press) IsNot Nothing OrElse _
                    mItem.Content.Text(Item.EventType.Press) IsNot Nothing Then
                    ItemEvents.Add(New ItemEvent(ptr(i), Item.EventType.Press, AddressOf ChangeItemStatus))
                    If addedNormalEvent = False Then
                        ItemEvents.Add(New ItemEvent(ptr(i), Item.EventType.Normal, AddressOf ChangeItemStatus))
                    End If
                End If

                Dim actRange As New Rectangle
                actRange.Width = mItem.Content.Range.Width
                actRange.Height = mItem.Content.Range.Height
                actRange.X = mItem.Content.Range.X + originRange.X
                actRange.Y = mItem.Content.Range.Y + originRange.Y

                If mItem.Type = Item.ItemType.Text Then
                    If actRange.Width = 0 OrElse actRange.Height = 0 Then
                        Dim text As String = ResTable.GetTextRes(mItem.Content.Text(Item.EventType.Normal))
                        Dim font As Font = ResTable.GetStyleRes(mItem.Content.Style(Item.EventType.Normal)).Font
                        Dim actSize As SizeF = bufferGrap.MeasureString(text, font, New SizeF(originRange.Width, originRange.Height))
                        actRange.Size = actSize.ToSize()
                        actRange.Width += 1
                    End If
                    ItemsToDraw.Add(ptr(i), New DrawingItem(ptr(i), mItem, actRange))
                ElseIf mItem.Type = Item.ItemType.Image Then
                    If actRange.Width = 0 OrElse actRange.Height = 0 Then
                        Dim mImage As Image = ResTable.GetImageRes(mItem.Content.Image(Item.EventType.Normal))
                        actRange.Size = mImage.Size
                    End If
                    ItemsToDraw.Add(ptr(i), New DrawingItem(ptr(i), mItem, actRange))
                ElseIf mItem.Type = Item.ItemType.Block Then
                    LoadItemsToDraw(mItem.Childs, mItem.Content.Range)
                End If
            End If
        Next
    End Sub

    Private Sub ChangeItemStatus(ByVal itemPtr As ItemPtr, ByVal Type As Item.EventType)
        Dim drawingItem As DrawingItem = ItemsToDraw(itemPtr)

        drawingItem.Status = Type
    End Sub

    Public Sub DrawToBuffer()
        If Update = UpdateFlag.Reflow Then
            LoadItemsToDraw(OriginalItems)
            Update = UpdateFlag.Repaint
        End If

        If bufferGrap Is Nothing Then
            ReloadGrap()
        End If
        bufferGrap.Clear(Color.White)

        Dim values As New ArrayList(ItemsToDraw.Values)

        For i As Integer = values.Count - 1 To 0 Step -1
            Dim drawingItem As DrawingItem = values(i)
            Dim mItem As ItemTag = drawingItem.Tag
            Dim range As Rectangle = drawingItem.ActualRange

            If mItem.Type = Item.ItemType.Text Then
                DrawText(bufferGrap, mItem.Content, range, drawingItem.Status)
            ElseIf mItem.Type = Item.ItemType.Image Then
                DrawImage(bufferGrap, mItem.Content, range, drawingItem.Status)
            End If
        Next
    End Sub

    Public Sub DrawToStage()
        StageGrap.DrawImage(bufferBmp, 0, 0)
    End Sub

    Public Sub Draw()
        DrawToBuffer()
        DrawToStage()
    End Sub

    Private Sub DrawText(ByRef Grap As Graphics, ByRef content As ItemContent, ByVal range As Rectangle, Optional ByVal status As Item.EventType = Item.EventType.Normal)
        Dim text As String = ResTable.GetTextRes(content.Text(status))
        Dim style As Resources.Style = ResTable.GetStyleRes(content.Style(status))

        Grap.DrawString(text, style.Font, style.Brush, range)

        'If Text.Effect.Shadow.Enable = True Then
        '    Grap.DrawString(Text.Text, Text.Effect.Shadow.Font.Font, Text.Effect.Shadow.Font.Brush, New Rectangle(range.X + Text.Effect.Shadow.Offset, range.Y + Text.Effect.Shadow.Offset, range.Width, range.Height))
        'End If
    End Sub

    Private Sub DrawImage(ByRef Grap As Graphics, ByRef content As ItemContent, ByVal range As Rectangle, Optional ByVal status As Item.EventType = Item.EventType.Normal)
        Dim imagePtr? As ResPtr = content.Image(status)
        Dim stylePtr? As ResPtr = content.Style(status)
        Dim mImage As Image
        Dim style As Resources.Style

        If imagePtr IsNot Nothing Then
            mImage = ResTable.GetImageRes(imagePtr)
        Else
            mImage = ResTable.GetImageRes(content.Image(Item.EventType.Normal))
        End If
        If stylePtr IsNot Nothing Then
            style = ResTable.GetStyleRes(stylePtr)
        Else
            style = ResTable.GetStyleRes(content.Style(Item.EventType.Normal))
        End If

        If style.TransParency <> 225 Then
            Dim matrixItems As Single()() = { _
               New Single() {1, 0, 0, 0, 0}, _
               New Single() {0, 1, 0, 0, 0}, _
               New Single() {0, 0, 1, 0, 0}, _
               New Single() {0, 0, 0, CDbl(style.TransParency) / 225.0F, 0}, _
               New Single() {0, 0, 0, 0, 1}}

            Dim colorMatrix As New Imaging.ColorMatrix(matrixItems)
            Dim imageAttr As New Imaging.ImageAttributes()
            imageAttr.SetColorMatrix( _
               colorMatrix, _
              Imaging.ColorMatrixFlag.Default, _
               Imaging.ColorAdjustType.Bitmap)
            Grap.DrawImage(mImage, range, 0, 0, mImage.Width, mImage.Height, GraphicsUnit.Pixel, imageAttr)
        Else
            Grap.DrawImage(mImage, range)
        End If
    End Sub

    Private Sub OnMouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        For Each ItemEvent As ItemEvent In ItemEvents
            Dim status As Item.EventType = ItemEvent.Type
            If status = Item.EventType.Hover Then
                Dim dItem As DrawingItem = ItemsToDraw(ItemEvent.Ptr)
                If dItem.Status <> Item.EventType.Hover Then
                    Dim range As Rectangle = dItem.ActualRange

                    If InRectangle(e.Location, range) Then
                        dItem.Status = Item.EventType.Hover
                        Update = UpdateFlag.Repaint
                    End If
                End If
            ElseIf status = Item.EventType.Normal Then
                Dim dItem As DrawingItem = ItemsToDraw(ItemEvent.Ptr)
                If dItem.Status <> Item.EventType.Normal Then
                    Dim range As Rectangle = dItem.ActualRange

                    If Not InRectangle(e.Location, range) Then
                        dItem.Status = Item.EventType.Normal
                        Update = UpdateFlag.Repaint
                    End If
                End If
            End If
        Next
    End Sub

    Private Function InRectangle(ByVal Point As Point, ByVal Rectangle As Rectangle) As Boolean
        If Rectangle.X < Point.X AndAlso _
            Point.X < (Rectangle.X + Rectangle.Width) AndAlso _
            Rectangle.Y < Point.Y AndAlso _
            Point.Y < (Rectangle.Y + Rectangle.Height) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub CleanLoadedItem()
        ItemsToDraw.Clear()
        ItemEvents.Clear()
    End Sub

    Private Sub DisposeGraphicsAndBuffer()
        StageGrap.Dispose()
        bufferBmp.Dispose()
        bufferGrap.Dispose()
        StageGrap = Nothing
        bufferBmp = Nothing
        bufferGrap = Nothing
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 检测冗余的调用

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                DisposeGraphicsAndBuffer()
                ItemTable.Dispose()
            End If

            CleanLoadedItem()
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

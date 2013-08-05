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
        Public Tag As ItemTag
        Public ActualRange As Rectangle
        Public Update As UpdateFlag

        Sub New(ByRef _tag As ItemTag, ByRef _actualRange As Rectangle)
            Tag = _tag
            ActualRange = _actualRange
            Update = UpdateFlag.Reflow
        End Sub
    End Class

    Dim Stage As Form
    Dim StageGrap As Graphics

    Dim bufferBmp As Bitmap
    Dim bufferGrap As Graphics

    Dim ResTable As Resources.ResTable

    Dim ItemTable As ItemTable
    Dim ItemsToDraw As ArrayList

    Sub New(ByRef _stageForm As Form, ByRef _itemTable As ItemTable, ByRef _resTable As Resources.ResTable)
        Stage = _stageForm
        ItemTable = _itemTable
        StageGrap = Graphics.FromHwnd(Stage.Handle)
        ResTable = _resTable
        ItemsToDraw = New ArrayList()
        bufferBmp = New Bitmap(Stage.Width, Stage.Height)
        bufferGrap = Graphics.FromImage(bufferBmp)

        AddHandler Stage.Paint, AddressOf Draw
    End Sub

    Public Sub ChangeStageSize(ByVal width As Integer, ByVal height As Integer)
        Stage.Width = width
        Stage.Height = height

        DisposeGraphicsAndBuffer()

        StageGrap = Graphics.FromHwnd(Stage.Handle)
        bufferBmp = New Bitmap(Stage.Width, Stage.Height)
        bufferGrap = Graphics.FromImage(bufferBmp)
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
            If mItem IsNot Nothing Then
                Dim actRange As New Rectangle
                actRange.Width = mItem.Content.Range.Width
                actRange.Height = mItem.Content.Range.Height
                actRange.X = mItem.Content.Range.X + originRange.X
                actRange.Y = mItem.Content.Range.Y + originRange.Y

                If mItem.Type = Item.ItemType.Text Then
                    If actRange.Width = 0 OrElse actRange.Height = 0 Then
                        Dim text As String = ResTable.GetTextRes(mItem.Content.Text)
                        Dim font As Font = ResTable.GetStyleRes(mItem.Content.Style).Font
                        Dim actSize As SizeF = bufferGrap.MeasureString(text, font, New SizeF(originRange.Width, originRange.Height))
                        actRange.Size = actSize.ToSize()
                    End If
                    ItemsToDraw.Add(New DrawingItem(mItem, actRange))
                ElseIf mItem.Type = Item.ItemType.Image Then
                    If actRange.Width = 0 OrElse actRange.Height = 0 Then
                        Dim mImage As Image = ResTable.GetImageRes(mItem.Content.Image)
                        actRange.Size = mImage.Size
                    End If
                    ItemsToDraw.Add(New DrawingItem(mItem, actRange))
                ElseIf mItem.Type = Item.ItemType.Block Then
                    LoadItemsToDraw(mItem.Childs, mItem.Content.Range)
                End If
            End If
        Next
    End Sub

    Public Sub Draw()
        bufferGrap.Clear(Color.White)
        For i As Integer = 0 To ItemsToDraw.Count - 1
            Dim mItem As ItemTag = ItemsToDraw(i).Tag
            Dim range As Rectangle = ItemsToDraw(i).ActualRange

            If mItem.Type = Item.ItemType.Text Then
                DrawText(bufferGrap, mItem.Content, range)
            ElseIf mItem.Type = Item.ItemType.Image Then
                DrawImage(bufferGrap, mItem.Content, range)
            End If
        Next

        StageGrap.DrawImage(bufferBmp, 0, 0)
    End Sub

    Private Sub DrawText(ByRef Grap As Graphics, ByRef content As ItemContent, ByVal range As Rectangle)
        Dim text As String = ResTable.GetTextRes(content.Text)
        Dim style As Resources.Style = ResTable.GetStyleRes(content.Style)

        Grap.DrawString(text, style.Font, style.Brush, range)

        'If Text.Effect.Shadow.Enable = True Then
        '    Grap.DrawString(Text.Text, Text.Effect.Shadow.Font.Font, Text.Effect.Shadow.Font.Brush, New Rectangle(range.X + Text.Effect.Shadow.Offset, range.Y + Text.Effect.Shadow.Offset, range.Width, range.Height))
        'End If
    End Sub

    Private Sub DrawImage(ByRef Grap As Graphics, ByRef content As ItemContent, ByVal range As Rectangle)
        Dim mImage As Image = ResTable.GetImageRes(content.Image)

        Grap.DrawImage(mImage, range)
    End Sub

    Public Sub CleanLoadedItem()
        ItemsToDraw.Clear()
    End Sub

    Private Sub DisposeGraphicsAndBuffer()
        StageGrap.Dispose()
        bufferBmp.Dispose()
        bufferGrap.Dispose()
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

            ItemsToDraw.Clear()
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

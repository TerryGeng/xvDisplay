Imports ResPtr = System.UInt16

Namespace Resources
    Public Enum ResType
        Undefined
        Image
        Text
        Sound
        Style
    End Enum

    Public Class Style
        Implements IDisposable

        Public Font As Font = Nothing
        Public Brush As SolidBrush = Nothing
        Public TransParency? As Integer = Nothing

        Sub New(ByRef _font As Font, ByRef _brush As Brush, ByRef _transparency As Integer)
            Font = _font
            Brush = _brush
            TransParency = _transparency
        End Sub

        Sub New()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If Font IsNot Nothing Then
                        Font.Dispose()
                    End If
                    If Brush IsNot Nothing Then
                        Brush.Dispose()
                    End If
                End If

                TransParency = Nothing
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

    Public Class ResTag
        Implements IDisposable

        Public Name As String = ""
        Public Type As ResType = ResType.Undefined
        Public Text As String = ""
        Public Image As Image = Nothing
        Public Style As Style = Nothing

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If Image IsNot Nothing Then
                        Image.Dispose()
                    End If
                    If Style IsNot Nothing Then
                        Style.Dispose()
                    End If
                End If
                Text = ""
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

    Public Class ResTable
        Implements IDisposable

        Dim ResList As ArrayList
        Dim NameToAddress As Hashtable
        Dim TempResStartFlag As ResPtr = 65535

        Sub New()
            ResList = New ArrayList(40)
            NameToAddress = New Hashtable(40)

            Dim defaultStyle As New Resources.Style(New Font(New FontFamily("Verdana"), 16), _
                                                                     New SolidBrush(Drawing.Color.Black), _
                                                                     255)
            Dim defaultStyleTag As New ResTag
            defaultStyleTag.Type = ResType.Style
            defaultStyleTag.Style = defaultStyle

            AddRes(defaultStyleTag)
        End Sub

        Public Function AddRes(ByRef tag As ResTag) As ResPtr
            Dim address As ResPtr = ResList.Add(tag)
            If tag.Name <> "" Then
                NameToAddress.Add(tag.Name, address)
            End If

            Return address
        End Function

        Public Function GetResPtr(ByVal name As String) As ResPtr
            Return NameToAddress(name)
        End Function

        Public Function GetTextRes(ByVal ptr As ResPtr) As String
            Dim tag As ResTag = ResList(ptr)
            If tag IsNot Nothing AndAlso tag.Type = ResType.Text Then
                Return tag.Text
            Else
                Throw New ApplicationException("Undefined resources '" + CStr(ptr) + "'.")
                Return Nothing
            End If
        End Function

        Public Function GetImageRes(ByVal ptr As ResPtr) As Image
            Dim tag As ResTag = ResList(ptr)
            If tag IsNot Nothing AndAlso tag.Type = ResType.Image Then
                Return tag.Image
            Else
                Throw New ApplicationException("Undefined resources '" + CStr(ptr) + "'.")
                Return Nothing
            End If
        End Function

        Public Function GetStyleRes(ByVal ptr As ResPtr) As Style
            Dim tag As ResTag = ResList(ptr)
            If tag IsNot Nothing AndAlso tag.Type = ResType.Style Then
                Return tag.Style
            Else
                Throw New ApplicationException("Undefined resources '" + CStr(ptr) + "'.")
                Return Nothing
            End If
        End Function

        Public Sub SetTempFlag()
            TempResStartFlag = ResList.Count
        End Sub

        Public Sub DisposeTempRes()
            For i As Integer = TempResStartFlag To ResList.Count - 1
                NameToAddress.Remove(ResList(i).Name)
                ResList(i).Dispose()
            Next

            If TempResStartFlag < ResList.Count Then
                ResList.RemoveRange(TempResStartFlag, ResList.Count - 1 - TempResStartFlag)
            End If

            TempResStartFlag = 65535
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    For i As Integer = 0 To ResList.Count - 1
                        NameToAddress.Remove(ResList(i).Name)
                        ResList(i).Dispose()
                    Next
                End If

                NameToAddress.Clear()
                ResList.Clear()
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
End Namespace
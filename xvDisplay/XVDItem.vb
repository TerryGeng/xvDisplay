Imports ItemPtr = System.UInt16
Imports ResPtr = System.UInt16
Imports ImageTransparency = System.Byte
Imports ItemList = System.Collections.ArrayList

Namespace Item
    Public Enum ItemType As System.Byte
        Undefined
        Text
        Image
        ItemSet
        Block
    End Enum

    Public Enum EventType
        Normal
        Hover
        Press
    End Enum

    Public Class ItemTable
        Implements IDisposable
        Dim NameToAddress As Hashtable
        Dim Tags As ItemList
        Dim TempTagStartFlag As ItemPtr = 65535

        Sub New()
            Tags = New ArrayList(20)
            NameToAddress = New Hashtable(20)
        End Sub

        Sub Dispose() Implements IDisposable.Dispose
            NameToAddress.Clear()
            Tags.Clear()
        End Sub

        Public Function AddItem(ByRef _itemTag As ItemTag) As ItemPtr
            Dim address As Integer = Tags.Add(_itemTag)
            If _itemTag.Name IsNot Nothing Then
                NameToAddress.Add(_itemTag.Name, address)
            End If

            Return address
        End Function

        Public Function CloneItem(ByRef tag As ItemTag) As ItemTag
            Dim newTag As New ItemTag

            newTag.Type = tag.Type
            newTag.Enable = tag.Enable
            If tag.Childs IsNot Nothing Then
                ReDim newTag.Childs(UBound(tag.Childs))
                tag.Childs.CopyTo(newTag.Childs, 0)
            End If
            newTag.Content.Image = tag.Content.Image
            newTag.Content.Range.Location = tag.Content.Range.Location
            newTag.Content.Range.Size = tag.Content.Range.Size
            newTag.Content.Text = tag.Content.Text
            newTag.Content.Style = tag.Content.Style

            Return newTag
        End Function

        Public Function GetItemByPtr(ByVal ptr As ItemPtr) As ItemTag
            Return Tags(ptr)
        End Function

        Public Function GetItemByName(ByVal name As String) As ItemTag
            Return Tags(NameToAddress(name))
        End Function

        Public Sub SetTempFlag()
            TempTagStartFlag = Tags.Count
        End Sub

        Public Function HasTempFlag() As Boolean
            If TempTagStartFlag <> 65535 Then
                Return True
            End If
            Return False
        End Function

        Public Sub DisposeTempTags()
            If TempTagStartFlag < Tags.Count Then
                For i As Integer = TempTagStartFlag To Tags.Count - 1
                    NameToAddress.Remove(Tags(i).Name)
                Next

                Tags.RemoveRange(TempTagStartFlag, Tags.Count - 1 - TempTagStartFlag)
            End If

            TempTagStartFlag = 65535
        End Sub
    End Class

    Public Class ItemTag
        Public Name As String
        Public Type As ItemType
        Public Enable As Boolean
        Public Parent As ItemPtr
        Public Childs() As ItemPtr = {}
        Public Content As ItemContent

        Sub New()
            Content = New ItemContent()
        End Sub
    End Class

    Public Class ItemContent
        Public Range As Rectangle = Nothing
        Public Style?(3) As ResPtr
        Public Text?(3) As ResPtr
        Public Image?(3) As ResPtr
    End Class
End Namespace


Option Strict Off

Public Class SearchRes
    Dim r As New List(Of Result)
    Dim y As Integer = 0

    Public Sub Clear()
        r.Clear()
        For i As Integer = Me.Controls.Count - 1 To 0 Step -1
            Me.Controls.RemoveAt(i)
        Next
        y = 0
    End Sub

    Public Function Add(ByVal description As String, ByVal score As Double, ByVal linkedindex As Integer)
        Dim ra As New Result()
        ra.SetDescription(description)
        ra.SetScore(score)
        ra.Tag = linkedindex
        r.Add(ra)
        Me.Controls.Add(ra)
        ra.Location = New Point(0, y)
        ra.Size = New Size(Me.Width * 0.9, 100)
        y += ra.Height
        Return ra
    End Function

    Public Sub DeselectAll()
        For Each ct As Result In r
            ct.Deactivate()
        Next
    End Sub
End Class

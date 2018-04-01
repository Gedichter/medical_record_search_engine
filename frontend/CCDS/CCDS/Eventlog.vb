Public Class Eventlog
    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If ListBox1.SelectedIndex < 0 Then Exit Sub
        Label1.Text = ListBox1.SelectedItem
    End Sub
End Class
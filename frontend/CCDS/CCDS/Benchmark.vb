Public Class Benchmark
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Button1.Enabled = False
        Dim arr() As String = Split(My.Resources.benchmark, vbNewLine)
        ProgressBar1.Maximum = arr.Length
        Dim name As String = TextBox1.Text & " (" & DateAndTime.Now.ToString.Replace("/", "-").Replace(":", "-").Replace(" ", "-") & ")"
        If Not IO.Directory.Exists(IO.Path.Combine(Application.StartupPath, name)) Then IO.Directory.CreateDirectory(name)
        For Each query In arr
            Dim qname As String = query.Replace(" ", "_") & ".txt"
            Dim qpath As String = IO.Path.Combine(Application.StartupPath, name, qname)
            Dim res As List(Of Form1.SearchResultStructure) = Form1.RunQuery(query, False)
            If IsNothing(res) Then
                MessageBox.Show("An error has occurred during the execution of a query. Evaluation aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Button1.Enabled = True
                Exit Sub
            End If
            WriteToDisk(qpath, res)

            ProgressBar1.Value += 1
            Label3.Text = "Execution " & ProgressBar1.Value & "/" & arr.Length & " (" & Math.Round(ProgressBar1.Value / arr.Length * 100) & "%)"
            ProgressBar1.Update()
            Label3.Update()
        Next
        TextBox1.Clear()
        Label3.Text = "Execution: 0/0 (0%)"
        Button1.Enabled = True
        ProgressBar1.Value = 0
        MessageBox.Show("Results of this benchmark have been saved to """ & IO.Path.Combine(Application.StartupPath, name) & """.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Reload()
    End Sub

    Private Sub WriteToDisk(ByVal file As String, ByVal result As List(Of Form1.SearchResultStructure))
        Dim strw As New IO.StreamWriter(file)
        For Each item In result
            strw.WriteLine(item.index & "<>" & item.ranking & "<>" & item.summary)
        Next
        strw.Close()
    End Sub

    Private Function ReadFromDisk(ByVal file As String) As List(Of Form1.SearchResultStructure)
        If Not IO.File.Exists(file) Then Return Nothing
        Dim res As New List(Of Form1.SearchResultStructure)
        Dim lines() As String = IO.File.ReadAllLines(file)
        For Each line In lines
            Dim items() As String = Split(line, "<>")
            Dim item As New Form1.SearchResultStructure With {.index = items(0), .ranking = items(1), .summary = items(2)}
            res.Add(item)
        Next
        Return res
    End Function

    Private Sub Benchmark_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Reload()
    End Sub

    Private Sub Reload()
        ListBox1.Items.Clear()
        ListBox2.Items.Clear()

        For Each timestamp As String In IO.Directory.GetDirectories(Application.StartupPath)
            Dim t As String = New IO.DirectoryInfo(timestamp).Name
            ListBox1.Items.Add(t)
            ListBox2.Items.Add(t)
        Next
    End Sub

    Private Sub Compare()
        TreeView1.Nodes.Clear()
        Dim folder1 As String = IO.Path.Combine(Application.StartupPath, ListBox1.SelectedItem)
        Dim folder2 As String = IO.Path.Combine(Application.StartupPath, ListBox2.SelectedItem)
        Dim total_overlap As Integer = 0
        Dim total_added As Integer = 0
        Dim total_removed As Integer = 0
        For Each query As String In Split(My.Resources.benchmark, vbNewLine)
            Dim qname As String = query.Replace(" ", "_") & ".txt"
            Dim res1 As List(Of Form1.SearchResultStructure) = ReadFromDisk(IO.Path.Combine(folder1, qname))
            Dim res2 As List(Of Form1.SearchResultStructure) = ReadFromDisk(IO.Path.Combine(folder2, qname))

            'compare both search results for this query
            Dim indices1 As New List(Of Integer)
            Dim indices2 As New List(Of Integer)
            For Each item As Form1.SearchResultStructure In res1
                indices1.Add(item.index)
            Next
            For Each item As Form1.SearchResultStructure In res2
                indices2.Add(item.index)
            Next
            Dim overlap As New List(Of Integer)
            Dim nowmissing As New List(Of Integer)
            For Each ind As Integer In indices1
                If indices2.Contains(ind) Then
                    overlap.Add(ind)
                    indices2.Remove(ind)
                Else 'this search result isn't coming up anymore
                    nowmissing.Add(ind)
                End If
            Next
            Dim nowadded As List(Of Integer) = indices2 'everything that is left in indices2 is new

            Dim tn As New TreeNode(query)
            Dim ch1 As New TreeNode(overlap.Count & " results equal (" & Math.Round(overlap.Count / res1.Count * 100) & "%)")
            total_overlap += overlap.Count
            Dim ch2 As New TreeNode(nowadded.Count & " results added (" & Math.Round(nowadded.Count / res1.Count * 100) & "%)")
            total_added += nowadded.Count
            Dim ch3 As New TreeNode(nowmissing.Count & " results lost (" & Math.Round(nowmissing.Count / res1.Count * 100) & "%)")
            total_removed += nowmissing.Count

            For Each item As Integer In overlap
                ch1.Nodes.Add(item)
            Next
            For Each item As Integer In nowadded
                ch2.Nodes.Add(item)
            Next
            For Each item As Integer In nowmissing
                ch3.Nodes.Add(item)
            Next

            tn.Nodes.AddRange({ch1, ch2, ch3})
            TreeView1.Nodes.Add(tn)
        Next
        Dim overall As Integer = total_overlap + total_added + total_removed
        Label8.Text = total_overlap & " results equal (" & Math.Round(total_overlap / overall * 100) & "%)" & vbNewLine &
        total_added & " results added (" & Math.Round(total_added / overall * 100) & "%)" & vbNewLine &
             total_removed & " results lost (" & Math.Round(total_removed / overall * 100) & "%)"
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox2.SelectedIndexChanged, ListBox1.SelectedIndexChanged
        If ListBox1.SelectedIndex > -1 And ListBox2.SelectedIndex > -1 Then
            If IO.Directory.Exists(IO.Path.Combine(Application.StartupPath, ListBox1.SelectedItem)) And IO.Directory.Exists(IO.Path.Combine(Application.StartupPath, ListBox2.SelectedItem)) Then Compare()
        End If
    End Sub

    Private Sub TreeView1_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterSelect
        If Not IsNothing(e.Node) AndAlso IsNumeric(e.Node.Text) Then
            Dim index_report As Integer = e.Node.Text
            Form1.PullReport(index_report)
        End If
    End Sub
End Class
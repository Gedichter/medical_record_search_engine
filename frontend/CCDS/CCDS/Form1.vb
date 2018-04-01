Option Strict Off

Public Class Form1
    Private WithEvents tcpcl As New Net.Sockets.TcpClient()
    Private str As IO.Stream
    Private isconnected As Boolean = False
    Private PORT As Integer = 5008
    Private log As New List(Of String)
    Private tohighlight() As String

    Private Sub AddEvent(ByVal s As String)
        log.Add(DateAndTime.Now.ToString & ": " & s)
    End Sub

    Public Function RunQuery(ByVal s As String, Optional ByVal present As Boolean = True) As List(Of SearchResultStructure)
        SendRequest("query" & IIf(CheckBox1.Checked, "_expand", "") & IIf(CheckBox2.Checked, "_complex", "") & ":" + s.Replace(" ", ","))
        Dim start_time As DateTime = Now
        Dim ths As String = ReadAnswer()
        tohighlight = Split(ths, "|")
        Dim lst As New List(Of String)
        lst.AddRange(tohighlight)
        lst.RemoveAt(lst.Count - 1)
        tohighlight = lst.ToArray
        Label2.Text = "Keywords: " & ths.Replace("|", "   ")
        Dim res As String = ReadAnswer()
        Dim stop_time As DateTime = Now
        Dim elapsed_time As TimeSpan = stop_time.Subtract(start_time)
        ToolStripStatusLabel1.Text = "Last query took " & elapsed_time.TotalSeconds.ToString("0.000000") & "s."
        If Not IsNothing(res) Then
            If present Then SearchResults1.Clear()
            If res.StartsWith("err_no") Then
                Return Nothing
            ElseIf res.StartsWith("err_invalid") Then
                MessageBox.Show("Server didn't know how to process query. Make sure it is formatted properly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Else
                AddEvent("executed query")
                Dim baseresults() As String = res.Split("|")
                Dim maxranking As Double = Split(baseresults(0), "@")(1)
                If maxranking = 0 Then maxranking = 1
                Dim reslist As New List(Of SearchResultStructure)
                For Each result As String In baseresults
                    Dim index As Integer = Split(result, "@")(0)
                    Dim ranking As Double = Split(result, "@")(1)
                    Dim summary As String = Split(result, "@")(2)

                    reslist.Add(New SearchResultStructure With {.index = index, .ranking = ranking, .summary = summary})
                Next

                If present Then
                    PresentResults(reslist)
                End If
                Return reslist
            End If
        End If
        Return Nothing
    End Function

    Structure SearchResultStructure
        Dim index As Integer
        Dim ranking As Double
        Dim summary As String
    End Structure

    Public Sub PresentResults(ByVal reslist As List(Of SearchResultStructure))
        For Each item As SearchResultStructure In reslist
            Dim ra As Result = SearchResults1.Add(item.summary, item.ranking, item.index)
            ra.allowchange = True
            For Each q As String In tohighlight
                HighlightKeyword(ra.RichTextBox1, q, Color.Red)
            Next
            ra.allowchange = False
        Next
    End Sub

    Public Sub PullReport(ByVal nb As String, Optional ByVal jumpto As String = "")
        SendRequest("serve:" + nb)
        Dim res As String = ReadAnswer()
        If IsNothing(res) Then
            MessageBox.Show("The index of the report you specified could not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Exit Sub
        End If
        If res.StartsWith("err_no") Then
            MessageBox.Show("Report #" & CStr(nb) & " not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        ElseIf res.StartsWith("err_invalid") Then
            MessageBox.Show("Server didn't know how to process query. Make sure it is formatted properly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            Label1.Text = "Report #" & nb
            If Not IsNothing(res) Then RichTextBox1.Text = res.Replace(vbLf & vbLf, "|").Replace(vbLf, " ").Replace("|", vbNewLine & vbNewLine).Replace(vbNewLine & " ", vbNewLine).Replace("  ", " ")
            For Each q In tohighlight
                HighlightKeyword(RichTextBox1, q, Color.Red)
            Next
            AddEvent("pulled report")
        End If
    End Sub

    Public Function Connect()
        Try
            tcpcl = New Net.Sockets.TcpClient
            tcpcl.Connect(Net.IPAddress.Loopback, PORT)
            str = tcpcl.GetStream
            isconnected = True
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_connected_icon__1_
            Return True
        Catch
            isconnected = False
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_offline_icon__1_
            Return False
        End Try
    End Function

    Public Function CheckConnection() As Boolean
        Try
            Dim tmp(0) As Byte
            str.Write(tmp, 0, tmp.Length)
            isconnected = True
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_connected_icon__1_
            Return True
        Catch ex As Exception
            isconnected = False
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_offline_icon__1_
            Return False
        End Try
    End Function

    Private Sub SendRequest(ByVal s As String)
        Try
            Dim buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(s)
            str.Write(buffer, 0, buffer.Length)
            isconnected = True
            ToolStripSplitButton1.Text = "Connected [" & DateAndTime.Now.ToString & "]"
        Catch ex As Exception
            ToolStripSplitButton1.Text = "Server not connected, unable to execute request"
            AddEvent("error: " & ex.ToString)
            isconnected = False
        End Try
    End Sub

    Private Function ReadAnswer() As String
        Try
            Dim buffer(1024) As Byte

            Dim recv As Integer = str.Read(buffer, 0, buffer.Length)
            AddEvent("received " & CStr(recv) + " bytes")
            Dim length As Integer = System.Text.Encoding.UTF8.GetString(buffer)
            AddEvent("incoming message is " + CStr(length) + " bytes")
            Dim received As Integer = 0
            Dim msg As New System.Text.StringBuilder
            While (received < length)
                Dim buff(1024) As Byte
                received += str.Read(buff, 0, buff.Length)
                msg.Append(System.Text.Encoding.UTF8.GetString(buff))
            End While
            isconnected = True
            ToolStripSplitButton1.Text = "Connected [" & DateAndTime.Now.ToString & "]"
            Return msg.ToString
        Catch ex As Exception
            AddEvent("error: " & ex.ToString)
            ToolStripSplitButton1.Text = "Server not connected, unable to execute request"
            isconnected = False
            Return Nothing
        End Try
    End Function

    Private Sub HighlightKeyword(ByVal rtb As RichTextBox, ByVal word As String, ByVal cl As Color)
        If rtb.Text.Contains(word) Then
            Dim oldstart As Integer = rtb.SelectionStart
            Dim start As Integer = 0
            Do
                Dim startindex As Integer = rtb.Find(word, start, RichTextBoxFinds.WholeWord)
                If startindex < 0 Or startindex > rtb.TextLength - 1 Then Exit Do
                rtb.Select(startindex, word.Length)
                rtb.SelectionColor = cl
                start = startindex + 1
            Loop
            rtb.SelectionStart = oldstart
            rtb.SelectionLength = 0
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Connect() Then
            ToolStripSplitButton1.Text = "Connected [" & DateAndTime.Now.ToString & "]"
            isconnected = True
            AddEvent("connected")
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_connected_icon__1_
        Else
            AddEvent("could not connect")
            ToolStripSplitButton1.Image = My.Resources.Network_Drive_offline_icon__1_
        End If
        If Not My.Settings.checkinterval = 0 Then
            Timer1.Interval = My.Settings.checkinterval * 1000
            Timer1.Start()
            If My.Settings.checkinterval = 0 Then
                NeverToolStripMenuItem.Checked = True
            ElseIf My.Settings.checkinterval = 10 Then
                Every10SecondsToolStripMenuItem.Checked = True
            Else
                Every60SecondsToolStripMenuItem.Checked = True
            End If
        End If
        Panel1.Size = New Size(Me.Width, 0.945 * (Me.Height - Panel1.Location.Y))
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim result As Boolean = CheckConnection()
        If result Then
            ToolStripSplitButton1.Text = "Connected [" & DateAndTime.Now.ToString & "]"
        Else
            ToolStripSplitButton1.Text = "Server not connected"
            'try to re-connect
            If Connect() Then
                ToolStripSplitButton1.Text = "ReConnected [" & DateAndTime.Now.ToString & "]."
            End If
        End If
    End Sub

    Private Sub RequestSpecificToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RequestSpecificToolStripMenuItem.Click
        Dim nb As String = InputBox("Report #: ")
        If IsNumeric(nb) Then
            PullReport(nb)
        End If
    End Sub

    Private Sub Every10SecondsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NeverToolStripMenuItem.Click, Every60SecondsToolStripMenuItem.Click, Every10SecondsToolStripMenuItem.Click
        Dim ct As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        My.Settings.checkinterval = CInt(ct.Tag)
        My.Settings.Save()
        Timer1.Interval = CInt(ct.Tag) * 1000
        If CInt(ct.Tag) = 0 Then
            Timer1.Stop()
        Else
            Timer1.Start()
        End If
        NeverToolStripMenuItem.Checked = False
        Every10SecondsToolStripMenuItem.Checked = False
        Every60SecondsToolStripMenuItem.Checked = False
        ct.Checked = True
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F5 Then
            RequestSpecificToolStripMenuItem_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            If Not TextBox1.Text.Contains("(") And Not TextBox1.Text.Contains(")") Then
                TextBox1.Text = "+(" & TextBox1.Text & ")"
            End If
            e.SuppressKeyPress = True
                RunQuery(TextBox1.Text)
            End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        TextBox1.Text = TextBox1.Text.ToLower()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        MessageBox.Show("Developed in Harvard IACS: AC297r in cooperation with CCDS" & vbNewLine & vbNewLine & "Vincent Casser" & vbNewLine & "Shiyu Huang" & vbNewLine & "Filip Michalsky", "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()
    End Sub

    Private Sub BenchmarkToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles BenchmarkToolStripMenuItem.Click
        Benchmark.Show()
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        TextBox1.Focus()
    End Sub

    Private Sub EventlogToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles EventlogToolStripMenuItem1.Click
        Eventlog.ListBox1.Items.AddRange(log.ToArray)
        Eventlog.Show()
    End Sub

    Private Sub SupportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SupportToolStripMenuItem.Click
        Process.Start("mailto:vincent.casser@gmail.com&subject=CCDS 297R Frontend")
    End Sub

    Private Sub Form1_ResizeEnd(sender As Object, e As EventArgs) Handles MyBase.ResizeEnd
        PictureBox3.Width = Me.Width / 2
        PictureBox2.Width = Me.Width / 2
        PictureBox2.Location = New Point(Me.Width / 2, 0)
    End Sub
End Class

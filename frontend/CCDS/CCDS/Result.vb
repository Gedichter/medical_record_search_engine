Option Strict Off

Public Class Result
    Private activated As Boolean = False
    Private basecolor As Color = Color.White
    Private highlightcolor As Color = Color.LightGreen
    Private selectedcolor As Color = Color.Green
    Public allowchange As Boolean = False

    Public Sub SetDescription(ByVal s As String)
        RichTextBox1.Text = s
    End Sub

    Public Sub SetScore(ByVal d As Double)
        Label2.Text = Math.Round(d, 2).ToString("#,##0.00")
    End Sub

    Private Sub Result_MouseEnter(sender As Object, e As EventArgs) Handles MyBase.MouseEnter, Label2.MouseEnter, RichTextBox1.MouseEnter
        Me.BackColor = selectedcolor
        RichTextBox1.BackColor = selectedcolor
    End Sub

    Private Sub Result_MouseLeave(sender As Object, e As EventArgs) Handles MyBase.MouseLeave, Label2.MouseLeave, RichTextBox1.MouseLeave
        If Not activated Then Me.BackColor = basecolor : RichTextBox1.BackColor = basecolor
    End Sub

    Public Sub Deactivate()
        Me.BackColor = basecolor
        RichTextBox1.BackColor = basecolor
        activated = False
    End Sub

    Private Sub Result_Click(sender As Object, e As EventArgs) Handles MyBase.Click, Label2.Click, RichTextBox1.Click
        CType(Me.Parent, SearchRes).DeselectAll()
        activated = True
        Me.BackColor = highlightcolor
        RichTextBox1.BackColor = highlightcolor
        Form1.PullReport(Me.Tag, RichTextBox1.Text)
    End Sub

    Private Sub Result_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RichTextBox1.BackColor = basecolor
    End Sub

    Private Sub RichTextBox1_SelectionChanged(sender As Object, e As EventArgs) Handles RichTextBox1.SelectionChanged
        If Not allowchange Then RichTextBox1.SelectionStart = 0
    End Sub
End Class

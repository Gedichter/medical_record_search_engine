Imports System.Timers

Public Class SplashScreen

    Private Sub SplashScreen_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Form1.Opacity = 0
        Timer2.Start()
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Me.Opacity += 0.05
        If Me.Opacity = 1 Then
            Timer2.Stop()

            Dim port_used As String = ""
            While Not IsNumeric(port_used)
                port_used = InputBox("Port number:")
            End While
            Form1.USEPORT = port_used
            Form1.Show()
            Timer1.Start()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Stop()
        Me.Hide()
        Form1.Opacity = 1
    End Sub
End Class
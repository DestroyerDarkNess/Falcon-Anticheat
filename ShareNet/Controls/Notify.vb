Imports System.Windows.Forms
Imports System.Drawing

Public Class Notify

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        Me.Location = New Point(CInt((Screen.PrimaryScreen.WorkingArea.Width / 1) - (Me.Width / 1)), CInt((Screen.PrimaryScreen.WorkingArea.Height / 1) - (Me.Height / 1)))
    End Sub

    Private Sub Notify_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
        Me.Opacity = 0.1 + 100 / 100
    End Sub

#Region " ControlsFuncs "

    Dim progreso As Integer = 0

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        progreso += 1
        Me.Location = New Point(CInt((Screen.PrimaryScreen.WorkingArea.Width / 1) - (Me.Width / 1)), CInt((Screen.PrimaryScreen.WorkingArea.Height / 1) - (Me.Height / 1)))
        If AlphaBar1.Value = 100 Then
            progreso = 0
            Timer1.Enabled = False
            Me.Visible = False
            Me.Close()
        Else
            AlphaBar1.Value = progreso
        End If
    End Sub

#End Region

End Class
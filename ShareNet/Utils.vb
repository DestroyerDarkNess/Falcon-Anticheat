Imports System.Windows.Forms
Imports System.Drawing

Namespace Tools

    Public Class Utils

        Public Sub FormNotificacion(ByVal formulario As Object)
            Dim fh As Form = TryCast(formulario, Form)
            fh.ShowInTaskbar = False
            fh.Show()
            fh.Location = New Point(CInt((Screen.PrimaryScreen.WorkingArea.Width / 1) - (formulario.Width / 1)), CInt((Screen.PrimaryScreen.WorkingArea.Height / 1) - (formulario.Height / 1)))
        End Sub

    End Class

End Namespace


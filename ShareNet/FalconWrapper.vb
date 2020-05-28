Imports System.Windows.Forms
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks
Imports RGiesecke.DllExport

Namespace Core

    Public Class FalconWrapper

        Private Shared WithEvents AnticheatMonitor As New System.Windows.Forms.Timer With {.Interval = 1, .Enabled = True}
        Private Shared WithEvents CrashMonitor As New System.Windows.Forms.Timer With {.Interval = 1, .Enabled = True}

        Private Shared AntiCheatClient As New AntiCheat.MainProc
        Private Shared DetectEx As Boolean = False

        Public Sub New()
            AntiCheatClient.Rare_Character_scanning = False ' Detecta caracteres chinos y simbolos, Dejarlo en False, no esta terminado al 100% , esta bug ;.;
            AntiCheatClient.Enable_AntiDump = False  ' Poner en TRUE si quieres activar el AntiDump
            AntiCheatClient.Antidll_injection = False ' Tener q probar si no se bugea el juego con esta funcion. 
            AntiCheatClient.CreateDevice()
            ' AntiCheatClient.HashScanner()
        End Sub

        <DllExport>
        Public Shared Sub Init()
            ' AntiCheatClient.Rare_Character_scanning = False ' Detecta caracteres chinos y simbolos, Dejarlo en False, no esta terminado al 100% , esta bug ;.;
            ' AntiCheatClient.Enable_AntiDump = False  ' Poner en TRUE si quieres activar el AntiDump
            ' AntiCheatClient.Antidll_injection = False ' Tener q probar si no se bugea el juego con esta funcion. 
            ' AntiCheatClient.CreateDevice()
            ' AntiCheatClient.HashScanner()
        End Sub

        Private Shared Sub AnticheatMonitor_Tick(sender As Object, e As EventArgs) Handles AnticheatMonitor.Tick
            If DetectEx = False Then
                If AntiCheatClient.DetectionState = AntiCheat.MainProc.ResultType.Danger Then
                    DetectEx = True
                    EndApp()
                    AnticheatMonitor.Enabled = False
                End If
            End If
        End Sub

        Private Shared Hmonitor As HaloCrashHook = New HaloCrashHook
        Private Shared ShowAD As Boolean = False

        Private Shared Sub CrashMonitor_Tick(sender As Object, e As EventArgs) Handles CrashMonitor.Tick

            If Hmonitor.ResultState = HaloCrashHook.ResultType.Notify Then
                If ShowAD = False Then
                    ' Notify.ResultText = Hmonitor.ResultText
                    'Notify.Show()
                    MsgBox(Hmonitor.ResultText)
                    ShowAD = True
                End If
            ElseIf Hmonitor.ResultState = HaloCrashHook.ResultType.Indeterminate Then
                ShowAD = False
            End If
        End Sub

        Private Shared Sub EndApp()
            '  Destroyer.LogFuncs.WriteLog("Description: " & AntiCheatClient.DetectDescription & vbNewLine & _
            '         "Log : " & AntiCheatClient.LogResult, Destroyer.LogFuncs.InfoType.None)

        End Sub

    End Class

End Namespace


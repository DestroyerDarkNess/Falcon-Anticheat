Imports System.Text
Imports System.Security.Cryptography
Imports System.IO

Namespace Core

    Public Class Hack_Scanner

        Private thread As New Threading.Thread(Sub() threadLoop())

        Private whitelistedModules As New List(Of String)
        Private blacklistedModules As New List(Of String)

        Private running As Boolean = False
        Private interval As Integer = 500
        Private action2 As Action = Action.Close
        Private mode2 As Mode = Mode.Blacklist

        Public Function GetMD5(ByVal filePath As String)
            Dim md5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider
            Dim f As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)

            f = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            md5.ComputeHash(f)
            f.Close()

            Dim hash As Byte() = md5.Hash
            Dim buff As StringBuilder = New StringBuilder
            Dim hashByte As Byte

            For Each hashByte In hash
                buff.Append(String.Format("{0:X2}", hashByte))
            Next

            Dim md5string As String
            md5string = buff.ToString()

            Return md5string

        End Function

#Region " Enums "
        Public Enum Action
            Close = 0
            MessageBox = 1
        End Enum

        Public Enum Mode
            Blacklist = 0
            Whitelist = 1
        End Enum
#End Region

#Region " Setters "
        ''' <summary>
        ''' Sets the running state to value and starts the threadloop.
        ''' </summary>
        ''' <param name="value">Running state.</param>
        ''' <remarks></remarks>
        Public Sub setRunning(value As Boolean)
            If (value) Then
                running = True
                thread = New Threading.Thread(Sub() threadLoop())
                thread.Start()
            Else
                running = False
            End If
        End Sub

        ''' <summary>
        ''' Sets the mode for what so search for.
        ''' </summary>
        ''' <param name="mode">Mode.</param>
        ''' <remarks></remarks>
        Public Sub setMode(mode As Mode)
            mode2 = mode
        End Sub

        ''' <summary>
        ''' Sets the delay for the threadloop.
        ''' </summary>
        ''' <param name="value">Delay.</param>
        ''' <remarks></remarks>
        Public Sub setInterval(value As Integer)
            interval = value
        End Sub

        ''' <summary>
        ''' Sets the action for what to do when the program found nonwanted modules.
        ''' </summary>
        ''' <param name="action">Action.</param>
        ''' <remarks></remarks>
        Public Sub setAction(action As Action)
            action2 = action
        End Sub
#End Region

#Region " Modes "
#Region " Whitelist "
        ''' <summary>
        ''' Sets the whitelist of MD5 hashes to be exceptioned.
        ''' </summary>
        ''' <param name="pModules">Hash list.</param>
        ''' <remarks></remarks>
        Public Sub setWhitelist(pModules As List(Of String))
            whitelistedModules = pModules
        End Sub

        ''' <summary>
        ''' Adds the MD5 hash to the whitelist.
        ''' </summary>
        ''' <param name="pModule">Hash.</param>
        ''' <remarks></remarks>
        Public Sub addToWhitelist(pModule As String)
            If (Not whitelistedModules.Contains(pModule)) Then
                whitelistedModules.Add(pModule)
            End If
        End Sub

        ''' <summary>
        ''' Adds the MD5 hashes to the whitelist.
        ''' </summary>
        ''' <param name="pModules">Hash list.</param>
        ''' <remarks></remarks>
        Public Sub addToWhitelist(pModules As List(Of String))
            For Each pModuleName As String In pModules
                If (Not whitelistedModules.Contains(pModuleName)) Then
                    whitelistedModules.Add(pModuleName)
                End If
            Next
        End Sub

        Private Function checkWhitelist() As List(Of String)
            Dim nonwantedModules As New List(Of String)
            For Each pModule As ProcessModule In Process.GetCurrentProcess().Modules
                If (Not whitelistedModules.Contains(GetMD5(pModule.FileName))) Then
                    nonwantedModules.Add(pModule.ModuleName)
                End If
            Next
            Return nonwantedModules
        End Function
#End Region
#Region " Blacklist "
        ''' <summary>
        ''' Sets the list of MD5 hashed to the blacklist.
        ''' </summary>
        ''' <param name="hashes">Hash list.</param>
        ''' <remarks></remarks>
        Public Sub setBlacklist(hashes As List(Of String))
            blacklistedModules = hashes
        End Sub

        ''' <summary>
        ''' Adds a hash to the MD5 hash blacklist.
        ''' </summary>
        ''' <param name="hash">Hash.</param>
        ''' <remarks></remarks>
        Public Sub addToBlacklist(hash As String)
            If (Not whitelistedModules.Contains(hash)) Then
                blacklistedModules.Add(hash)
            End If
        End Sub
        ''' <summary>
        ''' Adds a hash list to the blacklist.
        ''' </summary>
        ''' <param name="hashes">Hash list.</param>
        ''' <remarks></remarks>
        Public Sub addToBlacklist(hashes As List(Of String))
            For Each hash As String In hashes
                If (Not blacklistedModules.Contains(hash)) Then
                    blacklistedModules.Add(hash)
                End If
            Next
        End Sub

        Private Function checkBlacklist() As List(Of String)
            Dim nonwantedModules As New List(Of String)
            For Each pModule As ProcessModule In Process.GetCurrentProcess().Modules
                If (blacklistedModules.Contains(GetMD5(pModule.FileName))) Then
                    nonwantedModules.Add(pModule.ModuleName)
                End If
            Next
            Return nonwantedModules
        End Function
#End Region
#End Region

        Private Sub doAction(pModules As List(Of String))
            Select Case action2
                Case Action.Close
                    '  System.Reflection.Assembly.
                Case Action.MessageBox
                    MsgBox(String.Format("Unknown module(s) found.{0}Name: {1}", Environment.NewLine, pModules(0)))
                Case Else
                    'Application.Exit()
            End Select
        End Sub

        Private Sub threadLoop()
            While running
                Dim modules As List(Of String) = IIf(mode2.Equals(Mode.Blacklist), checkBlacklist, checkWhitelist)
                If (modules.Count > 0) Then
                    doAction(modules)
                End If
                Threading.Thread.Sleep(interval)
            End While
        End Sub
    End Class

End Namespace


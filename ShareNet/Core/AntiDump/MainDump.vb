Imports System.Runtime.InteropServices

' ***********************************************************************
' Author           : Destroyer
' Last Modified On : 01-04-2020
' Discord          : Destroyer#8328
' ***********************************************************************
' <copyright file="MainDump.vb" company="S4Lsalsoft">
'     Copyright (c) S4Lsalsoft. All rights reserved.
' </copyright>
' ***

Namespace Destroyer.AntiDump
    Public Class MainDump

#Region " Pinvoke's "

        <System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet:=System.Runtime.InteropServices.CharSet.Auto, SetLastError:=True)> _
        Public Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
        End Function

        <System.Runtime.InteropServices.DllImport("kernel32.dll")>
        Private Shared Function ZeroMemory(ByVal addr As IntPtr, ByVal size As IntPtr) As IntPtr
        End Function

        <System.Runtime.InteropServices.DllImport("kernel32.dll")>
        Private Shared Function VirtualProtect(ByVal lpAddress As IntPtr, ByVal dwSize As IntPtr, ByVal flNewProtect As IntPtr, ByRef lpflOldProtect As IntPtr) As IntPtr
        End Function

#End Region

#Region " Properties "

        Private Shared _LogResult As String = String.Empty
        Public Shared ReadOnly Property LogResult As String
            Get
                Return _LogResult
            End Get
        End Property

#End Region

        Const PAGE_READWRITE As UInteger = 4

        Public Shared Function AntiDumpEnabled() As Boolean
            Try
                Dim OldProtect As UInteger = 0
                Dim pBaseAddr As IntPtr = GetModuleHandle(Nothing)

                VirtualProtect(pBaseAddr, 4096, PAGE_READWRITE, OldProtect)
                ZeroMemory(pBaseAddr, 4096)
                _LogResult = "Anti Dump Triggered Properly" & vbNewLine
                Return True
            Catch ex As Exception
                _LogResult = "Error activating Anti Dump. | " & ex.Message & vbNewLine
                Return False
            End Try
        End Function


    End Class
End Namespace


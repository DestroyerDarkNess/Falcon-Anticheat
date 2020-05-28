Imports System.Runtime.InteropServices

Namespace Destroyer.AntiInject

    Public Class MainInject

#Region " Pinvoke's "

        <DllImport("kernel32.dll", CharSet:=System.Runtime.InteropServices.CharSet.Auto, SetLastError:=True)> _
        Public Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
        End Function

        <DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function GetProcAddress(
           ByVal ModuleHandle As IntPtr,
           ByVal ProcName As String) As IntPtr
        End Function


        <DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function WriteProcessMemory(
            ByVal hProcess As IntPtr,
           ByVal lpBaseAddress As IntPtr,
           ByVal lpBuffer As Byte(),
           ByVal nSize As UInteger,
            ByRef lpNumberOfBytesWritten As UInt32) As Boolean
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

        Public Shared Sub ScrewLoadLibrary()
            Try
                Dim LoadLibraryA As IntPtr = CType(GetProcAddress(CInt(GetModuleHandle("kernel32")), "LoadLibraryA"), IntPtr)
                Dim LoadLibraryW As IntPtr = CType(GetProcAddress(CInt(GetModuleHandle("kernel32")), "LoadLibraryW"), IntPtr)
                If LoadLibraryA <> IntPtr.Zero Then WriteProcessMemory(Process.GetCurrentProcess.Handle, LoadLibraryA, New Byte() {&HC2, &H4, &H0, &H90}, 4, 0)

                If LoadLibraryW <> IntPtr.Zero Then WriteProcessMemory(Process.GetCurrentProcess.Handle, LoadLibraryW, New Byte() {&HC2, &H4, &H0, &H90}, 4, 0)
                _LogResult = "AntiInjectionLoad"
            Catch ex As Exception
                _LogResult = ex.Message
            End Try
        End Sub

    End Class

End Namespace


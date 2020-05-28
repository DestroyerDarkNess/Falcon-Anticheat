Imports System.Text
Imports System.IO

Namespace Engine

    Public Class ModeluesInspect

        Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As UInt32, ByVal bInheritHandle As Int32, ByVal dwProcessId As UInt32) As IntPtr

        Public Function GetProcessModules(ByVal Process_Name As String) As List(Of String)
            Dim DataS As New List(Of String)
            Try

                Dim pc As Process() = Process.GetProcessesByName(Process_Name)

                Dim hndProc As IntPtr = OpenProcess(&H2 Or &H8 Or &H10 Or &H20 Or &H400, 1, CUInt(pc(0).Id))
                If hndProc = IntPtr.Zero Then
                    DataS.Add("Error")
                    Return DataS '{("Error")}
                End If

                Dim ModulesCount As Integer = pc(0).Modules.Count - 1
                For index As Integer = 0 To ModulesCount
                    DataS.Add(pc(0).Modules(index).FileName) 'DataS.Append(pc(0).Modules(index).FileName & vbNewLine)
                Next

                Return DataS
            Catch ex As Exception
                DataS.Add("Error")
                Return DataS '{("Error")}
            End Try
        End Function

        Public Function GetProcessListEx() As List(Of Process)
            Return (From p As Process In Process.GetProcesses).ToList
        End Function

        Function findOffset(ByVal filename As String, ByVal sourcesrc As String) As String
            Dim spos As Integer = 0 ' <== THIS IS THE STARTING OFFSET
            Dim epos As Integer = 0 ' <== THIS IS THE ENDING OFFSET
            Dim offsetlist As New List(Of Byte)
            For Each b As Byte In Encoding.Convert(Encoding.ASCII, Encoding.Unicode, Encoding.ASCII.GetBytes(sourcesrc))
                offsetlist.Add(String.Format("&H{0:x2}", b))
            Next
            Using reader As New BinaryReader(File.Open(filename, FileMode.Open))
                Dim pos As Integer = 0
                Dim curct As Integer = 1
                Dim length As Integer = reader.BaseStream.Length
                Do While pos < length
                    Dim value As Byte = reader.ReadByte()
                    If value = offsetlist(curct - 1) Then
                        If curct = 1 Then spos = pos
                        If curct = offsetlist.Count Then
                            epos = pos
                            Exit Do
                        End If
                        curct += 1
                    Else
                        If curct > 1 Then curct = 1
                    End If
                    pos += 1
                Loop
            End Using
            Return String.Format("{0}|{1}", Hex(spos), Hex(epos))
        End Function

    End Class

End Namespace

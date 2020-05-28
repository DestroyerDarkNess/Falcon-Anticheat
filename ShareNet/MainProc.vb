Imports ShareNet.Destroyer.Protect
Imports System.Windows.Forms
Imports System.Reflection
Imports System.Management
Imports ShareNet.Core

Namespace AntiCheat

    Public Class MainProc

#Region " Properties "

        Private _DetectionState As ResultType = ResultType.Indeterminate
        Public ReadOnly Property DetectionState As ResultType
            Get
                Return _DetectionState
            End Get
        End Property

        Private _LogResult As String = String.Empty
        Public ReadOnly Property LogResult As String
            Get
                Return _LogResult
            End Get
        End Property

        Private _DetectDescription As String = String.Empty
        Public ReadOnly Property DetectDescription As String
            Get
                Return _DetectDescription
            End Get
        End Property

        Private _Rare_Character_scanning As Boolean = False
        Public Property Rare_Character_scanning As Boolean
            Get
                Return _Rare_Character_scanning
            End Get
            Set(value As Boolean)
                _Rare_Character_scanning = value
            End Set
        End Property

        Private _Enable_AntiDump As Boolean = False
        Public Property Enable_AntiDump As Boolean
            Get
                Return _Enable_AntiDump
            End Get
            Set(value As Boolean)
                _Enable_AntiDump = value
            End Set
        End Property

        Private _Antidll_injection As Boolean = False
        Public Property Antidll_injection As Boolean
            Get
                Return _Antidll_injection
            End Get
            Set(value As Boolean)
                _Antidll_injection = value
            End Set
        End Property

        Private _ProcessEnd_Detection() As String = Nothing
        Public Property ProcessEnd_Detection() As String()
            Get
                Return _ProcessEnd_Detection
            End Get
            Set(value As String())
                _ProcessEnd_Detection = value
            End Set
        End Property

#End Region

#Region " Types "

        Public Enum ResultType
            Secure = 0
            Danger = 1
            Indeterminate = 2
            IError = 3
        End Enum

        Private Structure ResultScan
            Shared ProcessResult As String = String.Empty
            Shared DebuggersResult As String = String.Empty
            Shared VMResult As String = String.Empty
            Shared Finish As Boolean = False
            Shared BadFund As Boolean = False
        End Structure

#End Region

#Region " Declare "

        Private Utilities As New Tools.Utils
        Private WithEvents CheckS1 As New System.Windows.Forms.Timer With {.Interval = 1}
        Private GamePath As String = Assembly.GetCallingAssembly.Location

#End Region

        Public Sub New()
            Dim AntiCheatNotify As New Notify
            Utilities.FormNotificacion(AntiCheatNotify)
            TimingCheckEngine.Start(10000)
            BPoints.FirsStart = PointsType.Starting
            Try : AddHandler Application.ThreadException, AddressOf Application_Exception_Handler _
              : Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, False) _
              : Catch : End Try
            If My.Computer.FileSystem.FileExists(Destroyer.LogFuncs.LogFile) = True Then
                My.Computer.FileSystem.DeleteFile(Destroyer.LogFuncs.LogFile)
            End If
            Destroyer.LogFuncs.WriteLog("////////////////////////////////////  [ Falcon Anti-Cheat  ]  ////////////////////////////////////", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Mod v1 For Halo Custom Edition                                 Compatible Wiht HAC", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Author           : Destroyer and Harold", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Last Modified On : 20-04-2020", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Discord          : Destroyer#8328 Or H a r o l d#7626", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("////////////////////////////////////  [ -----------------  ]  ////////////////////////////////////", Destroyer.LogFuncs.InfoType.None)
        End Sub

        Public Sub CreateDevice()
            Destroyer.LogFuncs.WriteLog("Starting Falcon Anticheat Engine ...", Destroyer.LogFuncs.InfoType.None)
            If _Enable_AntiDump = True Then
                Destroyer.AntiDump.MainDump.AntiDumpEnabled()
                _LogResult += Destroyer.AntiDump.MainDump.LogResult & vbNewLine
            End If
            If _Antidll_injection = True Then
                Destroyer.AntiInject.MainInject.ScrewLoadLibrary()
                _LogResult += Destroyer.AntiInject.MainInject.LogResult & vbNewLine
            End If
            ProcessStartWatcher.Start()
            ProcessStopWatcher.Start()
            DestroProtect_Start()
            Dim ShareEngine As New Task(ScanAsync, TaskCreationOptions.LongRunning)
            ReScan = True
            ShareEngine.Start()
            Destroyer.LogFuncs.WriteLog("Starting ScanEngine ...", Destroyer.LogFuncs.InfoType.None)
            BPoints.FirsStart = PointsType.Finish
            Destroyer.LogFuncs.WriteLog("Main BP DestroyerEngine: " & BPDescriptor(BPoints.DestroyerEngine), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Main BP AsyncEngine: " & BPDescriptor(BPoints.AsyncEngine), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Main BP FE: " & BPDescriptor(BPoints.FirsStart), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Main Asha MS: | Adress : 0x" & AshaMS & "***", Destroyer.LogFuncs.InfoType.None)
        End Sub

        Public Sub HashScanner()
            Dim SignatureList() As String = Core.SignatureList.HackSignature

            Dim AI As New Hack_Scanner
            AI.setAction(Hack_Scanner.Action.Close) 'Set the action for what to do when it found a unwanted module.
            AI.setMode(Hack_Scanner.Mode.Blacklist) 'Sets checking mode to blacklist.
            AI.setInterval(500) 'Sets the interval to 500ms.
            For Each HashType As String In SignatureList
                AI.addToBlacklist(HashType) 'Adds a MD5 hash to the blacklist
            Next
            AI.setRunning(True) 'Starts the thread loop.
        End Sub

        Private Sub Application_Exception_Handler(ByVal sender As Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
            Dim ex As Exception = CType(e.Exception, Exception)
            Destroyer.LogFuncs.WriteLog(ex.Message, Destroyer.LogFuncs.InfoType.Exception)
        End Sub

#Region " ScanHost "

        Private HostList As String() = Nothing

        Public Sub ScanHost(ByVal GameHost As String())
            HostList = GameHost
            Dim ShareHostEngine As New Task(ScanHostAsync, TaskCreationOptions.LongRunning)
            ShareHostEngine.Start()
        End Sub

        Private ScanHostAsync As New Action(
            Sub()
                Dim list As New List(Of String)
                Dim HostFile As String = Environment.SystemDirectory & "\drivers\etc\hosts"
                If System.IO.File.Exists(HostFile) = True Then
                    Dim lines As String() = IO.File.ReadAllLines(HostFile)
                    Using r As System.IO.StreamReader = New System.IO.StreamReader(HostFile)

                        For i As Integer = 0 To lines.Length - 1
                            Dim LineText As String = r.ReadLine()
                            If LineText.Length > 1 Then
                                If LineText.Substring(0, 1).Contains("#") = False Then
                                    Dim Filter As String = LineText.Remove(0)
                                    Dim LocalHostE As String = "127.0.0.1"
                                    Filter = LineText.Replace(LocalHostE, "").Trim()
                                    Filter = Filter.Trim()

                                    For Each Lhost As String In HostList
                                        If LCase(Lhost) = LCase(Filter) Then
                                            _DetectDescription = "Host Blocking " & Filter
                                            _DetectionState = ResultType.Danger
                                        End If
                                    Next

                                End If
                            End If

                        Next

                    End Using

                End If

            End Sub)

#End Region

#Region " ScanEngine "

        Private EngineHook As New Engine.ModeluesInspect
        Private ReScan As Boolean = False

        Private ScanAsync As New Action(
            Sub()

                If ReScan = True Then
                    BPoints.AsyncEngine = PointsType.Starting
                    Destroyer.LogFuncs.WriteLog("Main BP DestroyerEngine: " & BPDescriptor(BPoints.DestroyerEngine), Destroyer.LogFuncs.InfoType.None)
                    Dim ProcessList As List(Of Process) = EngineHook.GetProcessListEx()
                    For Each Pcess As Process In ProcessList
                        Dim ProcessName As String = Pcess.ProcessName
                        Dim MwText As String = Pcess.MainWindowTitle

                        Dim ScanWindowsTitle As Boolean = TextAnalice(LCase(MwText), _Rare_Character_scanning)
                        Dim ScanModule As Boolean = ModuleSelf(ProcessName)

                    Next

                    ReScan = False
                    ClockAsyncEngine.Start(1)
                    BPoints.AsyncEngine = PointsType.Finish
                End If

            End Sub)

        Private Function TextAnalice(ByVal Textstr As String, ByVal IsAlphabetic As Boolean) As Boolean
            If IsAlphabetic = True Then
                If StringIsAlphabetic(Textstr) = False Then
                    _DetectDescription = "Malicious program detected - " & Textstr
                    _DetectionState = ResultType.Danger
                    Return True
                End If
            End If
            For Each ItemSTR As String In Engine.TextDictionary.ShareMaliciusText
                If Textstr.Contains(LCase(ItemSTR)) = True Then
                    _DetectDescription = "Malicious program detected - " & ItemSTR
                    _DetectionState = ResultType.Danger
                    Return True
                End If
            Next
            Return True
        End Function

        Private Function StringIsAlphabetic(ByVal str As String) As Boolean
            Return Not Convert.ToBoolean((From c As Char In LCase(str) Where Not "abcdefghijklmnopqrstuvwxyz0123456789".Contains(c)).Count)
        End Function

        Private Function ModuleSelf(ByVal ProcName As String)
            Destroyer.LogFuncs.WriteLog(" ", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog(" ", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("------------------------------ Module Scaning Starting For: " & ProcName, Destroyer.LogFuncs.InfoType.None)
            Dim ModuleList As List(Of String) = EngineHook.GetProcessModules(ProcName)
            For Each Mdule As String In ModuleList
                If Not Mdule = "Error" Then
                    If Mdule = GamePath Then
                        Exit For
                    End If
                    Dim PartDesplace As String() = Mdule.Split("\")
                    If Not LCase(PartDesplace(1)) = "windows" Then
                        Destroyer.LogFuncs.WriteLog(Mdule, Destroyer.LogFuncs.InfoType.None)
                        If Net.BinaryCheck.IsNetAssembly(Mdule) = True Then
                            Dim scan As Boolean = Net.NetAnalysis.NetScan(Mdule)
                            If scan = True Then
                                Dim DetectFamily As String = String.Empty
                                If Not Net.NetAnalysis.Detection1 = String.Empty Then
                                    DetectFamily = Net.NetAnalysis.Detection1
                                ElseIf Not Net.NetAnalysis.Detection2 = String.Empty Then
                                    DetectFamily = Net.NetAnalysis.Detection2
                                End If

                                If DetectFamily = String.Empty Then
                                    _DetectDescription = "CheatNET.Variant.U"
                                    _LogResult = "ProcN : " & ProcName & "   Module:" & Mdule
                                    _DetectionState = ResultType.Danger
                                Else
                                    _DetectDescription = "CheatNET." & DetectFamily
                                    _LogResult = "ProcN : " & ProcName & "   Module:" & Mdule
                                    _DetectionState = ResultType.Danger
                                End If
                                Destroyer.LogFuncs.WriteLog("------------------------------ Module Scaning Ending For: " & ProcName, Destroyer.LogFuncs.InfoType.None)
                                Return True
                            End If
                        End If
                    End If

                End If
            Next
            Destroyer.LogFuncs.WriteLog("///////////////////////////////////////////////////////", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog(" ", Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog(" ", Destroyer.LogFuncs.InfoType.None)
            Return True
        End Function

#End Region

#Region " Process Watcher"

        Private WithEvents ProcessStartWatcher As ManagementEventWatcher =
     New ManagementEventWatcher(
         New WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"))

        Private WithEvents ProcessStopWatcher As ManagementEventWatcher =
            New System.Management.ManagementEventWatcher(
                New WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"))

        Public Sub ProcessStartWatcher_EventArrived(ByVal sender As Object, ByVal e As EventArrivedEventArgs) _
  Handles ProcessStartWatcher.EventArrived

            Dim ProcessName As String = e.NewEvent.Properties("ProcessName").Value
            If ProcessName.ToLower.EndsWith(".exe") Then ProcessName = ProcessName.Substring(0, ProcessName.Length - 4)
            Dim MwText As String = Get_Process_Window_Title(ProcessName)
            Destroyer.LogFuncs.WriteLog("Process Arrived: " & ProcessName & " - " & MwText, Destroyer.LogFuncs.InfoType.None)
            Dim ScanModule As Boolean = ModuleSelf(ProcessName)
            Dim ScanWindowsTitle As Boolean = TextAnalice(LCase(MwText), _Rare_Character_scanning)

        End Sub

        Private Sub ProcessStopWatcher_Stopped(ByVal sender As Object, ByVal e As EventArrivedEventArgs) _
        Handles ProcessStopWatcher.EventArrived


            If Not _ProcessEnd_Detection(0) = Nothing Then
                Dim ProcessName As String = e.NewEvent.Properties("ProcessName").Value
                If ProcessName.ToLower.EndsWith(".exe") Then ProcessName = ProcessName.Substring(0, ProcessName.Length - 4)
                For Each ProcSTR As String In _ProcessEnd_Detection
                    If ProcSTR.ToLower.EndsWith(".exe") Then ProcSTR = ProcSTR.Substring(0, ProcSTR.Length - 4)

                    If LCase(ProcSTR) = ProcessName Then
                        _DetectDescription += "Process-Stop: " & ProcessName & vbNewLine
                        _DetectionState = ResultType.Danger
                    End If
                Next
            End If

        End Sub

        Private Function Get_Process_Window_Title(ByVal ProcessName As String) As String
            Dim ProcessArray = Process.GetProcessesByName(ProcessName)
            If ProcessArray.Length = 0 Then Return Nothing Else Return ProcessArray(0).MainWindowTitle
        End Function


#End Region

#Region " Destroyer Protection Check "

        Private Sub DestroProtect_Start()
            Destroyer.LogFuncs.WriteLog("Starting DestroyerEngine ...", Destroyer.LogFuncs.InfoType.None)
            DestroyerProtect.Start(Destroyer.AntiAnalysis.MainAnalysis.SearchType.FromNameandTitle, _
                             Destroyer.AntiDebug.MainDebug.DebugDetectionTypes.AllScanEgines, _
                             DestroyerProtect.VMScanType.By_Directory_Known)
            BPoints.DestroyerEngine = PointsType.Starting
            Destroyer.LogFuncs.WriteLog("Main BP DestroyerEngine: " & BPDescriptor(BPoints.DestroyerEngine), Destroyer.LogFuncs.InfoType.None)
            CheckS1.Enabled = True
        End Sub

        Private Sub CheckS1_Tick(sender As Object, e As EventArgs) Handles CheckS1.Tick
            BPoints.DestroyerEngine = PointsType.Starting
            Dim Finispscan As DestroyerProtect.ResultType = DestroyerProtect.Process_scanner
            Dim FinisDbugscan As DestroyerProtect.ResultType = DestroyerProtect.Debugers_scanner
            Dim FinisVMscan As DestroyerProtect.ResultType = DestroyerProtect.VM_scanner
            Dim Mdirs As New Destroyer.MainDirScan
            Dim MdirBool As Boolean = Mdirs.MaliciousDir()

            If MdirBool = True Then
                _LogResult += "Detected Dnspy or De4dot" & vbNewLine
                _DetectDescription += Destroyer.MainDirScan.LogResult & vbNewLine
                _DetectionState = ResultType.Danger
                CheckS1.Enabled = False
            End If

            If Finispscan = DestroyerProtect.ResultType.Danger Then
                ResultScan.BadFund = True
                ResultScan.ProcessResult = "[DANGER] Malicious Process has been Detected"
            ElseIf Finispscan = DestroyerProtect.ResultType.Secure Then
                ResultScan.ProcessResult = "[SECURE] Malicious Process Not Detected"
            End If

            If FinisDbugscan = DestroyerProtect.ResultType.Danger Then
                ResultScan.BadFund = True
                ResultScan.DebuggersResult = "[DANGER] Debuggers Detected"
            ElseIf FinisDbugscan = DestroyerProtect.ResultType.Secure Then
                ResultScan.DebuggersResult = "[SECURE] Debuggers not Detected"
            End If

            If FinisVMscan = DestroyerProtect.ResultType.Danger Then
                ResultScan.BadFund = True
                ResultScan.VMResult = "[DANGER] Virutal Machines Have Been Detected"
            ElseIf FinisVMscan = DestroyerProtect.ResultType.Secure Then
                ResultScan.VMResult = "[SECURE] No Virtual Machines Found"
            End If

            If Not ResultScan.ProcessResult = String.Empty And Not ResultScan.DebuggersResult = String.Empty And Not ResultScan.VMResult = String.Empty Then
                If ResultScan.BadFund = True Then
                    _LogResult += DestroyerProtect.LogResult & vbNewLine
                    _DetectDescription += DestroyerProtect.DetectDescription & vbNewLine
                    _DetectionState = ResultType.Danger
                    ' CheckS1.Enabled = False
                Else
                    ' Me.Clock.Start(10)
                    'CheckS1.Enabled = False
                End If
                BPoints.DestroyerEngine = PointsType.Finish
            End If

        End Sub

#End Region

#Region " Tiempo Regresivo "

        ''' <summary>
        ''' The <see cref="TimeMeasurer"/> instance that measure time intervals.
        ''' </summary>
        Private WithEvents Clock As New TimeMeasurer With {.UpdateInterval = 100}

        Private WithEvents ClockAsyncEngine As New TimeMeasurer With {.UpdateInterval = 100}

        Private WithEvents TimingCheckEngine As New TimeMeasurer With {.UpdateInterval = 100}

        Private Sub TimingCheckEngine_RemainingTimeFinished(ByVal sender As Object, ByVal e As TimeMeasurer.TimeMeasureEventArgs) _
     Handles TimingCheckEngine.RemainingTimeFinished

            If BPoints.FirsStart = PointsType.Indeterminate Then
                BPoints.FirsStart = PointsType.UnLoad
            End If

            If BPoints.DestroyerEngine = PointsType.Indeterminate Then
                BPoints.DestroyerEngine = PointsType.UnLoad
            End If

            If BPoints.AsyncEngine = PointsType.Indeterminate Then
                BPoints.AsyncEngine = PointsType.UnLoad
            End If
            Destroyer.LogFuncs.WriteLog("Time Measurer BP DestroyerEngine: " & BPDescriptor(BPoints.DestroyerEngine), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Time Measurer BP AsyncEngine: " & BPDescriptor(BPoints.AsyncEngine), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Time Measurer BP FE: " & BPDescriptor(BPoints.FirsStart), Destroyer.LogFuncs.InfoType.None)
            Destroyer.LogFuncs.WriteLog("Time Measurer Asha MS: | Adress : 0x" & AshaMS & "***", Destroyer.LogFuncs.InfoType.None)
            TimingCheckEngine.Start(20000)
        End Sub

        Private Sub ClockAsyncEngine_RemainingTimeFinished(ByVal sender As Object, ByVal e As TimeMeasurer.TimeMeasureEventArgs) _
      Handles ClockAsyncEngine.RemainingTimeFinished
            ReScan = True
            BPoints.AsyncEngine = PointsType.Indeterminate
        End Sub

        Private Sub Clock_RemainingTimeFinished(ByVal sender As Object, ByVal e As TimeMeasurer.TimeMeasureEventArgs) _
        Handles Clock.RemainingTimeFinished
            BPoints.DestroyerEngine = PointsType.Indeterminate
            DestroProtect_Start()
        End Sub

#End Region

#Region " BreackPoints | Asha-MS Engine "

        Private AshaMS As Integer = 0

        Private Structure BPoints
            Shared FirsStart As PointsType = PointsType.Indeterminate
            Shared DestroyerEngine As PointsType = PointsType.Indeterminate
            Shared AsyncEngine As PointsType = PointsType.Indeterminate
            Shared Finish As PointsType = PointsType.Indeterminate
            Shared LoadingEx As PointsType = PointsType.Indeterminate
        End Structure

        Private Enum PointsType
            Starting = 0
            Finish = 1
            UnLoad = 2
            Indeterminate = 3
            IError = 4
        End Enum

        Private Function BPDescriptor(ByVal PointB As PointsType) As String
            Select Case PointB
                Case PointsType.Starting : Return "Starting"
                Case PointsType.Finish : Return "Terminate"
                Case PointsType.UnLoad : Return "UnLoad"
                Case PointsType.Indeterminate : Return "Indeterminate"
                Case PointsType.IError : Return "IError"
            End Select
            Return "Nothing"
        End Function



        Private WithEvents CheckS2 As New System.Windows.Forms.Timer With {.Interval = 1, .Enabled = True}

        Private Sub CheckS2_Tick(sender As Object, e As EventArgs) Handles CheckS2.Tick

            If BPoints.FirsStart = PointsType.Indeterminate Then
                AshaMS += 1
            End If

            If BPoints.DestroyerEngine = PointsType.Indeterminate Then
                AshaMS += 1
            End If

            If BPoints.AsyncEngine = PointsType.Indeterminate Then
                AshaMS += 1
            End If

            If BPoints.FirsStart = PointsType.UnLoad Then
                _LogResult += "FirsStart No Starting..." & vbNewLine
                _DetectDescription = "Check.BreackPoint - FirsStart." & vbNewLine
                _DetectionState = ResultType.Danger
                CheckS2.Enabled = False
            End If

            If BPoints.AsyncEngine = PointsType.UnLoad Then
                _LogResult += "AsyncEngine No Starting..." & vbNewLine
                _DetectDescription = "Check.BreackPoint - AsyncEngine." & vbNewLine
                _DetectionState = ResultType.Danger
                CheckS2.Enabled = False
            End If

            If BPoints.DestroyerEngine = PointsType.UnLoad Then
                _LogResult += "DestroyerEngine No Starting..." & vbNewLine
                _DetectDescription = "Check.BreackPoint - DestroyerEngine." & vbNewLine
                _DetectionState = ResultType.Danger
                CheckS2.Enabled = False
            End If


            '////////////////////////////////////////////////////////

            If AshaMS > 100 Then
                _LogResult += "Asha-MS Detected Errors" & vbNewLine
                _DetectDescription = "Asha-MS | Error Code : 0x" & AshaMS & "404" & vbNewLine
                _DetectionState = ResultType.Danger
                CheckS2.Enabled = False
            End If

        End Sub


#End Region

    End Class

End Namespace


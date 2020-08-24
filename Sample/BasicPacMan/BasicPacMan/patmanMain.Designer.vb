<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class patmanMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(patmanMain))
        Me.SequenceTimer = New System.Windows.Forms.Timer(Me.components)
        Me.AnticheatMonitor = New System.Windows.Forms.Timer(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'SequenceTimer
        '
        Me.SequenceTimer.Interval = 5
        '
        'AnticheatMonitor
        '
        Me.AnticheatMonitor.Interval = 1
        '
        'Timer1
        '
        Me.Timer1.Interval = 1
        '
        'patmanMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.ClientSize = New System.Drawing.Size(959, 750)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "patmanMain"
        Me.Text = "The PacMan Game | Falcon Anticheat | @FPS = 0"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents SequenceTimer As System.Windows.Forms.Timer
    Friend WithEvents AnticheatMonitor As System.Windows.Forms.Timer
    Friend WithEvents Timer1 As System.Windows.Forms.Timer

End Class

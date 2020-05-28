Namespace Net

    Public Class DataBasesWrapper

#Region " References database "

        Public Shared Function GetDataA() As String
            Return LCase(Net.SuspiciusImports.Database.GetData)
        End Function

        Public Shared Function GetDatA() As String
            Return LCase(Net.SuspiciusImports.Database.GetDat)
        End Function

#End Region

#Region " Classes database "

        Public Shared Function GetDataB() As String
            Return LCase(Net.SuspiciusClases.Database.GetDataC)
        End Function

        Public Shared Function GetDatB() As String
            Return LCase(Net.SuspiciusClases.Database.GetDatC)
        End Function

#End Region

    End Class

End Namespace


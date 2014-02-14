﻿Imports Aricie.DNN.UI.Attributes

Namespace Services.Flee
    <Serializable()>
    Public Class SimpleOrExpression(Of T)

        Public Sub New()

        End Sub

        Public Sub New(value As T)
            Me.Simple = value
        End Sub

        Public Property Mode As SimpleOrExpressionMode

        <ConditionalVisible("Mode", False, True, SimpleOrExpressionMode.Simple)>
        Public Overridable Property Simple As T

        <ConditionalVisible("Mode", False, True, SimpleOrExpressionMode.Expression)>
        Public Property Expression As New FleeExpressionInfo(Of T)

        Public Function GetValue(owner As Object, dataContext As IContextLookup) As T
            Select Case Mode
                Case SimpleOrExpressionMode.Simple
                    Return Simple
                Case SimpleOrExpressionMode.Expression
                    Return Me.Expression.Evaluate(owner, dataContext)
            End Select
        End Function

    End Class
End NameSpace
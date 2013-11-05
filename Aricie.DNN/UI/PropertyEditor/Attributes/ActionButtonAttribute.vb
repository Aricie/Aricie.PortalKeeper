﻿Namespace UI.Attributes

    <AttributeUsage(AttributeTargets.Method)> _
    Public Class ActionButtonAttribute
        Inherits Attribute

        Public Sub New()

        End Sub

        Public Sub New(strIconPath As String)
            Me.IconPath = strIconPath
        End Sub

        Public Property IconPath As String = ""

    End Class


End Namespace
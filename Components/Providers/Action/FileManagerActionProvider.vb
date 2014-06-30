﻿Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.Services.Flee
Imports System.IO
Imports Aricie.DNN.UI.WebControls

Namespace Aricie.DNN.Modules.PortalKeeper


    <ActionButton(IconName.HddO, IconOptions.Normal)> _
   <DisplayName("File Manager Action")> _
   <Description("This provider allows to browse or delete files and folders, given a parent path by dynamic expressions")> _
   <Serializable()> _
    Public Class FileManagerActionProvider(Of TEngineEvents As IConvertible)
        Inherits FileAccessActionProvider(Of TEngineEvents)


        <ExtendedCategory("File")> _
        Public Property Mode As FileManagerMode

        <ExtendedCategory("File")> _
        <ConditionalVisible("Mode", False, True, FileManagerMode.GetFiles, FileManagerMode.GetDirectories)> _
        Public Property Pattern() As String = ""

        <SortOrder(1003)> _
        <ExtendedCategory("File")> _
        <ConditionalVisible("Mode", False, True, FileManagerMode.Copy, FileManagerMode.Move)> _
        Public Property TargetPathExpression() As New FleeExpressionInfo(Of String)("")

        <ExtendedCategory("File")> _
        <ConditionalVisible("Mode", False, True, FileManagerMode.Copy)> _
        Public Property Overwrite As Boolean = True

        Public Overrides Function BuildResult(actionContext As PortalKeeperContext(Of TEngineEvents), async As Boolean) As Object
            Dim sourceMapPath As String = Me.GetFileMapPath(actionContext)
            Select Case Mode
                Case FileManagerMode.GetFiles
                    If Directory.Exists(sourceMapPath) Then
                        If Not String.IsNullOrEmpty(Me.Pattern) Then
                            Return Directory.GetFiles(sourceMapPath, Pattern)
                        Else
                            Return Directory.GetFiles(sourceMapPath)
                        End If
                    Else
                        Return New String() {}
                    End If
                Case FileManagerMode.GetDirectories
                    If Not String.IsNullOrEmpty(Me.Pattern) Then
                        Return Directory.GetDirectories(sourceMapPath, Pattern)
                    Else
                        Return Directory.GetDirectories(sourceMapPath)
                    End If
                Case FileManagerMode.Delete
                    If Directory.Exists(sourceMapPath) Then
                        Dim dir As New DirectoryInfo(sourceMapPath)
                        dir.Delete(True)
                    ElseIf File.Exists(sourceMapPath) Then
                        File.Delete(sourceMapPath)
                    Else
                        Return False
                    End If
                Case FileManagerMode.Copy
                    Dim targetPath As String = Me.TargetPathExpression.Evaluate(actionContext, actionContext)
                    Dim targetMapPath As String = Me.FilePath.GetMapPath(targetPath)
                    If Directory.Exists(sourceMapPath) Then
                        If Not Directory.Exists(targetMapPath) Then
                            Directory.CreateDirectory(targetMapPath)
                        End If
                        CopyDirectory(sourceMapPath, targetMapPath, Me.Overwrite)
                    ElseIf File.Exists(sourceMapPath) Then
                        File.Copy(sourceMapPath, targetMapPath, Me.Overwrite)
                    Else
                        Return False
                    End If
                Case FileManagerMode.Move
                    Dim targetPath As String = Me.TargetPathExpression.Evaluate(actionContext, actionContext)
                    Dim targetMapPath As String = Me.FilePath.GetMapPath(targetPath)
                    If Directory.Exists(sourceMapPath) OrElse File.Exists(sourceMapPath) Then
                        Directory.Move(sourceMapPath, targetMapPath)
                    Else
                        Return False
                    End If
            End Select
            Return True
        End Function

        Protected Overrides Function GetOutputType() As Type
            Select Case Mode
                Case FileManagerMode.GetFiles
                    Return GetType(String())
                Case FileManagerMode.GetDirectories
                    Return GetType(String())
                Case FileManagerMode.Delete
                    Return GetType(Boolean)
                Case FileManagerMode.Copy
                   Return GetType(Boolean)
                Case FileManagerMode.Move
                   Return GetType(Boolean)
            End Select
            Return GetType(Boolean)
        End Function
    End Class
End Namespace
﻿Imports Aricie.DNN.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls.EditControls
Imports System.Xml.Serialization
Imports Aricie.DNN.Services
Imports System.Text
Imports Aricie.Services

Namespace UI.WebControls
    Public Class SubPathContainer
        Implements ISelector(Of KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo))

        Private _SubEntities As New Dictionary(Of String, Object)

        <Browsable(False)> _
        Public Property OriginalPath As String = ""

        '<AutoPostBack()> _
        <LabelMode(LabelMode.Top)> _
        <Editor(GetType(BreadCrumbsEditControl), GetType(EditControl))> _
        <Selector("Key", "Value", False, True, "Root", "", True, True)> _
        Public Property Path As String = ""

        <Browsable(False)> _
        Public Property OriginalEntity As Object


        <XmlIgnore()> _
        Public Property SubEntity As Object
            Get
                Return GetSubEntity()
            End Get
            Set(value As Object)
                '_SubEntity = value
            End Set
        End Property

        <ActionButton(IconName.Undo, IconOptions.Normal)> _
        Public Sub CloseSubEditor(pe As AriciePropertyEditorControl)
            pe.RootEditor.CloseSubEditor()
        End Sub


        Public Function GetParentEntities() As Dictionary(Of String, Object)
            Dim toReturn As New Dictionary(Of String, Object)
            toReturn.Add("", Me.OriginalEntity)
            Dim innerList As IList(Of KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo)) = Me.GetSelectorG("Path")
            For Each pathPair As KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo) In innerList
                If Path.Contains(pathPair.Key) AndAlso Not Path = pathPair.Key Then
                    toReturn(pathPair.Key) = GetSubEntity(pathPair.Key)
                End If
            Next
            Return toReturn
        End Function

        Private Function GetSubEntity() As Object
            Return GetSubEntity(Path)
        End Function

        Private Function GetSubEntity(objPath As String) As Object
            Dim toReturn As Object = Nothing
            If Not _SubEntities.TryGetValue(objPath, toReturn) Then
                If Not "" = objPath Then
                    Dim propAccess As New DeepObjectPropertyAccess(Me.OriginalEntity)
                    Dim tokenPath As String = objPath.Replace("."c, ":"c).Replace("["c, ":").Replace("]"c, "")
                    toReturn = propAccess.GetValue(tokenPath)
                Else
                    toReturn = Me.OriginalEntity
                End If
                Me._SubEntities(objPath) = toReturn
            End If
            Return toReturn
        End Function

        Public Function GetSelector(propertyName As String) As IList Implements ISelector.GetSelector
            Return DirectCast(GetSelectorG(propertyName), IList)
        End Function

        Public Function GetSelectorG(propertyName As String) As IList(Of KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo)) Implements ISelector(Of KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo)).GetSelectorG
            Dim toReturn As New List(Of KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo))
            Dim dicoBuilder As New StringBuilder()
            Dim split As String() = Me.OriginalPath.Split("."c)
            Select Case propertyName
                Case "Path"
                    For i As Integer = LBound(split) To UBound(split) Step 1
                        Dim segment As String = split(i)
                        Dim itemIcone = New IconActionInfo()
                        dicoBuilder.Append(segment)
                        Dim subPath As String = dicoBuilder.ToString()
                        If segment.Contains("["c) Then
                            Dim entity As Object = GetSubEntity(subPath)
                            If entity IsNot Nothing Then
                                'recuperation de l'attribut d'icone
                                Dim entityButton As ActionButtonInfo = ActionButtonInfo.FromMember(entity.GetType)
                                If Not entityButton Is Nothing Then
                                    itemIcone = entityButton.IconAction
                                End If

                                'recuperation du friendlyName
                                Dim strFriendlyName = ReflectionHelper.GetFriendlyName(entity)
                                If Not strFriendlyName.StartsWith(entity.GetType.Name) Then
                                    segment = """" & strFriendlyName & """"
                                End If
                            End If
                        End If
                        toReturn.Add(New KeyValuePair(Of String, BreadCrumbsEditControl.IconInfo)(subPath, New BreadCrumbsEditControl.IconInfo() With {.Text = segment, .Icon = itemIcone}))
                        If i <> UBound(split) Then
                            dicoBuilder.Append("."c)
                        End If
                    Next
            End Select
            Return toReturn
        End Function
    End Class
End NameSpace
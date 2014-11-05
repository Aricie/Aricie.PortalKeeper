﻿Imports Aricie.DNN.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls.EditControls
Imports System.Xml.Serialization
Imports Aricie.DNN.Services
Imports System.Text
Imports Aricie.Services
Imports System.Web.UI
Imports Aricie.DNN.Services.Flee

Namespace UI.WebControls
    Public Class SubPathContainer
        Implements ISelector(Of KeyValuePair(Of String, IconInfo))
        Implements IExpressionVarsProvider
        Implements IContextProvider




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

        <LabelMode(LabelMode.Top)> _
        <ExtendedCategory("")> _
       <Editor(GetType(BreadCrumbsEditControl), GetType(EditControl))> _
       <Selector("Key", "Value", False, True, "Root", "", True, True)> _
        Public Property BottomPath As String
            Get
                Return Path
            End Get
            Set(value As String)
                Path = value
            End Set
        End Property

        <ActionButton(IconName.Home, IconOptions.Normal)> _
        Public Sub CloseSubEditor(pe As AriciePropertyEditorControl)
            pe.RootEditor.CloseSubEditor()
        End Sub

        <ConditionalVisible("Path", True, True, "")> _
        <ActionButton(IconName.LevelUp, IconOptions.Normal)> _
        Public Sub LevelUp(pe As AriciePropertyEditorControl)
            Dim lastDotIdx As Integer = Me.Path.LastIndexOf("."c)
            If lastDotIdx > 0 Then
                Path = Path.Substring(0, lastDotIdx)
            Else
                Path = ""
            End If
            pe.ItemChanged = True
        End Sub


        Public Function GetParentEntities() As Dictionary(Of String, Object)
            Dim toReturn As New Dictionary(Of String, Object)
            toReturn.Add("", Me.OriginalEntity)
            Dim innerList As IList(Of KeyValuePair(Of String, IconInfo)) = Me.GetSelectorG("Path")
            For Each pathPair As KeyValuePair(Of String, IconInfo) In innerList
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
                    propAccess.LevelAccess = TokenLevelAccess.PropertiesOnly
                    Dim tokenPath As String = objPath.Replace("."c, ":"c).Replace("["c, ":").Replace("]"c, "")
                    Try
                        toReturn = propAccess.GetValue(tokenPath)
                        'Catch ex As Exception
                        '    Dim message As String = "DataSource subpath unavailable: """ & tokenPath & """"
                        '    ExceptionHelper.LogException(New ApplicationException(message, ex))
                    Catch
                        toReturn = Nothing
                    End Try
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

        Public Function GetSelectorG(propertyName As String) As IList(Of KeyValuePair(Of String, IconInfo)) Implements ISelector(Of KeyValuePair(Of String, IconInfo)).GetSelectorG
            Dim toReturn As New List(Of KeyValuePair(Of String, IconInfo))
            Dim dicoBuilder As New StringBuilder()
            Dim split As String() = Me.OriginalPath.Split("."c)
            Select Case propertyName
                Case "Path", "BottomPath"
                    For i As Integer = LBound(split) To UBound(split) Step 1
                        Try
                            Dim segment As String = split(i)
                            Dim itemIcon = New IconActionInfo()
                            dicoBuilder.Append(segment)
                            Dim subPath As String = dicoBuilder.ToString()
                            Dim skip As Boolean
                            If segment.Contains("["c) Then

                                Dim entity As Object = GetSubEntity(subPath)
                                If entity IsNot Nothing Then
                                    'recuperation de l'attribut d'icone
                                    Dim entityButton As ActionButtonInfo = ActionButtonInfo.FromMember(entity.GetType)
                                    'Dim include As Boolean
                                    If entityButton IsNot Nothing Then
                                        itemIcon = entityButton.IconAction
                                        'include = True
                                    End If

                                    'recuperation du friendlyName
                                    Dim strFriendlyName = ReflectionHelper.GetFriendlyName(entity)
                                    If Not strFriendlyName.StartsWith(entity.GetType.Name) Then
                                        segment = strFriendlyName
                                        'include = True
                                    End If
                                    'If include Then

                                    'End If
                                Else
                                    skip = True
                                End If
                            End If
                            If Not skip Then
                                toReturn.Add(New KeyValuePair(Of String, IconInfo)(subPath, New IconInfo() With {.Text = segment, .Icon = itemIcon}))
                            End If

                            If i <> UBound(split) Then
                                dicoBuilder.Append("."c)
                            End If
                        Catch ex As Exception
                            ExceptionHelper.LogException(ex)
                        End Try
                    Next
            End Select
            Return toReturn


        End Function

        Public Sub AddVariables(currentProvider As IExpressionVarsProvider, ByRef existingVars As IDictionary(Of String, Type)) Implements IExpressionVarsProvider.AddVariables
            Dim dicos As New List(Of IDictionary(Of String, Type))
            For Each objEntityPair In Me.GetParentEntities().Reverse
                Dim parentProvider As IExpressionVarsProvider = TryCast(objEntityPair.Value, IExpressionVarsProvider)
                If (parentProvider IsNot Nothing) Then
                    Dim tempVars As IDictionary(Of String, Type) = New Dictionary(Of String, Type)
                    parentProvider.AddVariables(currentProvider, tempVars)
                    dicos.Add(tempVars)

                    currentProvider = parentProvider
                End If
            Next
            For Each objPair As KeyValuePair(Of String, Type) In From objDico In Enumerable.Reverse(dicos) From objPair1 In objDico Select objPair1
                existingVars(objPair.Key) = objPair.Value
            Next
        End Sub

        Public Function GetContext(objType As Type) As Object Implements IContextProvider.GetContext
            Return (From objIContext In Me.GetParentEntities().Values.OfType(Of IContextProvider)() _
                    Where objIContext.HasContect(objType) _
                    Select objIContext.GetContext(objType)).FirstOrDefault()
        End Function

        Public Function HasContect(objType As Type) As Boolean Implements IContextProvider.HasContect
            Return Me.GetParentEntities().Values.OfType(Of IContextProvider)() _
                .Any(Function(contextProvider) _
                         contextProvider.HasContect(objType))
        End Function
    End Class
End Namespace
﻿Imports Aricie.DNN.ComponentModel
Imports System.ComponentModel
Imports Aricie.Collections
Imports Aricie.DNN.UI.Attributes
Imports DotNetNuke.UI.Skins.Controls
Imports DotNetNuke.Entities.Profile
Imports System.Xml.Serialization
Imports Aricie.DNN.UI.WebControls.EditControls
Imports DotNetNuke.UI.WebControls
Imports DotNetNuke.Entities.Users
Imports Aricie.DNN.UI.WebControls
Imports Aricie.DNN.Settings
Imports DotNetNuke.Services.Localization
Imports System.Linq
Imports Aricie.DNN.Services
Imports System.Globalization
Imports System.Threading
Imports Aricie.Services

Namespace Aricie.DNN.Modules.PortalKeeper

    <ActionButton(IconName.User, IconOptions.Normal)> _
    <Serializable()> _
    Public Class UserBotInfo
        Inherits NamedConfig

        Private _NoOverride As Boolean
        Private _AnonymousRanking As Boolean

        Private _UserParameters As New Dictionary(Of String, UserVariableInfo)

        Private _PropertyDefinitions As New Dictionary(Of String, GeneralPropertyDefinition)
        Private _Entities As New Dictionary(Of String, Object)
        Private _Rankings As List(Of ProbeRanking)

        <Browsable(False)> _
        Public ReadOnly Property IsAuthenticated As Boolean
            Get
                If DnnContext.Current.HttpContext IsNot Nothing Then
                    Return DnnContext.Current.HttpContext.User.Identity.IsAuthenticated
                End If
                Return False
            End Get
        End Property

      

        <Browsable(False)> _
        Public Property UserParameters() As SimpleList(Of UserVariableInfo)
            Get
                Return New SimpleList(Of UserVariableInfo)(_UserParameters.Values)
            End Get
            Set(ByVal value As SimpleList(Of UserVariableInfo))
                _UserParameters = value.Instances.ToDictionary(Of String, UserVariableInfo)(Function(objUserVariable) objUserVariable.Name, Function(objUserVariable) objUserVariable)
            End Set
        End Property


        '<Obsolete("Use XmlParameters")> _
        <Browsable(False)> _
        Public Property Parameters() As SerializableList(Of Object)
            Get
                ' pas de get pour accomoder le property editor
                Return Nothing
            End Get
            Set(ByVal value As SerializableList(Of Object))
                If value IsNot Nothing Then
                    Me.Clear()
                    For i As Integer = 0 To Math.Min(value.Count - 1, Me.UserParameters.Instances.Count - 1)
                        Dim userParameter As UserVariableInfo = Me.UserParameters.Instances(i)
                        Select Case userParameter.Mode
                            Case UserParameterMode.PropertyDefinition
                                Me._PropertyDefinitions(userParameter.Name) = DirectCast(value(i), GeneralPropertyDefinition)
                            Case Else
                                Me._Entities(userParameter.Name) = value(i)
                        End Select
                    Next
                End If
            End Set
        End Property


        <Browsable(False)> _
        Public Property XmlParameters() As SerializableDictionary(Of String, Object)
            Get
                Dim toReturn As New SerializableDictionary(Of String, Object)
                For Each userParameter As UserVariableInfo In Me._UserParameters.Values
                    Select Case userParameter.Mode
                        Case UserParameterMode.PropertyDefinition
                            Dim propDef As GeneralPropertyDefinition = Nothing
                            If Not Me._PropertyDefinitions.TryGetValue(userParameter.Name, propDef) Then
                                'Throw New Exception(String.Format("Missing UserParameter {0}", userParameter.Name))
                            Else
                                toReturn(userParameter.Name) = propDef
                            End If
                        Case Else
                            Dim item As Object = Nothing
                            If Not Me._Entities.TryGetValue(userParameter.Name, item) Then
                                'Throw New Exception(String.Format("Missing UserParameter {0}", userParameter.Name))
                            Else
                                toReturn(userParameter.Name) = item
                            End If
                    End Select
                Next
                Return toReturn
            End Get
            Set(ByVal value As SerializableDictionary(Of String, Object))
                If value IsNot Nothing AndAlso value.Count > 0 Then
                    For i As Integer = 0 To Me.UserParameters.Instances.Count - 1
                        Dim userParameter As UserVariableInfo = Me.UserParameters.Instances(i)
                        Dim objValue As Object = Nothing
                        If value.TryGetValue(userParameter.Name, objValue) Then
                            Select Case userParameter.Mode
                                Case UserParameterMode.PropertyDefinition
                                    Me._PropertyDefinitions(userParameter.Name) = DirectCast(objValue, GeneralPropertyDefinition)
                                Case Else
                                    Me._Entities(userParameter.Name) = objValue
                            End Select
                        End If
                    Next
                End If
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property HasPropertyDefinition() As Boolean
            Get
                Return Me.PropertyDefinitions.Count > 0
            End Get
        End Property

        <XmlIgnore()> _
        <ExtendedCategory("Configuration")> _
            <ConditionalVisible("HasPropertyDefinition", False, False)> _
            <Editor(GetType(ProfileEditorEditControl), GetType(EditControl))> _
            <LabelMode(LabelMode.Top)> _
        Public Property PropertyDefinitions() As ProfilePropertyDefinitionCollection
            Get
                Dim toReturn As New ProfilePropertyDefinitionCollection
                For Each galDef As GeneralPropertyDefinition In Me._PropertyDefinitions.Values
                    toReturn.Add(galDef.ToDNNProfileDefinition)
                Next
                Return toReturn
            End Get
            Set(ByVal value As ProfilePropertyDefinitionCollection)
                Me._PropertyDefinitions = New Dictionary(Of String, GeneralPropertyDefinition)
                Dim index As Integer = 0
                For Each userParameter As UserVariableInfo In Me._UserParameters.Values
                    If userParameter.Mode = UserParameterMode.PropertyDefinition Then
                        If index < value.Count Then
                            'Me._PropertyDefinitions(userParameter.Name) = DirectCast(value(index), GeneralPropertyDefinition)
                            Me._PropertyDefinitions(userParameter.Name) = GeneralPropertyDefinition.FromDNNProfileDefinition(value(index))
                            index += 1
                        End If
                    End If
                Next
            End Set
        End Property

        '  <XmlIgnore()> _
        '<ExtendedCategory("Configuration")> _
        '    <ConditionalVisible("HasPropertyDefinition", False, False)> _
        '    <Editor(GetType(ProfileEditorEditControl), GetType(EditControl))> _
        '    <LabelMode(LabelMode.Top)> _
        '  Public Property PropertyDefinitions() As List(Of GeneralPropertyDefinition)
        '      Get
        '          Return New List(Of GeneralPropertyDefinition)(Me._PropertyDefinitions.Values)
        '      End Get
        '      Set(ByVal value As List(Of GeneralPropertyDefinition))
        '          Me._PropertyDefinitions = New Dictionary(Of String, GeneralPropertyDefinition)
        '          Dim index As Integer = 0
        '          For Each userParameter As UserVariableInfo In Me._UserParameters.Instances
        '              If userParameter.Mode = UserParameterMode.PropertyDefinition Then
        '                  If index < value.Count Then
        '                      Me._PropertyDefinitions(userParameter.Name) = DirectCast(value(index), GeneralPropertyDefinition)
        '                      index += 1
        '                  End If
        '              End If
        '          Next
        '      End Set
        '  End Property



        <Browsable(False)> _
        Public ReadOnly Property HasEntities() As Boolean
            Get
                For Each userParameter As UserVariableInfo In Me._UserParameters.Values
                    If userParameter.Mode = UserParameterMode.ReflectedEditor AndAlso Not userParameter.IsReadOnly Then
                        Return True
                    End If
                Next
                Return False
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property HasReadonlyEntities() As Boolean
            Get
                For Each userParameter As UserVariableInfo In Me._UserParameters.Values
                    If userParameter.Mode = UserParameterMode.ReflectedEditor AndAlso userParameter.IsReadOnly Then
                        Return True
                    End If
                Next
                Return False
            End Get
        End Property

        '<Editor(GetType(ListEditControl), GetType(EditControl))> _

        <OnDemand(False)> _
        <ExtendedCategory("Configuration")> _
            <ConditionalVisible("HasEntities", False, False)> _
            <CollectionEditor(NoAdd:=True, NoDeletion:=True, ShowAddItem:=False, Ordered:=False, Paged:=False, DisplayStyle:=CollectionDisplayStyle.Accordion, EnableExport:=False)> _
            <LabelMode(LabelMode.Top)> _
            <XmlIgnore()> _
        Public Property Entities() As SerializableList(Of UserParameterWrapper)
            Get
                Dim toReturn As New SerializableList(Of UserParameterWrapper)
                toReturn.AddRange(From userParameter In Me._UserParameters.Values _
                                  Where userParameter.Mode = UserParameterMode.ReflectedEditor AndAlso Not userParameter.IsReadOnly _
                                  Select New UserParameterWrapper(userParameter.Title, userParameter.Decription, Me._Entities, userParameter.Name))
                Return toReturn
            End Get
            Set(ByVal value As SerializableList(Of UserParameterWrapper))
                ' on ne fait rien mais on laisse la propriété en read write pour le property editor
            End Set
        End Property

        <XmlIgnore()> _
        <OnDemand(False)> _
       <ExtendedCategory("Configuration")> _
           <ConditionalVisible("HasReadonlyEntities", False, False)> _
           <CollectionEditor(True, False, False, False, 11, CollectionDisplayStyle.Accordion, False)> _
        Public ReadOnly Property ReadonlyEntities() As SerializableList(Of UserParameterWrapper)
            Get
                Dim toReturn As New SerializableList(Of UserParameterWrapper)
                toReturn.AddRange(From userParameter In Me._UserParameters.Values _
                                  Where userParameter.Mode = UserParameterMode.ReflectedEditor AndAlso userParameter.IsReadOnly _
                                 Select New UserParameterWrapper(userParameter.Title, userParameter.Decription, Me._Entities, userParameter.Name))
                Return toReturn
            End Get
        End Property




        <IsReadOnly(True)> _
        <XmlIgnore()> _
        <OnDemand(False)> _
       <ExtendedCategory("Configuration")> _
           <CollectionEditor(True, False, False, False, 11, CollectionDisplayStyle.Accordion, False)> _
        Public Property ComputedEntities() As SerializableDictionary(Of String, Object)




        Private _BotHistory As New WebBotHistory

        <LabelMode(LabelMode.Top), IsReadOnly(True)> _
        <ExtendedCategory("History")> _
        Public Property BotHistory() As WebBotHistory
            Get
                Return _BotHistory
            End Get
            Set(ByVal value As WebBotHistory)
                _BotHistory = value
            End Set
        End Property


        <ExtendedCategory("Advanced")> _
        Public Property NoOverride() As Boolean
            Get
                Return _NoOverride
            End Get
            Set(ByVal value As Boolean)
                _NoOverride = value
            End Set
        End Property


        <ExtendedCategory("Advanced")> _
        Public Property AnonymousRanking() As Boolean
            Get
                Return _AnonymousRanking
            End Get
            Set(ByVal value As Boolean)
                _AnonymousRanking = value
            End Set
        End Property


        <XmlIgnore()> _
        <OnDemand(False)> _
        <LabelMode(LabelMode.Top), IsReadOnly(True)> _
       <ExtendedCategory("Rankings")> _
        Public Property Rankings() As List(Of ProbeRanking)
            Get
                Return _Rankings
            End Get
            Set(ByVal value As List(Of ProbeRanking))
                _Rankings = value
            End Set
        End Property


        <ConditionalVisible("IsAuthenticated", False, False)> _
       <ActionButton(IconName.Rocket, IconOptions.Normal)> _
        Public Sub Run(ByVal ape As AriciePropertyEditorControl)
            ape.Page.Validate()
            If ape.IsValid Then
                Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
                If PortalKeeperConfig.Instance.SchedulerFarm.AvailableUserBots.TryGetValue(SettingsController.GetModuleSettings(Of KeeperModuleSettings)(SettingsScope.ModuleSettings, ape.ParentModule.ModuleId).UserBotName, userSettings) Then
                    Me.BotHistory.NextSchedule = Now
                    Me.Save(ape.ParentModule.UserInfo, userSettings)

                    Dim key As String = "UserBot" & userSettings.Name
                    ape.Page.Session.Remove(key)
                    ape.DisplayMessage(Localization.GetString("UserBotRunPlanned.Message", ape.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
                End If
            End If
        End Sub


        <ConditionalVisible("IsAuthenticated", False, False)> _
        <ActionButton(IconName.FloppyO, IconOptions.Normal)> _
        Public Sub Save(ByVal ape As AriciePropertyEditorControl)
            ape.Page.Validate()
            If ape.IsValid Then
                Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
                If PortalKeeperConfig.Instance.SchedulerFarm.AvailableUserBots.TryGetValue(SettingsController.GetModuleSettings(Of KeeperModuleSettings)(SettingsScope.ModuleSettings, ape.ParentModule.ModuleId).UserBotName, userSettings) Then
                    Me.BotHistory.NextSchedule = Nothing
                    If Me.Save(ape.ParentModule.UserInfo, userSettings) Then
                        Dim key As String = "UserBot" & userSettings.Name
                        ape.Page.Session.Remove(key)
                        ape.DisplayMessage(Localization.GetString("UserBotSaved.Message", ape.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
                    Else
                        ape.DisplayMessage(Localization.GetString("UserBotNotSaved.Message", ape.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning)
                    End If
                End If
            End If
        End Sub

        Public Function Save(objUser As UserInfo, userSettings As UserBotSettings(Of ScheduleEvent)) As Boolean
            'Dim userBotEntity As UserBotInfo = DirectCast(Me.ctUserBotEntities.DataSource, UserBotInfo)
            Dim readOnlyUserBot As UserBotInfo = userSettings.GetUserBotInfo(objUser, True)
            Me.RevertReadonlyParameters(readOnlyUserBot)
            'Me.UserBot = userBotEntity
            'Me.BindSettings()
            If userSettings.Bot.UseMutex Then
                Dim owned As Boolean
                Dim mutexId As String = BotInfo(Of ScheduleEvent).GetMutexId(objUser.UserID.ToString(CultureInfo.InvariantCulture))
             Using objMutex As New Mutex(False, mutexId)
                    Try
                       If (Not userSettings.Bot.SynchronisationTimeout.Value = TimeSpan.Zero AndAlso objMutex.WaitOne(userSettings.Bot.SynchronisationTimeout.Value)) OrElse (userSettings.Bot.SynchronisationTimeout.Value = TimeSpan.Zero AndAlso objMutex.WaitOne()) Then
                            owned = True
                            userSettings.SetUserBotInfo(objUser, objUser.PortalID, Me)
                            Return True
                        End If
                    Catch ex As AbandonedMutexException
                        ExceptionHelper.LogException(ex)
                        owned = True
                    Catch ex As Exception
                        ExceptionHelper.LogException(ex)
                    Finally
                        If owned Then
                            objMutex.ReleaseMutex()
                        End If
                    End Try
                End Using
            Else
                userSettings.SetUserBotInfo(objUser, objUser.PortalID, Me)
                Return True
            End If
            Return False
        End Function



        <ConditionalVisible("IsAuthenticated", False, False)> _
        <ActionButton(IconName.Undo, IconOptions.Normal)> _
        Public Sub Cancel(ByVal ape As AriciePropertyEditorControl)
            Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
            If PortalKeeperConfig.Instance.SchedulerFarm.AvailableUserBots.TryGetValue(SettingsController.GetModuleSettings(Of KeeperModuleSettings)(SettingsScope.ModuleSettings, ape.ParentModule.ModuleId).UserBotName, userSettings) Then
                Dim key As String = "UserBot" & userSettings.Name
                ape.Page.Session.Remove(key)
            End If
            ape.Page.Response.Redirect(DotNetNuke.Common.Globals.NavigateURL())
        End Sub

        <ConditionalVisible("IsAuthenticated", False, False)> _
        <ActionButton(IconName.TrashO, IconOptions.Normal, "DeleteUserBot.Alert")> _
        Public Sub Delete(ByVal ape As AriciePropertyEditorControl)
            Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
            If PortalKeeperConfig.Instance.SchedulerFarm.AvailableUserBots.TryGetValue(SettingsController.GetModuleSettings(Of KeeperModuleSettings)(SettingsScope.ModuleSettings, ape.ParentModule.ModuleId).UserBotName, userSettings) Then
                userSettings.SetUserBotInfo(ape.ParentModule.UserInfo, ape.ParentModule.PortalId, Nothing)
                Dim key As String = "UserBot" & userSettings.Name
                ape.Page.Session.Remove(key)
                ape.DisplayMessage(Localization.GetString("UserBotDeleted.Message", ape.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
                ape.Page.Response.Redirect(DotNetNuke.Common.Globals.NavigateURL())
            End If
        End Sub



        Private Sub Clear()
            Me._PropertyDefinitions = New Dictionary(Of String, GeneralPropertyDefinition)
            Me._Entities = New Dictionary(Of String, Object)
        End Sub

        Public Sub SetPropertyDefinition(ByVal name As String, ByVal def As GeneralPropertyDefinition)
            Me._PropertyDefinitions(name) = def
        End Sub
        Public Sub SetEntity(ByVal name As String, ByVal obj As Object)
            Me._Entities(name) = obj
        End Sub

        Public Sub RevertReadonlyParameters(originalVersion As UserBotInfo)
            For Each objVar As UserVariableInfo In Me.UserParameters.Instances
                If objVar.IsReadOnly Then
                    Select Case objVar.Mode
                        Case UserParameterMode.PropertyDefinition
                            Dim propDef As GeneralPropertyDefinition = Nothing
                            If originalVersion._PropertyDefinitions.TryGetValue(objVar.Name, propDef) Then
                                Me.SetPropertyDefinition(objVar.Name, propDef)
                            End If
                        Case UserParameterMode.ReflectedEditor
                            Dim entity As Object = Nothing
                            If originalVersion._Entities.TryGetValue(objVar.Name, entity) Then
                                Me.SetEntity(objVar.Name, entity)
                            End If
                    End Select
                End If
            Next
        End Sub

        Public Function GetParameterValues(ByVal objUser As UserInfo) As IDictionary(Of String, Object)
            Dim toReturn As IDictionary(Of String, Object) = Me.GetParameterValues
            toReturn("User") = objUser
            Return toReturn
        End Function

        Public Function GetParameterValues() As IDictionary(Of String, Object)
            Dim toReturn As New SerializableDictionary(Of String, Object)
            For Each objPair As KeyValuePair(Of String, GeneralPropertyDefinition) In Me._PropertyDefinitions
                toReturn(objPair.Key) = objPair.Value.TypedValue
            Next
            For Each objPair As KeyValuePair(Of String, Object) In Me._Entities
                toReturn(objPair.Key) = objPair.Value
            Next

            toReturn("UserBot") = Me
            Return toReturn
        End Function

    End Class
End Namespace
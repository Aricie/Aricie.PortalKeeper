﻿Imports Aricie.DNN.ComponentModel
Imports System.Xml.Serialization
Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports Aricie.DNN.UI.WebControls.EditControls
Imports Aricie.DNN.Diagnostics
Imports Aricie.Collections
Imports DotNetNuke.UI.WebControls
Imports Aricie.Services
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Services.Personalization
Imports Aricie.DNN.Services
Imports Aricie.DNN.Services.Workers
Imports Aricie.DNN.Services.Flee
Imports System.Globalization

Namespace Aricie.DNN.Modules.PortalKeeper
    <Serializable()> _
        <XmlInclude(GetType(UserVariableInfo))> _
    Public Class UserBotSettings(Of TEngineEvent As IConvertible)
        Inherits NamedConfig
        Implements ISelector(Of BotInfo(Of TEngineEvent))

#Region "Fields"

        Private _BotName As String = ""

        Private _Compress As Boolean

        Private _Encrypt As Boolean = True

        Private _FormerName As String = ""

        Private _DisableTemplateBot As Boolean = True

        'Private _UserBot As BotInfo(Of TEngineEvent)

        Private _UserParameters As New SimpleList(Of UserVariableInfo)

        Private _VariableOverwrites As New Variables

        Private _Bot As BotInfo(Of TEngineEvent)

        Private _Probes As New SimpleList(Of ProbeSettings(Of TEngineEvent))

        Private _ResourceFile As String = "SharedResources"

#End Region

#Region "Public properties"

        <ExtendedCategory("MasterBot")> _
            <Editor(GetType(SelectorEditControl), GetType(EditControl))> _
            <ProvidersSelector()> _
            <AutoPostBack()> _
        Public Property BotName() As String
            Get
                Return _BotName
            End Get
            Set(ByVal value As String)
                _BotName = value
                Me._Bot = Nothing
            End Set
        End Property

        <ExtendedCategory("MasterBot")> _
        Public Property DisableTemplateBot() As Boolean
            Get
                Return _DisableTemplateBot
            End Get
            Set(ByVal value As Boolean)
                _DisableTemplateBot = value
            End Set
        End Property

        <ExtendedCategory("MasterBot")> _
            <Editor(GetType(PropertyEditorEditControl), GetType(EditControl))> _
            <LabelMode(LabelMode.Top)> _
        Public ReadOnly Property Bot() As BotInfo(Of TEngineEvent)
            Get
                If _Bot Is Nothing Then
                    For Each webBot As BotInfo(Of TEngineEvent) In PortalKeeperConfig.Instance.GetBotFarm(Of TEngineEvent).Bots.Instances
                        If webBot.Name = Me._BotName Then
                            _Bot = ReflectionHelper.CloneObject(webBot)
                            Exit For ' on a trouvé notre robot, pas la peine de continuer
                        End If
                    Next
                End If
                Return _Bot
            End Get
        End Property

        <ExtendedCategory("Parameters")> _
        Public Property Compress() As Boolean
            Get
                Return _Compress
            End Get
            Set(ByVal value As Boolean)
                _Compress = value
            End Set
        End Property


        <ExtendedCategory("Parameters")> _
        Public Property Encrypt() As Boolean
            Get
                Return _Encrypt
            End Get
            Set(ByVal value As Boolean)
                _Encrypt = value
            End Set
        End Property


        <ExtendedCategory("Parameters")> _
         <Editor(GetType(CustomTextEditControl), GetType(EditControl))> _
            <Width(300)> _
        Public Property FormerName() As String
            Get
                Return _FormerName
            End Get
            Set(ByVal value As String)
                _FormerName = value
            End Set
        End Property

        <ExtendedCategory("Parameters")> _
        Public Property ResourceFile() As String
            Get
                Return _ResourceFile
            End Get
            Set(ByVal value As String)
                _ResourceFile = value
            End Set
        End Property


        <ExtendedCategory("Parameters")> _
            <Editor(GetType(PropertyEditorEditControl), GetType(EditControl))> _
            <LabelMode(LabelMode.Top)> _
        Public Property UserParameters() As SimpleList(Of UserVariableInfo)
            Get
                Return _UserParameters
            End Get
            Set(ByVal value As SimpleList(Of UserVariableInfo))
                _UserParameters = value
            End Set
        End Property

        <Editor(GetType(PropertyEditorEditControl), GetType(EditControl))> _
           <LabelMode(LabelMode.Top)> _
           <ExtendedCategory("Parameters")> _
        Public Property VariableOverwrites() As Variables
            Get
                Return _VariableOverwrites
            End Get
            Set(ByVal value As Variables)
                _VariableOverwrites = value
            End Set
        End Property

        <ExtendedCategory("Probes")> _
            <Editor(GetType(PropertyEditorEditControl), GetType(EditControl))> _
            <LabelMode(LabelMode.Top)> _
        Public Property Probes() As SimpleList(Of ProbeSettings(Of TEngineEvent))
            Get
                Return _Probes
            End Get
            Set(ByVal value As SimpleList(Of ProbeSettings(Of TEngineEvent)))
                _Probes = value
            End Set
        End Property

      

        <ExtendedCategory("ActionCommands")> _
        Public Property ActionCommands() As New SerializableList(Of ActionCommand)



        <NonSerialized()> _
        Private lockDefaultVars As New Object
        <NonSerialized()> _
        Private _DefaultVars As Dictionary(Of String, Object)

        <XmlIgnore()> _
        <Browsable(False)> _
        Private ReadOnly Property DefaultVars As Dictionary(Of String, Object)
            Get
                If Me._DefaultVars Is Nothing Then
                    SyncLock lockDefaultVars
                        If Me._DefaultVars Is Nothing Then
                            Dim pContext As PortalKeeperContext(Of TEngineEvent) = PortalKeeperContext(Of TEngineEvent).Instance
                            Dim tempVars As SerializableDictionary(Of String, Object) = Me.Bot.Variables.EvaluateVariables(pContext, pContext)
                            If Me._VariableOverwrites.Instances.Count > 0 Then
                                Dim overwrites As Dictionary(Of String, Object) = Me._VariableOverwrites.EvaluateVariables(pContext, pContext)
                                For Each overwrite As KeyValuePair(Of String, Object) In overwrites
                                    tempVars(overwrite.Key) = overwrite.Value
                                Next
                            End If
                            Me._DefaultVars = tempVars
                        End If
                    End SyncLock
                End If
                Return _DefaultVars
            End Get
        End Property


        <NonSerialized()> _
        Private Shared WithEvents _AsynchronousProbingTaskQueue As TaskQueue(Of UserBotProbeAccess)

        Private Shared queueLock As New Object


        Private Shared ReadOnly Property AsynchronousProbingTaskQueue() As TaskQueue(Of UserBotProbeAccess)
            Get
                If _AsynchronousProbingTaskQueue Is Nothing Then
                    SyncLock queueLock
                        If _AsynchronousProbingTaskQueue Is Nothing Then
                            Dim objTaskQueueInfo As New TaskQueueInfo(1, True, TimeSpan.FromMilliseconds(2000), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000))
                            _AsynchronousProbingTaskQueue = New TaskQueue(Of UserBotProbeAccess)(AddressOf GetRankingAsync, objTaskQueueInfo)
                        End If
                    End SyncLock
                End If
                Return _AsynchronousProbingTaskQueue
            End Get
        End Property

#End Region

#Region "Methods"




        Friend Function GetRankings(ByVal encryptionKey As String, ByVal initVector As String) As List(Of ProbeRanking)

            Dim toReturn As List(Of ProbeRanking) = CacheHelper.GetGlobal(Of List(Of ProbeRanking))(Me.Name)
            If toReturn Is Nothing Then
                Dim userBotAccess As New UserBotProbeAccess(encryptionKey, initVector, Me)
                AsynchronousProbingTaskQueue.EnqueueTask(userBotAccess)
            End If
            Return toReturn


        End Function



        Friend Function RunUserBots(ByVal encryptionKey As String, ByVal initVector As String, ByVal events As IList(Of TEngineEvent), ByVal forceRun As Boolean) As Integer
            Dim toreturn As Integer
            If Me.Bot IsNot Nothing Then
                If (Not forceRun AndAlso Me.Bot.Enabled) OrElse (forceRun AndAlso Me.Bot.ForceRun) Then
                    For Each pid As Integer In NukeHelper.PortalIds
                        Dim shuffledUsers As IList(Of UserInfo) = New List(Of UserInfo)(NukeHelper.GetPortalUsers(pid))
                        Common.ShuffleList(Of UserInfo)(shuffledUsers)
                        For Each objUser As UserInfo In shuffledUsers
                            If Me.RunUserBot(objUser, encryptionKey, initVector, events, forceRun) Then
                                toreturn += 1
                            End If
                        Next
                    Next
                End If
            End If
            Return toreturn
        End Function

        Friend Function RunUserBot(objUser As UserInfo, ByVal encryptionKey As String, ByVal initVector As String, ByVal events As IList(Of TEngineEvent), ByVal forceRun As Boolean) As Boolean
            Dim toreturn As Boolean
            If Not Me.Bot.AsyncLockBot.ContainsKey(objUser.UserID) Then
                Dim userBotInfo As UserBotInfo = Me.GetUserBotInfo(encryptionKey, initVector, objUser, objUser.PortalID, False)
                If userBotInfo IsNot Nothing Then
                    If userBotInfo.Enabled Then
                        Dim runContext As New BotRunContext(Of TEngineEvent)
                        runContext.AsyncLockId = objUser.UserID
                        runContext.Events = events
                        runContext.History = userBotInfo.BotHistory
                        runContext.UserParams = userBotInfo.GetParameterValues(objUser)
                        Dim saver As New UserBotSaver(Me, encryptionKey, initVector, objUser, userBotInfo)
                        runContext.RunEndDelegate = AddressOf saver.SaveBot
                        toreturn = Me.Bot.RunBot(runContext, forceRun)
                    End If
                End If
            End If
            Return toreturn
        End Function




        Friend Function GetUserBotInfo(ByVal encryptionKey As String, ByVal initVector As String, ByVal user As UserInfo, ByVal pid As Integer, ByVal createIfNull As Boolean) As UserBotInfo
            Dim userBot As UserBotInfo = Nothing '= CacheHelper.GetGlobal(Of UserBotInfo)(Me.Name, user.UserID.ToString(CultureInfo.InvariantCulture))
            If userBot Is Nothing Then


                Dim pc As New PersonalizationController
                Dim pInfo As PersonalizationInfo = pc.LoadProfile(user.UserID, pid)

                Dim strUserDataFormat As String = DirectCast(Personalization.GetProfile(pInfo, "Aricie.PortalKeeper.UserBotFormat", Me.Name), String)
                If String.IsNullOrEmpty(strUserDataFormat) AndAlso Not String.IsNullOrEmpty(Me._FormerName) Then
                    strUserDataFormat = DirectCast(Personalization.GetProfile(pInfo, "Aricie.PortalKeeper.UserBotFormat", Me._FormerName), String)
                    If Not String.IsNullOrEmpty(Me._FormerName) Then
                        Personalization.SetProfile(pInfo, "Aricie.PortalKeeper.UserBotFormat", Me.Name, strUserDataFormat)
                        Personalization.RemoveProfile(pInfo, "Aricie.PortalKeeper.UserBotFormat", Me._FormerName)
                    End If
                End If
                Dim userDataFormat As UserBotFormat
                If String.IsNullOrEmpty(strUserDataFormat) Then
                    userDataFormat = New UserBotFormat
                Else
                    userDataFormat = ReflectionHelper.Deserialize(Of UserBotFormat)(strUserDataFormat)
                End If


                Dim userData As String = DirectCast(Personalization.GetProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me.Name), String)
                If String.IsNullOrEmpty(userData) AndAlso Not String.IsNullOrEmpty(Me._FormerName) Then
                    userData = DirectCast(Personalization.GetProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me._FormerName), String)
                    If Not String.IsNullOrEmpty(Me._FormerName) Then
                        Personalization.SetProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me.Name, userData)
                        Personalization.RemoveProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me._FormerName)
                    End If
                End If

                If Not String.IsNullOrEmpty(userData) Then
                    Try
                        If userDataFormat.Encrypted Then
                            Try
                                Dim tempData As String = Common.Decrypt(userData, encryptionKey, initVector, GetSalt(user, pid))
                                userData = tempData
                            Catch ex As Exception
                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
                            End Try
                        End If
                        If userDataFormat.Compressed Then
                            Try
                                Dim tempData As String = Common.DoDeCompress(userData, CompressionMethod.Gzip)
                                userData = tempData
                            Catch ex As Exception
                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
                            End Try
                        End If
                        userBot = ReflectionHelper.Deserialize(Of UserBotInfo)(userData)
                        Me.SetDefaultParameters(userBot)
                        ' on place le user bot dans le cache après avoir potentiellement complété ses paramètres avec ceux fournis par les paramètres du user bot
                        'CacheHelper.SetCacheDependant(Of UserBotInfo)(userBot, StaticInfo.KeyBucket.PKPCachingUserBotsDependency, TimeSpan.FromMinutes(10), Me.Name, user.UserID.ToString(CultureInfo.InvariantCulture))
                    Catch ex As Exception
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
                    End Try
                End If
                If userBot Is Nothing AndAlso createIfNull Then

                    userBot = New UserBotInfo
                    userBot.Enabled = False
                    ' ici on prend les paramètres nom et description du UserBot, pas du MasterBot
                    userBot.Name = Name
                    userBot.Decription = Decription
                    Me.SetDefaultParameters(userBot)

                End If
            End If


            Return userBot
        End Function

        Friend Sub SetUserBotInfo(ByVal encryptionKey As String, ByVal initVector As String, ByVal user As UserInfo, ByVal pid As Integer, ByVal objUserBotInfo As UserBotInfo)
            Dim pc As New PersonalizationController
            Dim pInfo As PersonalizationInfo = pc.LoadProfile(user.UserID, pid)
            If objUserBotInfo IsNot Nothing Then
                Me.SetDefaultParameters(objUserBotInfo)
                Dim objUserBotFormat As New UserBotFormat
                Dim userData As String = ReflectionHelper.Serialize(objUserBotInfo).OuterXml
                
                If Me._Compress Then
                    objUserBotFormat.Compressed = True
                    userData = Common.DoCompress(userData, CompressionMethod.Gzip)
                Else
                    objUserBotFormat.Compressed = False
                End If

                If Me._Encrypt Then
                    objUserBotFormat.Encrypted = True
                    userData = Common.Encrypt(userData, encryptionKey, initVector, GetSalt(user, pid))
                Else
                    objUserBotFormat.Encrypted = False
                End If

                Dim strFormat As String = ReflectionHelper.Serialize(objUserBotFormat).OuterXml
                Personalization.SetProfile(pInfo, "Aricie.PortalKeeper.UserBotFormat", Me.Name, strFormat)
                Personalization.SetProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me.Name, userData)
                pc.SaveProfile(pInfo)
            Else
                Personalization.SetProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me.Name, "")
                Personalization.RemoveProfile(pInfo, "Aricie.PortalKeeper.UserBot", Me.Name)
                pc.SaveProfile(pInfo)
            End If
            CacheHelper.RemoveCache(Of UserBotInfo)(Me.Name, user.UserID.ToString(CultureInfo.InvariantCulture))
        End Sub





        Private Sub SetDefaultParameters(userBot As UserBotInfo)
            For Each userParameter As UserVariableInfo In UserParameters.Instances
                Dim targetName As String = userParameter.Name
                Dim targetMode As UserParameterMode = userParameter.Mode

                ' les différentes conditions dans lesquelles on doit gérer un paramètre sont
                ' - le paramètre n'existe pas dans les settings utilisateurs
                ' - le paramètre est déclaré comme overridable
                Dim userBotParameterTarget = userBot.UserParameters.Instances.Find(Function(ovi) ovi.Name = targetName AndAlso ovi.Mode = targetMode)

                Dim parameterMatchesInUserBotSettings As Boolean = False
                If userBotParameterTarget IsNot Nothing Then
                    parameterMatchesInUserBotSettings = True
                End If
                Dim userBotSettingsOverwritable As Boolean = userParameter.Override AndAlso ((Not userBot.NoOverride) OrElse userParameter.ForceOverride)

                If (Not parameterMatchesInUserBotSettings OrElse userBotSettingsOverwritable) Then
                    Dim value As Object = Nothing
                    Select Case userParameter.Mode
                        Case UserParameterMode.PropertyDefinition
                            Dim propDef As GeneralPropertyDefinition = userParameter.PropertyDefinition.Clone
                            If String.IsNullOrEmpty(propDef.DefaultValue) Then
                                If DefaultVars.TryGetValue(userParameter.Name, value) Then
                                    propDef.PropertyValue = value.ToString
                                Else
                                    Throw New Exception(String.Format("Missing UserParameter {0}", userParameter.Name))
                                End If
                            Else
                                propDef.PropertyValue = propDef.DefaultValue
                            End If
                            userBot.SetPropertyDefinition(userParameter.Name, propDef)
                        Case Else
                            If Not DefaultVars.TryGetValue(userParameter.Name, value) Then
                                Throw New Exception(String.Format("Missing UserParameter {0}", userParameter.Name))
                            Else
                                userBot.SetEntity(userParameter.Name, ReflectionHelper.CloneObject(value))
                            End If
                    End Select
                End If
            Next
            ' on ne veut que les paramètres activés au niveau du user bot
            Dim enabledList As New SimpleList(Of UserVariableInfo)
            For Each uvi In UserParameters.Instances
                If uvi.Enabled Then
                    enabledList.Instances.Add(uvi)
                End If
            Next
            userBot.UserParameters = enabledList
        End Sub

#End Region

#Region "ISelector"

        Public Function GetSelector(ByVal propertyName As String) As IList Implements ISelector.GetSelector
            Return DirectCast(GetSelectorG(propertyName), IList)
        End Function



        Public Function GetSelectorG(ByVal propertyName As String) As System.Collections.Generic.IList(Of BotInfo(Of TEngineEvent)) Implements ComponentModel.ISelector(Of BotInfo(Of TEngineEvent)).GetSelectorG
            Return PortalKeeperConfig.Instance.GetBotFarm(Of TEngineEvent).Bots.Instances
        End Function

#End Region

#Region "Private methods"

        Private Function GetSalt(ByVal user As UserInfo, ByVal portalId As Integer) As String
            Return user.Email & "-"c & portalId
        End Function

        Private Shared objRankingLock As New Object

        Private Shared Sub GetRankingAsync(objProbeAccess As UserBotProbeAccess)
            SyncLock objRankingLock
                Dim toReturn As List(Of ProbeRanking) = CacheHelper.GetGlobal(Of List(Of ProbeRanking))(objProbeAccess.UserBotSettings.Name)
                If toReturn Is Nothing Then
                    toReturn = New List(Of ProbeRanking)
                    For Each objParamProb As ProbeSettings(Of TEngineEvent) In objProbeAccess.UserBotSettings.Probes.Instances
                        If objParamProb.Enabled Then
                            Dim toAdd As New ProbeRanking
                            toAdd.Name = objParamProb.Name
                            toAdd.Decription = objParamProb.Decription
                            For Each pid As Integer In NukeHelper.PortalIds
                                Dim users As List(Of UserInfo) = NukeHelper.GetPortalUsers(pid)
                                For Each objUser As UserInfo In users
                                    If objUser.Membership.Approved Then
                                        Dim objUserBotInfo As UserBotInfo = objProbeAccess.UserBotSettings.GetUserBotInfo(objProbeAccess.EncryptionKey, objProbeAccess.InitVector, objUser, pid, False)
                                        If objUserBotInfo IsNot Nothing AndAlso objUserBotInfo.Enabled Then
                                            Dim objProbeInstance As ProbeInstance = objParamProb.GetProbe(objUserBotInfo, objUser)
                                            If objProbeInstance IsNot Nothing Then
                                                toAdd.Items.Instances.Add(objProbeInstance)
                                            End If
                                        End If
                                    End If
                                Next
                            Next
                            If toAdd.Items.Instances.Count > 0 Then
                                toAdd.Items.Instances.Sort()
                                If objParamProb.SortDirection = SortDirection.Descending Then
                                    toAdd.Items.Instances.Reverse()
                                End If
                                If toAdd.Items.Instances.Count > objParamProb.RankingsSize Then
                                    toAdd.Items.Instances = New SerializableList(Of ProbeInstance)(toAdd.Items.Instances.GetRange(0, objParamProb.RankingsSize))
                                End If
                            End If
                            If toAdd IsNot Nothing Then
                                toReturn.Add(toAdd)
                            End If
                        End If
                    Next
                    CacheHelper.SetCache(Of List(Of ProbeRanking))(toReturn, TimeSpan.FromMinutes(60), objProbeAccess.UserBotSettings.Name)
                End If
            End SyncLock
        End Sub

#End Region

#Region "Inner Classes"

        <Serializable()> _
        Public Class UserBotFormat


            Private _Encrypted As Boolean = True
            Private _Compressed As Boolean


            Public Property Encrypted() As Boolean
                Get
                    Return _Encrypted
                End Get
                Set(ByVal value As Boolean)
                    _Encrypted = value
                End Set
            End Property


            Public Property Compressed() As Boolean
                Get
                    Return _Compressed
                End Get
                Set(ByVal value As Boolean)
                    _Compressed = value
                End Set
            End Property

        End Class

        Private Class UserBotAccess

            Private _EncryptionKey As String
            Private _InitVector As String


            Public Sub New(encryptionKey As String, initVector As String)
                Me._EncryptionKey = encryptionKey
                Me._InitVector = initVector
            End Sub


            Public Property EncryptionKey() As String
                Get
                    Return _EncryptionKey
                End Get
                Set(ByVal value As String)
                    _EncryptionKey = value
                End Set
            End Property




            Public Property InitVector() As String
                Get
                    Return _InitVector
                End Get
                Set(ByVal value As String)
                    _InitVector = value
                End Set
            End Property

        End Class


        Private Class UserBotProbeAccess
            Inherits UserBotAccess

            Public Sub New(encryptionKey As String, initVector As String, objUserBotSettings As UserBotSettings(Of TEngineEvent))
                MyBase.New(encryptionKey, initVector)
                Me._UserBotSettings = objUserBotSettings

            End Sub

            Private _UserBotSettings As UserBotSettings(Of TEngineEvent)
            Public Property UserBotSettings() As UserBotSettings(Of TEngineEvent)
                Get
                    Return _UserBotSettings
                End Get
                Set(ByVal value As UserBotSettings(Of TEngineEvent))
                    _UserBotSettings = value
                End Set
            End Property


        End Class



        Private Class UserBotSaver
            Inherits UserBotAccess

            Public Sub New(ByVal userSettings As UserBotSettings(Of TEngineEvent), ByVal encryptionKey As String, ByVal initVector As String, ByVal objUser As UserInfo, ByVal userBot As UserBotInfo)
                MyBase.New(encryptionKey, initVector)
                Me._UserBotSettings = userSettings
                'Me._EncryptionKey = encryptionKey
                'Me._InitVector = initVector
                Me._User = objUser

                Me._UserBot = userBot
            End Sub


            Private _UserBotSettings As UserBotSettings(Of TEngineEvent)


            Public Property UserBotSettings() As UserBotSettings(Of TEngineEvent)
                Get
                    Return _UserBotSettings
                End Get
                Set(ByVal value As UserBotSettings(Of TEngineEvent))
                    _UserBotSettings = value
                End Set
            End Property






            Private _User As UserInfo

            Public Property User() As UserInfo
                Get
                    Return _User
                End Get
                Set(ByVal value As UserInfo)
                    _User = value
                End Set
            End Property






            Private _UserBot As UserBotInfo

            Public Property UserBot() As UserBotInfo
                Get
                    Return _UserBot
                End Get
                Set(ByVal value As UserBotInfo)
                    _UserBot = value
                End Set
            End Property

            Public Sub SaveBot(ByVal botHistory As WebBotHistory, ByVal runContext As PortalKeeperContext(Of TEngineEvent))
                Me._UserBot.BotHistory = botHistory
                Me._UserBotSettings.SetUserBotInfo(Me.EncryptionKey, Me.InitVector, Me._User, Me._User.PortalID, Me._UserBot)

            End Sub

        End Class

#End Region

    End Class
End Namespace
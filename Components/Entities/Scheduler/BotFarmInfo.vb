﻿Imports Aricie.DNN.ComponentModel
Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports Aricie.ComponentModel
Imports System.Security.Cryptography
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls.EditControls
Imports Aricie.DNN.Services
Imports System.Xml.Serialization
Imports System.Threading
Imports Aricie.DNN.Diagnostics
Imports System.Globalization
Imports Aricie.DNN.UI.Controls
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.UI.Skins
Imports DotNetNuke.UI.Skins.Controls
Imports Aricie.DNN.UI.WebControls
Imports Aricie.DNN.Services.Files
Imports System.Security.Cryptography.X509Certificates
Imports System.Xml
Imports Aricie.DNN.Security.Trial
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Services.Exceptions

Namespace Aricie.DNN.Modules.PortalKeeper

    Public Enum SynchronizationMode
        Monitor
        Mutex
    End Enum



    <ActionButton(IconName.Calendar, IconOptions.Normal)> _
    <XmlInclude(GetType(UserVariableInfo))> _
    <Serializable()> _
    Public Class BotFarmInfo(Of TEngineEvent As IConvertible)
        Inherits NamedConfig




#Region "Private members"

     

        Private _Schedule As New STimeSpan(TimeSpan.FromSeconds(15))

        Private _EnableLogs As Boolean

        Private _Synchronization As SynchronizationMode = SynchronizationMode.Monitor

        Private _MutexName As String = "Aricie.PKP.FarmMutex"

        Private _SynchronisationTimeout As New STimeSpan(TimeSpan.FromSeconds(1))

#End Region

#Region "Public properties"


        <TrialLimited(TrialPropertyMode.NoAdd Or TrialPropertyMode.NoDelete), LabelMode(LabelMode.Top), Editor(GetType(PropertyEditorEditControl), GetType(EditControl)), ExtendedCategory("Bots"), MainCategory()>
        Public Property Bots As New SimpleList(Of BotInfo(Of TEngineEvent))


        <ExtendedCategory("UserBots")>
        Public Property EnableUserBots As Boolean


        <TrialLimited(TrialPropertyMode.NoAdd Or TrialPropertyMode.NoDelete), LabelMode(LabelMode.Top), Editor(GetType(PropertyEditorEditControl), GetType(EditControl)), ConditionalVisible("EnableUserBots", False, True), ExtendedCategory("UserBots")>
        Public Property UserBots As New SimpleList(Of UserBotSettings(Of TEngineEvent))


        <XmlIgnore()> _
        <Browsable(False)>
        Public ReadOnly Property AvailableUserBots() As IDictionary(Of String, UserBotSettings(Of TEngineEvent))
            Get
                Return ProviderList(Of UserBotSettings(Of TEngineEvent)).GetAvailable(Me.UserBots.Instances)
            End Get
        End Property


      
        <ExtendedCategory("TechnicalSettings")>
        Public Property Schedule() As STimeSpan
            Get
                Return _Schedule
            End Get
            Set(ByVal value As STimeSpan)
                _Schedule = value
            End Set
        End Property

        <ExtendedCategory("TechnicalSettings")>
        Public Property EnableLogs() As Boolean
            Get
                Return _EnableLogs
            End Get
            Set(ByVal value As Boolean)
                _EnableLogs = value
            End Set
        End Property


        <ExtendedCategory("TechnicalSettings")>
        Public Property Synchronization() As SynchronizationMode
            Get
                Return _Synchronization
            End Get
            Set(ByVal value As SynchronizationMode)
                _Synchronization = value
            End Set
        End Property

        <Required(True)> _
        <ConditionalVisible("Synchronization", False, True, SynchronizationMode.Mutex)> _
        <ExtendedCategory("TechnicalSettings")>
        Public Property MutexName() As String
            Get
                Return _MutexName
            End Get
            Set(ByVal value As String)
                _MutexName = value
            End Set
        End Property


        <ExtendedCategory("TechnicalSettings")>
        Public Property SynchronisationTimeout() As STimeSpan
            Get
                Return _SynchronisationTimeout
            End Get
            Set(ByVal value As STimeSpan)
                _SynchronisationTimeout = value
            End Set
        End Property


#End Region


#Region "methods"

        '<NonSerialized()> _
        'Private lastRun As DateTime = DateTime.MinValue
        Private Shared botlock As New Object


        'Private Shared _FarmMutex As Mutex

        'Private ReadOnly Property FarmMutex As Mutex
        '    Get
        '        If _FarmMutex Is Nothing Then
        '            _FarmMutex = New Mutex(False, Me._MutexName)
        '        End If
        '        Return _FarmMutex
        '    End Get
        'End Property

      

        <ActionButton(IconName.Rocket, IconOptions.Normal)>
        Public Sub RunForcedBots(pmb As AriciePortalModuleBase)
            Dim flowid As String = Guid.NewGuid.ToString
            If PortalKeeperConfig.Instance.SchedulerFarm.EnableLogs Then

                Dim objStep As New StepInfo(Debug.PKPDebugType, "Manual Run Start", WorkingPhase.InProgress, False, False, -1, flowid)
                PerformanceLogger.Instance.AddDebugInfo(objStep)
            End If
            Dim nbRuns As Integer = Me.RunBots(CType(PortalKeeperSchedule.ScheduleEventList, IList(Of TEngineEvent)), True, Guid.NewGuid.ToString)

            If PortalKeeperConfig.Instance.SchedulerFarm.EnableLogs Then

                Dim objStep As New StepInfo(Debug.PKPDebugType, "Manual Run End", WorkingPhase.InProgress, True, False, -1, flowid)
                PerformanceLogger.Instance.AddDebugInfo(objStep)
            End If

            Skin.AddModuleMessage(pmb, String.Format(Localization.GetString("ManualRun.Message", pmb.LocalResourceFile), nbRuns), ModuleMessage.ModuleMessageType.GreenSuccess)
        End Sub


        Public Function RunBots(ByVal events As IList(Of TEngineEvent), ByVal forceRun As Boolean, flowId As String) As Integer
            'Dim minNextRun As DateTime = lastRun.Add(Me._Schedule)
            'If minNextRun < Now Then
            Dim toreturn As Integer
            Select Case Me._Synchronization
                Case SynchronizationMode.Monitor
                    If Monitor.TryEnter(botlock, Me._SynchronisationTimeout.Value) Then
                        Try
                            toreturn = Me.RunBotsUnlocked(events, forceRun, flowId)
                        Catch ex As Exception
                            AsyncLogger.Instance.AddException(ex)
                            'DotNetNuke.Services.Exceptions.LogException(ex)
                        Finally
                            Monitor.Exit(botlock)
                        End Try
                        'lastRun = Now
                    End If
                Case SynchronizationMode.Mutex
                    Using objMutex As New Mutex(False, Me._MutexName)
                        If objMutex.WaitOne(Me._SynchronisationTimeout.Value) Then
                            Try
                                toreturn = Me.RunBotsUnlocked(events, forceRun, flowId)
                            Catch ex As Exception
                                AsyncLogger.Instance.AddException(ex)
                                'DotNetNuke.Services.Exceptions.LogException(ex)
                            Finally
                                objMutex.ReleaseMutex()
                            End Try
                        End If
                    End Using
            End Select
            'Else
            'Thread.Sleep(GetHalfSchedule)
            'End If
            Return toreturn
        End Function

        ''<ConditionalVisible("Storage", False, True, UserBotStorage.Personalisation)> _
        '<ActionButton(IconName.Unlock, IconOptions.Normal)> _
        'Public Sub UnlockUserBotManager()
        '    For Each objUserBot As UserBotSettings(Of TEngineEvent) In Me.UserBots.Instances
        '        objUserBot.SetEncrypter(Me)
        '    Next
        'End Sub


        'Private Function GetHalfSchedule() As TimeSpan
        '    Return TimeSpan.FromTicks(Me._Schedule.Ticks \ 2)
        'End Function


        Private Function RunBotsUnlocked(ByVal events As IList(Of TEngineEvent), ByVal forceRun As Boolean, flowId As String) As Integer
            Dim toreturn As Integer
            'Dim flowid As String = ""
            If Me._EnableLogs Then
                'If flowId = "" Then
                '    flowId = Guid.NewGuid.ToString
                'End If
                'flowId = Guid.NewGuid.ToString
                Dim objStep As New StepInfo(Debug.PKPDebugType, "Farm Run Start", WorkingPhase.InProgress, False, False, -1, flowId)
                PerformanceLogger.Instance.AddDebugInfo(objStep)
            End If
            For Each webBot As BotInfo(Of TEngineEvent) In Me.Bots.Instances
                'Dim unused As Boolean = True
                'For Each objUserBotSettings As UserBotSettings(Of TEngineEvent) In Me.AvailableUserBots.Values
                '    If objUserBotSettings.BotName = webBot.Name AndAlso objUserBotSettings.DisableTemplateBot Then
                '        unused = False
                '    End If
                'Next
                If Not webBot.MasterBotDisabled AndAlso Not webBot.AsyncLockBot.ContainsKey(-1) Then
                    'Dim botHistory = Aricie.DNN.Settings.SettingsController.LoadFileSettings(Of WebBotHistory)(GetLogMapPath(), True)
                    Dim runContext As New BotRunContext(Of TEngineEvent)(webBot)
                    runContext.Events = events
                    runContext.History = webBot.BotHistory
                    runContext.RunEndDelegate = AddressOf webBot.SaveHistory

                    If webBot.RunBot(runContext, forceRun) Then
                        toreturn += 1
                    End If
                    'Aricie.DNN.Settings.SettingsController.SaveFileSettings(Of WebBotHistory)(GetLogMapPath(), webBot.BotHistory)
                End If
            Next
            If Me.EnableUserBots Then
                If Me._EnableLogs Then
                    Dim objStep As New StepInfo(Debug.PKPDebugType, "User Bots Start", WorkingPhase.InProgress, False, False, -1, flowId)
                    PerformanceLogger.Instance.AddDebugInfo(objStep)
                End If
                For Each userSettings As UserBotSettings(Of TEngineEvent) In Me.AvailableUserBots.Values

                    toreturn += userSettings.RunUserBots(events, forceRun)
                Next
            End If
            If Me._EnableLogs Then
                Dim nbRuns As New KeyValuePair(Of String, String)("Nb of Bots run", toreturn.ToString(CultureInfo.InvariantCulture))
                Dim objStep As New StepInfo(Debug.PKPDebugType, "Farm Run End", WorkingPhase.InProgress, False, False, -1, flowId, nbRuns)
                PerformanceLogger.Instance.AddDebugInfo(objStep)
            End If
            Return toreturn
        End Function

        'Public Sub Encrypt(Of T)(ByVal objSmartFile As SmartFile(Of T)) Implements IEncrypter.Encrypt
        '    If Not objSmartFile.Encrypted Then
        '        Dim salt As Byte() = Nothing
        '        Dim newPayLoad As String = Common.Encrypt(objSmartFile.PayLoad, Me._EncryptionKey, Me._InitVector, salt)
        '        objSmartFile.Encrypt(newPayLoad, salt)
        '    End If
        'End Sub

        'Public Sub Decrypt(Of T)(ByVal objSmartFile As SmartFile(Of T)) Implements IEncrypter.Decrypt
        '    If objSmartFile.Encrypted Then
        '        Dim newPayLoad As String = Common.Decrypt(objSmartFile.PayLoad, Me._EncryptionKey, "", objSmartFile.SaltBytes)
        '        objSmartFile.Decrypt(newPayLoad)
        '    End If
        'End Sub

      



#End Region


    End Class
End Namespace
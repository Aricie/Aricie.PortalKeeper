Imports Aricie.DNN.UI.Controls
Imports Aricie.DNN.Settings
Imports Aricie.DNN.Security.Trial
Imports Aricie.Collections
Imports Aricie.DNN.Security.Cryptography
Imports Aricie.Security.Cryptography
Imports System.Security.Cryptography
Imports System.Diagnostics
Imports DotNetNuke.Common
Imports DotNetNuke.UI.Skins.Controls
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.UI.Skins
Imports Aricie.DNN.Configuration
Imports Aricie.DNN.Services
Imports Aricie.Services
Imports Aricie.DNN.Diagnostics
Imports DotNetNuke.UI.WebControls
Imports System.Collections.Generic
Imports System.Xml
Imports Aricie.DNN.Security
Imports System.Linq
Imports System.IO

Namespace Aricie.DNN.Modules.PortalKeeper.UI
    Partial Class KeeperConfig
        Inherits AriciePortalModuleBase






        Public Sub New()
            MyBase.New()
#If DEBUG Then
            AddHandler DnnContext.Current.Debug, AddressOf Me.OnDebug
#End If
        End Sub


        Private _KeeperConfig As PortalKeeperConfig
        Private _KeeperSettings As FirewallSettings

        Protected ReadOnly Property KeeperConfig() As PortalKeeperConfig
            Get

                If _KeeperConfig Is Nothing Then
                    If Me.UserInfo.IsSuperUser Then
                        If Not Me.IsPostBack OrElse Session("KeeperConfig") Is Nothing Then
                            Session("KeeperConfig") = ReflectionHelper.CloneObject(Of PortalKeeperConfig)(PortalKeeperConfig.Instance)
                        End If
                        _KeeperConfig = DirectCast(Session("KeeperConfig"), PortalKeeperConfig)
                    Else
                        _KeeperConfig = PortalKeeperConfig.Instance
                    End If
                End If

                Return _KeeperConfig
            End Get
        End Property

        Protected ReadOnly Property KeeperSettings() As FirewallSettings
            Get
                If _KeeperSettings Is Nothing Then
                    If Not Me.IsPostBack OrElse Session("KeeperSettings") Is Nothing Then
                        Session("KeeperSettings") = SettingsController.GetModuleSettings(Of FirewallSettings)(SettingsScope.PortalSettings, NukeHelper.PortalId)
                    End If
                    _KeeperSettings = DirectCast(Session("KeeperSettings"), FirewallSettings)
                End If
                Return _KeeperSettings
            End Get
        End Property


        Private _UserBotSettings As UserBotSettings(Of ScheduleEvent)

        Protected ReadOnly Property UserBotSettings As UserBotSettings(Of ScheduleEvent)
            Get
                If _UserBotSettings Is Nothing Then
                    Me.KeeperConfig.SchedulerFarm.AvailableUserBots.TryGetValue(Me.KeeperModuleSettings.UserBotName, _UserBotSettings)
                End If
                Return _UserBotSettings
            End Get
        End Property


        Private _UserBot As UserBotInfo

        Public Property UserBot() As UserBotInfo
            Get
                If _UserBot Is Nothing Then

                    If Me.UserBotSettings IsNot Nothing Then
                        Dim key As String = "UserBot" & Me.UserBotSettings.Name
                        If Not Me.IsPostBack OrElse Session(key) Is Nothing Then

                            Session(key) = Me.UserBotSettings.GetUserBotInfo(Me.UserInfo, True)
                        End If
                        _UserBot = DirectCast(Session(key), UserBotInfo)
                        If Me.KeeperModuleSettings.DisplayRankings Then
                            Dim objRankings As List(Of ProbeRanking) = Me.UserBotSettings.GetRankings()
                            If objRankings IsNot Nothing Then
                                _UserBot.Rankings = objRankings
                            End If
                        End If
                    End If
                End If

                Return _UserBot
            End Get
            Set(value As UserBotInfo)
                Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
                If Me.KeeperConfig.SchedulerFarm.AvailableUserBots.TryGetValue(Me.KeeperModuleSettings.UserBotName, userSettings) Then
                    userSettings.SetUserBotInfo(Me.UserInfo, Me.PortalId, value)
                    Dim key As String = "UserBot" & userSettings.Name
                    Session.Remove(key)
                    _UserBot = Nothing
                End If

            End Set
        End Property


        Protected ReadOnly Property KeeperModuleSettings() As KeeperModuleSettings
            Get
                Return Me.GetModuleSettings(Of KeeperModuleSettings)()
            End Get
        End Property

#Region "Event Handlers"

        Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
            Try
                AddHandler Me.KC.Debug, AddressOf Me.OnDebug
                If Not Me.IsPostBack Then
                    AssertInstalled()
#If DEBUG Then
                    Me.divDebug.Visible = True
#End If
                End If
                EnforceFreeVersion()
                BindSettings()
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub


        Private Sub EnforceFreeVersion()

            Dim tc As TrialController = TrialController.Instance(New TrialProvider())
            Dim objTrialStatus As TrialStatusInfo = tc.LimitModule(Me, True)
            Me.KC.TrialStatus = objTrialStatus
            Me.KS.TrialStatus = objTrialStatus
        End Sub

        Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            Try
                If Not Me.IsPostBack Then
                    ManageVisibility()
                End If
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        'Private Sub cmdSaveConfig_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSaveConfig.Click
        '    Try
        '        If Me.UserInfo.IsSuperUser AndAlso Me.KC.IsValid Then
        '            Dim objSettings As PortalKeeperConfig = DirectCast(Me.KC.DataSource, PortalKeeperConfig)
        '            PortalKeeperConfig.Save(objSettings)
        '            Skin.AddModuleMessage(Me, Localization.GetString("HostConfigSaved.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
        '        Else
        '            Skin.AddModuleMessage(Me, Localization.GetString("HostConfigInvalid.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning)
        '        End If
        '    Catch ex As Exception
        '        ProcessModuleLoadException(Me, ex)
        '    End Try
        'End Sub

        Protected Sub cmdSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdSave.Click
            Try
                If Me.IsAdmin AndAlso Me.KS.IsValid Then
                    Dim objSettings As FirewallSettings = DirectCast(Me.KS.DataSource, FirewallSettings)
                    SettingsController.SetModuleSettings(Of FirewallSettings)(SettingsScope.PortalSettings, Me.PortalId, objSettings)
                    Skin.AddModuleMessage(Me, Localization.GetString("ConfigSaved.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
                Else
                    Skin.AddModuleMessage(Me, Localization.GetString("ConfigInvalid.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning)
                End If
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        'Protected Sub cmdRunBots_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdRunBots.Click
        '    Try
        '        'Dim flowid As String = Guid.NewGuid.ToString
        '        'If PortalKeeperConfig.Instance.SchedulerFarm.EnableLogs Then

        '        '    Dim objStep As New StepInfo(Debug.PKPDebugType, "Manual Run Start", _
        '        '                                WorkingPhase.InProgress, False, False, -1, flowid)
        '        '    PerformanceLogger.Instance.AddDebugInfo(objStep)
        '        'End If
        '        'Dim nbRuns As Integer = PortalKeeperConfig.Instance.SchedulerFarm.RunBots(PortalKeeperSchedule.ScheduleEventList, True, Guid.NewGuid.ToString)

        '        'If PortalKeeperConfig.Instance.SchedulerFarm.EnableLogs Then

        '        '    Dim objStep As New StepInfo(Debug.PKPDebugType, "Manual Run End", _
        '        '                                WorkingPhase.InProgress, True, False, -1, flowid)
        '        '    PerformanceLogger.Instance.AddDebugInfo(objStep)
        '        'End If

        '        'Skin.AddModuleMessage(Me, String.Format(Localization.GetString("ManualRun.Message", Me.LocalResourceFile), nbRuns), ModuleMessage.ModuleMessageType.GreenSuccess)
        '        PortalKeeperConfig.Instance.SchedulerFarm.RunForcedBots(Me)
        '        Me.BindSettings()
        '    Catch ex As Exception
        '        ProcessModuleLoadException(Me, ex)
        '    End Try
        'End Sub

        Protected Sub cmdInstall_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdInstall.Click
            Try
                Configuration.ConfigHelper.ProcessModuleUpdate(Configuration.ConfigActionType.Install, New PortalKeeperConfigUpdate)
                Response.Redirect(NavigateURL())
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        Protected Sub cmdUninstall_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdUninstall.Click
            Try
                Dim uninstallProcedure As IUpdateProvider
                Dim errorsUpdater As IUpdateProvider = KeeperConfig.FirewallConfig.CustomErrorsConfig.GetUpdateProvider
                If ConfigHelper.IsInstalled(errorsUpdater, True) Then
                    uninstallProcedure = New MultiUpdateProvider(errorsUpdater, New PortalKeeperConfigUpdate)
                Else
                    uninstallProcedure = New PortalKeeperConfigUpdate
                End If
                ConfigHelper.ProcessModuleUpdate(Configuration.ConfigActionType.Uninstall, uninstallProcedure)
                Response.Redirect(Me.Request.Url.AbsoluteUri)
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        Private Sub cmdClearProbes_Click(sender As Object, e As System.EventArgs) Handles cmdClearProbes.Click
            Try

                CacheHelper.RemoveCache("PKPProbes")
                Skin.AddModuleMessage(Me, Localization.GetString("ProbesCleared.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        Protected Sub cmdDebug_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdDebug.Click
            Me.OnCmdDebug()
        End Sub

        Private Sub cmdCancelSettings_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdCancelSettings.Click
            Try
                Me.Response.Redirect(NavigateURL)
            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub

        'Private Sub cmdCancelUserBot_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdCancelUserBot.Click
        '    Try
        '        Me.Response.Redirect(NavigateURL)
        '    Catch ex As Exception
        '        ProcessModuleLoadException(Me, ex)
        '    End Try
        'End Sub

        'Private Sub cmdSaveUserBot_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSaveUserBot.Click
        '    Try
        '        If Me.ctUserBotEntities.IsValid Then
        '            Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
        '            If Me.KeeperConfig.SchedulerFarm.AvailableUserBots.TryGetValue(Me.KeeperModuleSettings.UserBotName, userSettings) Then
        '                Dim userBotEntity As UserBotInfo = DirectCast(Me.ctUserBotEntities.DataSource, UserBotInfo)
        '                Dim readOnlyUserBot As UserBotInfo = userSettings.GetUserBotInfo(Me.KeeperConfig.SchedulerFarm, Me.UserInfo, Me.PortalId, True)
        '                userBotEntity.RevertReadonlyParameters(readOnlyUserBot)
        '                Me.UserBot = userBotEntity
        '                Me.BindSettings()
        '                Skin.AddModuleMessage(Me, Localization.GetString("UserBotSaved.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
        '            End If

        '        End If
        '    Catch ex As Exception
        '        ProcessModuleLoadException(Me, ex)
        '    End Try
        'End Sub

        'Private Sub cmdDeleteUserBot_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdDeleteUserBot.Click
        '    Try
        '        Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
        '        If Me.KeeperConfig.SchedulerFarm.AvailableUserBots.TryGetValue(Me.KeeperModuleSettings.UserBotName, userSettings) Then
        '            Me.UserBot = Nothing
        '            Me.BindSettings()
        '            Skin.AddModuleMessage(Me, Localization.GetString("UserBotDeleted.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess)
        '        End If
        '    Catch ex As Exception
        '        ProcessModuleLoadException(Me, ex)
        '    End Try
        'End Sub

#End Region

#Region "Private methods"

        Private Sub BindSettings()
            If Me.UserInfo.IsSuperUser Then
                Me.KC.LocalResourceFile = Me.SharedResourceFile
                Me.KC.DataSource = Me.KeeperConfig
                Me.KC.DataBind()
            Else
                If Me.KeeperConfig.SchedulerFarm.EnableUserBots AndAlso Not String.IsNullOrEmpty(Me.KeeperModuleSettings.UserBotName) Then
                    If Me.KeeperModuleSettings.UserBot IsNot Nothing AndAlso Me.KeeperModuleSettings.UserBot.Bot IsNot Nothing Then
                        Dim userSettings As UserBotSettings(Of ScheduleEvent) = Nothing
                        If Me.KeeperConfig.SchedulerFarm.AvailableUserBots.TryGetValue(Me.KeeperModuleSettings.UserBotName, userSettings) Then

                            If Me.KeeperModuleSettings.EnableActionCommands AndAlso Me.KeeperModuleSettings.UserBot.ActionCommands.Count > 0 Then
                                Me.divActionCommands.Visible = True
                                If Me.divActionCommands.Controls.Count = 0 Then
                                    For Each objActionCommand As ActionCommand In Me.KeeperModuleSettings.UserBot.ActionCommands
                                        Dim cb As New CommandButton()
                                        Me.divActionCommands.Controls.Add(cb)
                                        cb.ID = "cmd" & objActionCommand.Name
                                        cb.Text = objActionCommand.Name
                                        If Not String.IsNullOrEmpty(objActionCommand.ResxKey) Then
                                            cb.ResourceKey = objActionCommand.ResxKey
                                        End If
                                        cb.ImageUrl = objActionCommand.IconPath
                                        cb.CommandName = objActionCommand.Name
                                        AddHandler cb.Click, AddressOf ActionCommandClick
                                    Next
                                End If
                            End If

                            Me.ctUserBotEntities.LocalResourceFile = Me.SharedResourceFile
                            Me.ctUserBotEntities.DataSource = Me.UserBot

                            Me.ctUserBotEntities.DataBind()
                            If Me.KeeperModuleSettings.DisplayBotConfig Then
                                Me.lblBotDefinition.Visible = True
                                Me.ctlUserBot.LocalResourceFile = Me.SharedResourceFile.Replace("SharedResources", Me.KeeperModuleSettings.UserBot.ResourceFile)
                                Me.ctlUserBot.EditMode = DotNetNuke.UI.WebControls.PropertyEditorMode.View
                                Me.ctlUserBot.DataSource = Me.KeeperModuleSettings.UserBot.Bot
                                Me.ctlUserBot.DataBind()
                            Else
                                Me.lblBotDefinition.Visible = False
                            End If
                        End If
                    End If
                End If
            End If
            If Me.KeeperConfig.FirewallConfig.EnablePortalLevelSettings AndAlso Me.IsAdmin Then
                Me.KS.LocalResourceFile = Me.SharedResourceFile
                Me.KS.DataSource = Me.KeeperSettings
                Me.KS.DataBind()
            End If
        End Sub


        Private Sub ActionCommandClick(sender As Object, e As EventArgs)
            Dim cb As CommandButton = DirectCast(sender, CommandButton)
            Dim commandName As String = cb.CommandName
            For Each objActionCommand As ActionCommand In Me.KeeperModuleSettings.UserBot.ActionCommands
                If objActionCommand.Name = commandName Then
                    Try
                        Dim resultContext As New PortalKeeperContext(Of ScheduleEvent)
                        resultContext.Init(Me.UserBotSettings.Bot, Me.UserBot.GetParameterValues(Me.UserInfo))
                        resultContext.Init(objActionCommand)
                        objActionCommand.BatchRun(PortalKeeperSchedule.ScheduleEventList, resultContext)
                        Dim resultVars As SerializableDictionary(Of String, Object) = resultContext.ComputeDynamicExpressions(objActionCommand.ComputedVars, True)
                        If resultVars.Count > 0 Then
                            UserBot.ComputedEntities = resultVars
                        End If
                        Me.BindSettings()
                    Catch ex As Exception
                        Aricie.Services.ExceptionHelper.LogException(ex)
                    End Try
                End If
            Next
        End Sub

        Private Sub AssertInstalled()
            If Me.UserInfo.IsSuperUser Then
                Me.divInstall.Visible = True
                Configuration.ConfigHelper.AssertIsInstalled(Me, New PortalKeeperConfigUpdate, Me.cmdInstall, New Control() {Me.cmdUninstall, Me.divConfig})
                Me.AddConfirm(Me.cmdUninstall)
            Else
                Me.divInstall.Visible = False
            End If
        End Sub

        Private Sub ManageVisibility()
            If Me.UserInfo.IsSuperUser Then
                Me.divHostConfig.Visible = True
                Me.divPortalSettings.Visible = Me.KeeperConfig.FirewallConfig.EnablePortalLevelSettings
            Else
                Me.divHostConfig.Visible = False
                If Me.IsAdmin Then
                    If Not Me.KeeperConfig.FirewallConfig.EnablePortalLevelSettings Then
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(Me, Localization.GetString("PortalLevelSettingsDisabled.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning)
                    Else
                        Me.divPortalSettings.Visible = True
                    End If
                End If
                If Me.KeeperConfig.SchedulerFarm.EnableUserBots AndAlso Not String.IsNullOrEmpty(Me.KeeperModuleSettings.UserBotName) Then
                    If Me.KeeperModuleSettings.UserBot IsNot Nothing Then
                        Me.divUserBot.Visible = True
                        If Not DotNetNuke.Security.Permissions.ModulePermissionController.HasModulePermission(Me.ModuleConfiguration.ModulePermissions, "EDIT") Then
                            Me.ctUserBotEntities.EditMode = DotNetNuke.UI.WebControls.PropertyEditorMode.View
                            Me.divUserBotCmds.Visible = False
                        Else
                            Me.ctUserBotEntities.EditMode = DotNetNuke.UI.WebControls.PropertyEditorMode.Edit
                            Me.divUserBotCmds.Visible = True
                        End If
                    Else
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(Me, Localization.GetString("NoUserBot.Message", Me.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning)
                    End If
                End If
            End If

        End Sub


#End Region


#Region "Debug"

        Private Sub OnCmdDebug()

            Try

                'Dim RSAalg As New RSACryptoServiceProvider(2048)
                'DnnContext.Instance.AddModuleMessage(String.Format("KeySize: {0}", RSAalg.KeySize.ToString()), ModuleMessage.ModuleMessageType.GreenSuccess)
                Dim monTexte As String = CryptoHelper.GetNewSalt(1000).ToBase64()
                DnnContext.Instance.AddModuleMessage(String.Format("Texte: {0}", monTexte), ModuleMessage.ModuleMessageType.GreenSuccess)


                Dim sw As New Stopwatch()
                sw.Start()

                'Dim test As String = "test".ToUTF8().WriteSecureString(True, Encoding.UTF8, True).ReadSecureStringToBytes(Encoding.UTF8).FromUTF8()

                For i As Integer = 0 To 0

                    'Dim processed As Byte() = RSAalg.EncryptByBlocks(monTexte.FromBase64())
                    'DnnContext.Instance.AddModuleMessage(String.Format("Encrypted: {0}", processed.ToBase64()), ModuleMessage.ModuleMessageType.GreenSuccess)
                    'processed = RSAalg.DecryptBlocks(processed)
                    Dim processed As String = CryptoHelper.WriteSecureString(monTexte, True).ReadSecureString()

                    DnnContext.Instance.AddModuleMessage(String.Format("Decrypted: {0}", processed), ModuleMessage.ModuleMessageType.GreenSuccess)
                Next


                DnnContext.Instance.AddModuleMessage(String.Format("Time Elapsed: {0}", Common.FormatTimeSpan(sw.Elapsed)), ModuleMessage.ModuleMessageType.GreenSuccess)

            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub


        Private Sub OnDebug(sender As Object, e As DebugEventArgs)
            Try

            Catch ex As Exception
                ProcessModuleLoadException(Me, ex)
            End Try
        End Sub



#End Region

    End Class
End Namespace



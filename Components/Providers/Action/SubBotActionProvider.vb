﻿Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports Aricie.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls.EditControls
Imports Aricie.DNN.UI.WebControls

Namespace Aricie.DNN.Modules.PortalKeeper

    <ActionButton(IconName.Android, IconOptions.Normal)>
    <DisplayName("Sub Bot Action Provider")>
    <Description("This provider allows to run a sub-bot as a dedicated action. Because of the sequential nature of the engine, the subbot triggers each previous step up to the current step, the rules and action should be configured accordingly")>
    Public Class SubBotActionProvider(Of TEngineEvents As IConvertible)
        Inherits AsyncEnabledActionProvider(Of TEngineEvents)


        Private _CatchUpPreviousSteps As Boolean

        Private _ForceRun As Boolean = False

        Private _SubBot As New BotInfo(Of TEngineEvents)

        <ExtendedCategory("")>
        <MainCategory()>
        Public Property CatchUpPreviousSteps() As Boolean
            Get
                Return _CatchUpPreviousSteps
            End Get
            Set(ByVal value As Boolean)
                _CatchUpPreviousSteps = value
            End Set
        End Property


        <ExtendedCategory("")>
        <MainCategory()>
        Public Property ForceRun() As Boolean
            Get
                Return _ForceRun
            End Get
            Set(ByVal value As Boolean)
                _ForceRun = value
            End Set
        End Property


        <ExtendedCategory("")>
        <MainCategory()>
        <Editor(GetType(PropertyEditorEditControl), GetType(EditControl))>
        <LabelMode(LabelMode.Top)>
        Public Property SubBot() As BotInfo(Of TEngineEvents)
            Get
                Return _SubBot
            End Get
            Set(ByVal value As BotInfo(Of TEngineEvents))
                _SubBot = value
            End Set
        End Property


        Protected Overloads Overrides Function Run(ByVal actionContext As PortalKeeperContext(Of TEngineEvents), ByVal aSync As Boolean) As Boolean
            If Me.DebuggerBreak Then
                Common.CallDebuggerBreak()
            End If
            Dim listEvents As New List(Of TEngineEvents)
            If Me._CatchUpPreviousSteps Then
                For Each prevStep As TEngineEvents In actionContext.PreviousSteps
                    listEvents.Add(prevStep)
                Next
            End If
            listEvents.Add(actionContext.CurrentEventStep)
            Dim runContext As New BotRunContext(Of TEngineEvents)(Me.SubBot, DateTime.MinValue)
            runContext.Events = listEvents
            runContext.EngineContext = actionContext
            Dim mainEngine As RuleEngineSettings(Of TEngineEvents) = actionContext.CurrentEngine
            runContext.EngineContext.Init(Me.SubBot)
            Dim toReturn As Boolean = Me.SubBot.RunBot(runContext, Me._ForceRun)
            runContext.EngineContext.SetEngine(mainEngine)
            Return toReturn
        End Function

    End Class
End Namespace
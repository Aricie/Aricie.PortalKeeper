﻿Imports Aricie.DNN.Services.Flee
Imports Aricie.DNN.UI.Attributes
Imports Aricie.Services
Imports System.Reflection
Imports Aricie.DNN.Services
Imports System.Linq
Imports System.ComponentModel
Imports System.Xml.Serialization
Imports Aricie.Collections
Imports DotNetNuke.UI.WebControls

Namespace Aricie.DNN.Modules.PortalKeeper

    Public Enum EventHandlerType
        DelegateExpression
        KeeperAction
    End Enum


    <Serializable()> _
    Public Class KeeperObjectAction(Of TEngineEvents As IConvertible)
        Inherits GeneralObjectAction

        <ExtendedCategory("Action")> _
        <ConditionalVisible("HasType", False, True)> _
       <ConditionalVisible("ActionMode", False, True, ObjectActionMode.AddEventHandler)> _
        Public Property EventHandlerType As EventHandlerType


        <ExtendedCategory("Action")> _
        <ConditionalVisible("HasType", False, True)> _
       <ConditionalVisible("ActionMode", False, True, ObjectActionMode.AddEventHandler)> _
       <ConditionalVisible("EventHandlerType", False, True, EventHandlerType.DelegateExpression)> _
        Public Overrides Property DelegateExpression As FleeExpressionInfo(Of [Delegate])
            Get
                Return MyBase.DelegateExpression
            End Get
            Set(value As FleeExpressionInfo(Of [Delegate]))
                MyBase.DelegateExpression = value
            End Set
        End Property

        <ExtendedCategory("Action")> _
        <ConditionalVisible("HasType", False, True)> _
      <ConditionalVisible("ActionMode", False, True, ObjectActionMode.AddEventHandler)> _
      <ConditionalVisible("EventHandlerType", False, True, EventHandlerType.KeeperAction)> _
        Public Property PassArguments As Boolean


        <Browsable(False)> _
       <XmlIgnore()> _
        Public ReadOnly Property AvailableParametersAndTypes As SerializableDictionary(Of String, Type)
            Get
                Dim toReturn As New SerializableDictionary(Of String, Type)
                For Each objParam As ParameterInfo In Me.CandidateEventParams
                    Dim objParamName As String = objParam.Name
                    If objParamName = "e" Then
                        objParamName = DynamicControlAdapter.EventArgsVarName
                    End If
                    toReturn.Add(objParamName, objParam.ParameterType)
                Next
                Return toReturn
            End Get
        End Property



        <ExtendedCategory("Action")> _
        <ConditionalVisible("HasType", False, True)> _
      <ConditionalVisible("ActionMode", False, True, ObjectActionMode.AddEventHandler)> _
      <ConditionalVisible("PassArguments", False, True)> _
      <ConditionalVisible("EventHandlerType", False, True, EventHandlerType.KeeperAction)> _
        <LabelMode(LabelMode.Left)> _
        <XmlIgnore()> _
        <CollectionEditor(DisplayStyle:=CollectionDisplayStyle.List)> _
        Public ReadOnly Property AvailableParameters As Dictionary(Of String, String)
            Get
                Return AvailableParametersAndTypes.ToDictionary(Of String, String)(Function(objPair) objPair.Key, Function(objPair) ReflectionHelper.GetSafeTypeName(objPair.Value))
            End Get
        End Property




        <ExtendedCategory("Action")> _
        <ConditionalVisible("HasType", False, True)> _
      <ConditionalVisible("ActionMode", False, True, ObjectActionMode.AddEventHandler)> _
      <ConditionalVisible("EventHandlerType", False, True, EventHandlerType.KeeperAction)> _
        Public Property KeeperAction As New KeeperAction(Of TEngineEvents)

        Private _CandidateEventParams As ParameterInfo() = Nothing

        <Browsable(False)> _
        Public ReadOnly Property CandidateEventParams As ParameterInfo()

            Get
                If _CandidateEventParams Is Nothing Then
                    _CandidateEventParams = ReflectionHelper.GetEventParameters(Me.CandidateEvent)
                End If
                Return _CandidateEventParams
            End Get
        End Property

        Protected Overrides Sub AddEventHandler(owner As Object, globalVars As IContextLookup, target As Object)
            If Me.EventHandlerType = PortalKeeper.EventHandlerType.DelegateExpression Then
                MyBase.AddEventHandler(owner, globalVars, target)
            Else
                Dim objDelegateInfo As New KeeperObjectActionDelegateInfo(Of TEngineEvents)(Me, DirectCast(owner, PortalKeeperContext(Of TEngineEvents)))
                Dim objaction As eventhandler = AddressOf objDelegateInfo.RunHandler
                ReflectionHelper.AddEventHandler(candidateEvent, target, objaction)
            End If
        End Sub

    End Class

    Public Class KeeperObjectActionDelegateInfo(Of TEngineEvents As IConvertible)

        Public Sub New()

        End Sub

        Public Sub New(objActions As KeeperObjectAction(Of TEngineEvents))
            Me.KeeperObjectAction = objActions
        End Sub

        Public Sub New(objActions As KeeperObjectAction(Of TEngineEvents), objContext As PortalKeeperContext(Of TEngineEvents))
            Me.New(objActions)
            ActionContext = objContext
        End Sub

        Public Property EventParameters As ParameterInfo()

        Public Property KeeperObjectAction As KeeperObjectAction(Of TEngineEvents)

        Public Property ActionContext As PortalKeeperContext(Of TEngineEvents)

        'ParamArray objParams() As Object
        Public Sub RunHandler(sender As Object, e As EventArgs)
            If KeeperObjectAction.PassArguments Then
                ActionContext.SetVar(KeeperObjectAction.CandidateEventParams(0).Name, sender)
                Dim objParamName As String = KeeperObjectAction.CandidateEventParams(1).Name
                If objParamName = "e" Then
                    objParamName = DynamicControlAdapter.EventArgsVarName
                End If
                ActionContext.SetVar(objParamName, e)
            End If
            Me.KeeperObjectAction.KeeperAction.Run(ActionContext)
        End Sub

    End Class


End Namespace



﻿Imports System.Web.UI.Adapters
Imports Aricie.Collections
Imports Aricie.DNN.UI.WebControls.EditControls
Imports Aricie.Services
Imports System.Reflection
Imports Aricie.DNN.Services

Namespace Aricie.DNN.Modules.PortalKeeper

    Public Enum HandlersRegistrationStep
        OnInit
        CreateChildControls
        OnLoad
    End Enum

    Public Enum ControlStep
        OnInit
        CreateChildControls
        OnLoad
        OnPreRender
        Render
        RenderChildren
        OnUnload
        ControlEvent
        ChildControlEvent
    End Enum

    Public Enum ControlBaseHandlerMode
        Before = -1
        Removed = 0
        After = 1
    End Enum


    Public Enum AdaptedControlMode
        Type
        Path
    End Enum

    Public Enum AdapterControlMode
        Type
        DynamicAdapter
    End Enum


    Public Class DynamicControlAdapter(Of TAdaptedControl)
        Inherits DynamicControlAdapter

        Public ReadOnly Property AdaptedControl As TAdaptedControl
            Get
                Return DirectCast(DirectCast(Me.Control, Object), TAdaptedControl)
            End Get
        End Property

        Private _Settings As ControlAdapterSettings

        Public Overrides ReadOnly Property Settings As ControlAdapterSettings
            Get
                If _Settings Is Nothing Then
                    If (Not PortalKeeperConfig.Instance.ControlAdapters.AdaptersDictionary.TryGetValue(GetType(TAdaptedControl), _Settings)) Then
                        Throw New ApplicationException(String.Format("Dynamic Control Adapter for control {0} was not found in the global configuration", Me.Control.GetType.AssemblyQualifiedName))
                    End If
                End If
                Return _Settings
            End Get
        End Property


    End Class





    Public MustInherit Class DynamicControlAdapter
        Inherits ControlAdapter 'Base(Of T)

        Public Const EventArgsVarName As String = "Eargs"

        Private Shared ReadOnly _BaseType As Type = GetType(DynamicControlAdapter(Of ))

        Public Shared Function GetGenericType(adaptedType As Type) As Type
            Return _BaseType.MakeGenericType(adaptedType)
        End Function


        Public MustOverride ReadOnly Property Settings As ControlAdapterSettings



        Public Delegate Sub ControlEventHandler()


        Protected Overrides Sub OnInit(e As EventArgs)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add(EventArgsVarName, e)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.OnInit(e)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.OnInit), False)
                If Me.Settings.HandlersRegistrationStep = HandlersRegistrationStep.OnInit Then
                    Me.RegisterEventHandlers()
                End If
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
        End Sub

        Protected Overrides Sub CreateChildControls()
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.CreateChildControls()
                                                                   End Sub), New DynamicHandlerStep(ControlStep.CreateChildControls), False)
                If Me.Settings.HandlersRegistrationStep = HandlersRegistrationStep.CreateChildControls Then
                    Me.RegisterEventHandlers()
                End If
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try

            
        End Sub


        Protected Overrides Sub OnLoad(e As EventArgs)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add(EventArgsVarName, e)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.OnLoad(e)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.OnLoad), False)
                If Me.Settings.HandlersRegistrationStep = HandlersRegistrationStep.OnLoad Then
                    Me.RegisterEventHandlers()
                End If
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
           
        End Sub

        Protected Overrides Sub OnPreRender(e As EventArgs)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add(EventArgsVarName, e)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.OnPreRender(e)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.OnPreRender), False)
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
         

        End Sub

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add("writer", writer)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.Render(writer)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.Render), False)

            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
          
        End Sub

        Protected Overrides Sub RenderChildren(writer As HtmlTextWriter)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add("writer", writer)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.RenderChildren(writer)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.RenderChildren), False)
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
          
        End Sub


        Protected Overrides Sub OnUnload(e As EventArgs)
            Try
                Dim parameters As New SerializableDictionary(Of String, Object)
                parameters.Add(EventArgsVarName, e)
                Me.ProcessStep(parameters, New ControlEventHandler(Sub()
                                                                       MyBase.OnUnload(e)
                                                                   End Sub), New DynamicHandlerStep(ControlStep.OnUnload), True)
            Catch ex As Exception
                ExceptionHelper.LogException(ex)
            End Try
        
        End Sub

        Public Sub ProcessStep(parameters As IDictionary(Of String, Object), ByVal baseHandler As ControlEventHandler, ByVal newStep As DynamicHandlerStep, ByVal endSequence As Boolean)
            Dim keeperContext As PortalKeeperContext(Of SimpleEngineEvent) = PortalKeeperContext(Of SimpleEngineEvent).Instance(HttpContext.Current)
            If Not keeperContext.Disabled Then
                Dim dynamicHandler As DynamicHandlerSettings = Nothing
                If (Not Settings.DynamicHandlersDictionary.TryGetValue(newStep, dynamicHandler) OrElse dynamicHandler.BaseHandlerMode = ControlBaseHandlerMode.Before) AndAlso baseHandler IsNot Nothing Then
                    baseHandler.Invoke()
                End If
                If dynamicHandler IsNot Nothing AndAlso dynamicHandler.Enabled Then
                    parameters.Add("Adapter", Me)
                    keeperContext.Init(dynamicHandler, parameters)
                    dynamicHandler.ProcessRules(keeperContext, SimpleEngineEvent.Run, endSequence)
                    If baseHandler IsNot Nothing AndAlso dynamicHandler.BaseHandlerMode = ControlBaseHandlerMode.After Then
                        baseHandler.Invoke()
                    End If
                End If

            End If
        End Sub


        Private Sub RegisterEventHandlers()
            For Each objDynamicHandler As DynamicHandlerSettings In Me.Settings.DynamicHandlers
                If objDynamicHandler.Enabled AndAlso objDynamicHandler.MainControlStep = ControlStep.ControlEvent OrElse objDynamicHandler.MainControlStep = ControlStep.ChildControlEvent Then
                    Dim ctrl As Control = Me.Control
                    If Not String.IsNullOrEmpty(objDynamicHandler.ChildControlId) Then
                        ctrl = ctrl.FindControl(objDynamicHandler.ChildControlId)
                    End If
                    If ctrl IsNot Nothing Then
                        If objDynamicHandler.EventInfo Is Nothing Then
                            objDynamicHandler.RegisterEvent(ctrl)
                        End If
                        If objDynamicHandler.EventInfo IsNot Nothing Then
                            Dim closureHandler As DynamicHandlerSettings = objDynamicHandler

                            Dim dynamicEventHandler As New EventHandler(Of EventArgs)(Sub(sender As Object, ea As EventArgs)
                                                                                          closureHandler.DynamicEventHandler(Me, sender, ea)
                                                                                      End Sub)

                            ReflectionHelper.AddEventHandler(Of EventArgs)(objDynamicHandler.EventInfo, ctrl, dynamicEventHandler)
                        Else
                            Throw New ApplicationException(String.Format("Control Event was not found for dynamic handler {0}", objDynamicHandler.GetStep().ToString()))
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Control was not found for dynamic handler {0}", objDynamicHandler.GetStep().ToString()))
                    End If
                End If
            Next
        End Sub

    End Class


End Namespace



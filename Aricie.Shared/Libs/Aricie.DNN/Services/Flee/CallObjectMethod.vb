﻿Imports System.Reflection
Imports Aricie.DNN.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports Aricie.DNN.UI.WebControls.EditControls
Imports DotNetNuke.UI.WebControls
Imports Aricie.Services
Imports System.Threading

Namespace Services.Flee
    ''' <summary>
    ''' Runs method as flee action
    ''' </summary>
    ''' <typeparam name="TObjectType"></typeparam>
    ''' <remarks></remarks>
    <DisplayName("Call Method")> _
    <Serializable()> _
    Public Class CallObjectMethod(Of TObjectType)
        Inherits ObjectAction(Of TObjectType)
        Implements ISelector(Of MethodInfo)

        ''' <summary>
        ''' Gets or sets the method name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExtendedCategory("Instance")> _
        <Editor(GetType(SelectorEditControl), GetType(EditControl))> _
        <ProvidersSelector()> _
        Public Property MethodName() As String = String.Empty

        <ExtendedCategory("Instance")> _
        Public MethodIndex As Integer = 1


        ''' <summary>
        ''' Runs the method
        ''' </summary>
        ''' <param name="owner"></param>
        ''' <param name="globalVars"></param>
        ''' <remarks></remarks>
        Public Overrides Sub Run(ByVal owner As Object, ByVal globalVars As IContextLookup)
            Dim args As New List(Of Object)
            For Each objParam As KeyValuePair(Of String, Object) In Me.Parameters.EvaluateVariables(owner, globalVars)
                args.Add(objParam.Value)
            Next
            Dim potentialsMembers As List(Of MemberInfo) = Nothing
            Dim targetMethod As MethodInfo
            If ReflectionHelper.GetFullMembersDictionary(GetType(TObjectType)).TryGetValue(Me._MethodName, potentialsMembers) Then
                Dim index As Integer = 0
                For Each potentialMember As MemberInfo In potentialsMembers
                    If TypeOf potentialMember Is MethodInfo Then
                        targetMethod = DirectCast(potentialMember, MethodInfo)
                        If targetMethod.GetParameters.Length = args.Count Then
                            index += 1
                            If index = MethodIndex Then
                                If targetMethod.IsStatic Then
                                    targetMethod.Invoke(Nothing, args.ToArray)
                                Else
                                    Dim target As Object = Me.Instance.Evaluate(owner, globalVars)
                                    If Me.LockTarget Then
                                        SyncLock target
                                            targetMethod.Invoke(target, args.ToArray)
                                        End SyncLock
                                    Else
                                        targetMethod.Invoke(target, args.ToArray)
                                    End If
                                End If
                                Exit Sub
                            End If
                        End If
                    End If
                Next

            End If
            Throw New Exception(String.Format("Method {0} with {2} parameters was not found in type {1}", Me._MethodName, args.Count, ReflectionHelper.GetSafeTypeName(GetType(TObjectType))))
        End Sub

        ''' <summary>
        ''' Returns a list of the methods on the generic type
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSelector(ByVal propertyName As String) As System.Collections.IList Implements ComponentModel.ISelector.GetSelector
            Return DirectCast(GetSelectorG(propertyName), IList)
        End Function

        ''' <summary>
        ''' Returns a list of the methods on the generic type
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSelectorG(ByVal propertyName As String) As System.Collections.Generic.IList(Of System.Reflection.MethodInfo) Implements ComponentModel.ISelector(Of System.Reflection.MethodInfo).GetSelectorG
            Dim toReturn As New List(Of MethodInfo)
            For Each objMember As MemberInfo In ReflectionHelper.GetMembersDictionary(Of TObjectType)().Values
                If TypeOf objMember Is MethodInfo Then
                    toReturn.Add(DirectCast(objMember, MethodInfo))
                End If
            Next
            Return toReturn
        End Function


    End Class
End NameSpace
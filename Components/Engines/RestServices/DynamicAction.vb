Imports Aricie.DNN.UI.Attributes
Imports Aricie.DNN.Entities
Imports System.Xml.Serialization
Imports System.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls

Namespace Aricie.DNN.Modules.PortalKeeper
    <ActionButton(IconName.CodeFork, IconOptions.Normal)> _
    <Serializable()> _
    Public Class DynamicAction
        Inherits RuleEngineSettings(Of SimpleEngineEvent)

        Public Property DynamicAttributes As New ActionAttributes()

        <SortOrder(0)> _
        <ExtendedCategory("WebAction")> _
        <CollectionEditor(False, True, False, False, 0, CollectionDisplayStyle.List, False, 5, "", True)> _
        Public Property HttpVerbs() As New OneOrMore(Of WebMethod)(WebMethod.Get)

        <ExtendedCategory("WebAction")> _
        Public Property EnforceActionName As Boolean

        <ExtendedCategory("WebAction")> _
        Public Property Parameters As New List(Of DynamicParameter)


        Private _ParametersDictionary As Dictionary(Of String, DynamicParameter)

        <XmlIgnore()> _
        <Browsable(False)> _
        Public ReadOnly Property ParametersDictionary As Dictionary(Of String, DynamicParameter)
            Get
                If _ParametersDictionary Is Nothing Then
                    SyncLock Me
                        If _ParametersDictionary Is Nothing Then
                            Dim tempDico As New Dictionary(Of String, DynamicParameter)(StringComparer.OrdinalIgnoreCase)
                            For Each objParam As DynamicParameter In Parameters
                                tempDico(objParam.Name) = objParam
                            Next
                            _ParametersDictionary = tempDico
                        End If
                    End SyncLock
                End If
                Return _ParametersDictionary
            End Get
        End Property

        <ExtendedCategory("WebAction")> _
        Public Property DefaultResponse As New EnabledFeature(Of CreateHttpResponseInfo)

        <ExtendedCategory("Rules")> _
        Public Overrides Property Mode As RuleEngineMode = RuleEngineMode.Actions

    End Class
End Namespace
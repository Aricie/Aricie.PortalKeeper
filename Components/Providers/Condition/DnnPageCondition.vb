﻿Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports Aricie.DNN.UI.WebControls
Imports DotNetNuke.Entities.Tabs
Imports DotNetNuke.Framework
Imports Aricie.DNN.UI.WebControls.EditControls

Namespace Aricie.DNN.Modules.PortalKeeper

    <ActionButton(IconName.Sitemap, IconOptions.Normal)>
    <DisplayName("DotNetNuke Page Condition")>
    <Description("Matches according to the target DNN Page as defined in the url")>
    Public Class DnnPageCondition
        Inherits DnnPageCondition(Of RequestEvent)

    End Class


    <ActionButton(IconName.Sitemap, IconOptions.Normal)>
    <DisplayName("DotNetNuke Page Condition")>
    <Description("Matches according to the target DNN Page as defined in the url")>
    Public Class DnnPageCondition(Of TEngineEvents As IConvertible)
        Inherits SelectionSetCondition(Of TEngineEvents)

        <ExtendedCategory("Condition")>
        Public Property MatchAnyDNNPageRequest As Boolean

        <InnerEditor(GetType(SelectorEditControl), GetType(ItemsAttributes))>
        <CollectionEditor(DisplayStyle:=CollectionDisplayStyle.List, EnableExport:=True, Paged:=True)>
        <ConditionalVisible("MatchAnyDNNPageRequest", True, True)>
        <ExtendedCategory("Condition")>
        Public Overrides Property Items As List(Of Integer)
            Get
                Return MyBase.Items
            End Get
            Set(value As List(Of Integer))
                MyBase.Items = value
            End Set
        End Property

        Public Overrides Function Match(context As PortalKeeperContext(Of TEngineEvents)) As Boolean
            If Me.MatchAnyDNNPageRequest Then
                Return TypeOf context.DnnContext.HttpContext.CurrentHandler Is DotNetNuke.Framework.PageBase
            End If
            Return MyBase.Match(context)
        End Function

        Public Overrides Function GetCurrentValue(ByVal objContext As PortalKeeperContext(Of TEngineEvents)) As Integer
            If TypeOf objContext.DnnContext.HttpContext.CurrentHandler Is PageBase Then
                Dim currentPage As TabInfo = objContext.DnnContext.Portal.ActiveTab
                If currentPage IsNot Nothing Then
                    Return currentPage.TabID
                End If
            End If

            Return -1
        End Function

        Public Overrides Function GetSelectorAttribute() As Attribute

            Return New SelectorAttribute(GetType(TabSelector), "TabPath", "TabID", True, False, "", "", False, False)
        End Function
    End Class
End Namespace
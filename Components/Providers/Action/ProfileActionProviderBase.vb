Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.Services.Flee
Imports Aricie.DNN.UI.WebControls
Imports DotNetNuke.Entities.Users
Imports Aricie.DNN.UI.WebControls.EditControls
Imports DotNetNuke.Entities.Profile

Namespace Aricie.DNN.Modules.PortalKeeper
    <ActionButton(IconName.User, IconOptions.Normal)> _
    <Serializable()> _
    Public MustInherit Class ProfileActionProviderBase(Of TEngineEvents As IConvertible)
        Inherits OutputAction(Of TEngineEvents)

        Public Property UserMode As ProfileUserMode

        <ConditionalVisible("UserMode", True, True, ProfileUserMode.CurrentUser, ProfileUserMode.ByUserinfo)> _
        <Selector(GetType(PortalSelector), "PortalName", "PortalID", False, False, "", "", False, False)> _
      <Editor(GetType(SelectorEditControl), GetType(EditControl))> _
        Public Property PortalId As Integer

        <ConditionalVisible("UserMode", False, True, ProfileUserMode.ByUserId)> _
        Public Property UserIdExpression As New SimpleOrExpression(Of Integer)

        <ConditionalVisible("UserMode", False, True, ProfileUserMode.ByUsername)> _
        Public Property UsernameExpression As New SimpleOrExpression(Of String)

        <ConditionalVisible("UserMode", False, True, ProfileUserMode.ByUserinfo)> _
        Public Property UserInfoExpression As New SimpleExpression(Of UserInfo)

        Public Function GetUser(objContext As PortalKeeperContext(Of TEngineEvents)) As UserInfo
            Dim toReturn As UserInfo = Nothing
            Select Case UserMode
                Case ProfileUserMode.CurrentUser
                    toReturn = objContext.DnnContext.User
                Case ProfileUserMode.ByUserId
                    toReturn = UserController.GetUser(Me.PortalId, UserIdExpression.GetValue(objContext, objContext), True)
                Case ProfileUserMode.ByUsername
                    toReturn = UserController.GetUserByName(Me.PortalId, UsernameExpression.GetValue(objContext, objContext), True)
                Case ProfileUserMode.ByUserinfo
                    toReturn = UserInfoExpression.Evaluate(objContext, objContext)
            End Select
            Return toReturn
        End Function

        Public Property ProfileType As ProfileType

        <ConditionalVisible("ProfileType", False, True, ProfileType.Identity)> _
        Public Property AsString As Boolean


        <Required(True)> _
        <ConditionalVisible("ProfileType", False, True, ProfileType.Personalization)> _
        Public Property NamingContainer As String = ""

        <Required(True)> _
        Public Property PropertyName As String = ""


    End Class
End Namespace
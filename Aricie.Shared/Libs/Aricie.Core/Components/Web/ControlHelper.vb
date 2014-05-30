Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Linq

Namespace Web.UI

    Public Module ControlHelper

        Public Function FindControlsSemiRecursive(Of T As Class)(ByVal container As Control) As List(Of T)
            Return FindControlsRecursive(Of T)(container, 1000, container.GetType)
        End Function

        Public Function FindControlsRecursive(Of T As Class)(ByVal container As Control) As List(Of T)
            Return FindControlsRecursive(Of T)(container, 1000)
        End Function

        Public Function FindControlsRecursive(Of T As Class)(ByVal container As Control, ByVal maxDepth As Integer) As List(Of T)

            Return FindControlsRecursive(Of T)(container, maxDepth, Nothing)
        End Function

        Public Function FindControlsRecursive(Of T As Class)(ByVal container As Control, ByVal maxDepth As Integer, ByVal stopType As Type) As List(Of T)
            Dim toReturn As New List(Of T)
            For Each cont As Control In container.Controls
                If TypeOf cont Is T Then
                    toReturn.Add(DirectCast(DirectCast(cont, Object), T))
                End If
                If cont.Controls.Count > 0 AndAlso maxDepth > 0 Then
                    If stopType Is Nothing OrElse cont.GetType IsNot stopType Then
                        toReturn.AddRange(FindControlsRecursive(Of T)(cont, maxDepth - 1, stopType))
                    End If
                End If
            Next
            Return toReturn
        End Function

        Public Function FindControlRecursive(Of T As {Control})(ByVal objControl As Control) As T
            Return DirectCast(FindControlRecursive(objControl, GetType(T)), T)
        End Function

        Public Function FindControlRecursive(ByVal objControl As Control, ByVal parentType As Type) As Control
            If objControl.Parent Is Nothing Then
                Return Nothing
            Else
                If objControl.Parent.GetType Is parentType _
                   Or objControl.Parent.GetType.IsSubclassOf(parentType) Then
                    Return objControl.Parent
                Else
                    Return FindControlRecursive(objControl.Parent, parentType)
                End If
            End If
        End Function

        <ObsoleteAttribute("Obsolete, use FindParentControlRecursive instead")>
        Public Function FindControlRecursive(ByVal objControl As Control, ByVal ParamArray parentTypes() As Type) As Control
            Return FindParentControlRecursive(objControl, parentTypes)
        End Function

        ''' <summary>
        ''' Find the first of the parent controls matching with the types specified in parameters
        ''' </summary>
        ''' <param name="startControl">control to start the search for parent controls</param>
        ''' <param name="parentTypes">types of the parent controls to find</param>
        ''' <returns>First Matching Parent, nothing if not found</returns>
        ''' <remarks></remarks>
        Public Function FindParentControlRecursive(ByVal startControl As Control, ByVal ParamArray parentTypes() As Type) As Control
            If startControl.Parent Is Nothing Then
                Return Nothing
            Else
                If parentTypes.Any(Function(candidateParentType) candidateParentType.IsInstanceOfType(startControl.Parent)) Then
                    Return startControl.Parent
                End If
            End If
            Return FindParentControlRecursive(startControl.Parent, parentTypes)
        End Function
        ''' <summary>
        ''' Recherche un controle par son id dans toute l'arborescence enfant de startingControl
        ''' </summary>
        ''' <param name="startingControl">Controle parent de l'arborescence</param>
        ''' <param name="id">Id du controle � chercher</param>
        ''' <returns>controle trouv�</returns>
        ''' <remarks></remarks>
        Public Function FindControlRecursive(ByVal startingControl As Control, ByVal id As String) As Control
            Dim found As Control = Nothing

            For Each activeControl As Control In startingControl.Controls
                found = activeControl

                If String.Compare(id, found.ID, True) <> 0 Then
                    found = Nothing
                End If

                If found Is Nothing Then
                    found = FindControlRecursive(activeControl, id)
                End If

                If found IsNot Nothing Then
                    Exit For
                End If
            Next

            Return found
        End Function

        Public Function FindControlRecursive(ByVal objControl As Control, ByVal parentType As Type, ByVal controlId As String) As Control
            If objControl.Parent Is Nothing Then
                Return Nothing
            Else
                If (objControl.Parent.GetType Is parentType Or objControl.Parent.GetType.IsSubclassOf(parentType)) And objControl.Parent.ID = controlID Then
                    Return objControl.Parent
                Else
                    Return FindControlRecursive(objControl.Parent, parentType)
                End If
            End If
        End Function

        Public Function FindNearbyControl(Of T As Control)(ByVal control As Control, ByVal controlId As String) As T
            Dim toReturn As Control = FindControl(control, controlID)
            If toReturn IsNot Nothing AndAlso TypeOf toReturn Is T Then
                Return DirectCast(toReturn, T)
            End If
            Return Nothing
        End Function

        Public Function FindControl(ByVal control As Control, ByVal controlId As String) As Control
            Dim namingContainer As Control = control
            Dim toReturn As Control = Nothing
            If (control IsNot control.Page) Then
                Do While ((toReturn Is Nothing) AndAlso (namingContainer IsNot control.Page))
                    namingContainer = namingContainer.NamingContainer
                    If (namingContainer Is Nothing) Then
                        Throw New ArgumentException("No Naming Container: check controlId ", controlId)
                    End If
                    toReturn = namingContainer.FindControl(controlId)
                Loop
                Return toReturn
            End If
            Return control.FindControl(controlID)
        End Function

    End Module

End Namespace
Imports Aricie.DNN.UI.Attributes
Imports System.ComponentModel
Imports Aricie.DNN.UI.Controls
Imports DotNetNuke.UI.WebControls
Imports Aricie.Web.UI
Imports DotNetNuke.Entities.Modules
Imports System.Web.UI


Namespace UI.WebControls.EditControls
    ''' <summary>
    ''' Base Control to edit a property
    ''' </summary>
    Public MustInherit Class AricieEditControl
        Inherits EditControl

        Private _OnDemandEnabled As Boolean = True

        Public Event ItemChanged As DotNetNuke.UI.WebControls.PropertyChangedEventHandler

        Private _ParentModule As PortalModuleBase

        Private _ParentEditor As PropertyEditorControl
        Private _ParentField As FieldEditorControl

        Private _InnerEditorAttributes As New Dictionary(Of String, InnerEditorAttribute)

        Private _InnerAttributes As Dictionary(Of String, IList(Of Attribute))

        Public ReadOnly Property OndemandEnabled() As Boolean
            Get

                Return Me._OnDemandEnabled

            End Get
        End Property

        Public ReadOnly Property ParentModule() As PortalModuleBase
            Get
                If _ParentModule Is Nothing Then
                    _ParentModule = Web.UI.ControlHelper.FindControlRecursive(Of PortalModuleBase)(Me)
                End If
                Return _ParentModule
            End Get
        End Property

        Public ReadOnly Property ParentAricieModule() As AriciePortalModuleBase
            Get
                If TypeOf Me.ParentModule Is AriciePortalModuleBase Then
                    Return DirectCast(ParentModule, AriciePortalModuleBase)
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property InnerAttributes() As Dictionary(Of String, IList(Of Attribute))
            Get
                If Me._InnerAttributes Is Nothing Then
                    _InnerAttributes = New Dictionary(Of String, IList(Of Attribute))
                    For Each editorAttributes As KeyValuePair(Of String, InnerEditorAttribute) In _InnerEditorAttributes
                        Me._InnerAttributes(editorAttributes.Key) = editorAttributes.Value.GetAttributes(Me.ParentField.DataSource.GetType)
                    Next
                End If
                Return _InnerAttributes
            End Get
        End Property

        Public ReadOnly Property ParentEditor() As PropertyEditorControl
            Get
                If Me._ParentEditor Is Nothing Then
                    Dim parentControl As PropertyEditorControl = Web.UI.ControlHelper.FindControlRecursive(Of PropertyEditorControl)(Me)
                    If Not parentControl Is Nothing Then
                        Me._ParentEditor = parentControl
                    End If
                End If
                Return Me._ParentEditor
            End Get
        End Property

        Public ReadOnly Property ParentAricieEditor() As AriciePropertyEditorControl
            Get
                If TypeOf Me.ParentEditor Is AriciePropertyEditorControl Then
                    Return DirectCast(ParentEditor, AriciePropertyEditorControl)
                End If
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ParentField() As FieldEditorControl
            Get
                If Me._ParentField Is Nothing Then
                    Dim parentControl As FieldEditorControl = Web.UI.ControlHelper.FindControlRecursive(Of FieldEditorControl)(Me)
                    If Not parentControl Is Nothing Then
                        Me._ParentField = parentControl
                    End If
                End If
                Return Me._ParentField
            End Get
        End Property

        Public ReadOnly Property ParentAricieField() As AricieFieldEditorControl
            Get
                If TypeOf Me.ParentField Is AricieFieldEditorControl Then
                    Return DirectCast(ParentField, AricieFieldEditorControl)
                End If
                Return Nothing
            End Get
        End Property


        ' Properties
        Protected Overrides Property StringValue() As String
            Get
                If (Me.Value Is Nothing) Then
                    Return String.Empty
                End If

                Return Me.Value.ToString
            End Get
            Set(ByVal value As String)
                Me.Value = value
            End Set
        End Property


        Protected Overrides Sub OnAttributesChanged()
            If (Not CustomAttributes Is Nothing) Then
                For Each objAttribute As Attribute In CustomAttributes
                    If TypeOf objAttribute Is InnerEditorAttribute Then
                        Dim key As String = ""
                        If TypeOf objAttribute Is InnerPropertyEditorAttribute Then
                            key = DirectCast(objAttribute, InnerPropertyEditorAttribute).PropertyName
                        End If
                        Me._InnerEditorAttributes(key) = DirectCast(objAttribute, InnerEditorAttribute)
                    End If
                Next
            End If
        End Sub

        Protected Function BuildEditor(ByVal editInfo As EditorInfo, ByVal container As Control) As EditControl

            Dim objEditControl As EditControl = AricieFieldEditorControl.CreateEditControl(editInfo, Me.ParentAricieField, container)
            objEditControl.LocalResourceFile = Me.LocalResourceFile

            Return objEditControl

        End Function

        Public Function BuildEditInfo(ByVal objValue As Object, editMode As PropertyEditorMode) As EditorInfo
            Return Me.BuildEditInfo(objValue, "", editMode)
        End Function

        Public Function BuildEditInfo(ByVal objValue As Object, ByVal fieldName As String, editMode As PropertyEditorMode) As EditorInfo
            Dim adapter As IEditorInfoAdapter

            Dim cutomAttributes(-1) As Attribute
            Dim tempAttributes As IList(Of Attribute) = Nothing
            If Me.InnerAttributes.TryGetValue(fieldName, tempAttributes) Then
                Array.Resize(Of Attribute)(cutomAttributes, tempAttributes.Count)
                tempAttributes.CopyTo(cutomAttributes, 0)
            End If

            Dim toReturn As EditorInfo
            If fieldName = "" Then
                adapter = New InnerEditorInfoAdapter(Me.Name, objValue, editMode, cutomAttributes)
                toReturn = adapter.CreateEditControl
            Else

                adapter = New AricieStandardEditorInfoAdapter(objValue, fieldName, cutomAttributes)
                toReturn = adapter.CreateEditControl
                toReturn.EditMode = editMode
                toReturn.Type = Aricie.Services.ReflectionHelper.GetPropertiesDictionary(objValue.GetType)(fieldName).GetValue(objValue, Nothing).GetType().AssemblyQualifiedName
                toReturn.Attributes = cutomAttributes

                'For Each custattr As Attribute In cutomAttributes
                '    If TypeOf (custattr) Is EditorAttribute Then
                '        Dim editAttr As EditorAttribute = CType(custattr, EditorAttribute)
                '        toReturn.Editor = editAttr.EditorTypeName
                '        Exit For
                '    End If
                'Next

            End If

            Return toReturn

        End Function

        Protected Sub OnItemChanged(ByVal sender As Object, ByVal e As DotNetNuke.UI.WebControls.PropertyEditorEventArgs)
            RaiseEvent ItemChanged(sender, e)
        End Sub


    End Class
End Namespace

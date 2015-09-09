Imports Aricie.DNN.ComponentModel
Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports Aricie.Services
Imports Aricie.ComponentModel
Imports Microsoft.VisualBasic.CompilerServices
Imports DotNetNuke.UI.Skins.Controls
Imports DotNetNuke.UI.WebControls
Imports System.Xml.Serialization
Imports Aricie.DNN.UI.WebControls.EditControls
Imports System.Reflection
Imports Aricie.Collections
Imports Aricie.DNN.UI.WebControls
Imports DotNetNuke.Services.Localization

Namespace Services.Flee

    <Serializable()> _
    Public Class GeneralVariableInfo(Of T)
        Inherits GeneralVariableInfo

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(strName As String)
            MyBase.New(strName)
        End Sub

        Private _ConstDotNetType As New DotNetType(GetType(T))

        <Browsable(False)> _
        Public Overrides Property DotNetType As DotNetType
            Get
                Return _ConstDotNetType
            End Get
            Set(value As DotNetType)
                'do nothing
            End Set
        End Property

    End Class

    <Serializable()> _
    Public Class GeneralVariableInfo
        Inherits VariableInfo
        Implements ISelector(Of MethodInfo)
        Implements IExpressionVarsProvider


        Public Overrides Function GetFriendlyDetails() As String
            Dim toReturn As String = MyBase.GetFriendlyDetails()
            Dim nextSegment As String = ""
            Select Case Me.VariableMode
                Case Flee.VariableMode.Constructor
                    nextSegment = "cTor " & Me.MethodName
                Case Flee.VariableMode.Delegate
                    nextSegment = "Delegate: " & Me.TargetInstance.Expression
                Case Flee.VariableMode.Expression
                    nextSegment = "Expression: """ & Me.SimpleExpression.Expression & """"
                Case Flee.VariableMode.Instance
                    nextSegment = "Instance: " & ReflectionHelper.GetFriendlyName(Me.Instance)
            End Select
            toReturn = String.Format("{0} {2} {1}", toReturn, UIConstants.TITLE_SEPERATOR, nextSegment)
            Return toReturn
        End Function


        Private _Instance As Object
        Private _SimpleExpression As SimpleExpression(Of Object)
        Private _FleeExpression As New FleeExpressionInfo(Of Object)



        Public Sub New()

        End Sub

        Public Sub New(strName As String)
            Me.Name = strName
        End Sub


        Public Overridable Property DotNetType As New DotNetType()

        <Browsable(False)> _
        Public ReadOnly Property HasType As Boolean
            Get
                Return DotNetType.GetDotNetType() IsNot Nothing
            End Get
        End Property

        <Browsable(False)> _
        Public Overrides ReadOnly Property VariableType As String
            Get
                Return DotNetType.TypeName
            End Get
        End Property

        <ConditionalVisible("HasType", False, True)> _
        Public Property VariableMode As VariableMode = VariableMode.Expression

        <ExtendedCategory("", "Evaluation")> _
        <ConditionalVisible("VariableMode", True, True, VariableMode.Instance)> _
        <ConditionalVisible("HasType", False, True)> _
        Public Property InstanceMode As InstanceMode = InstanceMode.Off

        <ExtendedCategory("", "Evaluation")> _
        <ConditionalVisible("VariableMode", True, True, VariableMode.Instance)> _
        <ConditionalVisible("HasType", False, True)> _
        Public Overrides Property EvaluationMode() As VarEvaluationMode
            Get
                Return MyBase.EvaluationMode
            End Get
            Set(ByVal value As VarEvaluationMode)
                MyBase.EvaluationMode = value
            End Set
        End Property

        <ExtendedCategory("", "Evaluation")> _
        <ConditionalVisible("VariableMode", True, True, VariableMode.Instance)> _
        <ConditionalVisible("HasType", False, True)> _
        Public Overrides Property Scope() As VariableScope
            Get
                Return MyBase.Scope
            End Get
            Set(ByVal value As VariableScope)
                MyBase.Scope = value
            End Set
        End Property


        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Instance, VariableMode.Expression)> _
        Public Property UseClone() As Boolean

        ''' <summary>
        ''' Get or sets the instance of the object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Instance)> _
        <XmlIgnore()> _
        Public Property Instance As Object
            Get
                If Me.DotNetType.GetDotNetType() IsNot Nothing AndAlso (Me._Instance Is Nothing _
                                                                            OrElse (Me._Instance.GetType() IsNot Me.DotNetType.GetDotNetType() _
                                                                                    AndAlso Not ReflectionHelper.CanConvert(Me._Instance.GetType(), Me.DotNetType.GetDotNetType()))) _
                                                                        AndAlso (Me.VariableMode = Flee.VariableMode.Instance OrElse Me._InstanceMode = Flee.InstanceMode.ContextLess) Then
                    Me._Instance = Me.EvaluateOnce(DnnContext.Current, DnnContext.Current)
                End If
                Return Me._Instance
            End Get
            Set(value As Object)
                If value IsNot Nothing AndAlso value.GetType() IsNot Me.DotNetType.GetDotNetType() Then
                    value = Conversions.ChangeType(value, Me.DotNetType.GetDotNetType())
                End If
                _Instance = value
            End Set
        End Property

        <Browsable(False)> _
        Public Property SerializableInstance() As Serializable(Of Object)
            Get
                If Me.VariableMode = Flee.VariableMode.Instance OrElse Me.InstanceMode <> Flee.InstanceMode.Off Then
                    Return New Serializable(Of Object)(Instance)
                End If
                Return Nothing
            End Get
            Set(ByVal value As Serializable(Of Object))
                Me.Instance = value.Value
            End Set
        End Property




        ''' <summary>
        ''' Is the variable and advanced expression
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Expression)> _
        Public Property AdvancedExpression() As Boolean

        ''' <summary>
        ''' Gets the flee expression that will be used
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Expression)> _
        <ConditionalVisible("AdvancedExpression", False, True)> _
        Public Property FleeExpression() As FleeExpressionInfo(Of Object)
            Get
                Return _FleeExpression
            End Get
            Set(value As FleeExpressionInfo(Of Object))
                _FleeExpression = value
                _SimpleExpression = Nothing
            End Set
        End Property

        ''' <summary>
        ''' Retrieve the simple expression that will be evaluated
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlIgnore()> _
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Expression)> _
        <ConditionalVisible("AdvancedExpression", True, True)> _
        Public Property SimpleExpression() As SimpleExpression(Of Object)
            Get
                If _SimpleExpression Is Nothing Then
                    _SimpleExpression = New SimpleExpression(Of Object)(Me.FleeExpression)
                End If
                Return _SimpleExpression
            End Get
            Set(ByVal value As SimpleExpression(Of Object))
                _SimpleExpression = value
                _SimpleExpression.SlaveExpression = FleeExpression
                Me.FleeExpression.Expression = _SimpleExpression.Expression
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets whether the expression should be compiled or evaluated
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Expression)> _
        Public Property AsCompiledExpression() As Boolean


        <AutoPostBack()> _
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Delegate)> _
        <Editor(GetType(SelectorEditControl), GetType(EditControl))> _
        <Selector("Name", "Name", False, True, "<Select a Method Name>", "", False, True)> _
        Public Property MethodName As String = ""




        <Editor(GetType(SelectorEditControl), GetType(EditControl))> _
        <ProvidersSelector("Key", "Value")> _
        <ConditionalVisible("HasType", False, True)> _
        <ConditionalVisible("VariableMode", False, True, VariableMode.Constructor, VariableMode.Delegate)> _
        Public Property MethodIndex As Integer = 1

        <Browsable(False)> _
        Public ReadOnly Property RequiresInstance As Boolean
            Get
                Dim targetMethod As MethodInfo = Me.GetMethodInfo()
                If targetMethod IsNot Nothing Then
                    Return targetMethod.IsStatic
                End If
                Return False
            End Get
        End Property

        <ConditionalVisible("VariableMode", False, True, VariableMode.Delegate)> _
        <ConditionalVisible("RequiresInstance", False, True)>
        Public Property TargetInstance As New SimpleExpression(Of Object)

        <ConditionalVisible("HasType", False, True)> _
<ConditionalVisible("VariableMode", False, True, VariableMode.Constructor)> _
        Public Property Parameters() As New Variables

        <ConditionalVisible("HasType", False, True)> _
       <ConditionalVisible("VariableMode", False, True, VariableMode.Instance)> _
      <ActionButton(IconName.Refresh, IconOptions.Normal)> _
        Public Sub ResetInstance(ape As AriciePropertyEditorControl)
            Me._Instance = Nothing
            ape.ItemChanged = True
            Dim message As String = Localization.GetString("InstanceReset.Message", ape.LocalResourceFile)
            ape.DisplayMessage(message, ModuleMessage.ModuleMessageType.GreenSuccess)
        End Sub


        <ConditionalVisible("VariableMode", False, True, VariableMode.Constructor)> _
       <ActionButton(IconName.Key, IconOptions.Normal)> _
        Public Sub SetParameters(ape As AriciePropertyEditorControl)
            If Me.VariableMode = VariableMode.Constructor Then
                Dim objParameters As ParameterInfo()

                objParameters = SelectedMember.GetParameters()
                Me.Parameters = Variables.GetFromParameters(objParameters)
                ape.ItemChanged = True
                Dim message As String = Localization.GetString("ParametersCreated.Message", ape.LocalResourceFile)
                ape.DisplayMessage(message, ModuleMessage.ModuleMessageType.GreenSuccess)
            End If
        End Sub


        ''' <summary>
        ''' Evaluates the variable 
        ''' </summary>
        ''' <param name="owner"></param>
        ''' <param name="globalVars"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Evaluate(ByVal owner As Object, ByVal globalVars As IContextLookup) As Object
            Dim toReturn As Object

            If Me._Instance IsNot Nothing Then
                If Me.VariableMode = Flee.VariableMode.Instance OrElse Me._InstanceMode <> Flee.InstanceMode.Off Then
                    If Scope = VariableScope.Global AndAlso globalVars IsNot Nothing Then
                        globalVars.Items(Me.Name) = Me._Instance
                    End If
                    Return Me._Instance
                Else
                    Me._Instance = Nothing
                End If
            End If
            toReturn = MyBase.Evaluate(owner, globalVars)
            If Me._InstanceMode = InstanceMode.InContextEval Then
                Me._Instance = toReturn
            End If
            If _UseClone Then
                Return ReflectionHelper.CloneObject(toReturn)
            End If
            Return toReturn
        End Function

        ''' <summary>
        ''' Evaluate variable once
        ''' </summary>
        ''' <param name="owner"></param>
        ''' <param name="globalVars"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function EvaluateOnce(ByVal owner As Object, ByVal globalVars As IContextLookup) As Object
            Dim toReturn As Object = Nothing
            Select Case Me.VariableMode
                Case Flee.VariableMode.Instance
                    If ReflectionHelper.CanCreateObject(Me.DotNetType.GetDotNetType()) Then
                        toReturn = ReflectionHelper.CreateObject(Me.DotNetType.GetDotNetType())
                    End If
                Case Flee.VariableMode.Constructor
                    Dim args As Object()
                    args = (From objParam In Me.Parameters.EvaluateVariables(owner, globalVars) Select objParam.Value).ToArray()
                    Try
                        toReturn = System.Activator.CreateInstance(Me.DotNetType.GetDotNetType(), args)
                    Catch ex As Exception
                        Dim newEx As ApplicationException
                        If args IsNot Nothing Then
                            Dim argsList As New SerializableList(Of Object)(args)
                            newEx = New ApplicationException(String.Format("Error calling constructor for type {0} with args {1}", Me.DotNetType.GetDotNetType, ReflectionHelper.Serialize(argsList).Beautify()), ex)
                        Else
                            newEx = New ApplicationException(String.Format("Error calling constructor for type {0} ", Me.DotNetType.GetDotNetType), ex)
                        End If

                        Throw newEx
                    End Try
                Case Flee.VariableMode.Expression
                    If Me._AsCompiledExpression Then
                        toReturn = Me.FleeExpression.GetCompiledExpression(owner, globalVars)
                    Else
                        toReturn = Me.FleeExpression.Evaluate(owner, globalVars)
                    End If
                Case Flee.VariableMode.Delegate
                    Dim potentialsMembers As List(Of MemberInfo) = Nothing
                    Dim targetMethod As MethodInfo = Me.GetMethodInfo()
                    If targetMethod IsNot Nothing Then
                        If RequiresInstance Then
                            Dim target As Object = Me.TargetInstance.Evaluate(owner, globalVars)
                            toReturn = [Delegate].CreateDelegate(Me.DotNetType.GetDotNetType(), target, targetMethod)
                        Else
                            toReturn = [Delegate].CreateDelegate(Me.DotNetType.GetDotNetType(), targetMethod)
                        End If
                    Else
                        Throw New Exception(String.Format("Can't create Delegate for method {0} in type {1}", targetMethod.Name, Me.DotNetType.GetDotNetType().FullName))
                    End If
            End Select
            Return toReturn
        End Function


        Public Function GetMethodInfo() As MethodInfo
            Dim toReturn As MethodInfo = Nothing
            If Me.DotNetType.GetDotNetType() IsNot Nothing Then
                Dim potentialsMembers As List(Of MemberInfo) = Nothing
                If ReflectionHelper.GetFullMembersDictionary(Me.DotNetType.GetDotNetType).TryGetValue(Me._MethodName, potentialsMembers) Then
                    Dim index As Integer = 0
                    For Each potentialMember As MemberInfo In potentialsMembers
                        If TypeOf potentialMember Is MethodInfo Then
                            index += 1
                            If index = MethodIndex Then
                                toReturn = DirectCast(potentialMember, MethodInfo)
                            End If
                        End If
                    Next
                End If
            End If
            Return toReturn
        End Function


        Public Function GetSelector(propertyName As String) As IList Implements ISelector.GetSelector
            Select Case propertyName
                Case "MethodIndex"
                    Dim toReturn As New Dictionary(Of String, Integer)
                    Dim index As Integer = 1
                    For Each objMember As MemberInfo In Me.SelectedMembers
                        toReturn.Add(ReflectionHelper.GetMemberSignature(objMember), index)
                        index += 1
                    Next
                    If toReturn.Count = 0 Then
                        toReturn.Add("", 1)
                    End If
                    Return toReturn.ToList()
                Case "MethodName"
                    Return DirectCast(GetSelectorG(propertyName), IList)
            End Select
            Return New ArrayList
        End Function


        <XmlIgnore()> _
       <Browsable(False)> _
        Public ReadOnly Property SelectedMembers As List(Of MethodBase)
            Get

                Dim toReturn As New List(Of MethodBase)
                If Me.DotNetType.GetDotNetType() IsNot Nothing Then
                    Dim members As List(Of MemberInfo) = Nothing
                    If Me.VariableMode = Flee.VariableMode.Constructor Then
                        toReturn.AddRange(Me.DotNetType.GetDotNetType.GetConstructors())
                    ElseIf ReflectionHelper.GetFullMembersDictionary(Me.DotNetType.GetDotNetType(), True, False).TryGetValue(Me.MethodName, members) Then
                        toReturn.AddRange(members.OfType(Of MethodBase)())
                    End If
                End If
                Return toReturn
            End Get
        End Property

        <XmlIgnore()> _
        <Browsable(False)> _
        Public ReadOnly Property SelectedMember As MethodBase
            Get
                Dim tmpMembers As List(Of MethodBase) = SelectedMembers
                If tmpMembers.Count >= MethodIndex Then
                    Return SelectedMembers(MethodIndex - 1)
                End If
                Return Nothing
            End Get
        End Property

        Public Function GetSelectorG(propertyName As String) As IList(Of MethodInfo) Implements ISelector(Of MethodInfo).GetSelectorG
            Return ReflectionHelper.GetMembersDictionary(Me.DotNetType.GetDotNetType()).Values.OfType(Of MethodInfo)().ToList()
        End Function

        Public Sub AddVariables(currentProvider As IExpressionVarsProvider, ByRef existingVars As IDictionary(Of String, Type)) Implements IExpressionVarsProvider.AddVariables
            If VariableMode = VariableMode.Expression AndAlso AdvancedExpression Then
                FleeExpression.AddVariables(currentProvider, existingVars)
            End If
        End Sub
    End Class
End Namespace
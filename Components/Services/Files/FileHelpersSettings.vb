Imports Aricie.DNN.UI.Attributes
Imports Aricie.DNN.ComponentModel
Imports Aricie.DNN.UI.WebControls.EditControls
Imports System.ComponentModel
Imports Aricie.Collections
Imports DotNetNuke.UI.WebControls
Imports System.IO
Imports System.Linq
Imports FileHelpers
Imports FileHelpers.Dynamic
Imports System.Reflection
Imports Aricie.Services

Namespace Aricie.DNN.Modules.PortalKeeper
    
    Public  Enum CSVDelimiterMode
        SimpleChar
        Native
        EscapedString
    End Enum


    Public Class FileHelpersSettings

        Private mRecordType As Type

        Public Property FileHelpersMode() As FileHelpersMode = FileHelpersMode.Delimiter

        Public Property RecordsMode() As RecordsMode = RecordsMode.StaticType

        <ConditionalVisible("FileHelpersMode", False, True, FileHelpersMode.Delimiter)> _
        Property DelimiterMode() As CSVDelimiterMode = CSVDelimiterMode.SimpleChar

        <ConditionalVisible("DelimiterMode", False, True, CSVDelimiterMode.SimpleChar)> _
        <ConditionalVisible("FileHelpersMode", False, True, FileHelpersMode.Delimiter)> _
        Public Property Delimiter As Char = ","c

        <ConditionalVisible("DelimiterMode", False, True, CSVDelimiterMode.EscapedString)> _
        <ConditionalVisible("FileHelpersMode", False, True, FileHelpersMode.Delimiter)> _
        Public Property EscapedDelimiter As string = "\t"

        <ConditionalVisible("FileHelpersMode", False, True, FileHelpersMode.Delimiter)> _
        Public Property IncludeHeaders As Boolean = True

        <ConditionalVisible("RecordsMode", False, True, RecordsMode.StaticType)> _
        Public Property RecordType() As New DotNetType()

        <ConditionalVisible("RecordsMode", False, True, RecordsMode.DynamicExpressions)> _
        Public Property InputEntityVarName As String = "CurrentInput"

        <ConditionalVisible("RecordsMode", False, True, RecordsMode.DynamicExpressions)> _
        <LineCount(10)> _
        <Width(500)> _
        <Editor(GetType(CustomTextEditControl), GetType(EditControl))> _
        Public Property FieldVars() As String = String.Empty




        Public Sub Serialize(inputObjects As IEnumerable, tw As TextWriter)
            Dim target As New List(Of Object)()
            Dim objRecordType As Type
            If Me.RecordsMode = RecordsMode.StaticType Then
                objRecordType = Me.RecordType.GetDotNetType
                target.AddRange(inputObjects.Cast (Of Object)())
            Else
                Dim dynamicExps As List(Of SerializableDictionary(Of String, Object)) = Me.ComputeDynamicExpressions(inputObjects)
                Dim classId As String = ""
                Select Case Me.FileHelpersMode
                    Case FileHelpersMode.Delimiter
                        Dim cb As New DelimitedClassBuilder("DelimClass", Me.Delimiter)
                        If dynamicExps.Count > 0 Then
                            Dim sampleData As SerializableDictionary(Of String, Object) = dynamicExps(0)
                            For Each objPair As KeyValuePair(Of String, Object) In sampleData
                                cb.AddField(objPair.Key, objPair.Value.GetType)
                                classId &= objPair.Key
                            Next
                            cb.ClassName &= Math.Abs(classId.GetHashCode).ToString()
                        End If
                        objRecordType = cb.CreateRecordClass
                    Case Else
                        Dim cb As New FixedLengthClassBuilder("FixedClass", FixedMode.AllowVariableLength)
                        If dynamicExps.Count > 0 Then
                            Dim sampleData As SerializableDictionary(Of String, Object) = dynamicExps(0)
                            For Each objPair As KeyValuePair(Of String, Object) In sampleData
                                cb.AddField(objPair.Key, 10, objPair.Value.GetType)
                                classId &= objPair.Key
                            Next
                            cb.ClassName &= Math.Abs(classId.GetHashCode).ToString()
                        End If
                        objRecordType = cb.CreateRecordClass
                End Select
                For Each item As SerializableDictionary(Of String, Object) In dynamicExps
                    Dim newRecord As Object = ReflectionHelper.CreateObject(objRecordType)
                    For Each objField As FieldInfo In newRecord.GetType.GetFields()
                        If Not item(objField.Name).GetType() Is objField.FieldType Then
                            objField.SetValue(newRecord, Convert.ChangeType(item(objField.Name), objField.FieldType))
                        Else
                            objField.SetValue(newRecord, item(objField.Name))
                        End If
                    Next
                    target.Add(newRecord)
                Next
            End If
            Dim engine As FileHelperEngine = Nothing
            Select Case Me.FileHelpersMode
                Case FileHelpersMode.Delimiter
                    Dim delimiterEngine As New DelimitedFileEngine(objRecordType)
                    Select Case DelimiterMode
                        Case CSVDelimiterMode.SimpleChar
                            delimiterEngine.Options.Delimiter = Me.Delimiter
                        Case CSVDelimiterMode.EscapedString
                            delimiterEngine.Options.Delimiter = Regex.Unescape(EscapedDelimiter)
                    End Select
                    If Me.IncludeHeaders Then
                        'Dim fields As FieldInfo() = objRecordType.GetFields((BindingFlags.NonPublic Or (BindingFlags.Public Or (BindingFlags.Instance Or BindingFlags.DeclaredOnly))))
                        'Dim fieldNames As New List(Of String)
                        'For Each objField As FieldInfo In fields
                        '    fieldNames.Add(objField.Name)
                        'Next
                        Dim header As String = String.Join(Me.Delimiter, delimiterEngine.Options.FieldsNames)
                        delimiterEngine.HeaderText = header
                    End If
                    engine = delimiterEngine
                Case Else
                    Dim fixedEngine As New FixedFileEngine(objRecordType)
                    engine = fixedEngine
            End Select
            engine.WriteStream(tw, target)
        End Sub

        Public Function DeSerialize(input As String) As IEnumerable

            Dim objRecordType As Type = Nothing
            If Me.RecordsMode = RecordsMode.StaticType Then
                objRecordType = Me.RecordType.GetDotNetType
            Else
                'todo, g�rer le cas dynamique
            End If

            Dim engine As FileHelperEngine = Nothing
            Select Case Me.FileHelpersMode
                Case FileHelpersMode.Delimiter
                    'todo, cf plus haut: g�rer le cas dynamique
                    Dim delimiterEngine As New DelimitedFileEngine(objRecordType)
                     Select Case DelimiterMode
                        Case CSVDelimiterMode.SimpleChar
                            delimiterEngine.Options.Delimiter = Me.Delimiter
                        Case CSVDelimiterMode.EscapedString
                            delimiterEngine.Options.Delimiter = Regex.Unescape(EscapedDelimiter)
                    End Select
                    If Me.IncludeHeaders Then
                        'Dim fields As FieldInfo() = objRecordType.GetFields((BindingFlags.NonPublic Or (BindingFlags.Public Or (BindingFlags.Instance Or BindingFlags.DeclaredOnly))))
                        'Dim fieldNames As New List(Of String)
                        'For Each objField As FieldInfo In fields
                        '    fieldNames.Add(objField.Name)
                        'Next
                        Dim header As String = String.Join(Me.Delimiter, delimiterEngine.Options.FieldsNames)
                        delimiterEngine.HeaderText = header
                    End If
                    engine = delimiterEngine
                Case Else
                    Dim fixedEngine As New FixedFileEngine(objRecordType)
                    engine = fixedEngine
            End Select
            Dim listToReturn() As Object = engine.ReadString(input)
            Dim arrayToReturn As Array = Array.CreateInstance(objRecordType, listToReturn.Length)
            listToReturn.CopyTo(arrayToReturn, 0)
            Return arrayToReturn

        End Function

        Private Function ComputeDynamicExpressions(inputObjects As IEnumerable) As List(Of SerializableDictionary(Of String, Object))

            Dim toReturn As New List(Of SerializableDictionary(Of String, Object))
            'Dim fieldNames As Dictionary(Of String, String) = ParsePairs(Me.FieldVars, True)
            For Each inputObject As Object In inputObjects
                Dim context As New PortalKeeperContext(Of String)
                context.SetVar(Me.InputEntityVarName, inputObject)
                Dim objDumpSettings As New DumpSettings()
                objDumpSettings.EnableDump = True
                objDumpSettings.DumpAllVars = False
                objDumpSettings.DumpVariables = Me.FieldVars
                objDumpSettings.SkipNull = False
                Dim dump As SerializableDictionary(Of String, Object) = context.GetDump(objDumpSettings)
                toReturn.Add(dump)
            Next
            Return toReturn
        End Function


    End Class
End Namespace
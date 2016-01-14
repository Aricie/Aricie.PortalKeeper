Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization

Namespace ComponentModel


    Public Module JsonHelper

        <Extension()> _
        Public Sub SetDefaultSettings(objSettings As JsonSerializerSettings) 
            objSettings.Formatting = Formatting.Indented
            objSettings.DefaultValueHandling = DefaultValueHandling.Ignore
            objSettings.NullValueHandling = NullValueHandling.Ignore
            objSettings.ContractResolver = new XmlAwareContractResolver()
        End Sub

    End Module

    Public Class XmlAwareContractResolver
        Inherits DefaultContractResolver
	
	
#Region "Overrides of DefaultContractResolver"

        ''' <summary>
        ''' Creates a <see cref="JsonProperty" /> for the given <see cref="Reflection.MemberInfo" />.
        ''' </summary>
        Protected Overrides Function CreateProperty(member As MemberInfo, memberSerialization As MemberSerialization) As JsonProperty
            Dim [property] As JsonProperty = MyBase.CreateProperty(member, memberSerialization)
            Me.ConfigureProperty(member, [property])
            Return [property]
        End Function

#End Region

#Region "Private Methods"

        ' Determines whether a member is required or not and sets the appropriate JsonProperty settings
        Private Sub ConfigureProperty(member As MemberInfo, [property] As JsonProperty)
            ' Check for NonSerialized attributes
            If Attribute.IsDefined(member, GetType(NonSerializedAttribute), True) OrElse Attribute.IsDefined(member, GetType(XmlIgnoreAttribute), True) Then
                [property].Ignored = True
            End If
            'if (typeof(ICollection).IsAssignableFrom(property.PropertyType)  )
            '{
            '    var formerExp = property.GetIsSpecified;
            '    property.GetIsSpecified = delegate(object o)
            '    {
            '        return ((formerExp == null) || formerExp(o)) && (!IsEmptyCollection(o));
            '    };
            '}
        End Sub

        'private bool IsEmptyCollection(object o)
        '{
        '    if (o==null)
        '    {
        '        return false;
        '    }
        '    if  (!(o is ICollection))
        '    {
        '        return false;
        '    }
        '    if (((ICollection)o).Count > 0)
        '    {
        '        return false;
        '    }
        '    return true;

        '}

#End Region
    End Class
End NameSpace
Imports Aricie.DNN.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports Aricie.DNN.Services.Flee
Imports Aricie.DNN.Services.Filtering
Imports Aricie.Collections
Imports Aricie.ComponentModel
Imports System.ComponentModel
Imports DotNetNuke.UI.WebControls
Imports Aricie.DNN.UI.WebControls
Imports System.Globalization
Imports Aricie.DNN.Services

Namespace Entities

    Public Enum PagerQueryType
        PageNb
        ResultIndex
    End Enum

    Public Enum HttpClientMode
        WebClient
        Browser
    End Enum

    <ActionButton(IconName.Search, IconOptions.Normal)> _
    <Serializable()> _
    Public Class HtmlPageScrapsInfo(Of TScrap As HtmlPageScrapInfo)

        <ExtendedCategory("PageScraps")> _
        Public Property PageScraps As New SerializableList(Of TScrap)

        <ExtendedCategory("Global")> _
        Public Property MaxNbPages As Integer

        <ExtendedCategory("Global")> _
        Public Property MaxResultsPerPage As Integer

        <ExtendedCategory("Global")> _
        Public Property PrimaryKeys As New List(Of String)

        <ExtendedCategory("Advanced")> _
        Public Property Custom As New SerializableDictionary(Of String, String)

        <ExtendedCategory("Advanced")> _
        Public Property AdditionalColumns As New SerializableDictionary(Of String, FilteredString)

        Public Function GetPrimaryKey(input As Dictionary(Of String, String)) As String
            Dim tempKey As String = Nothing
            Return (From objPrimaryKey In PrimaryKeys Where input.TryGetValue(objPrimaryKey, tempKey)).Aggregate("", Function(current, objPrimaryKey) current & tempKey)
        End Function

    End Class

    <Serializable()> _
    Public Class OneOrMore(Of T As New)

        Public Property One As New T

        Public Property More As New List(Of T)

        <Browsable(False)> _
        Public ReadOnly Property All As IEnumerable(Of T)
            Get
                Dim toReturn As New List(Of T)(More.Count + 1)
                toReturn.Add(One)
                toReturn.AddRange(More)
                Return toReturn
            End Get
        End Property

    End Class


    <ActionButton(IconName.Search, IconOptions.Normal)> _
    <Serializable()> _
    Public Class HtmlPageScrapInfo
        Inherits NamedConfig

        <ExtendedCategory("Url")> _
        Public Property Requests As New OneOrMore(Of HtmlRequestInfo)

        <ExtendedCategory("Pager")> _
        Public Property UsePager As Boolean

        <ConditionalVisible("UsePager", False, True)> _
        <ExtendedCategory("Pager")> _
        Public Property MaxNbPage As Integer = 0

        <ExtendedCategory("Pager")> _
        <ConditionalVisible("UsePager", False, True)> _
        Public Property QueryType As PagerQueryType = PagerQueryType.PageNb

        <ConditionalVisible("QueryType", False, True, PagerQueryType.ResultIndex)> _
      <ConditionalVisible("UsePager", False, True)> _
      <ExtendedCategory("Pager")> _
        Public Property PageSize As Integer = 20

        <ConditionalVisible("UsePager", False, True)> _
        <ExtendedCategory("Pager")> _
        Public Property QueryParameter As String = "page"

        'todo: remove that obsolete property
        <Browsable(False)>
        Public Property PageParameter As String
            Get
                Return QueryParameter
            End Get
            Set(value As String)
                QueryParameter = value
            End Set
        End Property

        <ConditionalVisible("UsePager", False, True)> _
        <ExtendedCategory("Pager")> _
        Public Property IncludeFirstPage As Boolean

        <ConditionalVisible("UsePager", False, True)> _
        <ExtendedCategory("Pager")> _
        Public Property ZeroBasedIndex As Boolean

        <ExtendedCategory("Scraping")> _
        Public Property XPath As New XPathInfo("//title", True, True)

        <ExtendedCategory("Detail")> _
        Public Property ScrapDetail As Boolean

        <ExtendedCategory("Detail")> _
         <ConditionalVisible("ScrapDetail", False, True)> _
        Public Property ScrapDetails As New SerializableList(Of HtmlScrapDetailRequestInfo)


        '<ExtendedCategory("Detail")> _
        '<ConditionalVisible("ScrapDetail", False, True)> _
        <Browsable(False)> _
        Public Property DetailClientMode As HttpClientMode
            Get
                If ScrapDetails.Count > 0 Then
                    Return ScrapDetails(0).ClientMode
                End If
                Return HttpClientMode.WebClient
            End Get
            Set(value As HttpClientMode)
                If ScrapDetails.Count = 0 Then
                    Dim toAdd As New HtmlScrapDetailRequestInfo()
                    ScrapDetails.Add(toAdd)
                End If
                ScrapDetails(0).ClientMode = value
            End Set
        End Property

        '<ExtendedCategory("Detail")> _
        '<ConditionalVisible("DetailClientMode", False, True, HttpClientMode.Browser)> _
        <Browsable(False)> _
        Public Property DetailFrameId As String
            Get
                If ScrapDetails.Count > 0 Then
                    Return ScrapDetails(0).FrameId
                End If
                Return ""
            End Get
            Set(value As String)
                If ScrapDetails.Count = 0 Then
                    Dim toAdd As New HtmlScrapDetailRequestInfo()
                    ScrapDetails.Add(toAdd)
                End If
                ScrapDetails(0).FrameId = value
            End Set
        End Property

        '<ExtendedCategory("Detail")> _
        '<ConditionalVisible("ScrapDetail", False, True)> _
        <Browsable(False)> _
        Public Property DetailXPath As XPathInfo
            Get
                If ScrapDetails.Count > 0 Then
                    Return ScrapDetails(0).XPath
                End If
                Return New XPathInfo("//title", True, True)
            End Get
            Set(value As XPathInfo)
                If ScrapDetails.Count = 0 Then
                    Dim toAdd As New HtmlScrapDetailRequestInfo()
                    ScrapDetails.Add(toAdd)
                End If
                ScrapDetails(0).XPath = value
            End Set
        End Property

        Public Overrides Function GetFriendlyDetails() As String
            Return String.Format("{1}{0}{2}{0}{3}", UIConstants.TITLE_SEPERATOR, _
                                 IIf(Me.UsePager, "max " & Me.MaxNbPage & " ", "No ").ToString() & "page" & IIf(Me.MaxNbPage > 1, "s", "").ToString(), _
                                IIf(Me.ScrapDetail, Me.ScrapDetails.Count().ToString(CultureInfo.InvariantCulture) & " ", "No ").ToString() & "detail scrap" & IIf(Me.MaxNbPage > 1, "s", "").ToString(), _
                                Me.Requests.One.ClientMode.ToString())

        End Function

        <ExtendedCategory("Custom")> _
        Public Property Custom As New SerializableDictionary(Of String, String)

        '<ExtendedCategory("Custom")> _
        'Public Property CustomVars As New Variables()

    End Class

    <Serializable()> _
    Public Class HtmlRequestInfo

        Public Overridable Property Url As New CData("http://www.google.com")

        Public Property ClientMode As HttpClientMode = HttpClientMode.WebClient

        <ConditionalVisible("ClientMode", False, True, HttpClientMode.Browser)> _
        Public Property FrameId As String = ""


    End Class

    <Serializable()> _
 <DefaultProperty("FriendlyName")> _
    Public Class HtmlScrapRequestInfo
        Inherits HtmlRequestInfo

        Public Property XPath As New XPathInfo("//title", True, True)

    End Class

    Public Enum ScrapUrlMode
        Pattern
        Column
    End Enum


    <Serializable()> _
   <DefaultProperty("FriendlyName")> _
    Public Class HtmlScrapDetailRequestInfo
        Inherits HtmlScrapRequestInfo

        <SortOrder(0)> _
        Public Property UrlMode As ScrapUrlMode = ScrapUrlMode.Column

        <SortOrder(1)> _
        <ConditionalVisible("UrlMode", False, True, ScrapUrlMode.Pattern)> _
        Public Overrides Property Url As CData
            Get
                Return MyBase.Url
            End Get
            Set(value As CData)
                MyBase.Url = value
            End Set
        End Property


        <ConditionalVisible("UrlMode", False, True, ScrapUrlMode.Column)> _
        <SortOrder(1)> _
        Public Property DetailUrlColumn As String = "Url"

        <Browsable(False)> _
        Public ReadOnly Property FriendlyName As String
            Get
                Return Join({Me.DetailUrlColumn, Me.ClientMode.ToString(), Me.FrameId, Me.XPath.SelectExpression}, UIConstants.TITLE_SEPERATOR)
            End Get
        End Property

        Public Function GetUrl(columns As IDictionary(Of String, String)) As String
            Select Case Me.UrlMode
                Case ScrapUrlMode.Column
                    If columns.ContainsKey(Me.DetailUrlColumn) Then
                        Return columns(Me.DetailUrlColumn)
                    End If
                Case ScrapUrlMode.Pattern
                    Dim atr As New AdvancedTokenReplace()
                    atr.SetObjectReplace(columns, "Result")
                    Return atr.ReplaceAllTokens(Me.Url.Value)
            End Select
            Throw New ApplicationException("Unable to build Scrap detail url")
        End Function


    End Class


    Public Class FilteredString


        Public Property Source As New SimpleExpression(Of String)

        Public Property Filter As New ExpressionFilterInfo()


        Public Function Process(ByVal owner As Object, ByVal items As Aricie.DNN.Services.IContextLookup) As String
            Return Filter.Process(Source.Evaluate(owner, items))
        End Function


    End Class


End Namespace
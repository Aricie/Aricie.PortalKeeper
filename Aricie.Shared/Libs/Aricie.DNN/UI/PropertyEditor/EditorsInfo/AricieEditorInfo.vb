Imports DotNetNuke.UI.WebControls


Namespace UI.WebControls.EditorInfos
    Public Class AricieEditorInfo
        Inherits EditorInfo

        'Protected _hierarchy As AricieWebControlEntity
        Protected _autoPostBack As Boolean
        Private _length As Integer

        'Public Property Hierarchy() As AricieWebControlEntity
        '    Get
        '        Return _hierarchy
        '    End Get
        '    Set(ByVal value As AricieWebControlEntity)
        '        _hierarchy = value
        '    End Set
        'End Property

        Public Property AutoPostBack() As Boolean
            Get
                Return _autoPostBack
            End Get
            Set(ByVal value As Boolean)
                _autoPostBack = value
            End Set
        End Property

        Public Property Length() As Integer
            Get
                Return _length
            End Get
            Set(ByVal value As Integer)
                _length = value
            End Set
        End Property
    End Class
End Namespace


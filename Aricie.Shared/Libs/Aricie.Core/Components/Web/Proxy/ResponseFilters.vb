Imports System.IO
Imports System.IO.Compression
Imports System.Web
Imports System.Text
Imports Aricie.ComponentModel
Imports Aricie

Namespace IO

    <Flags()> _
    Public Enum ResponseFilterType
        None = 0
        TransformString = 1
        TransformStream = 2
        TransformWrite = 4
        TransformWriteString = 8
        CaptureStream = 16
        CaptureString = 32
    End Enum


    Public Class ResponseFilterStream
        Inherits Stream
        Private ReadOnly _stream As Stream

        Private ReadOnly _http As HttpContext

        Private _position As Long

        Private _cacheStream As MemoryStream = New MemoryStream(5000)

        Private _cachePointer As Integer


        Private _FilterType As ResponseFilterType = ResponseFilterType.TransformString Or ResponseFilterType.TransformStream _
                                                    Or ResponseFilterType.TransformWrite Or ResponseFilterType.TransformWriteString _
                                                    Or ResponseFilterType.CaptureStream Or ResponseFilterType.CaptureString

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return True
            End Get
        End Property

        Private ReadOnly Property IsCaptured As Boolean
            Get
                'If (Me.CaptureStream Is Nothing AndAlso Me.CaptureString Is Nothing AndAlso Me.TransformStream Is Nothing AndAlso Me.TransformString Is Nothing) Then
                '    Return False
                'End If
                Return ((Me._FilterType And ResponseFilterType.CaptureStream) = ResponseFilterType.CaptureStream) _
                    OrElse ((Me._FilterType And ResponseFilterType.CaptureString) = ResponseFilterType.CaptureString) _
                    OrElse ((Me._FilterType And ResponseFilterType.TransformStream) = ResponseFilterType.TransformStream) _
                   OrElse ((Me._FilterType And ResponseFilterType.TransformString) = ResponseFilterType.TransformString)
            End Get
        End Property

        Private ReadOnly Property IsOutputDelayed As Boolean
            Get
                'If (Me.TransformStream Is Nothing AndAlso Me.TransformString Is Nothing) Then
                '    Return False
                'End If
                'Return True
                Return ((Me._FilterType And ResponseFilterType.TransformStream) = ResponseFilterType.TransformStream) _
                   OrElse ((Me._FilterType And ResponseFilterType.TransformString) = ResponseFilterType.TransformString)
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Return CLng(0)
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Return Me._position
            End Get
            Set(ByVal value As Long)
                Me._position = value
            End Set
        End Property

        Public Sub New(ByVal responseStream As Stream, ByVal context As HttpContext)
            MyBase.New()
            Me._stream = responseStream
            Me._http = context
        End Sub

        Public Sub New(ByVal responseStream As Stream, ByVal context As HttpContext, objFilterType As ResponseFilterType)
            MyBase.New()
            Me._stream = responseStream
            Me._http = context
            Me._FilterType = objFilterType
        End Sub

        Public Overrides Sub Close()
            Me._stream.Close()
        End Sub

        Public Overrides Sub Flush()
            If (Me.IsCaptured AndAlso Me._cacheStream.Length > CLng(0)) Then
                If (Me._FilterType And ResponseFilterType.TransformStream) = ResponseFilterType.TransformStream Then
                    Me._cacheStream = Me.OnTransformCompleteStream(Me._cacheStream)
                End If
                If (Me._FilterType And ResponseFilterType.TransformString) = ResponseFilterType.TransformString Then
                    Me._cacheStream = Me.OnTransformCompleteStringInternal(Me._cacheStream)
                End If
                If (Me._FilterType And ResponseFilterType.CaptureStream) = ResponseFilterType.CaptureStream Then
                    Me.OnCaptureStream(Me._cacheStream)
                End If
                If (Me._FilterType And ResponseFilterType.CaptureString) = ResponseFilterType.CaptureString Then
                    Me.OnCaptureStringInternal(Me._cacheStream)
                End If
                If (Me.IsOutputDelayed) Then
                    Me._stream.Write(Me._cacheStream.ToArray(), 0, CInt(Me._cacheStream.Length))
                End If
                Me._cacheStream.SetLength(CLng(0))
            End If
            Me._stream.Flush()
        End Sub

        Protected Overridable Sub OnCaptureStream(ByVal ms As MemoryStream)

            RaiseEvent CaptureStream(ms)
        End Sub

        Protected Overridable Sub OnCaptureString(ByVal output As String)
            RaiseEvent CaptureString(output)
        End Sub

        Private Sub OnCaptureStringInternal(ByVal ms As MemoryStream)
            Dim str As String = Me._http.Response.ContentEncoding.GetString(ms.ToArray())
            Me.OnCaptureString(str)
        End Sub

        Protected Overridable Function OnTransformCompleteStream(ByVal ms As MemoryStream) As MemoryStream
            Dim args As New ChangedEventArgs(Of MemoryStream)(ms, ms)
            RaiseEvent TransformStream(Me, args)
            Return args.NewValue
        End Function

        Private Function OnTransformCompleteString(ByVal responseText As String) As String
            Dim args As New ChangedEventArgs(Of String)(responseText, responseText)
            RaiseEvent TransformString(Me, args)
            Return args.NewValue
        End Function

        Friend Function OnTransformCompleteStringInternal(ByVal ms As MemoryStream) As MemoryStream
            Dim clearBytes As Byte() = ms.ToArray()

            Dim contentEncodingHeader As String = _http.Response.Headers.Item("Content-Encoding")

            If Not contentEncodingHeader.IsNullOrEmpty() Then
                contentEncodingHeader = contentEncodingHeader.ToLowerInvariant()
                Select Case contentEncodingHeader
                    Case "gzip"
                        clearBytes = clearBytes.Decompress(CompressionMethod.Gzip)
                    Case "deflate"
                        clearBytes = clearBytes.Decompress(CompressionMethod.Deflate)
                End Select
            End If


            Dim str As String = Me._http.Response.ContentEncoding.GetString(clearBytes)

            str = OnTransformCompleteString(str)


            Dim bytes As Byte() = Me._http.Response.ContentEncoding.GetBytes(str)

            If Not contentEncodingHeader.IsNullOrEmpty() Then
                Select Case contentEncodingHeader
                    Case "gzip"
                        bytes = bytes.Compress(CompressionMethod.Gzip)
                    Case "deflate"
                        bytes = bytes.Compress(CompressionMethod.Deflate)
                End Select
            End If


            ms = New MemoryStream()
           
            ms.Write(bytes, 0, CInt(bytes.Length))
           
            Return ms
        End Function

        Protected Overridable Function OnTransformWrite(ByVal buffer As Byte()) As Byte()
            Dim args As New ChangedEventArgs(Of Byte())(buffer, buffer)
            RaiseEvent TransformWrite(Me, args)
            Return args.NewValue
        End Function

        Private Function OnTransformWriteString(ByVal value As String) As String
            Dim args As New ChangedEventArgs(Of String)(value, value)
            RaiseEvent TransformWriteString(Me, args)
            Return args.NewValue
        End Function

        Private Function OnTransformWriteStringInternal(ByVal buffer As Byte()) As Byte()
            Dim contentEncoding As Encoding = Me._http.Response.ContentEncoding
            Dim str As String = Me.OnTransformWriteString(contentEncoding.GetString(buffer))
            Return contentEncoding.GetBytes(str)
        End Function

        Public Overrides Function Read(ByVal buffer As Byte(), ByVal offset As Integer, ByVal count As Integer) As Integer
            Return Me._stream.Read(buffer, offset, count)
        End Function

        Public Overrides Function Seek(ByVal offset As Long, ByVal direction As SeekOrigin) As Long
            Return Me._stream.Seek(offset, direction)
        End Function

        Public Overrides Sub SetLength(ByVal length As Long)
            Me._stream.SetLength(length)
        End Sub

        Public Overrides Sub Write(ByVal buffer As Byte(), ByVal offset As Integer, ByVal count As Integer)
            If (Me.IsCaptured) Then
                Me._cacheStream.Write(buffer, 0, count)
                Me._cachePointer = Me._cachePointer + count
            End If
            If (Me._FilterType And ResponseFilterType.TransformWrite) = ResponseFilterType.TransformWrite Then
                buffer = Me.OnTransformWrite(buffer)
            End If

            If (Me._FilterType And ResponseFilterType.TransformWriteString) = ResponseFilterType.TransformWriteString Then
                buffer = Me.OnTransformWriteStringInternal(buffer)
            End If
            If (Not Me.IsOutputDelayed) Then
                Me._stream.Write(buffer, offset, count)
            End If
        End Sub

        Public Event CaptureStream As Action(Of MemoryStream)

        Public Event CaptureString As Action(Of String)

        'Public Event TransformStream As Func(Of MemoryStream, MemoryStream)

        Public Event TransformStream As EventHandler(Of ChangedEventArgs(Of MemoryStream))

        Public Event TransformString As EventHandler(Of ChangedEventArgs(Of String))

        Public Event TransformWrite As EventHandler(Of ChangedEventArgs(Of Byte()))

        Public Event TransformWriteString As EventHandler(Of ChangedEventArgs(Of String))
    End Class



    <Serializable()> _
    Public Class ResponseFilteringEventArgs
        Inherits EventArgs

        Public Sub New()

        End Sub

        Public Sub New(strResponse As String)
            Me.Response = strResponse
        End Sub

        Public Property Response As String

        Public Property ResponseStream As Stream


    End Class

    Public Class EventResponseFilter
        Inherits BaseResponseFilter

        Public Event ResponseFiltering As EventHandler(Of ResponseFilteringEventArgs)

        Public Sub New(inputStream As Stream, context As HttpContext)
            MyBase.New(inputStream, context)
        End Sub


        Public Overrides Function UpdateResponse(strResponse As String) As String
            Dim objArgs As New ResponseFilteringEventArgs(strResponse)
            RaiseEvent ResponseFiltering(Me, objArgs)
            Return objArgs.Response
        End Function

    End Class


    ''' <summary>
    ''' Used as an http response filter to modify the contents of the output html.
    ''' </summary>
    Public MustInherit Class BaseResponseFilter
        Inherits Stream
        Public Sub New(inputStream As Stream, context As HttpContext)
            m_ResponseStream = inputStream
            m_Context = context
            m_ResponseHtml = New StringBuilder()
        End Sub
#Region "Private members"
        Private m_ResponseStream As Stream
        Private m_Position As Long
        Private m_ResponseHtml As StringBuilder
        Private m_Context As HttpContext
#End Region

#Region "Basic Stream implementation overrides"
        Public Overrides ReadOnly Property CanRead() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get
                Return True
            End Get
        End Property
        Public Overrides ReadOnly Property Length() As Long
            Get
                Return 0
            End Get
        End Property
#End Region
#Region "Stream wrapper implementation"
        Public Overrides Sub Close()
            m_ResponseStream.Close()
        End Sub
        Public Overrides Property Position() As Long
            Get
                Return m_Position
            End Get
            Set(value As Long)
                m_Position = value
            End Set
        End Property
        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Return m_ResponseStream.Seek(offset, origin)
        End Function
        Public Overrides Sub SetLength(length As Long)
            m_ResponseStream.SetLength(length)
        End Sub
        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Return m_ResponseStream.Read(buffer, offset, count)
        End Function
#End Region
#Region "Stream implemenation that does stuff"
        ''' <summary>
        ''' Appends the bytes written to our string builder
        ''' </summary>
        ''' <param name="objBuffer"></param>
        ''' <param name="offset"></param>
        ''' <param name="count"></param>
        Public Overrides Sub Write(objBuffer As Byte(), offset As Integer, count As Integer)
            Dim data As Byte() = New Byte(count - 1) {}
            Buffer.BlockCopy(objBuffer, offset, data, 0, count)
            m_ResponseHtml.Append(m_Context.Response.ContentEncoding.GetString(objBuffer))
        End Sub
        ''' <summary>
        ''' Before the contents are flushed to the stream, the output is inspected and altered
        ''' and then written to the stream.
        ''' </summary>
        Public Overrides Sub Flush()
            UpdateOutputHtml()
            m_ResponseStream.Flush()
        End Sub
#End Region

        Private Sub UpdateOutputHtml()
            Dim output = Me.UpdateResponse(m_ResponseHtml.ToString())
            Dim outputBytes As Byte() = m_Context.Response.ContentEncoding.GetBytes(output)
            m_ResponseStream.Write(outputBytes, 0, outputBytes.GetLength(0))
        End Sub

        Public MustOverride Function UpdateResponse(strResponse As String) As String

    End Class
End Namespace



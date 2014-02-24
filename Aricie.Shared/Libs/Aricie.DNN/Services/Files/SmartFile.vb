﻿Imports Aricie.DNN.ComponentModel
Imports System.ComponentModel
Imports Aricie.DNN.UI.Attributes
Imports System.Web.Configuration
Imports Aricie.ComponentModel
Imports Aricie.Cryptography
Imports Aricie.Collections
Imports Aricie.DNN.UI.Controls
Imports DotNetNuke.UI.Skins.Controls
Imports DotNetNuke.UI.WebControls
Imports System.Xml.Serialization
Imports Aricie.DNN.UI.WebControls
Imports DotNetNuke.Services.FileSystem
Imports Aricie.Services
Imports System.Text
Imports System.Globalization
Imports DotNetNuke.Security.Permissions
Imports DotNetNuke.Entities.Users
Imports System.Xml
Imports DotNetNuke.Services.Localization

Namespace Services.Files
    <Serializable()> _
    Public Class SmartFile
        'Inherits SmartFileInfo

        Public Sub New()
        End Sub

        Public Sub New(key As EntityKey)
            Me.Key = key
        End Sub

        Public Sub New(key As EntityKey, encrypter As IEncrypter)
            Me.New(key)
            Me._encrypter = encrypter
        End Sub

        Protected _SaltBytes As Byte()

        Private _PayLoad As String = ""

        Public Property Key As EntityKey

        <IsReadOnly(True)> _
        Public Property Signed As Boolean




        <IsReadOnly(True)> _
        Public Property Compressed As Boolean

        Private _encrypter As IEncrypter


        Public ReadOnly Property HasEncrypter As Boolean
            Get
                Return _encrypter IsNot Nothing
            End Get
        End Property

        Public Sub SetEncrypter(ByVal encrypter As IEncrypter)
            _encrypter = encrypter
        End Sub

        <IsReadOnly(True)> _
        Public Property CustomEncryption As Boolean

        <IsReadOnly(True)> _
        Public Property Encrypted As Boolean

        <Browsable(False)> _
        <XmlIgnore()>
        Public ReadOnly Property SaltBytes As Byte()
            Get
                Return _SaltBytes
            End Get
        End Property

        <ConditionalVisible("Encrypted", False, True)> _
        <IsReadOnly(True)> _
        Public Property Salt As String
            Get
                If Me.Encrypted Then
                    Return Convert.ToBase64String(_SaltBytes)
                End If
                Return ""
            End Get
            Set(value As String)
                _SaltBytes = Convert.FromBase64String(value)
            End Set
        End Property


        Public ReadOnly Property MD5Checksum As String
            Get
                Return Common.Hash(Me.PayLoad, HashProvider.MD5)
            End Get
        End Property

        Public ReadOnly Property Sha256Checksum As String
            Get
                Return Common.Hash(Me.PayLoad, HashProvider.SHA256)
            End Get
        End Property


        Public Property ShowPayLoad As Boolean

        <ConditionalVisible("ShowPayLoad", False, True)> _
        <IsReadOnly(True)>
        Public Property PayLoad As String
            Get
                Return _PayLoad
            End Get
            Set(value As String)
                If _PayLoad <> value Then
                    Me.Compressed = False
                    Me.Encrypted = False
                End If
                _PayLoad = value
            End Set
        End Property

        Public Property UseCustomKey As Boolean

        <ConditionalVisible("UseCustomKey", False, True)> _
        Public Property CustomKey As String = ""


#Region "Public Methods"

        <ConditionalVisible("HasEncrypter", False, True)> _
      <ConditionalVisible("Encrypted", True, True)> _
      <ConditionalVisible("Compressed", True, True)> _
    <ActionButton(IconName.Compress, IconOptions.Normal)> _
        Public Sub Sign()
            If Me.Encrypted OrElse Me.Compressed Then
                Throw New ApplicationException("Encrypted or compressed Content cannot be Signed")
            End If
            If Not Me.Signed Then
                Dim doc As New XmlDocument()
                SyncLock Me
                    doc.LoadXml(Me.PayLoad)
                    If Me._encrypter IsNot Nothing Then
                        _encrypter.Sign(doc)
                        PayLoad = doc.OuterXml
                        Me.Signed = True
                    End If
                End SyncLock
            End If
        End Sub

        <ConditionalVisible("HasEncrypter", False, True)> _
        <ConditionalVisible("Signed", False, True)> _
     <ActionButton(IconName.CheckSquareO, IconOptions.Normal)> _
        Public Overloads Sub Verify(ape As AriciePropertyEditorControl)
            Dim message As String
            Dim messageType As ModuleMessage.ModuleMessageType
            If Me.Verify() Then
                message = Localization.GetString("SignatureVerified.Message", ape.LocalResourceFile)
                messageType = ModuleMessage.ModuleMessageType.GreenSuccess
            Else
                message = Localization.GetString("SignatureFailedToVerify.Message", ape.LocalResourceFile)
                messageType = ModuleMessage.ModuleMessageType.GreenSuccess
            End If
            ape.DisplayMessage(message, messageType)
        End Sub

        Public Overloads Function Verify() As Boolean
            If Not Me.Signed Then
                Throw New ApplicationException("Unsigned document cannot be verified")
            Else
                Dim doc As New XmlDocument()
                SyncLock Me
                    doc.LoadXml(Me.PayLoad)
                    If Me._encrypter IsNot Nothing Then
                        Return _encrypter.Verify(doc)
                    Else
                        Throw New ApplicationException("Cannot sign a smart file without an ecrypter")
                    End If
                End SyncLock
            End If
        End Function


        <ConditionalVisible("Compressed", True, True)> _
       <ActionButton(IconName.Compress, IconOptions.Normal)> _
        Public Sub Compress()
            If Me.Encrypted Then
                Throw New ApplicationException("Encrypted Content cannot be compressed")
            End If
            If Not Me.Compressed Then
                SyncLock Me
                    PayLoad = Common.DoCompress(PayLoad, CompressionMethod.Gzip)
                    Me.Compressed = True
                End SyncLock
            End If
        End Sub

        <ConditionalVisible("Compressed", False, True)> _
       <ActionButton(IconName.Expand, IconOptions.Normal)> _
        Public Sub Decompress()
            If Me.Encrypted Then
                Throw New ApplicationException("Encrypted Content cannot be decompressed")
            End If
            If Me.Compressed Then
                SyncLock Me
                    PayLoad = Common.DoDeCompress(PayLoad, CompressionMethod.Gzip)
                    Me.Compressed = False
                End SyncLock
            End If
        End Sub




        <ConditionalVisible("Encrypted", False, True)> _
            <ActionButton(IconName.Unlock, IconOptions.Normal)> _
        Public Sub Decrypt()
            Try
                If Me.Encrypted Then
                    Dim newPayLoad As String
                    If _encrypter IsNot Nothing Then
                        newPayLoad = _encrypter.Decrypt(Me.PayLoad, Me._SaltBytes)
                    Else
                        newPayLoad = Common.Decrypt(Me.PayLoad, GetKey(), "", Me._SaltBytes)
                    End If
                    Me.DecryptInternal(newPayLoad)
                End If
            Catch ex As Exception
                Throw New ApplicationException("Value was encrypted with a distinct key")
            End Try
        End Sub

        <ConditionalVisible("Encrypted", True, True)> _
        <ActionButton(IconName.Lock, IconOptions.Normal)> _
        Public Sub Encrypt()
            If Not Me.Encrypted Then
                Dim objSalt As Byte() = Nothing
                Dim newPayload As String
                If _encrypter IsNot Nothing Then
                    newPayload = _encrypter.Encrypt(Me.PayLoad, objSalt)
                    Me.Encrypt(newPayload, objSalt)
                Else
                    newPayload = Common.Encrypt(Me.PayLoad, GetKey(), "", objSalt)
                    Me.EncryptInternal(newPayload, objSalt)
                End If
            End If
        End Sub







        Public Function GetKey() As String
            If UseCustomKey Then
                Return Me.CustomKey
            Else
                Return New MachineKeySection().DecryptionKey
            End If
        End Function

        Public Sub Decrypt(newPayload As String)
            SyncLock Me
                Me.DecryptInternal(newPayload)
                Me.CustomEncryption = True
            End SyncLock
        End Sub



        Public Sub Encrypt(ByVal newPayLoad As String, ByVal salt As Byte())
            SyncLock Me
                Me.EncryptInternal(newPayLoad, salt)
                Me.CustomEncryption = True
            End SyncLock
        End Sub


        Public Overridable Sub UnWrap()
            Me.Decrypt()
            Me.Decompress()
        End Sub

        Public Sub Wrap(settings As SmartFileInfo)
            If settings.Sign AndAlso Me._encrypter IsNot Nothing Then
                Me.Sign()
            End If
            If settings.Compress Then
                Me.Compress()
            End If
            If settings.Encrypt Then
                Me.Encrypt()
            End If
        End Sub



#End Region




#Region "Shared Methods"

        Public Shared Function GetFileInfo(key As EntityKey, settings As SmartFileInfo) As FileInfo
            Dim objPath As String = settings.GetPath(key)
            Dim fileName As String = System.IO.Path.GetFileName(objPath)
            Dim objFolderInfo As FolderInfo = ObsoleteDNNProvider.Instance.GetFolderFromPath(key.PortalId, objPath)
            If objFolderInfo IsNot Nothing Then
                Dim objFiles As IEnumerable(Of DotNetNuke.Services.FileSystem.FileInfo) = ObsoleteDNNProvider.Instance.GetFiles(objFolderInfo)
                For Each objFile As FileInfo In objFiles
                    If objFile.FileName = fileName Then
                        Return objFile
                    End If
                Next
            End If
            Return Nothing
        End Function

        Public Shared Function LoadAndRead(Of T As New)(key As EntityKey, encrypter As IEncrypter, settings As SmartFileInfo) As T
            Dim toReturn As T
            Dim objFileInfo As FileInfo = GetFileInfo(key, settings)
            If objFileInfo IsNot Nothing Then
                Dim content As Byte() = ObsoleteDNNProvider.Instance.GetFileContent(objFileInfo)
                Dim objSmartFile As SmartFile(Of T) = ReflectionHelper.Deserialize(Of SmartFile(Of T))(Encoding.UTF8.GetChars(content))
                objSmartFile.SetEncrypter(encrypter)
                toReturn = objSmartFile.Value
            End If
            Return toReturn
        End Function

        Public Shared Function LoadSmartFile(Of T As New)(key As EntityKey, settings As SmartFileInfo) As SmartFile(Of T)
            Dim objFileInfo As FileInfo = GetFileInfo(key, settings)
            Return LoadSmartFile(Of T)(objFileInfo)
        End Function

        Public Shared Function LoadSmartFile(Of T As New)(objFileInfo As FileInfo) As SmartFile(Of T)
            If objFileInfo IsNot Nothing Then
                Dim content As Byte() = ObsoleteDNNProvider.Instance.GetFileContent(objFileInfo)
                Return ReflectionHelper.Deserialize(Of SmartFile(Of T))(Encoding.UTF8.GetChars(content))
            End If
            Return Nothing
        End Function


        Public Shared Function SaveSmartFile(value As SmartFile, settings As SmartFileInfo) As Boolean
            Dim objPath As String = settings.GetPath(value.Key)
            Dim objFolderInfo As FolderInfo = ObsoleteDNNProvider.Instance.GetFolderFromPath(value.Key.PortalId, objPath)
            If objFolderInfo Is Nothing Then
                Dim permissionUserId As Integer = -1
                If (settings.GrantUserView OrElse settings.GrantUserEdit) AndAlso value.Key.UserName <> "" Then
                    Dim objUser As UserInfo = DotNetNuke.Entities.Users.UserController.GetUserByName(value.Key.PortalId, value.Key.UserName)
                    If objUser IsNot Nothing Then
                        permissionUserId = objUser.UserID
                    End If
                End If
                CreateSecureFoldersRecursive(value.Key.PortalId, objPath, permissionUserId, settings)
                objFolderInfo = ObsoleteDNNProvider.Instance.GetFolderFromPath(value.Key.PortalId, objPath)
            End If
            If objFolderInfo IsNot Nothing Then
                Dim objFileInfo As FileInfo = GetFileInfo(value.Key, settings)
                If objFileInfo Is Nothing Then
                    objFileInfo = New FileInfo() With {.FileName = System.IO.Path.GetFileName(objPath), .ContentType = "text/xml", .FolderId = objFolderInfo.FolderID, .PortalId = value.Key.PortalId, .StorageLocation = 2}
                    Dim result As Integer = NukeHelper.FileController.AddFile(objFileInfo)
                    If result > 0 Then
                        Dim content As Byte() = Encoding.UTF8.GetBytes(ReflectionHelper.Serialize(value, False).OuterXml)
                        ObsoleteDNNProvider.Instance.UpdateFileContent(objFileInfo, content)
                    Else
                        Throw New ApplicationException("Save failed, DNN returned " & result.ToString(CultureInfo.InvariantCulture))
                    End If
                End If
            Else
                Throw New ApplicationException("Could not access Smart File Storage")
            End If
            Return True
        End Function

        Protected Shared Sub CreateSecureFoldersRecursive(portalId As Integer, path As String, permissionUserId As Integer, settings As SmartFileInfo)
            If path.Contains("/"c) Then
                Dim objFolderInfo As FolderInfo = ObsoleteDNNProvider.Instance.GetFolderFromPath(portalId, path)
                If objFolderInfo Is Nothing Then
                    Dim parentPath As String = path.TrimEnd("/"c)
                    If parentPath.Contains("/"c) Then
                        parentPath = parentPath.Substring(0, parentPath.LastIndexOf("/"c))
                        CreateSecureFoldersRecursive(portalId, parentPath, -1, settings)
                    End If
                    Dim folder As New FolderInfo() With { _
                        .PortalID = portalId, _
                        .FolderPath = path, _
                        .StorageLocation = 2, _
                        .IsProtected = False, _
                        .IsCached = False _
                    }
                    ObsoleteDNNProvider.Instance.AddFolder(folder)
                    folder = ObsoleteDNNProvider.Instance.GetFolderFromPath(folder.PortalID, folder.FolderPath)
                    If folder IsNot Nothing Then
                        If permissionUserId > 0 Then
                            Dim fpc As New FolderPermissionController()
                            If settings.GrantUserView Then
                                Dim fp As New FolderPermissionInfo()
                                With fp
                                    .UserID = permissionUserId
                                    .FolderID = folder.FolderID
                                    .FolderPath = folder.FolderPath
                                    .PermissionCode = "VIEW"
                                    .PermissionKey = "VIEW"
                                End With
                                fpc.AddFolderPermission(fp)
                            End If
                            If settings.GrantUserEdit Then
                                Dim fp As New FolderPermissionInfo()
                                With fp
                                    .UserID = permissionUserId
                                    .FolderID = folder.FolderID
                                    .FolderPath = folder.FolderPath
                                    .PermissionCode = "EDIT"
                                    .PermissionKey = "EDIT"
                                End With
                                fpc.AddFolderPermission(fp)
                            End If
                        End If
                    End If

                End If
            End If
        End Sub

#End Region


#Region "Private Methods"

        Private Sub EncryptInternal(ByVal newPayLoad As String, ByVal salt As Byte())
            SyncLock Me
                Me.PayLoad = newPayLoad
                Me._SaltBytes = salt
                Me.Encrypted = True
            End SyncLock
        End Sub

        Private Sub DecryptInternal(newPayload As String)
            SyncLock Me
                Me.PayLoad = newPayload
                Me.Encrypted = False
                Me.Salt = ""
            End SyncLock
        End Sub


#End Region

    End Class


    <Serializable()> _
    Public Class SmartFile(Of T)
        Inherits SmartFile

        Public Sub New()
        End Sub


        Public Sub New(key As EntityKey, value As T, settings As SmartFileInfo, encrypter As IEncrypter)
            MyBase.New(key, encrypter)
            Me.Value = value
            Me.Wrap(settings)
        End Sub

        Public Property ShowValue As Boolean

        Private _Value As T


        <XmlIgnore()> _
        <ConditionalVisible("ShowValue", False, True)> _
        Public Property Value As T
            Get
                If _Value Is Nothing Then
                    Me.UnWrap()
                    If Not String.IsNullOrEmpty(Me.PayLoad) Then
                        _Value = Aricie.Services.ReflectionHelper.Deserialize(Of T)(Me.PayLoad)
                    End If
                End If
                Return _Value
            End Get
            Set(value As T)
                If value IsNot Nothing Then
                    Me.PayLoad = Aricie.Services.ReflectionHelper.Serialize(value).OuterXml
                Else
                    Me.PayLoad = ""
                End If
            End Set
        End Property


    End Class

End Namespace
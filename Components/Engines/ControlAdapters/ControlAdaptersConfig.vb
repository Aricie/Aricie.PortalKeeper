Imports Aricie.DNN.ComponentModel
Imports System.Xml.Serialization
Imports System.ComponentModel
Imports Aricie.Collections
Imports Aricie.DNN.UI.Attributes
Imports Aricie.DNN.UI.WebControls
Imports DotNetNuke.UI.Skins.Controls

Namespace Aricie.DNN.Modules.PortalKeeper
    
    Public Class ControlAdaptersConfig
        Implements IEnabled

        Public Property Enabled As Boolean Implements IEnabled.Enabled

        Public Property Adapters As New SerializableList(Of ControlAdapterSettings)

        Private _AdaptersDictionary As Dictionary(Of Type, ControlAdapterSettings)

        <XmlIgnore()> _
        <Browsable(False)> _
        Public ReadOnly Property AdaptersDictionary As Dictionary(Of Type, ControlAdapterSettings)
            Get
                If _AdaptersDictionary Is Nothing Then
                    Dim newDico As New Dictionary(Of Type, ControlAdapterSettings)
                    For Each objSettings As ControlAdapterSettings In Adapters
                        Dim objControlType As Type = objSettings.ResolvedAdaptedControlType
                        If objControlType IsNot Nothing Then
                            newDico(objControlType) = objSettings
                        End If
                    Next
                    _AdaptersDictionary = newDico
                End If
                Return _AdaptersDictionary
            End Get
        End Property

        Public Function RegisterAdapters() As Integer
            If Me.Enabled Then
                Dim currentContext As HttpContext = HttpContext.Current
                If currentContext IsNot Nothing Then
                    Dim browser As HttpBrowserCapabilities = currentContext.Request.Browser
                    For Each objTypeAdapter As KeyValuePair(Of Type, ControlAdapterSettings) In Me.AdaptersDictionary
                        If objTypeAdapter.Value.IsEnabled Then
                            If Not browser.Adapters.Contains(objTypeAdapter.Key.AssemblyQualifiedName) Then
                                SyncLock browser.Adapters
                                    If Not browser.Adapters.Contains(objTypeAdapter.Key.AssemblyQualifiedName) Then
                                        browser.Adapters.Add(objTypeAdapter.Key.AssemblyQualifiedName, objTypeAdapter.Value.ResolvedAdapterControlType.AssemblyQualifiedName)
                                    End If
                                End SyncLock
                            End If
                        Else
                            If browser.Adapters.Contains(objTypeAdapter.Key.AssemblyQualifiedName) _
                                AndAlso DirectCast(browser.Adapters(objTypeAdapter.Key.AssemblyQualifiedName), String) = objTypeAdapter.Value.ResolvedAdapterControlType.AssemblyQualifiedName Then
                                browser.Adapters.Remove(objTypeAdapter.Key.AssemblyQualifiedName)
                            End If
                        End If
                    Next
                    Return browser.Adapters.Count
                End If
            End If
            Return 0
        End Function

        <ActionButton(IconName.Magic, IconOptions.Normal)> _
        Public Sub RegisterAdapters(ape As Aricie.DNN.UI.WebControls.AriciePropertyEditorControl)
            PortalKeeperModule.RegisterAdapters()
            ape.ItemChanged = True
            ape.DisplayLocalizedMessage("AdaptersRegistered.Message", ModuleMessage.ModuleMessageType.GreenSuccess)
        End Sub


    End Class
End NameSpace
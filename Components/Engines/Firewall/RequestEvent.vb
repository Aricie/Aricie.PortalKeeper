﻿Namespace Aricie.DNN.Modules.PortalKeeper

   

    Public Enum RequestEvent
        [Default]
        BeginRequest
        BeginRequestAfterDNN
        AuthenticateRequest
        PostMapRequestHandler
        PreRequestHandlerExecute
        PagePreInit
        PageInit
        PageLoad
        PagePreRender
        PagePreRenderComplete
        PostRequestHandlerExecute
        ReleaseRequestState
        EndRequest
    End Enum
End Namespace
﻿
Imports LazyFramework.CQRS.Command

Namespace EventHandling
    Public Interface IAmAnEvent
        Inherits IAmAnAction

        ReadOnly Property RunAsync As Boolean
        Property CommandSource As IAmACommand
    End Interface
End Namespace
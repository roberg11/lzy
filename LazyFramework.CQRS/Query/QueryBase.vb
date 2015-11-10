﻿Imports System.Security.Principal
Imports LazyFramework.CQRS.ExecutionProfile

Namespace Query
    Public MustInherit Class QueryBase
        Inherits ActionBase
        Implements IAmAQuery
        
        Public Overrides Function IsAvailable() As Boolean
            Return True
        End Function

        Public Overrides Function IsAvailable(profile As IExecutionProfile) As Boolean
            Return True
        End Function

        Public Overrides Function IsAvailable(profile As IExecutionProfile, o As Object) As Boolean
            Return True
        End Function
    End Class


    ''' <summary>
    ''' Queries which result in a given entity type or list of entity type
    ''' Queries should never resolve the entity. That is always done through the handler. 
    ''' </summary>
    ''' <typeparam name="TResultEntity"></typeparam>
    ''' <remarks></remarks>
    Public MustInherit Class QueryBase(Of TResultEntity)
        Inherits QueryBase

        Public Overrides Function IsAvailable(profile As IExecutionProfile, o As Object) As Boolean
            Return IsActionAvailable(profile, CType(o, TResultEntity))
        End Function
        Public Overrides Function IsAvailable(profile As IExecutionProfile) As Boolean
            Return IsActionAvailable(profile)
        End Function



        Public Overridable Function IsActionAvailable(profile As IExecutionProfile) As Boolean
            Return True
        End Function
        Public Overridable Function IsActionAvailable(profile As IExecutionProfile, entity As TResultEntity) As Boolean
            Return True
        End Function

    End Class


End Namespace
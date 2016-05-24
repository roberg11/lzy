﻿
Imports LazyFramework.CQRS.Dto
Imports System.Linq
Imports LazyFramework.CQRS.ExecutionProfile
Imports LazyFramework.CQRS.Security

Namespace Transform
    Public Class Handling

        Public Shared Function TransformResult(profile As IExecutionProfile, ByVal action As IAmAnAction, ByVal result As Object, Optional ByVal transformer As ITransformEntityToDto = Nothing) As Object
            Dim transformerFactory As ITransformerFactory = EntityTransformerProvider.GetFactory(action)

            'Hmmmm skal vi ha logikk her som sjekker om det er noe factory, og hvis det ikke er det bare returnere det den fikk inn. 
            'Egentlig er det jo bare commands som trenger dette. Queries bør jo gjøre dette selv.. Kanskje. 

            If TypeOf result Is IList Then
                Dim ret As New Concurrent.ConcurrentQueue(Of Object)
                Dim res As Object


                If Not transformerFactory.RunAsParallel OrElse Setup.ChickenMode Then
                    For Each e In CType(result, IList)
                        res = TransformAndAddAction(profile, action, If(transformer Is Nothing, transformerFactory.GetTransformer(action, e), transformer), e)
                        If res IsNot Nothing Then
                            ret.Enqueue(res)
                        End If
                    Next
                    Return ret.ToList
                Else
                    'Dim user = profile.User  'Have to copy this from outside of the loop
                    'Dim s = profile.Storage
                    'Dim cm = profile.ChickenMode
                    Dim Errors As New Concurrent.ConcurrentBag(Of Exception)

                    CType(result, IList).
                        Cast(Of Object).
                        AsParallel.ForAll(Sub(o As Object)
                                              Try
                                                  Dim temp = TransformAndAddAction(profile, action, If(transformer Is Nothing, transformerFactory.GetTransformer(action, o), transformer), o)
                                                  If temp IsNot Nothing Then
                                                      ret.Enqueue(temp)
                                                  End If
                                              Catch ex As Exception
                                                  Errors.Add(ex)
                                              End Try
                                          End Sub)

                    If Errors.Count > 0 Then
                        Throw Errors(0)
                    End If

                    Dim retList = ret.ToList
                    If transformerFactory.SortingFunc(action) IsNot Nothing Then
                        retList.Sort(transformerFactory.SortingFunc(action))
                    Else
                        If transformer IsNot Nothing AndAlso TypeOf (transformer) Is ISortingFunction AndAlso CType(transformer, ISortingFunction).SortingFunc(action) IsNot Nothing Then
                            retList.Sort(CType(transformer, ISortingFunction).SortingFunc(action))
                        End If
                    End If



                    Return retList
                End If
            Else
                Return TransformAndAddAction(profile, action, If(transformer Is Nothing, transformerFactory.GetTransformer(action, result), transformer), result)
            End If
        End Function

        Public Shared Function TransformAndAddAction(profile As IExecutionProfile, ByVal action As IAmAnAction, ByVal transformer As ITransformEntityToDto, e As Object) As Object
            Dim securityContext As Object
            If transformer Is Nothing Then Return Nothing
            If TypeOf (e) Is IProvideSecurityContext Then
                securityContext = DirectCast(e, IProvideSecurityContext).Context
            Else
                securityContext = e
            End If

            If Not Setup.ActionSecurity.EntityIsAvailableForUser(profile, action, securityContext) Then Return Nothing

            Dim transformEntity As Object = transformer.TransformEntity(e)
            If transformEntity Is Nothing Then Return Nothing

            If TypeOf (transformEntity) Is ISupportActionList Then
                CType(transformEntity, ISupportActionList).Actions.AddRange(ActionSecurity.Current.GetActionList(profile, action, e))
            End If
            If TypeOf transformEntity Is ActionContext.ActionContext Then

            End If
            Return transformEntity
        End Function
        Public Shared Function TransformAndAddAction(profile As IExecutionProfile, ByVal action As IAmAnAction, ByVal transformer As ITransformEntityToDto, e As IEnumerable) As IEnumerable(Of Object)
            Dim ret = New List(Of Object)
            For Each res In e
                Dim transRes = TransformAndAddAction(profile, action, transformer, res)
                If transRes IsNot Nothing Then
                    ret.Add(transRes)
                End If
            Next
            Return ret
        End Function
    End Class
End Namespace



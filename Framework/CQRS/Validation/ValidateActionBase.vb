﻿Namespace CQRS.Validation
    Public MustInherit Class ValidateActionBase(Of TAction As IAmAnAction)
        Implements IValidateAction

        Public Overridable ReadOnly Property DetailedExceptionInfo As Boolean = False

        Friend Sub InternalValidate(action As IAmAnAction) Implements IValidateAction.InternalValidate
            '    ValidatAction(CType(action, TAction))
            Dim val As New ValidationException
            Dim exList As New Dictionary(Of String, Exception)


            Dim [getType] As Type = Me.GetType

            For Each p In [getType].GetMethods(Reflection.BindingFlags.DeclaredOnly Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance)
                Try
                    If p.GetParameters.Count = 1 AndAlso p.GetParameters(0).ParameterType.IsAssignableFrom(action.GetType) Then
                        p.Invoke(Me, {action})
                    End If
                Catch ex As Exception
                    exList.Add(p.Name, ex.InnerException)
                End Try
            Next

            If exList.Any Then
                If Not DetailedExceptionInfo Then
                    val.ExceptionList = exList
                    Throw val
                Else
                    Dim ex As New DetailedValidationException(exList)
                    Throw ex
                End If

            End If

        End Sub

    End Class
End Namespace

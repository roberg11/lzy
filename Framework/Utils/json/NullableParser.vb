﻿Namespace Utils.Json
    Friend Class NullableParser
        Inherits Builder

        Public Overrides Function Parse(nextChar As IReader, t As Type) As Object
            TokenAcceptors.WhiteSpace(nextChar)

            Dim v As String = nextChar.BufferPeek()
            If v = "N" Or v = "n" Then 'Guess this is a NULL
                TokenAcceptors.BufferLegalCharacters(nextChar, "nulNUL")
                nextChar.ClearBuffer()
                Return Nothing
            End If

            Dim tParser = t.GetGenericArguments(0)
            If TokenAcceptors.TypeParserMapper.ContainsKey(tParser) Then
                Return TokenAcceptors.TypeParserMapper(tParser).Parse(nextChar, tParser)
            Else
                Return Reader.StringToObject(nextChar, tParser)
            End If

        End Function
    End Class
End Namespace

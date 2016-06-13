Option Explicit

Sub RunTrans(ByVal conn As IMonetra)
        Dim retval As Long
        Dim id As Integer
        Dim i As Long
        Dim keys() As String
        Dim num_keys As Long
        Dim output As String

        id = conn.TransNew
        conn.TransKeyVal id, "username", "test_ecomm:public"
        conn.TransKeyVal id, "password", "publ1ct3st"
        conn.TransKeyVal id, "action", "sale"
        conn.TransKeyVal id, "account", "4012888888881881"
        conn.TransKeyVal id, "expdate", "0520"
        conn.TransKeyVal id, "amount", "12.00"
        conn.TransKeyVal id, "zip", "32606"
        If conn.TransSend(id) = False Then
                MsgBox ("Failed to send trans" & conn.ConnectionError())
                Exit Sub
        End If

        output = ""
        retval = conn.ReturnStatus(id)
        If Not retval = conn.SUCCESS Then
                output = "Bad return status: " & retval & Chr(13) & Chr(10)
        End If
        output = output & "code: " & conn.ResponseParam(id, "code") & Chr(13) & Chr(10)
        output = output & "verbiage: " & conn.ResponseParam(id, "verbiage") & Chr(13) & Chr(10)

        keys = conn.ResponseKeys(id)
        num_keys = UBound(keys)+1

        output = output & "All " & num_keys & " response parameters:" & Chr(13) & Chr(10)

        For i = 0 To num_keys - 1
                output = output & keys(i) & "=" & conn.ResponseParam(id, keys(i)) & Chr(13) & Chr(10)
        Next
        conn.DeleteTrans id

        MsgBox (output)
End Sub

Sub RunReport(ByVal conn As IMonetra)
        Dim id As Integer
        Dim retval As Long
        Dim rows As Long
        Dim cols As Long
        Dim i As Long
        Dim j As Long
        Dim line As String
        Dim output As String

        id = conn.TransNew()
        conn.TransKeyVal id, "username", "test_ecomm:public"
        conn.TransKeyVal id, "password", "publ1ct3st"
        conn.TransKeyVal id, "action", "admin"
        conn.TransKeyVal id, "admin", "gut"
        If conn.TransSend(id) = False Then
                MsgBox ("Failed to send trans: " & conn.ConnectionError())
                Exit Sub
        End If

        output = ""

        retval = conn.ReturnStatus(id)
        If Not retval = conn.SUCCESS Then
                output = "Bad return status: " & retval
        Exit Sub
        End If
        conn.ParseCommaDelimited id

        rows = conn.NumRows(id)
        cols = conn.NumColumns(id)

        output = output & "Report Data (" & rows & " rows, " & cols & " cols):" & Chr(13) & Chr(10)

        line = ""
        For i = 0 To cols - 1
                If Not i = 0 Then
                        line = line + "|"
                End If
                line = line + conn.GetHeader(id, i)
        Next
        output = output & line & Chr(13) & Chr(10)

        For i = 0 To rows - 1
                line = ""
                For j = 0 To cols - 1
                        If Not j = 0 Then
                                line = line + "|"
                        End If
                        line = line & conn.GetCellbyNum(id, j, i)
                Next
                output = output & line & Chr(13) & Chr(10)
        Next

        conn.DeleteTrans id
        MsgBox (output)
End Sub

Sub Main()
    Dim conn As IMonetra
    Dim error As Long
    
    Set conn = New Monetra
    
    conn.SetSSL "testbox.monetra.com", 8665
    conn.SetBlocking True
    
    If conn.Connect() = False Then
            MsgBox ("Connect() failed" & conn.ConnectionError())
            Exit Sub
    End If

    RunTrans conn
    RunReport conn
    conn.DestroyConn        
End Sub


Option Explicit On
Option Strict On
Imports System
Imports System.String
Imports libmonetra

Module Module1
    Sub RunTrans(ByVal conn As IntPtr)
        Dim retval As Integer
        Dim id As IntPtr
        Dim i As Integer
        Dim key As String
        Dim num_keys As Integer
        Dim keys As IntPtr
        Console.WriteLine("Running transaction....")
        id = Monetra.M_TransNew(conn)
        Monetra.M_TransKeyVal(conn, id, "username", "test_ecomm:public")
        Monetra.M_TransKeyVal(conn, id, "password", "publ1ct3st")
        Monetra.M_TransKeyVal(conn, id, "action", "sale")
        Monetra.M_TransKeyVal(conn, id, "account", "4012888888881881")
        Monetra.M_TransKeyVal(conn, id, "expdate", "0512")
        Monetra.M_TransKeyVal(conn, id, "amount", "12.00")
        If Monetra.M_TransSend(conn, id) = 0 Then
            Console.WriteLine("Failed to send trans")
            Return
        End If
        retval = Monetra.M_ReturnStatus(conn, id)
        If retval <> Monetra.M_SUCCESS Then
            Console.WriteLine("Bad return status: " + retval.ToString())
        End If
        Console.WriteLine("code: " + Monetra.M_ResponseParam(conn, id, "code"))
        Console.WriteLine("verbiage: " + Monetra.M_ResponseParam(conn, id, "verbiage"))
        keys = Monetra.M_ResponseKeys(conn, id, num_keys)
        Console.WriteLine("All " + num_keys.ToString() + " response parameters:")
        For i = 0 To num_keys - 1
            key = Monetra.M_ResponseKeys_index(keys, num_keys, i)
            Console.WriteLine(key + "=" + Monetra.M_ResponseParam(conn, id, key))
        Next
        Monetra.M_FreeResponseKeys(keys, num_keys)
        Monetra.M_DeleteTrans(conn, id)
        Console.WriteLine("Transaction Done")
    End Sub
    Sub RunReport(ByVal conn As IntPtr)
        Dim id As IntPtr
        Dim retval As Integer
        Dim rows As Integer
        Dim cols As Integer
        Dim i As Integer
        Dim j As Integer
        Dim line As String
        Console.WriteLine("Running report....")
        id = Monetra.M_TransNew(conn)
        Monetra.M_TransKeyVal(conn, id, "username", "test_ecomm:public")
        Monetra.M_TransKeyVal(conn, id, "password", "publ1ct3st")
        Monetra.M_TransKeyVal(conn, id, "action", "admin")
        Monetra.M_TransKeyVal(conn, id, "admin", "gut")
        If Monetra.M_TransSend(conn, id) = 0 Then
            Console.WriteLine("Failed to send trans")
            Return
        End If
        retval = Monetra.M_ReturnStatus(conn, id)
        If retval <> Monetra.M_SUCCESS Then
            Console.WriteLine("Bad return status: " + retval.ToString())
        End If
        Monetra.M_ParseCommaDelimited(conn, id)
        rows = Monetra.M_NumRows(conn, id)
        cols = Monetra.M_NumColumns(conn, id)
        Console.WriteLine("Report Data (" + rows.ToString() + " rows, " + cols.ToString() + _
        " cols):")
        line = ""
        For i = 0 To cols - 1
            If Not i = 0 Then
                line = line + "|"
            End If
            line = line + Monetra.M_GetHeader(conn, id, i)
        Next
        Console.WriteLine(line)
        For i = 0 To rows - 1
            line = ""
            For j = 0 To cols - 1
                If Not j = 0 Then
                    line = line + "|"
                End If
                line = line + Monetra.M_GetCellbyNum(conn, id, j, i)
            Next
            Console.WriteLine(line)
        Next
        Monetra.M_DeleteTrans(conn, id)
        Console.WriteLine("Report Done")
    End Sub
    Public Sub Main()
        Dim conn As IntPtr
        Monetra.M_InitEngine("")
        Monetra.M_InitConn(conn)
        Monetra.M_SetSSL(conn, "testbox.monetra.com", 8665)
        Monetra.M_SetBlocking(conn, 1)
        If Monetra.M_Connect(conn) = 0 Then
            Console.WriteLine("M_Connect() failed" + Monetra.M_ConnectionError(conn))
            Return
        End If
        RunTrans(conn)
        RunReport(conn)
        Monetra.M_DestroyConn(conn)
        Monetra.M_DestroyEngine()
        Return
    End Sub
End Module


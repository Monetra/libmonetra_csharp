using System;
using System.Text;
using System.IO;
using System.Threading;
using libmonetra;

class MonetraTest {
	private const string host     = "testbox.monetra.com";
	private const int port        = 8665;
	private const int method      = 1; /* 0 = IP, 1 = SSL */
	private const string username = "test_ecomm:public";
	private const string password = "publ1ct3st";

	static void traditionalapi()
	{
		Monetra.M_CONN conn = Monetra.M_InitConn();
		if (method == 1) {
			Monetra.M_SetSSL(conn, host, port);
			Monetra.M_VerifySSLCert(conn, false);
		} else {
			Monetra.M_SetIP(conn, host, port);
		}
		if (!Monetra.M_Connect(conn)) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(conn));
			return;
		}
		Console.WriteLine("Connected, Sending Sale TXN...");
		Monetra.M_SetBlocking(conn, true);
		int id;

		id = Monetra.M_TransNew(conn);
		Monetra.M_TransKeyVal(conn, id, "username", username);
		Monetra.M_TransKeyVal(conn, id, "password", password);
		Monetra.M_TransKeyVal(conn, id, "action", "sale");
		Monetra.M_TransKeyVal(conn, id, "account", "4012888888881881");
		Monetra.M_TransKeyVal(conn, id, "expdate", "0525");
		Monetra.M_TransKeyVal(conn, id, "amount", "12.00");
		Monetra.M_TransKeyVal(conn, id, "zip", "32606");
		Monetra.M_TransKeyVal(conn, id, "comments", "Multiline Comment \"Test\"\nTest Me");
		if (!Monetra.M_TransSend(conn, id)) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(conn));
			return;
		}

		Console.WriteLine("Response:");
		string[] responsekeys = Monetra.M_ResponseKeys(conn, id);
		for (int i = 0; i < responsekeys.Length; i++) {
			Console.WriteLine(responsekeys[i] + " : " + Monetra.M_ResponseParam(conn, id, responsekeys[i]));
		}
		Monetra.M_DeleteTrans(conn, id);

		Console.WriteLine("Sending Unsettled Report Request...");
		id = Monetra.M_TransNew(conn);
		Monetra.M_TransKeyVal(conn, id, "username", username);
		Monetra.M_TransKeyVal(conn, id, "password", password);
		Monetra.M_TransKeyVal(conn, id, "action", "admin");
		Monetra.M_TransKeyVal(conn, id, "admin", "GUT");
		if (!Monetra.M_TransSend(conn, id)) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(conn));
			return;
		}
		if (Monetra.M_ReturnStatus(conn, id) != Monetra.M_SUCCESS) {
			Console.WriteLine("GUT failed: " + Monetra.M_ResponseParam(conn, id, "verbiage"));
		} else {
			Console.WriteLine("Response:");
			Monetra.M_ParseCommaDelimited(conn, id);
			int rows = Monetra.M_NumRows(conn, id);
			int columns = Monetra.M_NumColumns(conn, id);
			Console.WriteLine("Rows: " + rows.ToString() + " Cols: " + columns.ToString());
			
			for (int i=0; i<columns; i++) {
				if (i != 0)
					Console.Write("|");
				Console.Write(Monetra.M_GetHeader(conn, id, i));
			}
			Console.WriteLine("");
			for (int i=0; i<rows; i++) {
				for (int j=0; j<columns; j++) {
					if (j != 0)
						Console.Write("|");
					Console.Write(Monetra.M_GetCellByNum(conn, id, j, i));
				}
				Console.WriteLine("");
			}
			Console.WriteLine("");
		}
		Monetra.M_DeleteTrans(conn, id);
		Console.WriteLine("Disconnecting...");
		Monetra.M_DestroyConn(conn);
	}

	static void classapi()
	{
		Monetra conn = new Monetra();
		if (method == 1) {
			conn.SetSSL(host, port);
			conn.VerifySSLCert(false);
		} else {
			conn.SetIP(host, port);
		}
		if (!conn.Connect()) {
			Console.WriteLine("Connection Failed: " + conn.ConnectionError());
			return;
		}
		Console.WriteLine("Connected, Sending Sale TXN...");

		conn.SetBlocking(true);
		int id;
		
		id = conn.TransNew();
		conn.TransKeyVal(id, "username", username);
		conn.TransKeyVal(id, "password", password);
		conn.TransKeyVal(id, "action", "sale");
		conn.TransKeyVal(id, "account", "4012888888881881");
		conn.TransKeyVal(id, "expdate", "0525");
		conn.TransKeyVal(id, "amount", "12.00");
		conn.TransKeyVal(id, "zip", "32606");
		conn.TransKeyVal(id, "comments", "Multiline Comment \"Test\"\nTest Me");
		if (!conn.TransSend(id)) {
			Console.WriteLine("Connection Failed: " + conn.ConnectionError());
			return;
		}
		Console.WriteLine("Response:");
		string[] responsekeys = conn.ResponseKeys(id);
		for (int i = 0; i < responsekeys.Length; i++) {
			Console.WriteLine(responsekeys[i] + " : " + conn.ResponseParam(id, responsekeys[i]));
		}
		conn.DeleteTrans(id);

		Console.WriteLine("Sending Unsettled Report Request...");
		id = conn.TransNew();
		conn.TransKeyVal(id, "username", username);
		conn.TransKeyVal(id, "password", password);
		conn.TransKeyVal(id, "action", "admin");
		conn.TransKeyVal(id, "admin", "GUT");
		if (!conn.TransSend(id)) {
			Console.WriteLine("Connection Failed: " + conn.ConnectionError());
			return;
		}
		if (conn.ReturnStatus(id) != Monetra.M_SUCCESS) {
			Console.WriteLine("GUT failed: " + conn.ResponseParam(id, "verbiage"));
		} else {
			Console.WriteLine("Response:");
			conn.ParseCommaDelimited(id);
			int rows = conn.NumRows(id);
			int columns = conn.NumColumns(id);
			Console.WriteLine("Rows: " + rows.ToString() + " Cols: " + columns.ToString());

			for (int i=0; i<columns; i++) {
				if (i != 0)
					Console.Write("|");
				Console.Write(conn.GetHeader(id, i));
			}
			Console.WriteLine("");
			for (int i=0; i<rows; i++) {
				for (int j=0; j<columns; j++) {
					if (j != 0)
						Console.Write("|");
					Console.Write(conn.GetCellByNum(id, j, i));
				}
				Console.WriteLine("");
			}
			Console.WriteLine("");
		}
		conn.DeleteTrans(id);
		Console.WriteLine("Disconnecting...");
	}

	static unsafe void unsafeapi(bool do_report)
	{
		IntPtr conn;
		Monetra.M_InitConn(&conn);
		if (method == 1) {
			Monetra.M_SetSSL(&conn, host, port);
			Monetra.M_VerifySSLCert(&conn, 0);
		} else {
			Monetra.M_SetIP(&conn, host, port);
		}
		if (Monetra.M_Connect(&conn) == 0) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(&conn));
			return;
		}
		Console.WriteLine("Connected, Sending Sale TXN...");
		Monetra.M_SetBlocking(&conn, 1);
		IntPtr id;

		id = Monetra.M_TransNew(&conn);
		Monetra.M_TransKeyVal(&conn, id, "username", username);
		Monetra.M_TransKeyVal(&conn, id, "password", password);
		Monetra.M_TransKeyVal(&conn, id, "action", "sale");
		Monetra.M_TransKeyVal(&conn, id, "account", "4012888888881881");
		Monetra.M_TransKeyVal(&conn, id, "expdate", "0525");
		Monetra.M_TransKeyVal(&conn, id, "amount", "12.00");
		Monetra.M_TransKeyVal(&conn, id, "zip", "32606");
		Monetra.M_TransKeyVal(&conn, id, "comments", "Multiline Comment \"Test\"\nTest Me");
		if (Monetra.M_TransSend(&conn, id) == 0) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(&conn));
			return;
		}

		Console.WriteLine("Response:");
		int num_responsekeys = 0;
		IntPtr responsekeys = Monetra.M_ResponseKeys(&conn, id, &num_responsekeys);
		for (int i = 0; i < num_responsekeys; i++) {
			string key = Monetra.M_ResponseKeys_index(responsekeys, num_responsekeys, i);
			Console.WriteLine(key + " : " + Monetra.M_ResponseParam(&conn, id, key));
		}
		Monetra.M_FreeResponseKeys(responsekeys, num_responsekeys);

		Monetra.M_DeleteTrans(&conn, id);

		if (do_report) {
			Console.WriteLine("Sending Unsettled Report Request...");
			id = Monetra.M_TransNew(&conn);
			Monetra.M_TransKeyVal(&conn, id, "username", username);
			Monetra.M_TransKeyVal(&conn, id, "password", password);
			Monetra.M_TransKeyVal(&conn, id, "action", "admin");
			Monetra.M_TransKeyVal(&conn, id, "admin", "GUT");
			if (Monetra.M_TransSend(&conn, id) == 0) {
				Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(&conn));
				return;
			}
			if (Monetra.M_ReturnStatus(&conn, id) != Monetra.M_SUCCESS) {
				Console.WriteLine("GUT failed: " + Monetra.M_ResponseParam(&conn, id, "verbiage"));
			} else {
				Console.WriteLine("Response:");
				Monetra.M_ParseCommaDelimited(&conn, id);
				int rows = Monetra.M_NumRows(&conn, id);
				int columns = Monetra.M_NumColumns(&conn, id);
				Console.WriteLine("Rows: " + rows.ToString() + " Cols: " + columns.ToString());
			
				for (int i=0; i<columns; i++) {
					if (i != 0)
						Console.Write("|");
					Console.Write(Monetra.M_GetHeader(&conn, id, i));
				}
				Console.WriteLine("");
				for (int i=0; i<rows; i++) {
					for (int j=0; j<columns; j++) {
						if (j != 0)
							Console.Write("|");
						Console.Write(Monetra.M_GetCellByNum(&conn, id, j, i));
					}
					Console.WriteLine("");
				}
				Console.WriteLine("");
			}
			Monetra.M_DeleteTrans(&conn, id);
		}
		Console.WriteLine("Disconnecting...");
		Monetra.M_DestroyConn(&conn);
	}
	
	static unsafe void unsafeapiref(bool do_report)
	{
		IntPtr conn = (IntPtr)0;
		Monetra.M_InitConn(ref conn);
		if (method == 1) {
			Monetra.M_SetSSL(ref conn, host, port);
			Monetra.M_VerifySSLCert(ref conn, 0);
		} else {
			Monetra.M_SetIP(ref conn, host, port);
		}
		if (Monetra.M_Connect(ref conn) == 0) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(ref conn));
			return;
		}
		Console.WriteLine("Connected, Sending Sale TXN...");
		Monetra.M_SetBlocking(ref conn, 1);
		IntPtr id;

		id = Monetra.M_TransNew(ref conn);
		Monetra.M_TransKeyVal(ref conn, id, "username", username);
		Monetra.M_TransKeyVal(ref conn, id, "password", password);
		Monetra.M_TransKeyVal(ref conn, id, "action", "sale");
		Monetra.M_TransKeyVal(ref conn, id, "account", "4012888888881881");
		Monetra.M_TransKeyVal(ref conn, id, "expdate", "0525");
		Monetra.M_TransKeyVal(ref conn, id, "amount", "12.00");
		Monetra.M_TransKeyVal(ref conn, id, "zip", "32606");
		Monetra.M_TransKeyVal(ref conn, id, "comments", "Multiline Comment \"Test\"\nTest Me");
		if (Monetra.M_TransSend(ref conn, id) == 0) {
			Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(ref conn));
			return;
		}

		Console.WriteLine("Response:");
		int num_responsekeys = 0;
		IntPtr responsekeys = Monetra.M_ResponseKeys(ref conn, id, ref num_responsekeys);
		for (int i = 0; i < num_responsekeys; i++) {
			string key = Monetra.M_ResponseKeys_index(responsekeys, num_responsekeys, i);
			Console.WriteLine(key + " : " + Monetra.M_ResponseParam(ref conn, id, key));
		}
		Monetra.M_FreeResponseKeys(responsekeys, num_responsekeys);

		Monetra.M_DeleteTrans(ref conn, id);

		if (do_report) {
			Console.WriteLine("Sending Unsettled Report Request...");
			id = Monetra.M_TransNew(ref conn);
			Monetra.M_TransKeyVal(ref conn, id, "username", username);
			Monetra.M_TransKeyVal(ref conn, id, "password", password);
			Monetra.M_TransKeyVal(ref conn, id, "action", "admin");
			Monetra.M_TransKeyVal(ref conn, id, "admin", "GUT");
			if (Monetra.M_TransSend(ref conn, id) == 0) {
				Console.WriteLine("Connection Failed: " + Monetra.M_ConnectionError(ref conn));
				return;
			}
			if (Monetra.M_ReturnStatus(ref conn, id) != Monetra.M_SUCCESS) {
				Console.WriteLine("GUT failed: " + Monetra.M_ResponseParam(ref conn, id, "verbiage"));
			} else {
				Console.WriteLine("Response:");
				Monetra.M_ParseCommaDelimited(ref conn, id);
				int rows = Monetra.M_NumRows(ref conn, id);
				int columns = Monetra.M_NumColumns(ref conn, id);
				Console.WriteLine("Rows: " + rows.ToString() + " Cols: " + columns.ToString());
			
				for (int i=0; i<columns; i++) {
					if (i != 0)
						Console.Write("|");
					Console.Write(Monetra.M_GetHeader(ref conn, id, i));
				}
				Console.WriteLine("");
				for (int i=0; i<rows; i++) {
					for (int j=0; j<columns; j++) {
						if (j != 0)
							Console.Write("|");
						Console.Write(Monetra.M_GetCellByNum(ref conn, id, j, i));
					}
					Console.WriteLine("");
				}
				Console.WriteLine("");
			}
			Monetra.M_DeleteTrans(ref conn, id);
		}
		Console.WriteLine("Disconnecting...");
		Monetra.M_DestroyConn(ref conn);
	}

	static void disconnect_test()
	{
		Monetra conn = new Monetra();
		if (method == 1) {
			conn.SetSSL(host, port);
			conn.VerifySSLCert(false);
		} else {
			conn.SetIP(host, port);
		}
		if (!conn.Connect()) {
			Console.WriteLine("Connection Failed: " + conn.ConnectionError());
			return;
		}
		Console.WriteLine("Connected, you have 20s either shut down monetra or kill the link to monetra");
		conn.SetBlocking(true);

		for (int i=20; i >= 0; i--) {
			Console.Write(i.ToString() + "...");
			System.Threading.Thread.Sleep(1000);

			/* Check in realtime for a disconnect event */
			if (!conn.Monitor()) {
				Console.WriteLine("");
				Console.WriteLine("Disconnect detected: " + conn.ConnectionError());
				return;
			}
		}
		Console.WriteLine("");

		int id = conn.TransNew();
		conn.TransKeyVal(id, "username", username);
		conn.TransKeyVal(id, "password", password);
		conn.TransKeyVal(id, "action", "chkpwd");
		if (!conn.TransSend(id)) {
			Console.WriteLine("Connection Failed: " + conn.ConnectionError());
			return;
		}

		Console.WriteLine(" * Got a response, looks like you didn't disconnect");
		conn.DeleteTrans(id);
		Console.WriteLine("Disconnecting...");
		conn.DestroyConn();
		conn = null;

	}

	static void threadproc()
	{
		/* Use unsafe API because it uses more calls that could be
		 * negatively affected by threads.  Better test. */
		Console.WriteLine("Enter Thread " + Thread.CurrentThread.Name);
		unsafeapi(false);
		Console.WriteLine("Exit Thread " + Thread.CurrentThread.Name);
	}
	
	static void thread_test()
	{
		int num_threads = 50;
		ThreadStart func_call = new ThreadStart(threadproc);
		Thread[] mythreads = new Thread[num_threads];
		
		Console.WriteLine("Spawning " + num_threads.ToString() + " threads to connect...");
		/* Spawn */
		for (int i=0; i<num_threads; i++) {
			mythreads[i] = new Thread(func_call);
			mythreads[i].Name = i.ToString(); 
		 	mythreads[i].Start();
		}
		Console.WriteLine("Threads spawned, waiting on completion...");		
		/* Wait on completion */
		for (int i=0; i<num_threads; i++) {
			mythreads[i].Join();
		}
		Console.WriteLine("All threads exited");
	}
	
	static void Main()
	{
		Console.WriteLine("Version: " + Monetra.version);
		Console.WriteLine("Using Class API");
		classapi();
		Console.WriteLine("Using Traditional API");
		traditionalapi();
		Console.WriteLine("Using Unsafe API");
		unsafeapi(false);
		Console.WriteLine("Using Unsafe API by Ref");
		unsafeapiref(false);
		Console.WriteLine("Running Thread Test...");
		thread_test();
		Console.WriteLine("Running disconnect test ...");
		disconnect_test();
		Console.WriteLine("Exiting...");
	}


};

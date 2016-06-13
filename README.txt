LIBMONETRA C# .NET v0.9.10

================
Revision History
================
 * 0.9.0 - Initial Public Release
 * 0.9.1 - Ensure CSV cells are actually trimmed before insertion
 * 0.9.2 - Add additional sanity checks around parsing code to prevent
           exceptions from being thrown due to Invalid Array Indexes.
           Some whitespace cleanups as well.
 * 0.9.3 - Add another 'unsafe' emulation for use with VB.Net
 * 0.9.4 - Add interface definition for legacy COM integrations and example
           code for VB.Net, and VB6.  Compiled DLL and MSI installer now
           provided in package.
 * 0.9.5 - Additional threadsafety locks
 * 0.9.6 - Remove dead waits, instead wait on async read handle.
           Fix CompleteAuthorizations().
 * 0.9.7 - Make object disposable.  Fix bug with VisualStudio not cleanly
           closing connections (even though this did not happen with Mono).
 * 0.9.8 - null-check the connection pointer to prevent null pointer 
           dereference exceptions.
 * 0.9.9 - additional exception handling to ignore exceptions thrown
           when shutting down a socket that is already closed.
 * 0.9.10- Add .Net 4.5 detection at build time to enable TLSv1.1 and
           TLSv1.2 support.

========
Overview
========
This library attempts to emulate the libmonetra C API as closely as possible.
It provides 3 different methods of integration:

  1) API similar to the libmonetra C-API with one notable difference, that M_InitConn()
     returns an M_CONN class rather than passing in an object to be initialized.

  2) A true class-based implementation where-as the 'conn' isn't passed back to
     any of the functions, as a reference to it is contained within the initialized
     class.  So you could call Monetra conn = new Monetra(); conn.SetIP("localhost", 8333);
     Note the class methods remove the M_ prefix from the function names.

  3) A P/Invoke 'unsafe' emulation.  This should be a drop-in replacement for
     existing implementations which currently use the P/Invoke methods to the libmonetra.dll

This library is designed to be Thread-Safe, but has not yet been extensively tested
as such.

This class can be used from any .Net language (C#, VB.Net, J#, etc)


=====
FILES
=====
 - libmonetra.cs  - This is the entire implementation of the class, including all
                    3 integration methods.  It is namespaced as 'libmonetra', and
                    implements the class 'Monetra'.
 - monetratest.cs - Internal test cases during development of the library.  Provides
                    a good example use of all 3 integration methods.


========
Building
========
 - Requires .Net 2.0 or higher for some SSL calls
 - Needs references to System and System.Net
 - If building using 'mono' use 'gmcs' rather than 'mcs'
 - If you want the P/Invoke 'unsafe' API emulation, uncomment the #define USE_UNSAFE_API below


============================================
Migration from P/Invoke libmonetra API notes
============================================
 * Recommended migration away from 'unsafe' API.  Please see alternative method as well.
   - The connection pointer is no longer an IntPtr but rather an M_CONN class.
   - The connection pointer is no longer passed by reference (&) because classes are
     automatically passed by reference.
   - The connection pointer is now returned from M_InitConn() rather than passed in to
     be initialized.
   - The identifier returned by M_TransNew() is now of type 'int' rather than 'IntPtr'
   - M_SetDropFile() always returns false as it is unimplemented.
   - M_InitEngine() and M_DestroyEngine()s are No-Ops, they do not need to be called, but
     exist for compatibility purposes.
   - M_ValidateIdentifier() is a no-op, we always scan the hash table for a transaction
     as we are not returning direct pointers.
   - M_ResponseKeys() returns a string array, so there is no implementation for 
     M_ResponseKeys_index() or M_FreeResponseKeys() as they are not necessary.
   - M_TransBinaryKeyVal() takes a byte array rather than a string as an argument and does not
     take a length argument.
   - M_GetBinaryCell() returns a byte array rather than a string, it also does not take
     an outlen parameter to store the result length as it is not necessary.
 * Alternative Migration from P/Invoke methods.
   - #define USE_UNSAFE_API below and compile this as an 'unsafe' library.
   - add 'using libmonetra;' to the top of your project and include this library in your project.
   - Remove the old P/Invoke class definitions from your project.
   - Note: it is still necessary to call M_DestroyConn(), M_FreeResponseKeys(), etc to 
     clear up system resources when using the 'unsafe' methods since the class references
     are stored in a global Hashtable that the reference must be cleared.


====
TODO
====
 - Unit test
 - The 'cafile' specified by M_SetSSL_CAfile() is not honored, the default system
   certificate store is used instead.
 - The client certificates as provided by M_SetSSL_Files() are not currently honored.


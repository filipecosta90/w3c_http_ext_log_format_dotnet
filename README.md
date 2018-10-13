# w3c_http_ext_log_format_dotnet


please refer to https://github.com/aspnet/Hosting/issues/1013#issuecomment-419950463

CommonLogMiddleware.cs and CountingStream.cs retrived from https://github.com/Tratcher/CommonLog.
The CommonLogMiddleware.cs file logs in rfc-939 format, therefore, changes have been made to log in the extended log format, compliant with w3c specifications.

# W3C http extende log fiedls
|	Field	|	Appears As	|	Description	|	W3C standard?	|
|	:---:	|	:---:	|	:---:	|	:---:	|
|	Date	|	date	|	The date that the activity occurred.	|	TRUE	|
|	Time	|	time	|	The time that the activity occurred.	|	TRUE	|
|	Client IP Address	|	c-ip	|	The IP address of the client that accessed your server.	|	TRUE	|
|	User Name	|	cs-username	|	The name of the authenticated user who accessed your server. This does not include anonymous users, who are represented by a hyphen (-).	|	TRUE	|
|	Service Name	|	s-sitename	|	The Internet service and instance number that was accessed by a client.	|	TRUE	|
|	Server Name	|	s-computername	|	The name of the server on which the log entry was generated.	|	TRUE	|
|	Server IP Address	|	s-ip	|	The IP address of the server on which the log entry was generated.	|	TRUE	|
|	Server Port	|	s-port	|	The port number the client is connected to.	|	TRUE	|
|	Method	|	cs-method	|	The action the client was trying to perform (for example, a GET method).	|	TRUE	|
|	URI Stem	|	cs-uri-stem	|	The resource accessed; for example, Default.htm.	|	TRUE	|
|	URI Query	|	cs-uri-query	|	The query, if any, the client was trying to perform.	|	TRUE	|
|	Protocol Status	|	sc-status	|	The status of the action, in HTTP or FTP terms.	|	TRUE	|
|	Win32® Status	|	sc-win32-status	|	The status of the action, in terms used by Microsoft Windows®.	|	TRUE	|
|	Bytes Sent	|	sc-bytes	|	The number of bytes sent by the server.	|	TRUE	|
|	Bytes Received	|	cs-bytes	|	The number of bytes received by the server.	|	TRUE	|
|	Time Taken	|	time-taken	|	The duration of time, in milliseconds, that the action consumed.	|	TRUE	|
|	Protocol Version	|	cs-version	|	The protocol (HTTP, FTP) version used by the client. For HTTP this will be either HTTP 1.0 or HTTP 1.1.	|	TRUE	|
|	Host	|	cs-host	|	Displays the content of the host header.	|	TRUE	|
|	User Agent	|	cs(User-Agent)	|	The browser used on the client.	|	TRUE	|
|	Cookie	|	cs(Cookie)	|	The content of the cookie sent or received, if any.	|	TRUE	|
|	Referrer	|	cs(Referer)	|	The previous site visited by the user. This site provided a link to the current site.	|	TRUE	|
|	X-Forwarded-For Header	|	X-Forwarded-For Header	|	actual client source IP address	|	FALSE	|

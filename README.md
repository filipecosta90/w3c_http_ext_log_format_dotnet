# w3c_http_ext_log_format_dotnet


please refer to https://github.com/aspnet/Hosting/issues/1013#issuecomment-419950463

CommonLogMiddleware.cs and CountingStream.cs retrived from https://github.com/Tratcher/CommonLog.
The CommonLogMiddleware.cs file logs in rfc-939 format, therefore, changes have been made to log in the extended log format, compliant with w3c specifications.
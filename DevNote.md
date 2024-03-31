## Q&A

### Why using Dictionary<string,object> parameters?

Because AOT doesn't support Reflecton.

### Why not using EF Core?

Because MiniAuth would like to support multiple .NET Framework.

### Why not using Dapper?

Clean dependency.
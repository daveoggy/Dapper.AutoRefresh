# Dapper.AutoRefresh
An Async-Await Integration of SqlDependency and Dapper

## Quickstart

Create a database called `TestDB` and enable service broker:

``` sql
ALTER DATABASE [TestDB] SET  ENABLE_BROKER 
```

Grant subscribe query notifications to current user - where DB_USER is the current user (```SELECT SUSER_NAME()```):

``` sql
GRANT SUBSCRIBE QUERY NOTIFICATIONS TO [DB_USER]
```

Create a table:

``` sql
USE [TestDB]
GO

/****** Object:  Table [dbo].[MyType]    Script Date: 2/13/2023 10:40:28 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MyType](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_MyType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
) WITH (
  PAD_INDEX = OFF, 
  STATISTICS_NORECOMPUTE = OFF, 
  IGNORE_DUP_KEY = OFF, 
  ALLOW_ROW_LOCKS = ON, 
  ALLOW_PAGE_LOCKS = ON, 
  OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```

Create a console app:

``` c#
internal class Program
{
    static async Task Main(string[] args)
    {
        var connectionSB = new SqlConnectionStringBuilder()
        {
            DataSource = "localhost",
            InitialCatalog = "TestDB",
            IntegratedSecurity = true
        };

        var sqlAutoRefresh = new SqlAutoRefresh<MyType>("SELECT Id, Name FROM dbo.MyType", connectionSB.ToString());

        do
        {
            var data = await sqlAutoRefresh.GetLatest();

            data.ToList().ForEach(Console.WriteLine);

        } while (true);
    }
}

internal class MyType
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{Id} => {Name}";
    }
}
```

Run some update or insert commands and watch the console write out the changes:

``` sql
INSERT INTO dbo.MyType VALUES (NEWID(), 'My Name')
```

``` sql
UPDATE dbo.MyType SET ID = NEWID()
```

Console output example:

```
236bc50d-e1be-4f71-9cc9-78b002242c01 => Test Name
Notification Info: Update
Notification source: Data
Notification type: Change
4952fe75-35a4-4e60-9c90-329617c51cf4 => Test Name

Notification Info: Insert
Notification source: Data
Notification type: Change
4952fe75-35a4-4e60-9c90-329617c51cf4 => Test Name
f4030c76-6c4a-47b3-a6a7-a66073f3ddaf => My Name
```

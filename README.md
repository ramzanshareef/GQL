# GraphQL API with ASP.NET Core - Setup Guide

## Project Setup Steps

### 1. Create a new Class Library project or solution
- Create a new Class Library for the Data Access Layer (DAL)

### 2. Create a SQL file and run the DB script in the DAL
**File:** `DB.sql`

```sql
USE master;
GO

IF DB_ID('GQLDb') IS NOT NULL
BEGIN
	DROP DATABASE GQLDb;
END
GO

CREATE DATABASE GQLDb;
GO

USE GQLDb;
GO

IF OBJECT_ID('dbo.Todos', 'U') IS NOT NULL DROP TABLE dbo.Todos;
GO

CREATE TABLE dbo.Todos (
	TodoId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
	Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Status BIT DEFAULT 0
);
GO

INSERT INTO dbo.Todos (UserId, Title, Description, Status)
VALUES
(1, 'Buy groceries', 'Milk, eggs, bread', 0),
(1, 'Complete GraphQL API', 'Finish queries and mutations', 1),
(1, 'Workout', 'Gym session for 1 hour', 1),
(2, 'Read book', 'Read 30 pages of a novel', 1),
(3, 'Deploy app', 'Deploy .NET app to Azure', 0);
GO

SELECT * FROM dbo.Todos;
GO
```

### 3. Install Microsoft Entity Framework Core packages
Install the following NuGet packages:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.EntityFrameworkCore.Design`

**Note:** Scaffold DbContext using the Package Manager Console. 

**Connection String Example:**
```
Server=(localdb)\MSSQLLocalDB;Initial Catalog=GQLDb;Integrated Security=True;Pooling=false
```

**Package Manager Command:**
```powershell
Scaffold-DbContext "Server=(localdb)\MSSQLLocalDB;Initial Catalog=GQLDb;Integrated Security=True;Pooling=false" Microsoft.EntityFrameworkCore.SqlServer -OutputDir "Models"
```

### 4. Replace the Connection String in the Context File
Update the `OnConfiguring` method in `GqldbContext.cs`:

```csharp
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DAL.Models;

public partial class GqldbContext : DbContext
{
    public GqldbContext()
    {
    }

    public GqldbContext(DbContextOptions<GqldbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Todo> Todos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(
                new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("GQLConnectionString")
            );


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.TodoId).HasName("PK__Todos__9586255256A803B0");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasDefaultValue(false);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

### 5. Update the DbContext File method as follows to use the secured connection string
Update `appsettings.json` in DAL:

```json
{
  "ConnectionStrings": {
    "GQLConnectionString": "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=GQLDb"
  }
}
```

### 5b. Todo Model
The `Todo.cs` model in the DAL/Models folder:

```csharp
using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Todo
{
    public int TodoId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool? Status { get; set; }
}
```

### 6. Create interfaces and repos for all the services
**Interface:** `ITodoRepository.cs`

```csharp
public interface ITodoRepository
{
    List<Todo> GetAllTodos();

    List<Todo> GetAllTodosOfAUser(int userId);

    Todo AddTodo(Todo todo);

    Todo EditTodo(int userId, Todo newTodo);

    bool DeleteTodo(int userId, Todo todo);
}
```

**Repository:** `TodoRepository.cs` (in Services folder)

```csharp
using DAL.Interfaces;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Services
{
    public class TodoRepository : ITodoRepository
    {
        private readonly GqldbContext dbContext;

        public TodoRepository(GqldbContext gqldbContext)
        {
            dbContext = gqldbContext;
        }

        public List<Todo> GetAllTodos()
        {
            return dbContext.Todos.ToList();
        }
        
        public List<Todo> GetAllTodosOfAUser(int userId)
        {
            return dbContext.Todos.Where(todo => todo.UserId == userId).ToList();
        }

        public Todo AddTodo(Todo todo)
        {
            try
            {
                dbContext.Todos.Add(todo);
                dbContext.SaveChanges();
                return todo;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public Todo EditTodo(int userId, Todo newTodo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(newTodo);

                if (newTodo.UserId != userId)
                {
                    throw new Exception("Authentication failed");
                }
                var updatedTodo = dbContext.Todos.FirstOrDefault(t => t.UserId == userId && t.TodoId == newTodo.TodoId);
                if (updatedTodo == null) throw new Exception("Todo not found");

                updatedTodo.Title = newTodo.Title;
                updatedTodo.Description = newTodo.Description;
                updatedTodo.Status = newTodo.Status;
                dbContext.SaveChanges();
                return updatedTodo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DeleteTodo(int userId, Todo todo)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(todo);

                if (todo.UserId != userId)
                {
                    throw new Exception("Authentication failed");
                }
                var existing = dbContext.Todos.FirstOrDefault(t => t.UserId == userId && t.TodoId == todo.TodoId);
                if (existing == null) return false;

                dbContext.Todos.Remove(existing);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
```

### 7. Create a new project or continue with ASP.NET Core Web API
Keep checked for HTTPS and API support.

Add the following lines to have the DB connection access and repository access from that layer in `Program.cs`:

```csharp
builder.Services.AddDbContext<GqldbContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("GQLConnectionString"))
);
builder.Services.AddTransient<ITodoRepository, TodoRepository>();
```

**Update API appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "GQLConnectionString": "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=GQLDb"
  }
}
```

### 8. Install the following dependencies
- **graphql** - Creates the UI which allows us to send requests directly from the browser with a single UI
  - In the `Program.cs` file add `app.UseGraphiQL("/graphql");` 
  - In `launchSettings.json` set `launchUrl` to "/graphql"
  - Also use `app.UseGraphQL<ISchema>();` (middleware part)

- **GraphQL.Server.Transports.AspNetCore** (middleware part to be linked with .NET)

### 9. Create folders for Types, Schemas, Queries, and Mutations

#### In Types folder
Create separate types for each object class as needed.

**TodoType.cs:**
```csharp
using DAL.Models;
using GraphQL.Types;

namespace API.Types
{
    public class TodoType: ObjectGraphType<Todo>
    {
        public TodoType()
        {
            Field(x => x.TodoId);
            Field(x => x.UserId);
            Field(x => x.Title);
            Field(x => x.Description);
            Field(x => x.Status);
        }
    }
}
```

**TodoInputType.cs:**
```csharp
using GraphQL.Types;

namespace API.Types
{
    public class TodoInputType: InputObjectGraphType
    {
        public TodoInputType()
        {
            Field<IntGraphType>("todoId");
            Field<IntGraphType>("userId");
            Field<StringGraphType>("title");
            Field<StringGraphType>("description");
            Field<BooleanGraphType>("status");
        }
    }
}
```

#### In Queries folder
Create different object queries or root query as required.

**TodoQuery.cs:**
```csharp
using API.Types;
using DAL.Interfaces;
using GraphQL;
using GraphQL.Types;

namespace API.Queries
{
    public class TodoQuery: ObjectGraphType
    {
        public TodoQuery(ITodoRepository todoRepository)
        {
            Field<ListGraphType<TodoType>>("alltodos")
                .Resolve(context => todoRepository.GetAllTodos());

            Field<ListGraphType<TodoType>>("alltodosofuser")
                .Arguments(
                    new QueryArguments(
                        new QueryArgument<IntGraphType>() { Name = "userId" }
                    )
                )
                .Resolve(
                    context => todoRepository.GetAllTodosOfAUser(context.GetArgument<int>("userId"))
                );
        }
    }
}
```

**RootQuery.cs:**
```csharp
using GraphQL.Types;

namespace API.Queries
{
    public class RootQuery: ObjectGraphType
    {
        public RootQuery()
        {
            Field<TodoQuery>("todoQuery").Resolve(context => new { });
        }
    }
}
```

#### In Mutations folder
Create different object mutations and/or root mutation.

**TodoMutation.cs:**
```csharp
using API.Types;
using DAL.Interfaces;
using DAL.Models;
using GraphQL;
using GraphQL.Types;

namespace API.Mutations
{
    public class TodoMutation: ObjectGraphType
    {
        public TodoMutation(ITodoRepository todoRepository)
        {
            Field<TodoType>("addTodo")
                .Arguments(new QueryArguments(new QueryArgument<TodoInputType> { Name = "todo" }))
                .Resolve(
                    context =>
                    {
                        return todoRepository.AddTodo(context.GetArgument<Todo>("todo"));
                    }
                );

            Field<TodoType>("updateTodo")
               .Arguments(new QueryArguments(new QueryArgument<IntGraphType> { Name = "userId" }, new QueryArgument<TodoInputType> { Name = "updatedTodo" }))
               .Resolve(
                   context =>
                   {
                       return todoRepository.EditTodo(context.GetArgument<int>("userId"), context.GetArgument<Todo>("updatedTodo"));
                   }
               );

            Field<BooleanGraphType>("deleteTodo")
               .Arguments(new QueryArguments(new QueryArgument<IntGraphType> { Name = "userId" }, new QueryArgument<TodoInputType> { Name = "todoToDelete" }))
               .Resolve(
                   context =>
                   {
                       return todoRepository.DeleteTodo(context.GetArgument<int>("userId"), context.GetArgument<Todo>("todoToDelete"));
                   }
               );
        }
    }
}
```

**RootMutation.cs:**
```csharp
using GraphQL.Types;

namespace API.Mutations
{
    public class RootMutation: ObjectGraphType
    {
        public RootMutation()
        {
            Field<TodoMutation>("todoMutation").Resolve(context => new { });
        }
    }
}
```

#### In Schemas folder
Create a root schema.

**RootSchema.cs:**
```csharp
using API.Mutations;
using API.Queries;
using GraphQL.Types;

namespace API.Schemas
{
    public class RootSchema: Schema
    {
        public RootSchema(IServiceProvider serviceProvider): base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<RootQuery>();
            Mutation = serviceProvider.GetRequiredService<RootMutation>();
        }
    }
}
```

### 10. Finally in Program.cs
Add all the dependencies which are all the types, all the queries, all the mutations, all the schemas and a schema to be mapped to JSON as well as follows:

**Complete Program.cs:**
```csharp
using API.Mutations;
using API.Queries;
using API.Schemas;
using API.Types;
using DAL.Interfaces;
using DAL.Models;
using DAL.Services;
using GraphiQl;
using GraphQL.Types;
using GraphQL;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<GqldbContext>(
                option => option.UseSqlServer(builder.Configuration.GetConnectionString("GQLConnectionString"))
            );
            builder.Services.AddTransient<ITodoRepository, TodoRepository>();

            builder.Services.AddTransient<TodoType>();
            builder.Services.AddTransient<TodoInputType>();

            builder.Services.AddTransient<TodoQuery>();
            builder.Services.AddTransient<RootQuery>();

            builder.Services.AddTransient<TodoMutation>();
            builder.Services.AddTransient<RootMutation>();

            builder.Services.AddTransient<ISchema, RootSchema>();

            builder.Services.AddGraphQL(b =>
                b.AddAutoSchema<ISchema>().AddSystemTextJson()
            );


            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseGraphiQl("/graphql");
            app.UseGraphQL<ISchema>();

            app.MapControllers();
            app.Run();
        }
    }
}
```

## Additional Notes

- Ensure all connection strings are properly configured
- Use dependency injection for repositories and services
- GraphiQL UI will be available at `/graphiql` endpoint
- Follow the folder structure for proper organization of Types, Queries, Mutations, and Schemas

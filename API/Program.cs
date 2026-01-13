
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

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

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

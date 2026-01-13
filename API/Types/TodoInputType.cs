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

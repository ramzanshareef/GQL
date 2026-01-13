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

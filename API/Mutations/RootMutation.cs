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

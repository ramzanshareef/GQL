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

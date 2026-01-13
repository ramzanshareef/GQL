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

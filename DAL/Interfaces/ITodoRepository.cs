using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ITodoRepository
    {
        List<Todo> GetAllTodos();

        List<Todo> GetAllTodosOfAUser(int userId);

        Todo AddTodo(Todo todo);

        Todo EditTodo(int userId, Todo newTodo);

        bool DeleteTodo(int userId, Todo todo);
    }
}

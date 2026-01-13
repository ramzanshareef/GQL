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

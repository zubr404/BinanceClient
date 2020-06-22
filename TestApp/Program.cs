using DataBaseWork;
using DataBaseWork.Models;
using DataBaseWork.Repositories;
using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var usersRepo = new UserRepository(new DataBaseContext());
            

            try
            {
                var user = usersRepo.Create(new User() { Login = "login" });
                usersRepo.Create(new User() { Login = "login1" });
                //usersRepo.Update(new User() { ID = 1, Login = "login" });

                foreach (var item in usersRepo.Get())
                {
                    Console.WriteLine(item.ID);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            Console.WriteLine("FINISH");
            Console.ReadKey();
        }
    }
}

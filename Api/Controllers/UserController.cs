// using AutoMapper;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.OpenApi.Models;
// using AiPlugin.Application;
// using AiPlugin.Domain.Manifest;
// using AiPlugin.Domain;
// using AiPlugin.Infrastructure;

// namespace AiPlugin.Api.Controllers
// {
//     [ApiController]
//     [Route("[controller]")]
//     public class UserController : ControllerBase
//     {
//         public AiPluginDbContext TempConte { get; }

//         public UserController(AiPluginDbContext tempConte)
//         {
//             TempConte = tempConte;
//         }

//         [HttpPost]
//         public async Task<Guid> CreateUser()
//         {
//             await Task.Delay(2000);
//             TempConte.Users.Add(user);
//             await TempConte.SaveChangesAsync();
//             return user;
//         }
//     }
// }
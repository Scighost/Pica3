using Pica3.CoreApi;
using System.Net;


var client = new PicaClient(new WebProxy("127.0.0.1:10809"));


await client.LoginAsync("fjoisdfdsf", "fdjdk128383");

//await client.RegisterAccountAsync(new Pica3.CoreApi.Account.RegisterAccountRequest
//{
//    Account = "fjoisdfdsf",
//    Answer1 = "sfdsfs",
//    Answer2 = "fsfdgdfgdf",
//    Answer3 = "goisfdsfsd",
//    Birthday = "1982/03/02",
//    Gender = "bot",
//    Name = "nicnicesfd",
//    Password = "fdjdk128383",
//    Question1 = "fjsofsd",
//    Question2 = "giosfds",
//    Question3 = "fjdsfdsf",
//});





Console.WriteLine();
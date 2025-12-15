#r "nuget: BCrypt.Net-Next, 4.0.3"

using BCrypt.Net;

var password = "Huythinh1";
var hash = BCrypt.Net.BCrypt.HashPassword(password, 11);
Console.WriteLine($"Password: {password}");
Console.WriteLine($"BCrypt Hash: {hash}");

// Verify it works
var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
Console.WriteLine($"Verification: {isValid}");

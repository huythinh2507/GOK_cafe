using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

const int SaltSize = 16;
const int HashSize = 32;
const int Iterations = 10000;

var password = "Huythinh1";

// Generate a random salt
byte[] salt = new byte[SaltSize];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(salt);
}

// Hash the password with the salt
byte[] hash = KeyDerivation.Pbkdf2(
    password: password,
    salt: salt,
    prf: KeyDerivationPrf.HMACSHA256,
    iterationCount: Iterations,
    numBytesRequested: HashSize);

// Combine salt and hash
byte[] hashBytes = new byte[SaltSize + HashSize];
Array.Copy(salt, 0, hashBytes, 0, SaltSize);
Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

// Convert to base64
var hashedPassword = Convert.ToBase64String(hashBytes);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hashed Password (PBKDF2): {hashedPassword}");

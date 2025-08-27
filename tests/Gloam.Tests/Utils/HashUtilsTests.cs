using System.Security.Cryptography;
using Gloam.Core.Utils;

namespace Gloam.Tests.Utils;

public class HashUtilsTests
{
    [Test]
    public void ComputeSha256Hash_WithSameInput_ShouldReturnSameHash()
    {
        var input = "test string";
        var hash1 = HashUtils.ComputeSha256Hash(input);
        var hash2 = HashUtils.ComputeSha256Hash(input);

        Assert.That(hash1, Is.EqualTo(hash2));
        Assert.That(hash1, Has.Length.EqualTo(64)); // SHA-256 produces 64-character hex string
    }

    [Test]
    public void ComputeSha256Hash_WithDifferentInputs_ShouldReturnDifferentHashes()
    {
        var hash1 = HashUtils.ComputeSha256Hash("test1");
        var hash2 = HashUtils.ComputeSha256Hash("test2");

        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void ComputeSha256Hash_WithEmptyString_ShouldReturnValidHash()
    {
        var hash = HashUtils.ComputeSha256Hash("");

        Assert.That(hash, Is.Not.Null);
        Assert.That(hash, Has.Length.EqualTo(64));
        Assert.That(hash, Is.EqualTo("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"));
    }

    [Test]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        var password = "testPassword123";
        var (hash, salt) = HashUtils.HashPassword(password);

        Assert.That(hash, Is.Not.Null.And.Not.Empty);
        Assert.That(salt, Is.Not.Null.And.Not.Empty);

        // Base64-encoded 32-byte hash should be 44 characters
        Assert.That(hash, Has.Length.EqualTo(44));
        // Base64-encoded 16-byte salt should be 24 characters  
        Assert.That(salt, Has.Length.EqualTo(24));
    }

    [Test]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        var password = "testPassword123";
        var (hash1, salt1) = HashUtils.HashPassword(password);
        var (hash2, salt2) = HashUtils.HashPassword(password);

        Assert.That(hash1, Is.Not.EqualTo(hash2));
        Assert.That(salt1, Is.Not.EqualTo(salt2));
    }

    [Test]
    public void CheckPasswordHash_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "testPassword123";
        var (hash, salt) = HashUtils.HashPassword(password);

        var result = HashUtils.CheckPasswordHash(password, hash, salt);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckPasswordHash_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "testPassword123";
        var wrongPassword = "wrongPassword456";
        var (hash, salt) = HashUtils.HashPassword(password);

        var result = HashUtils.CheckPasswordHash(wrongPassword, hash, salt);

        Assert.That(result, Is.False);
    }

    [Test]
    public void CreatePassword_ShouldReturnFormattedString()
    {
        var password = "testPassword123";
        var result = HashUtils.CreatePassword(password);

        Assert.That(result, Contains.Substring(":"));
        var parts = result.Split(':');
        Assert.That(parts, Has.Length.EqualTo(2));
        Assert.That(parts[0], Is.Not.Empty); // Hash
        Assert.That(parts[1], Is.Not.Empty); // Salt
    }

    [Test]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "testPassword123";
        var hashSalt = HashUtils.CreatePassword(password);

        var result = HashUtils.VerifyPassword(password, hashSalt);

        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "testPassword123";
        var wrongPassword = "wrongPassword456";
        var hashSalt = HashUtils.CreatePassword(password);

        var result = HashUtils.VerifyPassword(wrongPassword, hashSalt);

        Assert.That(result, Is.False);
    }

    [Test]
    public void VerifyPassword_WithHashProtocol_ShouldWork()
    {
        var password = "testPassword123";
        var hashSalt = HashUtils.CreatePassword(password);
        var hashWithProtocol = "hash://" + hashSalt;

        var result = HashUtils.VerifyPassword(password, hashWithProtocol);

        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyPassword_WithInvalidFormat_ShouldThrowException()
    {
        var password = "testPassword123";
        var invalidFormat = "invalid_format_without_colon";

        Assert.Throws<FormatException>(() => HashUtils.VerifyPassword(password, invalidFormat));
    }

    [Test]
    public void GenerateRandomRefreshToken_ShouldReturnValidToken()
    {
        var token = HashUtils.GenerateRandomRefreshToken();

        Assert.That(token, Is.Not.Null.And.Not.Empty);

        // Default 32 bytes should produce base64 string of 44 characters
        Assert.That(token, Has.Length.EqualTo(44));
    }

    [Test]
    public void GenerateRandomRefreshToken_WithCustomSize_ShouldReturnCorrectSize()
    {
        var token = HashUtils.GenerateRandomRefreshToken(16);

        // 16 bytes should produce base64 string of 24 characters
        Assert.That(token, Has.Length.EqualTo(24));
    }

    [Test]
    public void GenerateRandomRefreshToken_MultipleCalls_ShouldReturnDifferentTokens()
    {
        var token1 = HashUtils.GenerateRandomRefreshToken();
        var token2 = HashUtils.GenerateRandomRefreshToken();

        Assert.That(token1, Is.Not.EqualTo(token2));
    }

    [Test]
    public void GenerateBase64Key_ShouldReturnValidKey()
    {
        var key = HashUtils.GenerateBase64Key();

        Assert.That(key, Is.Not.Null.And.Not.Empty);
        Assert.That(key, Has.Length.EqualTo(44)); // 32 bytes base64 encoded
    }

    [Test]
    public void GenerateBase64Key_WithCustomLength_ShouldReturnCorrectLength()
    {
        var key = HashUtils.GenerateBase64Key(16);

        Assert.That(key, Has.Length.EqualTo(24)); // 16 bytes base64 encoded
    }

    [Test]
    public void Encrypt_Decrypt_ShouldRoundTrip()
    {
        var plaintext = "This is a test message for encryption.";
        var key = Convert.FromBase64String(HashUtils.GenerateBase64Key()); // AES-256 key

        var encrypted = HashUtils.Encrypt(plaintext, key);
        var decrypted = HashUtils.Decrypt(encrypted, key);

        Assert.That(decrypted, Is.EqualTo(plaintext));
    }

    [Test]
    public void Encrypt_WithSameInput_ShouldReturnDifferentOutput()
    {
        var plaintext = "This is a test message for encryption.";
        var key = Convert.FromBase64String(HashUtils.GenerateBase64Key());

        var encrypted1 = HashUtils.Encrypt(plaintext, key);
        var encrypted2 = HashUtils.Encrypt(plaintext, key);

        Assert.That(encrypted1, Is.Not.EqualTo(encrypted2)); // Different IV each time
    }

    [Test]
    public void Encrypt_ShouldIncludeIV()
    {
        var plaintext = "Test";
        var key = Convert.FromBase64String(HashUtils.GenerateBase64Key());

        var encrypted = HashUtils.Encrypt(plaintext, key);

        Assert.That(encrypted.Length, Is.GreaterThan(16)); // At least IV (16 bytes) + some ciphertext
    }

    [Test]
    public void Decrypt_WithWrongKey_ShouldThrow()
    {
        var plaintext = "This is a test message.";
        var key1 = Convert.FromBase64String(HashUtils.GenerateBase64Key());
        var key2 = Convert.FromBase64String(HashUtils.GenerateBase64Key());

        var encrypted = HashUtils.Encrypt(plaintext, key1);

        Assert.Throws<CryptographicException>(() =>
            HashUtils.Decrypt(encrypted, key2)
        );
    }

    [Test]
    public void Encrypt_Decrypt_EmptyString_ShouldWork()
    {
        var plaintext = "";
        var key = Convert.FromBase64String(HashUtils.GenerateBase64Key());

        var encrypted = HashUtils.Encrypt(plaintext, key);
        var decrypted = HashUtils.Decrypt(encrypted, key);

        Assert.That(decrypted, Is.EqualTo(plaintext));
    }
}

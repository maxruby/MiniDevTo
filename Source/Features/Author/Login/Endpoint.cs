﻿namespace Author.Login;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/author/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request r, CancellationToken c)
    {
        var author = await Data.GetAuthor(r.UserName);

        if (author?.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(r.Password, author.PasswordHash))
            ThrowError("Invalid login credentials!");

        Response.FullName = author.FullName;
        Response.UserPermissions = Allow.NamesFor(Allow.Author);
        Response.Token.ExpiryDate = DateTime.UtcNow.AddHours(4);
        Response.Token.Value = JWTBearer.CreateToken(
            signingKey: Config["JwtSigningKey"]!,
            expireAt: DateTime.UtcNow.AddHours(4),
            permissions: Allow.Author,
            claims: (Claim.AuthorID, author.AuthorID));

        await SendAsync(Response);
    }
}
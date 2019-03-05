module Auth

open System

open System.Security.Cryptography.X509Certificates
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens
open System.Security.Cryptography

let securityHandler = JwtSecurityTokenHandler()
let rsa = RSA.Create 2048
let cert =
  let req =
    CertificateRequest(
      "CN=Self Signed",
      rsa,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1
    )

  req.CertificateExtensions.Add(
    X509BasicConstraintsExtension(
      true,
      false,
      0,
      true
    )
  )
  req.CertificateExtensions.Add(
    X509SubjectKeyIdentifierExtension(
      req.PublicKey,
      false
    )
  )

  req.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays -45.,
    DateTimeOffset.UtcNow.AddDays 365.
  )

let validationParams =
  let p = TokenValidationParameters()
  p.IssuerSigningKeys <-
    [ X509SecurityKey(cert)
    ]
  p.ValidIssuer <- "http://localhost:8080"
  p.RequireExpirationTime <- true
  p.ValidAudience <- "http://localhost:8080"
  p

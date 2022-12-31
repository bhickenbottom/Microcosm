using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Microcosm
{
    public static class ContextHelper
    {
        public static string EnvironmentVariableKey = "AuthKeySHA512";

        public static bool AuthForm(HttpContext context)
        {
            if (context.Request.Method == "POST")
            {
                if (context.Request.Form.ContainsKey("authKey"))
                {
                    string authKey = context.Request.Form["authKey"];
                    if (ContextHelper.IsAuthorized(authKey))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Deny(HttpContext context, string path)
        {
            if (ContextHelper.IsAuthorizationConfigured())
            {
                ContextHelper.Redirect(context, path, "Auth key is invalid.");
            }
            else
            {
                ContextHelper.Redirect(context, path, $"Environment variable {ContextHelper.EnvironmentVariableKey} not found.");
            }
        }

        public static void Redirect(HttpContext context, string path, string message)
        {
            message = WebUtility.UrlEncode(message);
            context.Response.Redirect($"{path}?message={message}");
        }

        public static bool IsAuthorizationConfigured()
        {
            string requiredAuthKey = Environment.GetEnvironmentVariable(EnvironmentVariableKey);
            return requiredAuthKey != null;
        }

        public static bool IsAuthorized(string authKey)
        {
            string requiredAuthKeyHexString = Environment.GetEnvironmentVariable(EnvironmentVariableKey);
            if (requiredAuthKeyHexString != null)
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] authKeyBytes = Encoding.UTF8.GetBytes(authKey);
                    byte[] authKeyHash = sha512.ComputeHash(authKeyBytes);
                    string authKeyHexString = ContextHelper.ToHexString(authKeyHash);
                    if (authKeyHexString == requiredAuthKeyHexString.ToLowerInvariant())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string ToHexString(byte[] bytes)
        {
            string hexString = BitConverter.ToString(bytes);
            hexString = hexString.Replace("-", string.Empty);
            hexString = hexString.ToLower();
            return hexString;
        }
    }
}

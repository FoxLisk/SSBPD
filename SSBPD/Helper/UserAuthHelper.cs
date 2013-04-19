using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSBPD.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace SSBPD.Helper
{
    public class UserExistsException : Exception { }
    public class PasswordTooWeakException : Exception { }
    public class EmailExistsException : Exception { }
    public class InvalidEmailException : Exception { }
    public class UserAuthHelper
    {
        private SSBPDContext _db;
        private SSBPDContext db
        {
            get
            {
                if (_db == null)
                {
                    _db = new SSBPDContext();
                }
                return _db;
            }
            set
            {
                _db = value;
            }
        }
        public User getUser(string username, string plaintextPassword)
        {
            plaintextPassword = plaintextPassword.Trim();
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(plaintextPassword))
            {
                return null;
            }
            User user;
            user = (from u in db.Users
                    where u.username.Equals(username)
                    select u).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            string encryptedPassword = encryptPlaintextPassword(plaintextPassword, user.salt);
            return encryptedPassword.Equals(user.password) ? user : null;
        }
        public User getUser(int userId, string plaintextPassword)
        {
            plaintextPassword = plaintextPassword.Trim();
            if (userId <= 0)
            {
                return null;
            }
            User user;
            user = db.Users.Find(userId);
            if (user == null)
            {
                return null;
            }

            string encryptedPassword = encryptPlaintextPassword(plaintextPassword, user.salt);
            return encryptedPassword.Equals(user.password) ? user : null;
        }
        /**
         * Tries to create a user. 
         * Throws UserExistsException if the username is in use, 
         *        EmailExistsException if the email is in use,
         *        InvalidEmailException if the email is not valid.
         */
        public User createUser(string username, string plaintextPassword, string emailAddress)
        {
            plaintextPassword = plaintextPassword.Trim();
            if (db.Users.Count(u => u.username.Equals(username)) > 0)
            {
                throw new UserExistsException();
            }
            if (db.Users.Count(u => u.email.Equals(emailAddress)) > 0)
            {
                throw new EmailExistsException();
            }
            if (!isValidEmail(emailAddress))
            {
                throw new InvalidEmailException();
            }
            if (isPasswordTooWeak(plaintextPassword))
            {
                throw new PasswordTooWeakException();
            }

            User user = new User();
            user.username = username;
            user.email = emailAddress;
            user.isAdmin = false;
            user.salt = generateSalt();
            user.password = encryptPlaintextPassword(plaintextPassword, user.salt);
            user.UserGuid = System.Guid.NewGuid();

            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }
        public bool updateUser(int userId, string oldPassword, string newPassword)
        {
            User user = getUser(userId, oldPassword);
            if (user == null)
            {
                return false;
            }
            if (isPasswordTooWeak(newPassword))
            {
                throw new PasswordTooWeakException();
            }
            user.password = encryptPlaintextPassword(newPassword, user.salt);
            db.SaveChanges();
            return true;
        }
        /**
         * <summary>
         * Does not check password validity - be careful
         * </summary>
         */
        public bool updateUser(int userId, string newPassword)
        {
            User user = db.Users.Find(userId);
            if (user == null)
            {
                return false;
            }
            user.password = encryptPlaintextPassword(newPassword, user.salt);
            db.SaveChanges();
            return true;
        }

        private static string generateSalt()
        {
            string salt = "";
            char[] alphabet = "abcdefghijklmnopqrstuvwzyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToArray();
            Random r = new Random();
            for (int i = 0; i < 16; i++)
            {
                salt += alphabet[r.Next(0, alphabet.Length)];
            }
            return salt;
        }

        private static string encryptPlaintextPassword(string plaintextPassword, string salt)
        {
            var hash = new SHA256Managed();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintextPassword);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];
            Array.Copy(plainTextBytes, plainTextWithSaltBytes, plainTextBytes.Length);
            Array.ConstrainedCopy(saltBytes, 0, plainTextWithSaltBytes, plainTextBytes.Length, saltBytes.Length);

            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

            Array.Copy(hashBytes, hashWithSaltBytes, hashBytes.Length);
            Array.ConstrainedCopy(saltBytes, 0, hashWithSaltBytes, hashBytes.Length, saltBytes.Length);

            string hashValue = Convert.ToBase64String(hashWithSaltBytes);
            return hashValue;
        }

        private static bool isPasswordTooWeak(string plaintextPassword)
        {
            if (String.IsNullOrWhiteSpace(plaintextPassword) || plaintextPassword.Length < 8)
            {
                return true;
            }
            return false;
        }

        private static bool isValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                RegexOptions.IgnoreCase);
        }
    }
}